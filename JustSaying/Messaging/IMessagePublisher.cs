using System.Threading;
using System.Threading.Tasks;

namespace JustSaying.Messaging
{
    public interface IMessagePublisher
    {
#if AWS_SDK_HAS_SYNC
        void Publish(object message);
#endif
        Task PublishAsync(object message);

        Task PublishAsync(object message, CancellationToken cancellationToken);
    }
}
