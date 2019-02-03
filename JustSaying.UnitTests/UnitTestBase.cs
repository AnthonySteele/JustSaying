using System;
using System.Threading.Tasks;
using JustSaying.AwsTools;
using JustSaying.AwsTools.QueueCreation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace JustSaying.UnitTests
{
    public abstract class UnitTestBase : IAsyncLifetime
    {
        protected ITestOutputHelper OutputHelper { get; }

        private bool _recordThrownExceptions;
        protected Exception ThrownException { get; private set; }

        public IServiceProvider Services { get; private set; }

        protected readonly IVerifyAmazonQueues QueueVerifier = Substitute.For<IVerifyAmazonQueues>();

        protected UnitTestBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        protected IServiceProvider CreateServices()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            return services.BuildServiceProvider();
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging((p) => p.AddXUnit(OutputHelper).SetMinimumLevel(LogLevel.Debug))
                .AddJustSaying(ConfigureJustSaying);

            services.AddSingleton(QueueVerifier);
        }

        protected virtual void ConfigureJustSaying(MessagingBusBuilder builder)
        {
            builder
                .Client((options) => options.WithClientFactory(() => Substitute.For<IAwsClientFactory>()))
                .Messaging((options) => options
                    .WithRegions("defaultRegion", "failoverRegion")
                    .WithActiveRegion("defaultRegion"));
        }


        public async Task InitializeAsync()
        {
            Given();

            try
            {
                Services = CreateServices();
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

        protected virtual Task WhenAsync()
        {
            return Task.CompletedTask;
        }

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
