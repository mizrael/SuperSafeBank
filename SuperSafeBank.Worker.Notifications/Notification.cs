using System;

namespace SuperSafeBank.Worker.Notifications
{
    public class Notification
    {
        public Notification(string recipient, string message)
        {
            if (string.IsNullOrWhiteSpace(recipient))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(recipient));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
            Recipient = recipient;
            Message = message;
        }
        public string Recipient { get; }
        public string Message { get; }

    }
}