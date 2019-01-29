using System;
using System.Threading.Tasks;
using JustSaying.AwsTools;
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

        public IServiceCollection Services { get; private set; }

        protected ITestOutputHelper OutputHelper { get; }

        protected UnitTestBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        protected IServiceCollection CreateServices()
        {
            var services = new ServiceCollection()
                .AddLogging((p) => p.AddXUnit(OutputHelper).SetMinimumLevel(LogLevel.Debug))
                .AddJustSaying(ConfigureJustSaying);

            ConfigureServices(services);

            return services;
        }

        protected virtual void ConfigureJustSaying(MessagingBusBuilder builder, IServiceProvider serviceProvider)
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
