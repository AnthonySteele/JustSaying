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
        private bool _recordThrownExceptions;
        protected Exception ThrownException { get; private set; }

        public IServiceProvider Services { get; private set; }

        protected ITestOutputHelper OutputHelper { get; }

        protected readonly IVerifyAmazonQueues QueueVerifier = Substitute.For<IVerifyAmazonQueues>();

        protected UnitTestBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        protected IServiceProvider CreateServices()
        {
            var services = new ServiceCollection();

            services
                .AddLogging((p) => p.AddXUnit(OutputHelper).SetMinimumLevel(LogLevel.Debug))
                .AddJustSaying((p) => ConfigureJustSaying(p, services));

            services.AddSingleton(QueueVerifier);
            ConfigureServices(services);

            return services.BuildServiceProvider();
        }

        protected virtual void ConfigureJustSaying(MessagingBusBuilder builder, IServiceCollection services)
        {
            builder
                .Client((options) => options.WithClientFactory(() => Substitute.For<IAwsClientFactory>()))
                .Messaging((options) => options
                    .WithRegions("defaultRegion", "failoverRegion")
                    .WithActiveRegion("defaultRegion"));
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
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
