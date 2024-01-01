using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaOpenBCI.Extensions;
using AvaloniaOpenBCI.Processes;
using CommunityToolkit.Mvvm.ComponentModel;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace AvaloniaOpenBCI.ViewModels;

public partial class ConsoleViewModel : ObservableObject, IDisposable, IAsyncDisposable
{
    /// <summary>
    ///     Lock for accessing <see cref="_writeCursor" />
    /// </summary>
    private readonly AsyncLock _writeCursorLock = new();

    // Queue for console updates
    private BufferBlock<ProcessOutput> _buffer = new();

    [ObservableProperty]
    private TextDocument _document = new();

    private bool _isDisposed;

    // Cancellation token source for updateTask
    private CancellationTokenSource? _updateCts;

    // Task that updates the console (runs on UI thread)
    private Task? _updateTask;

    /// <summary>
    ///     Current offset for write operations.
    /// </summary>
    private int _writeCursor;

    private static ILogger Logger
    {
        get => Log.Logger;
    }

    public bool IsUpdatesRunning
    {
        get => _updateTask?.IsCompleted == false;
    }

    /// <summary>
    ///     Timeout for acquiring locks on <see cref="_writeCursor" />
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public TimeSpan WriteCursorLockTimeout { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    ///     Gets a cancellation token using the cursor lock timeout
    /// </summary>
    private CancellationToken WriteCursorLockTimeoutToken
    {
        get => new CancellationTokenSource(WriteCursorLockTimeout).Token;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _updateCts?.Cancel();
        _updateCts?.Dispose();
        _updateCts = null;

        if (_updateTask is not null)
        {
            Logger.Debug("Waiting for console update task shutdown...");

            await _updateTask;
            _updateTask.Dispose();
            _updateTask = null;

            Logger.Debug("Console update task shutdown complete");
        }

        _isDisposed = true;

        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _updateCts?.Cancel();
        _updateCts?.Dispose();
        _updateCts = null;

        _buffer.Complete();

        if (_updateTask is not null)
        {
            Logger.Debug("Shutting down console update task");

            try
            {
                _updateTask.WaitWithoutException(new CancellationTokenSource(1000).Token);
                _updateTask.Dispose();
                _updateTask = null;
            }
            catch (OperationCanceledException)
            {
                Logger.Warning("During shutdown - Console update task cancellation timed out");
            }
            catch (InvalidOperationException e)
            {
                Logger.Warning(e, "During shutdown - Console update task dispose failed");
            }
        }

        _isDisposed = true;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Starts update task for processing Post messages.
    /// </summary>
    /// <exception cref="InvalidOperationException">If update task is already running</exception>
    public void StartUpdates()
    {
        if (_updateTask is not null)
        {
            throw new InvalidOperationException("Update task is already running");
        }
        _updateCts = new CancellationTokenSource();
        _updateTask = Dispatcher.UIThread.InvokeAsync(ConsoleUpdateLoop, DispatcherPriority.Send);
    }

    /// <summary>
    ///     Cancels the update task and waits for it to complete.
    /// </summary>
    public async Task StopUpdatesAsync()
    {
        Logger.Debug($"Stopping console updates, current buffer items: {_buffer.Count}");
        // First complete the buffer
        _buffer.Complete();
        // Wait for buffer to complete, max 3 seconds
        var completionCts = new CancellationTokenSource(3000);
        try
        {
            await _buffer.Completion.WaitAsync(completionCts.Token);
        }
        catch (TaskCanceledException e)
        {
            // We can still continue since this just means we lose
            // some remaining output
            Logger.Warning("Buffer completion timed out: " + e.Message);
        }

        // Cancel update task
        _updateCts?.Cancel();
        _updateCts = null;

        // Wait for update task
        if (_updateTask is not null)
        {
            await _updateTask;
            _updateTask = null;
        }
        Logger.Debug($"Stopped console updates with {_buffer.Count} buffer items remaining");
    }

    /// <summary>
    ///     Clears the console and sets a new buffer.
    ///     This also resets the write cursor to 0.
    /// </summary>
    public async Task Clear()
    {
        // Clear document
        Document.Text = string.Empty;
        // Reset write cursor
        await ResetWriteCursor();
        // Clear buffer and create new one
        _buffer.Complete();
        _buffer = new BufferBlock<ProcessOutput>();
    }

    /// <summary>
    ///     Resets the write cursor to be equal to the document length.
    /// </summary>
    public async Task ResetWriteCursor()
    {
        using (await _writeCursorLock.LockAsync(WriteCursorLockTimeoutToken))
        {
            Logger.Debug($"Reset cursor to end: ({_writeCursor} -> {Document.TextLength})");
            _writeCursor = Document.TextLength;
        }
        DebugPrintDocument();
    }

    private async Task ConsoleUpdateLoop()
    {
        // This must be run in the UI thread
        Dispatcher.UIThread.VerifyAccess();

        // Get cancellation token
        CancellationToken ct = _updateCts?.Token ??
                               throw new InvalidOperationException("Update cancellation token must be set");

        try
        {
            while (!ct.IsCancellationRequested)
            {
                ProcessOutput output;

                try
                {
                    output = await _buffer.ReceiveAsync(ct);
                }
                catch (InvalidOperationException e)
                {
                    // Thrown when buffer is completed, convert to OperationCanceledException
                    throw new OperationCanceledException("Update buffer completed", e);
                }

                string outputType = output.IsStdErr ? "stderr" : "stdout";
                Logger.Debug(
                    $"Processing: [{outputType}] (Text = {output.Text.ToRepr()}, " +
                    $"Raw = {output.RawText?.ToRepr()}, " +
                    $"CarriageReturn = {output.CarriageReturn}, " +
                    $"CursorUp = {output.CursorUp}, "
                );

                // Link the cancellation token to the write cursor lock timeout
                CancellationToken linkedCt = CancellationTokenSource
                    .CreateLinkedTokenSource(ct, WriteCursorLockTimeoutToken).Token;

                using (await _writeCursorLock.LockAsync(linkedCt))
                {
                    ConsoleUpdateOne(output);
                }
            }
        }
        catch (OperationCanceledException e)
        {
            Logger.Debug($"Console update loop canceled: {e.Message}");
        }
        catch (Exception e)
        {
            // Log other errors and continue here to not crash the UI thread
            Logger.Error(e, $"Unexpected error in console update loop: {e.GetType().Name} {e.Message}");
        }
    }

    /// <summary>
    ///     Handle one instance of ProcessOutput.
    ///     Calls to this function must be synchronized with <see cref="_writeCursorLock" />
    /// </summary>
    /// <remarks>Not checked, but must be run in the UI thread.</remarks>
    private void ConsoleUpdateOne(ProcessOutput output)
    {
        Debug.Assert(Dispatcher.UIThread.CheckAccess());

        // If we have a carriage return,
        // start current write at the beginning of the current line
        if (output.CarriageReturn > 0)
        {
            DocumentLine? currentLine = Document.GetLineByOffset(_writeCursor);

            // Get the start of current line as new write cursor
            int lineStartOffset = currentLine.Offset;

            // See if we need to move the cursor
            if (lineStartOffset == _writeCursor)
            {
                Logger.Debug(
                    $"Cursor already at start for carriage return " +
                    $"(offset = {lineStartOffset}, line = {currentLine.LineNumber})"
                );
            }
            else
            {
                // Also remove everything on current line
                // We'll temporarily do this for now to fix progress
                int lineEndOffset = currentLine.EndOffset;
                int lineLength = lineEndOffset - lineStartOffset;
                Document.Remove(lineStartOffset, lineLength);

                Logger.Debug(
                    $"Moving cursor to start for carriage return " + $"({_writeCursor} -> {lineStartOffset})"
                );
                _writeCursor = lineStartOffset;
            }
        }

        // Write new text
        if (!string.IsNullOrEmpty(output.Text))
        {
            DirectWriteLinesToConsole(output.Text);
        }

        // Handle cursor movements
        if (output.CursorUp > 0)
        {
            // Get the line and column of the current cursor
            TextLocation currentLocation = Document.GetLocation(_writeCursor);

            if (currentLocation.Line == 1)
            {
                // We are already on the first line, ignore
                Logger.Debug("Cursor up: Already on first line");
            }
            else
            {
                // We want to move up one line
                var targetLocation = new TextLocation(currentLocation.Line - 1, currentLocation.Column);
                int targetOffset = Document.GetOffset(targetLocation);

                // Update cursor to target offset
                Logger.Debug(
                    $"Cursor up: Moving (line {currentLocation.Line}, {_writeCursor})" +
                    $" -> (line {targetLocation.Line}, {targetOffset})"
                );

                _writeCursor = targetOffset;
            }
        }
        DebugPrintDocument();
    }

    /// <summary>
    ///     Write text potentially containing line breaks to the console.
    ///     <remarks>This call will hold a upgradeable read lock</remarks>
    /// </summary>
    private void DirectWriteLinesToConsole(string text)
    {
        // When our cursor is not at end, newlines should be interpreted as commands to
        // move cursor forward to the next linebreak instead of inserting a newline.

        // If text contains no newlines, we can just call DirectWriteToConsole
        // Also if cursor is equal to document length
        if (!text.Contains(Environment.NewLine) || _writeCursor == Document.TextLength)
        {
            DirectWriteToConsole(text);
            return;
        }

        // Otherwise we need to handle how linebreaks are treated
        // Split text into lines
        var lines = text.Split(Environment.NewLine).ToList();

        foreach (string lineText in lines.SkipLast(1))
        {
            // Insert text
            DirectWriteToConsole(lineText);

            // Set cursor to start of next line, if we're not already there
            DocumentLine? currentLine = Document.GetLineByOffset(_writeCursor);
            // If next line is available, move cursor to start of next line
            if (currentLine.LineNumber < Document.LineCount)
            {
                DocumentLine? nextLine = Document.GetLineByNumber(currentLine.LineNumber + 1);
                Logger.Debug(
                    $"Moving cursor to start of next line " + $"({_writeCursor} -> {nextLine.Offset})"
                );
                _writeCursor = nextLine.Offset;
            }
            else
            {
                // Otherwise move to end of current line, and direct insert a newline
                int lineEndOffset = currentLine.EndOffset;
                Logger.Debug(
                    $"Moving cursor to end of current line " + $"({_writeCursor} -> {lineEndOffset})"
                );
                _writeCursor = lineEndOffset;
                DirectWriteToConsole(Environment.NewLine);
            }
        }
    }

    /// <summary>
    ///     Write text to the console, does not handle newlines.
    ///     This should probably only be used by <see cref="DirectWriteLinesToConsole" />
    ///     <remarks>This call will hold a upgradeable read lock</remarks>
    /// </summary>
    private void DirectWriteToConsole(string text)
    {
        using (Document.RunUpdate())
        {
            // Need to replace text first if cursor lower than document length
            int replaceLength = Math.Min(Document.TextLength - _writeCursor, text.Length);
            if (replaceLength > 0)
            {
                string newText = text[..replaceLength];
                Logger.Debug(
                    $"Replacing: (cursor = {_writeCursor}, length = {replaceLength}, " +
                    $"text = {Document.GetText(_writeCursor, replaceLength).ToRepr()} " +
                    $"-> {newText.ToRepr()})"
                );

                Document.Replace(_writeCursor, replaceLength, newText);
                _writeCursor += replaceLength;
            }

            // If we replaced less than content.Length, we need to insert the rest
            int remainingLength = text.Length - replaceLength;
            if (remainingLength > 0)
            {
                string textToInsert = text[replaceLength..];
                Logger.Debug($"Inserting: (cursor = {_writeCursor}, " + $"text = {textToInsert.ToRepr()})");

                Document.Insert(_writeCursor, textToInsert);
                _writeCursor += textToInsert.Length;
            }
        }
    }

    /// <summary>
    ///     Debug function to print the current document to the console.
    ///     Includes formatted cursor position.
    /// </summary>
    [Conditional("DEBUG")]
    private void DebugPrintDocument()
    {
        if (!Logger.IsEnabled(LogEventLevel.Debug))
        {
            return;
        }

        string? text = Document.Text;
        // Add a number for each line
        // Add an arrow line for where the cursor is, for example (cursor on offset 3):
        //
        // 1 | This is the first line
        // ~~~~~~~^ (3)
        // 2 | This is the second line
        //

        var lines = text.Split(Environment.NewLine).ToList();
        int numberPadding = lines.Count.ToString().Length;
        for (int i = 0; i < lines.Count; i++)
        {
            lines[i] = $"{(i + 1).ToString().PadLeft(numberPadding)} | {lines[i]}";
        }
        DocumentLine? cursorLine = Document.GetLineByOffset(_writeCursor);
        int cursorLineOffset = _writeCursor - cursorLine.Offset;

        // Need to account for padding + line number + space + cursor line offset
        int linePadding = numberPadding + 3 + cursorLineOffset;
        string cursorLineArrow = new string('~', linePadding) + $"^ ({_writeCursor})";

        // If more than line count, append to end
        if (cursorLine.LineNumber >= lines.Count)
        {
            lines.Add(cursorLineArrow);
        }
        else
        {
            lines.Insert(cursorLine.LineNumber, cursorLineArrow);
        }

        string? textWithCursor = string.Join(Environment.NewLine, lines);

        Logger.Debug("[Current Document]");
        Logger.Debug(textWithCursor);
    }

    /// <summary>
    ///     Posts an update to the console
    ///     <remarks>Safe to call on non-UI threads</remarks>
    /// </summary>
    public void Post(ProcessOutput output)
    {
        // If update task is running, send to buffer
        if (_updateTask != null)
        {
            _buffer.Post(output);
            return;
        }
        // Otherwise, use manual update one
        Logger.Debug("Synchronous post update to console: {@Output}", output);
        Dispatcher.UIThread.Post(() => ConsoleUpdateOne(output));
    }

    /// <summary>
    ///     Posts an update to the console.
    ///     Helper for calling Post(ProcessOutput) with strings
    /// </summary>
    public void Post(string text)
    {
        Post(new ProcessOutput { Text = text });
    }

    /// <summary>
    ///     Posts an update to the console.
    ///     Equivalent to Post(text + Environment.NewLine)
    /// </summary>
    public void PostLine(string text)
    {
        Post(new ProcessOutput { Text = text + Environment.NewLine });
    }
}