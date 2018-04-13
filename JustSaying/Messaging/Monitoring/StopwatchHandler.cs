using System.Diagnostics;
using System.Threading.Tasks;
using JustSaying.Messaging.MessageHandling;

namespace JustSaying.Messaging.Monitoring
{
    public class StopwatchHandler<T> : IHandlerAsync<T> where T : class
    {
        private readonly IHandlerAsync<T> _inner;
        private readonly IMeasureHandlerExecutionTime _monitoring;

        public StopwatchHandler(IHandlerAsync<T> inner, IMeasureHandlerExecutionTime monitoring)
        {
            _inner = inner;
            _monitoring = monitoring;
        }

        public async Task<bool> Handle(MessageEnvelope<T> env)
        {
            var watch = Stopwatch.StartNew();
            var result = await _inner.Handle(env).ConfigureAwait(false);

            watch.Stop();

            _monitoring.HandlerExecutionTime(TypeName(_inner), TypeName(env.Message), watch.Elapsed);
            return result;
        }

        private static string TypeName(object obj) => obj.GetType().Name.ToLower();
    }
}
