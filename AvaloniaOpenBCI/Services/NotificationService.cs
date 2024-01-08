using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using AvaloniaOpenBCI.Exceptions;
using AvaloniaOpenBCI.Models;
using Serilog;
using Serilog.Events;

namespace AvaloniaOpenBCI.Services;

public class NotificationService : INotificationService
{
    private WindowNotificationManager? _notificationManager;

    public void Initialize(
        Visual? visual,
        NotificationPosition position = NotificationPosition.BottomRight,
        int maxItems = 4
    )
    {
        if (_notificationManager is not null)
            return;
        _notificationManager = new WindowNotificationManager(TopLevel.GetTopLevel(visual))
        {
            Position = position,
            MaxItems = maxItems
        };
    }

    public void Show(INotification notification)
    {
        _notificationManager?.Show(notification);
    }

    public void Show(
        string title,
        string message,
        NotificationType appearance = NotificationType.Information,
        TimeSpan? expiration = null
    )
    {
        Show(new Notification(title, message, appearance, expiration));
    }

    public void ShowPersistent(string title, string message, NotificationType appearance = NotificationType.Information)
    {
        Show(new Notification(title, message, appearance, TimeSpan.Zero));
    }

    public void ShowPersistent(
        AppException exception,
        NotificationType appearance = NotificationType.Error,
        LogEventLevel logLevel = LogEventLevel.Warning
    )
    {
        // Log exception
        Log.Logger.Write(logLevel, exception, "{Message}", exception.Message);

        Show(new Notification(exception.Message, exception.Details, appearance, TimeSpan.Zero));
    }

    /// <inheritdoc />
    public async Task<TaskResult<T>> TryAsync<T>(
        Task<T> task,
        string title = "Error",
        string? message = null,
        NotificationType appearance = NotificationType.Error
    )
    {
        try
        {
            return new TaskResult<T>(await task);
        }
        catch (Exception e)
        {
            Show(new Notification(title, message ?? e.Message, appearance));
            return TaskResult<T>.FromException(e);
        }
    }

    /// <inheritdoc />
    public async Task<TaskResult<bool>> TryAsync(
        Task task,
        string title = "Error",
        string? message = null,
        NotificationType appearance = NotificationType.Error
    )
    {
        try
        {
            await task;
            return new TaskResult<bool>(true);
        }
        catch (Exception e)
        {
            Show(new Notification(title, message ?? e.Message, appearance));
            return new TaskResult<bool>(false, e);
        }
    }
}
