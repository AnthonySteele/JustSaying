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
        public WhenAddingASubscriptionHandler(ITestOutputHelper outputHelper) : base(outputHelper)
        { }

        private readonly IHandlerAsync<Message> _handler = Substitute.For<IHandlerAsync<Message>>();

        protected override void ConfigureJustSaying(MessagingBusBuilder builder, IServiceCollection services)
        {
            base.ConfigureJustSaying(builder, services);

            services.AddSingleton(_handler);

            builder.Subscriptions(
                (options) => options.ForQueue<Message>());
        }

        protected override Task WhenAsync()
        {
            var messageBuss = Services.GetService<IMessagingBus>();

            var ctx = new CancellationTokenSource();
            ctx.CancelAfter(TimeSpan.FromSeconds(1));

            messageBuss.Start(ctx.Token);
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
