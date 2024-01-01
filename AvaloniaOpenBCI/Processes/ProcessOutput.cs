namespace AvaloniaOpenBCI.Processes;

public readonly record struct ProcessOutput
{
    /// <summary>
    ///     Parsed text with escape sequences and line endings removed
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    ///     Optional Raw output,
    ///     mainly for debug and logging.
    /// </summary>
    public string? RawText { get; init; }

    /// <summary>
    ///     True if output from stderr, false for stdout.
    /// </summary>
    public bool IsStdErr { get; init; }

    /// <summary>
    ///     Count of newlines to append to the output.
    ///     (Currently not used)
    /// </summary>
    public int NewLines { get; init; }

    /// <summary>
    ///     Instruction to clear last n lines
    ///     From carriage return '\r'
    /// </summary>
    public int CarriageReturn { get; init; }

    /// <summary>
    ///     Instruction to move write cursor up n lines
    ///     From Ansi sequence ESC[#A where # is count of lines
    /// </summary>
    public int CursorUp { get; init; }
}