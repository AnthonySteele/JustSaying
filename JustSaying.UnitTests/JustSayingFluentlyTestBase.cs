using System;
using System.Threading.Tasks;
using JustSaying.AwsTools;
using JustSaying.AwsTools.QueueCreation;
using NSubstitute;
using Xunit;

namespace JustSaying.UnitTests
{
    public abstract class JustSayingFluentlyTestBase : IAsyncLifetime
    {
        protected IPublishConfiguration Configuration;
        protected IAmJustSaying Bus;
        protected readonly IVerifyAmazonQueues QueueVerifier = Substitute.For<IVerifyAmazonQueues>();
        private bool _recordThrownExceptions;
        protected Exception ThrownException { get; private set; }

        public MessagingBusBuilder SystemUnderTest { get; private set; }

        protected virtual MessagingBusBuilder CreateSystemUnderTest()
        {
            if (Configuration == null)
            {
                Configuration = new MessagingConfig();
            }


            var builder = new MessagingBusBuilder()
                .Client((options) => options.WithClientFactory(AwsClient))
                .Messaging((options) => options
                    .WithRegion("defaultRegion")
                    .WithActiveRegion("defaultRegion"));

            return builder;
        }

        private IAwsClientFactory AwsClient()
        {
            return Substitute.For<IAwsClientFactory>();
        }

        public async Task InitializeAsync()
        {
            Given();

            try
            {
                SystemUnderTest = CreateSystemUnderTest();
                await WhenAsync().ConfigureAwait(false);
            }
            catch (Exception ex) when (_recordThrownExceptions)
            {
                ThrownException = ex;
            }
        }

        protected virtual void Given()
        {

        }

        protected abstract Task WhenAsync();

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public void RecordAnyExceptionsThrown()
        {
            _recordThrownExceptions = true;
        }
    }
}
