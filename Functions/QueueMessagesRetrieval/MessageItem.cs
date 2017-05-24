using System;
using System.Collections.Generic;

namespace Functions.QueueMessagesRetrieval
{
    public class MessageItem
    {
        public string lockToken { get; set; }
        public DateTime lockUntilUtc { get; set; }
        public long sequenceNumber { get; set; }
        public IDictionary<string, object> properties { get; set; }

        public MessageItem ConvertMe(Microsoft.ServiceBus.Messaging.BrokeredMessage message)
        {
            return new MessageItem
            {
                lockToken = message.LockToken.ToString(),
                lockUntilUtc = message.LockedUntilUtc,
                sequenceNumber = message.SequenceNumber,
                properties = message.Properties
            };
        }
    }
}
