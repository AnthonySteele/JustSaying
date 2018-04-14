using Amazon.SQS.Model;

namespace JustSaying.Messaging.MessageHandling
{
    /// <summary>
    /// This is a Proof of Concept of the "message envelope" idea for JustSaying,
    /// consider it clearly not for the current release, 
    /// but as a slightly fleshed-out suggestion for a path afterwards
    /// 
    /// The idea is that instead of all messages having a base class,
    /// which allows limited access to whatever metadata has been copied into the base class,
    /// we do "composition instead of inheritance"
    /// The handler receives a message inside an envelope
    /// 
    /// - The envelope also has the "Raw" message of type Amazon.SQS.Model.Message 
    ///  and thereby all existent metdata can be accessed.
    /// If this access is frequent or complex, you can build a helper method.
    /// You can promote that to an extension method on the envelope
    /// without having to alter the base class and issue a new revision of message type libs
    /// 
    /// - There is no need for a message base class at all.
    /// If your serializer can turn the message text into some type T  then you're good to use it.
    /// 
    /// Downsides:
    /// 
    /// - It is not good that the message handler signature has grown from
    ///  bool Handle(SomeMessage message)
    /// to
    ///  Task<bool> Handle(MessageEnvelope<SomeMessage> env)
    ///  But I dn't know a way around that.
    /// 
    /// - the publish will need a different envelope for publishing concerns like `DelaySeconds`
    /// 
    /// it's a breaking change, if you don't care about the envelope you'd have to add this code to the top of your handler:
    /// var message = env.Message;
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

    /// <summary>
    /// These extension methods are examples, not an exhaustive set.
    /// I don't know what medatadata are going to the important ones,
    /// demonstrating the pattern
    /// </summary>
    public static class MessageEnvelopeExtensions
    {
        public static string MessageId<T>(this MessageEnvelope<T> env)
        {
            return env.RawMessage.MessageId;
        }

        public static string ReceiptHandle<T>(this MessageEnvelope<T> env)
        {
            return env.RawMessage.ReceiptHandle;
        }

        public static string AttributeValue<T>(this MessageEnvelope<T> env, string name)
        {
            return env.RawMessage.Attributes[name];
        }

        public static bool HasAttribute<T>(this MessageEnvelope<T> env, string name)
        {
            return env.RawMessage.Attributes.ContainsKey(name);
        }
    }
}
