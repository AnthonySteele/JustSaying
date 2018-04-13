using System.Threading.Tasks;

namespace JustSaying.Messaging.MessageHandling
{
    /// <summary>
    /// Async message handler
    /// </summary>
    /// <typeparam name="T">Type of message to be handled</typeparam>
    public interface IHandlerAsync<T>
    {
        /// <summary>
        /// Handle a message of a given type
        /// </summary>
        /// <param name="env">Envelope of message to handle</param>
        /// <returns>Was handling successful?</returns>
        Task<bool> Handle(MessageEnvelope<T> env);
    }
}
