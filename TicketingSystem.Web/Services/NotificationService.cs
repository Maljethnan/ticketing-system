using Microsoft.AspNetCore.SignalR.Client;

namespace TicketingSystem.Web.Services;

public class NotificationItem
{
    public string Message { get; set; } = "";
    public string Type { get; set; } = "";
    public int? TicketId { get; set; }
    public DateTime Timestamp { get; set; }
}

public class NotificationService
{
    private HubConnection? _connection;
    public event Action<NotificationItem>? OnNotificationReceived;

    public async Task StartAsync(string url, string token)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(url + "/notificationHub", opt => opt.AccessTokenProvider = () => Task.FromResult<string?>(token))
            .Build();

        _connection.On<NotificationItem>("ReceiveNotification", item => OnNotificationReceived?.Invoke(item));
        await _connection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_connection is null)
            return;

        await _connection.StopAsync();
    }
}
