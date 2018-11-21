using System;
using System.Collections.Generic;

namespace JustSaying.Models
{
    public class PublishEnvelope
    {
        public PublishEnvelope(Message message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public Message Message { get; }

        public int? DelaySeconds { get; set; }
        public IDictionary<string, MessageAttributeValue> MessageAttributes { get; set; }
    }

    public abstract class Message
    {
        protected Message()
        {
            TimeStamp = DateTime.UtcNow;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string RaisingComponent { get; set; }
        public string Version{ get; private set; }
        public string SourceIp { get; private set; }
        public string Tenant { get; set; }
        public string Conversation { get; set; }
        public string ReceiptHandle { get; set; }
        public Uri QueueUri { get; set; }

        //footprint in order to avoid the same message being processed multiple times.
        public virtual string UniqueKey() => Id.ToString();
    }
}
