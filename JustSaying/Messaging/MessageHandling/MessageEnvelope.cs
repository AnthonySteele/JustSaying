using Amazon.SQS.Model;

namespace JustSaying.Messaging.MessageHandling
{
    public class MessageEnvelope<T>
    {
        public MessageEnvelope(Message rawMessage, T message)
        {
            RawMessage = rawMessage;
            Message = message;
        }

        public MessageEnvelope<U> Convert<U>() where U : class
        {
            return new MessageEnvelope<U>(RawMessage, Message as U);
        }

        public Message RawMessage { get; }

        public T Message { get; set; }
    }

    public static class MessageEnvelopeExtensions
    {
        public static string MessageId<T>(this MessageEnvelope<T> env) where T: class
        {
            return env.RawMessage.MessageId;
        }

        public static string ReceiptHandle<T>(this MessageEnvelope<T> env) where T : class
        {
            return env.RawMessage.ReceiptHandle;
        }

        public static string AttributeValue<T>(this MessageEnvelope<T> env, string name) where T : class
        {
            return env.RawMessage.Attributes[name];
        }

    }
}
