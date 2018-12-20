using System.Threading;

namespace JustSaying.Messaging.MessageHandling
{
    public class MessageContextAccessor: IMessageContextAccessor
    {
        private static readonly AsyncLocal<MessageContext> Context = new AsyncLocal<MessageContext>();

        public MessageContext MessageContext
        {
            get => Context.Value;
            set => Context.Value = value;
        }
    }
}
