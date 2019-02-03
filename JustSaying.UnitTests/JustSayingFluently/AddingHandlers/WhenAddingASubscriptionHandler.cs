using System;
using System.Threading;
using System.Threading.Tasks;
using JustSaying.AwsTools.QueueCreation;
using JustSaying.Messaging.MessageHandling;
using JustSaying.Messaging.MessageSerialization;
using JustSaying.Models;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace JustSaying.UnitTests.JustSayingFluently.AddingHandlers
{
    public class WhenAddingASubscriptionHandler : UnitTestBase
    {
        private readonly IHandlerAsync<Message> _handler = Substitute.For<IHandlerAsync<Message>>();

        public WhenAddingASubscriptionHandler(ITestOutputHelper outputHelper) : base(outputHelper)
        { }

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddSingleton<IHandlerAsync<Message>>(_handler);
        }

        protected override void ConfigureJustSaying(MessagingBusBuilder builder)
        {
            base.ConfigureJustSaying(builder);
            builder.Subscriptions(
                (options) => options.ForQueue<Message>());
        }

        protected override Task WhenAsync()
        {
            var messageBus = Services.GetService<IMessagingBus>();

            var ctx = new CancellationTokenSource();
            ctx.CancelAfter(TimeSpan.FromSeconds(1));

            messageBus.Start(ctx.Token);
            return Task.CompletedTask;
        }

        [Fact]
        public void TheTopicAndQueueIsCreatedInEachRegion()
        {
            QueueVerifier.Received()
                .EnsureTopicExistsWithQueueSubscribedAsync(
                    "defaultRegion",
                    Arg.Any<IMessageSerializationRegister>(),
                    Arg.Any<SqsReadConfiguration>(),
                    Arg.Any<IMessageSubjectProvider>());

            QueueVerifier.Received()
                .EnsureTopicExistsWithQueueSubscribedAsync(
                    "failoverRegion",
                    Arg.Any<IMessageSerializationRegister>(),
                    Arg.Any<SqsReadConfiguration>(),
                    Arg.Any<IMessageSubjectProvider>());
        }

        [Fact]
        public void TheSubscriptionIsCreatedInEachRegion()
        {
            //Bus.Received(2).AddNotificationSubscriber(Arg.Any<string>(), Arg.Any<INotificationSubscriber>());
        }

        [Fact]
        public void HandlerIsAddedToBus()
        {
            //Bus.Received().AddMessageHandler(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Func<IHandlerAsync<Message>>>());
        }

        [Fact]
        public void SerializationIsRegisteredForMessage()
        {
           // Bus.SerializationRegister.Received().AddSerializer<Message>(Arg.Any<IMessageSerializer>());
        }
    }
}
