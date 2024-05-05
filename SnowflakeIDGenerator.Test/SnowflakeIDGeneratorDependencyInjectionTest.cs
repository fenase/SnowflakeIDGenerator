﻿#if NET6_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using SnowflakeID.DependencyInjection;
using System.Globalization;

namespace SnowflakeID.Test
{
    public class SnowflakeIDGeneratorDependencyInjectionTest
    {
        [SetUp]
        public void Setup()
        {
            // Method intentionally left empty.
        }

        [Test]
        public void ServiceAddedTest()
        {
            ServiceCollection services = new();

            //before
            Assert.That(services, Is.Empty);

            services.AddSnowflakeIdGeneratorService();

            //after
            Assert.Multiple(() =>
            {
                Assert.That(services, Is.Not.Empty);
                Assert.That(services, Has.Count.GreaterThanOrEqualTo(1));
                Assert.That(services.Any(x => x.ServiceType.Name == nameof(ISnowflakeIDGenerator)), Is.True);
                Assert.That(services.Any(x => x.ImplementationType?.Name == nameof(SnowflakeIDGenerator)), Is.True);
            });
        }

        [Test]
        public void ServiceDefaultParametersTest()
        {
            ServiceCollection services = new();

            services.AddSnowflakeIdGeneratorService();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            Assert.That(serviceProvider, Is.Not.Null);

            var generator = serviceProvider!.GetService<ISnowflakeIDGenerator>();
            Assert.That(generator, Is.Not.Null);

            Snowflake snowflake = generator.GetSnowflake();
            Assert.Multiple(() =>
            {
                Assert.That(snowflake.MachineId, Is.EqualTo(1));
                Assert.That(snowflake.Epoch, Is.EqualTo(DateTime.UnixEpoch));
            });
        }

        [TestCase(4ul, "1970-01-01 00:00:00")]
        [TestCase(0ul, "1970-01-01 00:00:00")]
        [TestCase(10ul, "2000-01-01 00:00:00")]
        [TestCase(6ul, "1970-01-01 00:00:00")]
        [TestCase(1ul, "1970-01-01 00:00:00")]
        [TestCase(1ul, "2020-04-19 00:25:00")]
        public void ServiceCustomParametersTest(ulong machineId, string epochString)
        {
            DateTime epoch = DateTime.Parse(epochString, CultureInfo.InvariantCulture);
            ServiceCollection services = new();

            services.AddSnowflakeIdGeneratorService(machineId, epoch);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            Assert.That(serviceProvider, Is.Not.Null);

            ISnowflakeIDGenerator? generator = serviceProvider!.GetService<ISnowflakeIDGenerator>();
            Assert.That(generator, Is.Not.Null);

            Snowflake snowflake = generator.GetSnowflake();
            Assert.Multiple(() =>
            {
                Assert.That(snowflake.MachineId, Is.EqualTo(machineId));
                Assert.That(snowflake.Epoch, Is.EqualTo(epoch));
            });
        }
    }
}
#endif