using System;
using System.Threading.Tasks;

namespace JustSaying.Messaging.MessageHandling
{
    public class ExactlyOnceHandler<T> : IHandlerAsync<T> where T : class
    {
        private readonly IHandlerAsync<T> _inner;
        private readonly IMessageLockAsync _messageLock;
        private readonly int _timeOut;
        private readonly string _handlerName;

        public ExactlyOnceHandler(IHandlerAsync<T> inner, IMessageLockAsync messageLock, int timeOut, string handlerName)
        {
            _inner = inner;
            _messageLock = messageLock;
            _timeOut = timeOut;
            _handlerName = handlerName;
        }

        private const bool RemoveTheMessageFromTheQueue = true;
        private const bool LeaveItInTheQueue = false;

        public async Task<bool> Handle(MessageEnvelope<T> env)
        {
            var lockKey = $"{env.UniqueKey()}-{typeof(T).Name.ToLower()}-{_handlerName}";
            var lockResponse = await _messageLock.TryAquireLockAsync(lockKey, TimeSpan.FromSeconds(_timeOut)).ConfigureAwait(false);
            if (!lockResponse.DoIHaveExclusiveLock)
            {
                if (lockResponse.IsMessagePermanentlyLocked)
                {
                    return RemoveTheMessageFromTheQueue;
                }

                return LeaveItInTheQueue;
            }

            try
            {
                var successfullyHandled = await _inner.Handle(env).ConfigureAwait(false);
                if (successfullyHandled)
                {
                    await _messageLock.TryAquireLockPermanentlyAsync(lockKey).ConfigureAwait(false);
                }
                return successfullyHandled;
            }
            catch
            {
                await _messageLock.ReleaseLockAsync(lockKey).ConfigureAwait(false);
                throw;
            }
        }
    }
}
