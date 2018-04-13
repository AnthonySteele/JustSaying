using System.Threading.Tasks;
using JustSaying.Messaging.MessageHandling;
using JustSaying.TestingFramework;

namespace JustSaying.IntegrationTests.TestHandlers
{
    public class BlockingOrderProcessor : IHandlerAsync<OrderPlaced>
    {

        public BlockingOrderProcessor()
        {
        }

        public int ReceivedMessageCount { get; private set; }

        public TaskCompletionSource<object> DoneSignal { get; private set; }

        public Task<bool> Handle(MessageEnvelope<OrderPlaced> env)
        {
            ReceivedMessageCount++;
            return Task.FromResult(true);
        }
    }
}
