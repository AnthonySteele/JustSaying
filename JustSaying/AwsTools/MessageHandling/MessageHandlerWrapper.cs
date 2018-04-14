using System;
using System.Threading.Tasks;
using JustSaying.Messaging.MessageHandling;
using JustSaying.Messaging.Monitoring;

namespace JustSaying.AwsTools.MessageHandling
{
    public class MessageHandlerWrapper
    {
        private readonly IMessageMonitor _messagingMonitor;
        private readonly IMessageLockAsync _messageLock;

        public MessageHandlerWrapper(IMessageLockAsync messageLock, IMessageMonitor messagingMonitor)
        {
            _messageLock = messageLock;
            _messagingMonitor = messagingMonitor;
        }

        public Func<MessageEnvelope<object>, Task<bool>> WrapMessageHandler<T>(Func<IHandlerAsync<T>> futureHandler) where T : class
        {
            return async env =>
            {
                var handler = futureHandler();
                handler = MaybeWrapWithExactlyOnce(handler);
                handler = MaybeWrapWithStopwatch(handler);

                var typedEnv = env.Convert<T>();
                return await handler.Handle(typedEnv).ConfigureAwait(false);
            };
        }

        private IHandlerAsync<T> MaybeWrapWithExactlyOnce<T>(IHandlerAsync<T> handler) where T: class
        {
            var handlerType = handler.GetType();
            var exactlyOnceMetadata = new ExactlyOnceReader(handlerType);
            if (!exactlyOnceMetadata.Enabled)
            {
                return handler;
            }

            if (_messageLock == null)
            {
                throw new Exception("IMessageLock is null. You need to specify an implementation for IMessageLock.");
            }

            var handlerName = handlerType.FullName.ToLower();
            return new ExactlyOnceHandler<T>(handler, _messageLock, exactlyOnceMetadata.GetTimeOut(), handlerName);
        }

        private IHandlerAsync<T> MaybeWrapWithStopwatch<T>(IHandlerAsync<T> handler) where T : class
        {
            if (!(_messagingMonitor is IMeasureHandlerExecutionTime executionTimeMonitoring))
            {
                return handler;
            }

            return new StopwatchHandler<T>(handler, executionTimeMonitoring);
        }
    }
}
