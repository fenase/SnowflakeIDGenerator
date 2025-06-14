#if NET6_0_OR_GREATER || NET48_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace SnowflakeID.Test
{
    internal sealed class SnowflakeIDGeneratorDependencyInjectionTest
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        private static readonly DateTime UnixEpoch = DateTime.UnixEpoch;
#else
        // This should be DateTime.UnixEpoch. However, that constant is only available in .netCore and net5 or newer
        private static readonly DateTime UnixEpoch = new(year: 1970, month: 1, day: 1, hour: 0, minute: 0, second: 0, kind: DateTimeKind.Utc);
#endif

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

            _ = services.AddSnowflakeIdGeneratorService();

            //after
            Assert.Multiple(() =>
            {
                Assert.That(services, Is.Not.Empty);
                Assert.That(services, Has.Count.GreaterThanOrEqualTo(1));
                Assert.That(services.Any(x => x.ServiceType == typeof(ISnowflakeIDGenerator)), Is.True);
            });
        }

        [Test]
        public void ServiceDefaultParametersTest()
        {
            ServiceCollection services = new();

            _ = services.AddSnowflakeIdGeneratorService();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            Assert.That(serviceProvider, Is.Not.Null);

            ISnowflakeIDGenerator? generator = serviceProvider!.GetService<ISnowflakeIDGenerator>();
            Assert.Multiple(() =>
            {
                Assert.That(generator, Is.Not.Null);
                Assert.That(generator!.ConfiguredEpoch, Is.EqualTo(UnixEpoch));
                Assert.That(generator!.ConfiguredMachineId, Is.Zero);
            });

            Snowflake snowflake = generator!.GetSnowflake();
            Assert.Multiple(() =>
            {
                Assert.That(snowflake.MachineId, Is.Zero);
                Assert.That(snowflake.Epoch, Is.EqualTo(UnixEpoch));
            });
        }

        [TestCase(4ul, "1970-01-01 00:00:00")]
        [TestCase(0ul, "1970-01-01 00:00:00")]
        [TestCase(10ul, "2000-01-01 00:00:00")]
        [TestCase(6ul, "1970-01-01 00:00:00")]
        [TestCase(1ul, "1970-01-01 00:00:00")]
        [TestCase(1ul, "2020-04-19 00:25:00")]
        [TestCase(4ul, "1970-01-01")]
        [TestCase(0ul, "1970-01-01")]
        [TestCase(10ul, "2000-01-01")]
        [TestCase(6ul, "1970-01-01")]
        [TestCase(1ul, "1970-01-01")]
        [TestCase(1ul, "2020-04-19")]
        public void ServiceCustomParametersTest(ulong machineId, string epochString)
        {
            DateTime epoch = DateTime.Parse(epochString, CultureInfo.InvariantCulture);
            ServiceCollection services = new();

            _ = services.AddSnowflakeIdGeneratorService(machineId, epoch);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            Assert.That(serviceProvider, Is.Not.Null);

            ISnowflakeIDGenerator? generator = serviceProvider!.GetService<ISnowflakeIDGenerator>();
            Assert.Multiple(() =>
            {
                Assert.That(generator, Is.Not.Null);
                Assert.That(generator!.ConfiguredEpoch, Is.EqualTo(epoch));
                Assert.That(generator!.ConfiguredMachineId, Is.EqualTo(machineId));
            });

            Snowflake snowflake = generator!.GetSnowflake();
            Assert.Multiple(() =>
            {
                Assert.That(snowflake.MachineId, Is.EqualTo(machineId));
                Assert.That(snowflake.Epoch, Is.EqualTo(epoch));
            });
        }
    }
}
#endif
