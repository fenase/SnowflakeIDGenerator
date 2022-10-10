﻿using System;
using System.Threading;

[assembly: CLSCompliant(true)]
namespace SnowflakeID
{
    public class SnowflakeIDGenerator
    {
        private readonly ulong MACHINE_ID;

        private static ulong _Sequence;
        private static ulong Sequence { get => _Sequence; set => _Sequence = value % Snowflake.MaxSequence; }

        private static readonly object lockObject = new();
        private static readonly DateTime epoch = new DateTime(1970, 1, 1);

        private static ulong UltimoTimestamp { get; set; }




        /// <summary>
        /// Creates a SnowflakeIDGenerator for a given machine number
        /// </summary>
        /// <param name="machineId">Machine number</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="machineId"/> must be less than Snowflake.MaxMachineId</exception>
        [CLSCompliant(false)]
        public SnowflakeIDGenerator(ulong machineId)
        {
            if (machineId >= Snowflake.MaxMachineId)
            {
                throw new ArgumentOutOfRangeException(nameof(machineId), $"{nameof(machineId)} must be less than {Snowflake.MaxMachineId}. Got: {machineId}.");
            }
            MACHINE_ID = machineId;
        }

        /// <summary>
        /// Creates a SnowflakeIDGenerator for a given machine number
        /// </summary>
        /// <param name="machineId"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="machineId"/> must be less than Snowflake.MaxMachineId</exception>
        public SnowflakeIDGenerator(int machineId) : this((ulong)machineId)
        { }

        /// <summary>
        /// Gets next Snowflake id
        /// </summary>
        /// <returns><typeparamref cref="Snowflake">Snowflake</typeparamref></returns>
        /// <typeparam cref="Snowflake"/>
        public Snowflake GetSnowflake()
        {
            lock (lockObject)
            {
                ulong timestampActualMillis = ((ulong)DateTime.UtcNow.Subtract(epoch).Ticks) / ((ulong)TimeSpan.TicksPerMillisecond);

                if (Sequence == 0 && timestampActualMillis == UltimoTimestamp)
                {
                    do
                    {
                        Thread.Sleep(1);
                        timestampActualMillis = ((ulong)DateTime.UtcNow.Subtract(epoch).Ticks) / ((ulong)TimeSpan.TicksPerMillisecond);
                    } while (timestampActualMillis == UltimoTimestamp);
                }
                else if (timestampActualMillis != UltimoTimestamp)
                {
                    Sequence = 0;
                }
                UltimoTimestamp = timestampActualMillis;

                Snowflake snowflake = new Snowflake
                {
                    Timestamp = timestampActualMillis,
                    MachineId = MACHINE_ID,
                    Sequence = Sequence,
                };

                Sequence++;

                return snowflake;
            }
        }

        /// <summary>
        /// Gets next Snowlflake as number (<typeparamref cref="ulong">ulong</typeparamref>)
        /// </summary>
        /// <returns></returns>
        /// <typeparam cref="ulong">ulong</typeparam>
        [CLSCompliant(false)]
        public ulong GetCode()
        {
            return GetSnowflake().Id;
        }

        /// <summary>
        /// Gets next Snowlflake as <typeparamref cref="string">string</typeparamref>
        /// </summary>
        /// <returns></returns>
        /// <typeparam cref="string">string</typeparam>
        public string GetCodeString()
        {
            return GetSnowflake().Code;
        }


        /// <summary>
        /// Static method
        /// Gets next Snowlflake as <typeparamref cref="ulong">ulong</typeparamref> for a given <typeparamref cref="ulong"><paramref name="machineId"/></typeparamref>
        /// </summary>
        /// <returns></returns>
        /// <param name="machineId">Machine number</param>
        /// <typeparam cref="ulong">ulong</typeparam>
        [CLSCompliant(false)]
        public static ulong GetCode(ulong machineId)
        {
            return new SnowflakeIDGenerator(machineId).GetCode();
        }

        /// <summary>
        /// Gets next Snowlflake as <typeparamref cref="string">string</typeparamref> for a given <typeparamref cref="ulong"><paramref name="machineId"/></typeparamref>
        /// </summary>
        /// <param name="machineId"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static string GetCodeString(ulong machineId)
        {
            return new SnowflakeIDGenerator(machineId).GetCodeString();
        }

        /// <summary>
        /// Gets next Snowlflake as <typeparamref cref="string">string</typeparamref> for a given <typeparamref cref="int"><paramref name="machineId"/></typeparamref>
        /// </summary>
        /// <param name="machineId"></param>
        /// <returns></returns>
        public static string GetCodeString(int machineId) => GetCodeString((ulong)machineId);
    }
}
