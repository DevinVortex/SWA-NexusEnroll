using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.Factory;

public class EmailNotification : Notification
{
    private readonly string _recipient;
    private readonly string _message;

    public EmailNotification(string recipient, string message)
    {
        _recipient = recipient;
        _message = message;
    }

    public void Send() =>
        Console.WriteLine($"Sending Email to [{_recipient}]: [{_message}]");
}