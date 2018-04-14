using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using JustSaying.Messaging;
using JustSaying.Messaging.MessageSerialisation;
using Microsoft.Extensions.Logging;

namespace JustSaying.AwsTools.MessageHandling
{
    public class SqsPublisher : SqsQueueByName, IMessagePublisher
    {
        private readonly IAmazonSQS _client;
        private readonly IMessageSerialisationRegister _serialisationRegister;
        public Action<MessageResponse, object> MessageResponseLogger { get; set; }

        public SqsPublisher(RegionEndpoint region, string queueName, IAmazonSQS client,
            int retryCountBeforeSendingToErrorQueue, IMessageSerialisationRegister serialisationRegister,
            ILoggerFactory loggerFactory)
            : base(region, queueName, client, retryCountBeforeSendingToErrorQueue, loggerFactory)
        {
            _client = client;
            _serialisationRegister = serialisationRegister;
        }

#if AWS_SDK_HAS_SYNC
        public void Publish(object message)
        {
            var request = BuildSendMessageRequest(message);

            try
            {
                var response = _client.SendMessage(request);

                MessageResponseLogger?.Invoke(new MessageResponse
                {
                    HttpStatusCode = response?.HttpStatusCode,
                    MessageId = response?.MessageId
                }, message);
            }
            catch (Exception ex)
            {
                throw new PublishException(
                    $"Failed to publish message to SQS. QueueUrl: {request.QueueUrl} MessageBody: {request.MessageBody}",
                    ex);
            }
        }
#endif

        public Task PublishAsync(object message) => PublishAsync(message, CancellationToken.None);

        public async Task PublishAsync(object message, CancellationToken cancellationToken)
        {
            var request = BuildSendMessageRequest(message);
            try
            {
                var response = await _client.SendMessageAsync(request, cancellationToken).ConfigureAwait(false);
                MessageResponseLogger?.Invoke(new MessageResponse
                {
                    HttpStatusCode = response?.HttpStatusCode,
                    MessageId = response?.MessageId
                }, message);
            }
            catch (Exception ex)
            {
                throw new PublishException(
                    $"Failed to publish message to SQS. QueueUrl: {request.QueueUrl} MessageBody: {request.MessageBody}",
                    ex);
            }
        }

        private SendMessageRequest BuildSendMessageRequest(object message)
        {
            var request = new SendMessageRequest
            {
                MessageBody = GetMessageInContext(message),
                QueueUrl = Url
            };

            if (message.DelaySeconds.HasValue)
            {
                request.DelaySeconds = message.DelaySeconds.Value;
            }
            return request;
        }

        public string GetMessageInContext(object message) => _serialisationRegister.Serialise(message, serializeForSnsPublishing: false);
    }
}
