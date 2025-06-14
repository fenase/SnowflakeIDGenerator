using SnowflakeID.Exceptions;

namespace SnowflakeID.Test
{
    internal sealed class SnowflakeIDGeneratorTest
    {
        [SetUp]
        public void Setup()
        {
            // Method intentionally left empty.
        }

        [TestCase(0ul)]
        [TestCase(1ul)]
        [TestCase(2ul)]
        [TestCase(3ul)]
        [TestCase(33ul)]
        [TestCase(1021ul)]
        [TestCase(1023ul)]
        public void SequenceTest(ulong machineId)
        {
            Queue<string> generated = new();
            const int cant = 100000;
            DateTime d1 = DateTimeUtcMillis();
            for (int i = 0; i < cant; i++)
            {
                generated.Enqueue(SnowflakeIDGenerator.GetCodeString(machineId));
            }
            DateTime d2 = DateTimeUtcMillis();
            Snowflake previous = Snowflake.Parse(generated.Dequeue());
            Assert.Multiple(() =>
            {
                Assert.That(previous.MachineId, Is.EqualTo(machineId));
                Assert.That(d1, Is.LessThanOrEqualTo(previous.UtcDateTime));
                Assert.That(d2, Is.GreaterThanOrEqualTo(previous.UtcDateTime));
            });

            Snowflake? prevSnowflake = null;
            while (generated.Count > 0)
            {
                Snowflake current = Snowflake.Parse(generated.Dequeue());
                Assert.Multiple(() =>
                {
                    Assert.That(current.MachineId, Is.EqualTo(machineId));
                    Assert.That(previous, Is.LessThan(current));
                    Assert.That(d1, Is.LessThanOrEqualTo(current.UtcDateTime));
                    Assert.That(d2, Is.GreaterThanOrEqualTo(current.UtcDateTime));

                    if (prevSnowflake != null)
                    {
                        Assert.That(current, Is.GreaterThanOrEqualTo(prevSnowflake));
                        Assert.That(current, Is.GreaterThan(prevSnowflake));
#pragma warning disable NUnit2043 // Use ComparisonConstraint for better assertion messages in case of failure. Justification: Specifically testing the operators.
                        Assert.That(current >= prevSnowflake, Is.True); // testing operators
                        Assert.That(current > prevSnowflake, Is.True); // testing operators
#pragma warning restore NUnit2043 // Use ComparisonConstraint for better assertion messages in case of failure.
                    }
#pragma warning disable NUnit2043 // Use ComparisonConstraint for better assertion messages in case of failure. Justification: Specifically testing the operators.
                    Assert.That(current <= prevSnowflake, Is.False); // testing operators
                    Assert.That(current < prevSnowflake, Is.False); // testing operators
#pragma warning restore NUnit2043 // Use ComparisonConstraint for better assertion messages in case of failure.
                });

                prevSnowflake = current;
            }
        }

        [Test]
        public void ParallelTest()
        {
            List<Task<HashSet<Snowflake>>> tasks = [];
            const int TaskQuantity = 50;
            const int GeneratedQuantity = 10000;
            const ulong machineId = 1ul;

            DateTime d1;
            DateTime d2;

            try
            {
                d1 = DateTimeUtcMillis();
                for (int i = 0; i < TaskQuantity; i++)
                {
                    tasks.Add(new Task<HashSet<Snowflake>>(() =>
                    {
                        HashSet<Snowflake> list = [];
                        for (int j = 0; j < GeneratedQuantity; j++)
                        {
                            _ = list.Add((Snowflake)SnowflakeIDGenerator.GetCodeString(machineId));
                        }
                        return list;
                    }));
                }
                foreach (Task task in tasks)
                {
                    task.Start();
                }

                Task.WaitAll([.. tasks]);
                d2 = DateTimeUtcMillis();
            }
            catch (AggregateException ae)
            {
                string err = string.Empty;
                foreach (Exception e in ae.InnerExceptions)
                {
                    string errL = $"Exception {e.GetType()} from {e.Source}.";
                    Console.WriteLine(errL);
                    err += " -|- " + errL;
                }
                Assert.Fail(err);
                throw;
            }

            IEnumerable<Snowflake> combined = tasks.SelectMany(t => t.Result);

            Assert.Multiple(() =>
            {
                Assert.That(combined.Count(), Is.EqualTo(TaskQuantity * GeneratedQuantity));
                Assert.That(combined.Distinct().Count(), Is.EqualTo(combined.Count()));
            });
            foreach (Snowflake snowflake in combined)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(d1, Is.LessThanOrEqualTo(snowflake.UtcDateTime));
                    Assert.That(d2, Is.GreaterThanOrEqualTo(snowflake.UtcDateTime));
                    Assert.That(snowflake.MachineId, Is.EqualTo(machineId));
                });
            }
        }

        [TestCase(1024UL)]
        [TestCase(1025UL)]
        [TestCase(2025UL)]
        [TestCase(20025UL)]
        public void MachineErrorTest(ulong machineId)
        {
            Assert.Multiple(() =>
            {
                _ = Assert.Throws<ArgumentOutOfRangeException>(() => SnowflakeIDGenerator.GetCode(machineId));

                _ = Assert.Throws<ArgumentOutOfRangeException>(() => SnowflakeIDGenerator.GetCodeString(machineId));

                _ = Assert.Throws<ArgumentOutOfRangeException>(() => new SnowflakeIDGenerator(machineId));

                _ = Assert.Throws<ArgumentOutOfRangeException>(() => new SnowflakeIDGenerator((int)machineId));
            });
        }

        [Test]
        public void DefaultEpochTest()
        {
            ulong code = SnowflakeIDGenerator.GetCode(123);
            ulong codeDefaultEpochSetted = SnowflakeIDGenerator.GetCode(123, UnixEpoch);

            Snowflake snowflake = Snowflake.Parse(code);
            Snowflake snowflakeDefaultEpochSetted = Snowflake.Parse(codeDefaultEpochSetted, UnixEpoch);

            Assert.Multiple(() =>
            {
                Assert.That(snowflake, Is.LessThan(snowflakeDefaultEpochSetted));
                Assert.That(snowflake.Epoch, Is.EqualTo(snowflakeDefaultEpochSetted.Epoch));
                Assert.That(snowflake.Timestamp, Is.LessThanOrEqualTo(snowflakeDefaultEpochSetted.Timestamp));
            });
        }

        [Test]
        public void CustomTestFromString()
        {
            ulong code = SnowflakeIDGenerator.GetCode(123);
            ulong codeDefaultEpochSetted = SnowflakeIDGenerator.GetCode(123, CustomEpoch);

            Snowflake snowflake = Snowflake.Parse(code);
            Snowflake snowflakeDefaultEpochSetted = Snowflake.Parse(codeDefaultEpochSetted, CustomEpoch);

            Assert.Multiple(() =>
            {
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflake > snowflakeDefaultEpochSetted);
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflake >= snowflakeDefaultEpochSetted);
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflake < snowflakeDefaultEpochSetted);
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflake <= snowflakeDefaultEpochSetted);
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflake.CompareTo(snowflakeDefaultEpochSetted));
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflakeDefaultEpochSetted.CompareTo(snowflake));
                Assert.That(snowflake.Id, Is.GreaterThan(snowflakeDefaultEpochSetted.Id));
                Assert.That(snowflake.Epoch, Is.Not.EqualTo(snowflakeDefaultEpochSetted.Epoch));
                Assert.That(snowflake.Epoch, Is.EqualTo(UnixEpoch));
                Assert.That(snowflakeDefaultEpochSetted.Epoch, Is.EqualTo(CustomEpoch));
                Assert.That(snowflakeDefaultEpochSetted.Timestamp, Is.LessThan(snowflake.Timestamp));
                Assert.That(snowflakeDefaultEpochSetted.UtcDateTime, Is.EqualTo(snowflake.UtcDateTime).Within(1).Seconds);
            });
        }

        [Test]
        public void CustomTest()
        {
            Snowflake snowflake = SnowflakeIDGenerator.GetSnowflake(123);
            Snowflake snowflakeDefaultEpochSetted = SnowflakeIDGenerator.GetSnowflake(123, CustomEpoch);

            Assert.Multiple(() =>
            {
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflake > snowflakeDefaultEpochSetted);
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflake >= snowflakeDefaultEpochSetted);
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflake < snowflakeDefaultEpochSetted);
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflake <= snowflakeDefaultEpochSetted);
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflake.CompareTo(snowflakeDefaultEpochSetted));
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflakeDefaultEpochSetted.CompareTo(snowflake));
                Assert.That(snowflake.Id, Is.GreaterThan(snowflakeDefaultEpochSetted.Id));
                Assert.That(snowflake.Epoch, Is.Not.EqualTo(snowflakeDefaultEpochSetted.Epoch));
                Assert.That(snowflake.Epoch, Is.EqualTo(UnixEpoch));
                Assert.That(snowflakeDefaultEpochSetted.Epoch, Is.EqualTo(CustomEpoch));
                Assert.That(snowflakeDefaultEpochSetted.Timestamp, Is.LessThan(snowflake.Timestamp));
                Assert.That(snowflakeDefaultEpochSetted.UtcDateTime, Is.EqualTo(snowflake.UtcDateTime).Within(1).Seconds);
            });
        }

        [Test]
        public void DefaultEpochStringTest()
        {
            string codeString = SnowflakeIDGenerator.GetCodeString(123);
            string codeStringDefaultEpochSetted = SnowflakeIDGenerator.GetCodeString(123, UnixEpoch);

            Snowflake snowflakeString = Snowflake.Parse(codeString);
            Snowflake snowflakeStringDefaultEpochSetted = Snowflake.Parse(codeStringDefaultEpochSetted, UnixEpoch);

            Assert.Multiple(() =>
            {
                Assert.That(snowflakeString, Is.LessThan(snowflakeStringDefaultEpochSetted));
                Assert.That(snowflakeString.Epoch, Is.EqualTo(snowflakeStringDefaultEpochSetted.Epoch));
                Assert.That(snowflakeString.Timestamp, Is.LessThanOrEqualTo(snowflakeStringDefaultEpochSetted.Timestamp));
            });
        }

        [Test]
        public void CustomStringTest()
        {
            string codeString = SnowflakeIDGenerator.GetCodeString(123);
            string codeStringDefaultEpochSetted = SnowflakeIDGenerator.GetCodeString(123, CustomEpoch);

            Snowflake snowflakeString = Snowflake.Parse(codeString);
            Snowflake snowflakeStringDefaultEpochSetted = Snowflake.Parse(codeStringDefaultEpochSetted, CustomEpoch);

            Assert.Multiple(() =>
            {
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflakeString > snowflakeStringDefaultEpochSetted);
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflakeString >= snowflakeStringDefaultEpochSetted);
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflakeString < snowflakeStringDefaultEpochSetted);
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflakeString <= snowflakeStringDefaultEpochSetted);
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflakeString.CompareTo(snowflakeStringDefaultEpochSetted));
                _ = Assert.Throws<SnowflakesUsingDifferentEpochsException>(() => _ = snowflakeStringDefaultEpochSetted.CompareTo(snowflakeString));
                Assert.That(snowflakeString.Id, Is.GreaterThan(snowflakeStringDefaultEpochSetted.Id));
                Assert.That(snowflakeString.Epoch, Is.Not.EqualTo(snowflakeStringDefaultEpochSetted.Epoch));
                Assert.That(snowflakeString.Epoch, Is.EqualTo(UnixEpoch));
                Assert.That(snowflakeStringDefaultEpochSetted.Epoch, Is.EqualTo(CustomEpoch));
                Assert.That(snowflakeStringDefaultEpochSetted.Timestamp, Is.LessThan(snowflakeString.Timestamp));
            });
        }

        [Test]
        public void DateTimeMillisecondsTest()
        {
            // This tests aux function bellow. It truncates time to a millisecond
            DateTime d1 = DateTime.UtcNow;
            DateTime d2 = DateTimeOnlyMillis(d1);
            Assert.Multiple(() =>
            {
                Assert.That(d1.Kind, Is.EqualTo(DateTimeKind.Utc));
                Assert.That(d2.Kind, Is.EqualTo(DateTimeKind.Utc));
                Assert.That(d2.Year, Is.EqualTo(d1.Year));
                Assert.That(d2.Month, Is.EqualTo(d1.Month));
                Assert.That(d2.Day, Is.EqualTo(d1.Day));
                Assert.That(d2.Hour, Is.EqualTo(d1.Hour));
                Assert.That(d2.Minute, Is.EqualTo(d1.Minute));
                Assert.That(d2.Second, Is.EqualTo(d1.Second));
                Assert.That(d2.Millisecond, Is.EqualTo(d1.Millisecond));
            });
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Cryptographically secure randomness not needed")]
        public void SnowflakeAsStaticTest([Values] bool useCustomEpoch)
        {
            DateTime epoch = useCustomEpoch ? CustomEpoch : UnixEpoch;
            ulong machine = (ulong)new Random().Next(0, (int)Snowflake.MaxMachineId);

            SnowflakeIDGenerator generator = new(machine, epoch);

            Snowflake sStatic = SnowflakeIDGenerator.GetSnowflake(machine, epoch);
            Snowflake sGenerator = generator.GetSnowflake();

            Assert.Multiple(() =>
            {
                Assert.That(sStatic.Epoch, Is.EqualTo(epoch));
                Assert.That(sGenerator.Epoch, Is.EqualTo(epoch));

                Assert.That(sStatic.UtcDateTime, Is.EqualTo(sGenerator.UtcDateTime).Within(1).Seconds);

                Assert.That(sStatic.MachineId, Is.EqualTo(machine));
                Assert.That(sGenerator.MachineId, Is.EqualTo(machine));

                if (sStatic.Timestamp == sGenerator.Timestamp)
                {
                    Assert.That(sStatic.Sequence, Is.EqualTo(sGenerator.Sequence - 1));
                }
                else
                {
                    Assert.That(sStatic.Sequence, Is.Zero);
                }
            });
        }

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        private static readonly DateTime UnixEpoch = DateTime.UnixEpoch;
#else
        // This should be DateTime.UnixEpoch. However, that constant is only available in .netCore and net5 or newer
        private static readonly DateTime UnixEpoch = new(year: 1970, month: 1, day: 1, hour: 0, minute: 0, second: 0, kind: DateTimeKind.Utc);
#endif
        private static readonly DateTime CustomEpoch = new(year: 2020, month: 1, day: 1, hour: 0, minute: 0, second: 0, DateTimeKind.Utc);
        private static DateTime DateTimeUtcMillis() => DateTimeOnlyMillis(DateTime.UtcNow);
        private static DateTime DateTimeOnlyMillis(DateTime d)
        {
            ulong ml = ((ulong)d.Subtract(UnixEpoch).Ticks) / TimeSpan.TicksPerMillisecond; // same as floor (integer division -> multiplication)
            return DateTime.SpecifyKind(UnixEpoch.AddTicks((long)ml * TimeSpan.TicksPerMillisecond), DateTimeKind.Utc);
        }
    }
}
