using MassTransit;
using MassTransit.DependencyInjection.Testing;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Sample.Tests
{
    public class ContainerTestHarnessTests
    {
        public class SampleConsumer : IConsumer<WeatherForecast>
        {
            public Task Consume(ConsumeContext<WeatherForecast> context)
            {
                return Task.CompletedTask;
            }
        }

        public record WeatherForecast(int temperature);

        [Test]
        public async Task ClearMessages_Should_Reset_Consumed_Published_Sent()
        {
            await using ServiceProvider provider = new ServiceCollection()
                .AddMassTransitTestHarness(x =>
                {
                    x.AddConsumer<SampleConsumer>();
                })
                .BuildServiceProvider(true);
            ITestHarness harness = provider.GetRequiredService<ITestHarness>();
            WeatherForecast weatherForecast = new(1);
            await harness.Start();

            await harness.Bus.Publish(weatherForecast);
            ISendEndpoint sendEndpoint = await harness.GetConsumerEndpoint<SampleConsumer>();
            await sendEndpoint.Send(weatherForecast);
            Assert.IsTrue(await harness.Sent.Any<WeatherForecast>());
            Assert.IsTrue(await harness.Published.Any<WeatherForecast>());
            Assert.IsTrue(await harness.Consumed.Any<WeatherForecast>());

            ((ContainerTestHarness)harness).ClearMessages();

            Assert.False(await harness.Sent.Any<WeatherForecast>());
            Assert.False(await harness.Published.Any<WeatherForecast>());
            Assert.False(await harness.Consumed.Any<WeatherForecast>());
        }
    }
}
