using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustSaying.Messaging.MessageHandling
{
    public class ListHandler<T> : IHandlerAsync<T> where T: class
    {
        private readonly IEnumerable<IHandlerAsync<T>> _handlers;

        public ListHandler(IEnumerable<IHandlerAsync<T>> handlers)
        {
            _handlers = handlers;
        }

        public async Task<bool> Handle(MessageEnvelope<T> env)
        {
            var handlerTasks = _handlers.Select(h => h.Handle(env));
            var handlerResults = await Task.WhenAll(handlerTasks)
                .ConfigureAwait(false);

            return handlerResults.All(x => x);
        }
    }
}
