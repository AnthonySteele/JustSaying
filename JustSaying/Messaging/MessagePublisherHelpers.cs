using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using JustSaying.Models;

namespace JustSaying.Messaging
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MessagePublisherHelpers
    {
        public static Task PublishAsync(this IMessagePublisher publisher, Message message)
        {
            return PublishAsync(publisher, message, CancellationToken.None);
        }

        public static async Task PublishAsync(this IMessagePublisher publisher,
            Message message, CancellationToken cancellationToken)
        {
            if (publisher == null)
            {
                throw new ArgumentNullException(nameof(publisher));
            }

            await publisher.PublishAsync(new PublishEnvelope(message), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
