using SnowflakeID.Helpers;

namespace SnowflakeID.Test
{
    internal sealed class DateTimeHelperTest
    {
        [Test]
        public void TimestampFromEpochTest()
        {
            DateTime d = DateTime.UtcNow;
            ulong oldWay = ((ulong)d.Subtract(UnixEpoch).Ticks) / TimeSpan.TicksPerMillisecond;
            ulong newWay = DateTimeHelper.TimestampMillisFromEpoch(d, UnixEpoch);
            Assert.That(newWay, Is.EqualTo(oldWay));
        }

        [Test]
        public void DriftTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(DateTimeHelper.TimestampMillisFromEpoch(UnixEpoch, UnixEpoch), Is.Zero);
                Assert.That(DateTimeHelper.TimestampMillisFromEpoch(CustomEpoch, UnixEpoch), Is.EqualTo(1577836800000));
            });
        }

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [Test]
        public void UnixEpochEquivalentTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(UnixEpoch, Is.EqualTo(UnixEpochAsUnixEpoch));
                Assert.That(UnixEpoch.Year, Is.EqualTo(UnixEpochAsUnixEpoch.Year));
                Assert.That(UnixEpoch.Month, Is.EqualTo(UnixEpochAsUnixEpoch.Month));
                Assert.That(UnixEpoch.Day, Is.EqualTo(UnixEpochAsUnixEpoch.Day));
                Assert.That(UnixEpoch.Hour, Is.EqualTo(UnixEpochAsUnixEpoch.Hour));
                Assert.That(UnixEpoch.Minute, Is.EqualTo(UnixEpochAsUnixEpoch.Minute));
                Assert.That(UnixEpoch.Second, Is.EqualTo(UnixEpochAsUnixEpoch.Second));
                Assert.That(UnixEpoch.Millisecond, Is.EqualTo(UnixEpochAsUnixEpoch.Millisecond));
                Assert.That(UnixEpoch.Ticks, Is.EqualTo(UnixEpochAsUnixEpoch.Ticks));
            });
        }
#endif

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        private static readonly DateTime UnixEpochAsUnixEpoch = DateTime.UnixEpoch;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S6588:Use the \"UnixEpoch\" field instead of creating \"DateTime\" instances that point to the beginning of the Unix epoch",
                                                        Justification = "Using this since this is compatible with every version of the .net framework. A test case is created to assert that equality.")]
#endif
        private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime CustomEpoch = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
