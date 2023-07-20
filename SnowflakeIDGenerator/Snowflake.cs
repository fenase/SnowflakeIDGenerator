﻿// Copyright (c) Federico Seckel.
// Licensed under the BSD 3-Clause License. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace SnowflakeID
{
    /// <summary>
    /// This class represents the Snowflake object.
    /// <seealso href="https://en.wikipedia.org/wiki/Snowflake_ID">Wikipedia article about SnowflakeId</seealso>
    /// </summary>
    public class Snowflake : IEquatable<Snowflake>, IComparable<Snowflake>, IComparable
    {
        #region constants
        /// <summary>
        /// The amount of codes generated per millisecond. Sequence value is set to 0 when this value is reached.
        /// </summary>
        public const long MaxSequence = 4096; // Set to 0 then this value is reached. seq % MaxSequence

        /// <summary>
        /// Max number of machines / servers allowed. Range from 0 to MaxMachineId-1
        /// </summary>
        public const long MaxMachineId = 1024; //amount. Range: [0..1024) = [0..1023]


        private const int BITS_SHIFT_DATETIMEMILLIS = 22;
        private const int BITS_SHIFT_MACHINE = 12;
        private const int BITS_SHIFT_SEQUENCE = 0;


        private const ulong MASK_DATETIMEMILLIS_RIGHT_ALIGNED
                                                = 0b0000_0000_0000_0000_0000_0011_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111;
        private const ulong MASK_ESTACION_RIGHT_ALIGNED
                                                = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0011_1111_1111;
        private const ulong MASK_SECUENCIA_RIGHT_ALIGNED
                                                = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111;


        private static readonly DateTime defaultEpoch = new(year: 1970, month: 1, day: 1, hour: 0, minute: 0, second: 0, kind: DateTimeKind.Utc);

        /// <summary>
        /// Current epoch being used
        /// </summary>
        public DateTime Epoch { get; private set; }

        /// <summary>
        /// Total number of digits for the generated code
        /// </summary>
        public static readonly int NumberOfDigits = ulong.MaxValue.ToString(CultureInfo.InvariantCulture).Length;
        #endregion

        /// <summary>
        /// Class constructor using default epoch (UNIX time 1-1-1970)
        /// </summary>
        public Snowflake() : this(defaultEpoch) { }

        /// <summary>
        /// Class constructor using a custom date as epoch.
        /// </summary>
        /// <param name="epoch">Date to use as epoch</param>
        public Snowflake(DateTime epoch)
        {
            this.Epoch = epoch;
        }

        /// <summary>
        /// Sets the timeStamp portion of the snowflake based on current time and selected epoch. Gets real time of the snowflake based on selected epoch.
        /// </summary>
        public DateTime UtcDateTime { get; set; }

        /// <summary>
        /// Gets / Sets machine / server number
        /// </summary>
        [CLSCompliant(false)]
        public ulong MachineId
        {
            get => _MachineId;
            set
            {
                if (value >= MaxMachineId)
                {
                    throw new ArgumentOutOfRangeException(nameof(MachineId), $"{nameof(MachineId)} must be less than {MaxMachineId}. Got: {value}.");
                }
                else
                {
                    _MachineId = value;
                }
            }
        }

        /// <summary>
        /// Gets / Sets machine / server number
        /// </summary>
        public int MachineIdInt32
        {
            get => (int)MachineId;
            set
            {
                if (value < 0) { throw new ArgumentOutOfRangeException(nameof(MachineIdInt32)); }
                MachineId = (ulong)value;
            }
        }
        private ulong _MachineId;

        /// <summary>
        /// Gets / Sets sequence
        /// </summary>
        [CLSCompliant(false)]
        public ulong Sequence
        {
            get => _Sequence;
            set
            {
                if (value >= MaxSequence)
                {
                    throw new ArgumentOutOfRangeException(nameof(Sequence), $"{nameof(Sequence)} must be less than {MaxSequence}. Got: {value}.");
                }
                else
                {
                    _Sequence = value;
                }
            }
        }

        /// <summary>
        /// Gets / Sets machine / server number
        /// </summary>
        public int SequenceInt32
        {
            get => (int)Sequence;
            set
            {
                if (value < 0) { throw new ArgumentOutOfRangeException(nameof(SequenceInt32)); }
                Sequence = (ulong)value;
            }
        }
        private ulong _Sequence;

        /// <summary>
        /// Gets / Sets timeStamp as number of milliseconds since selected epoch
        /// </summary>
        [CLSCompliant(false)]
        public ulong Timestamp
        {
            get
            {
                return ((ulong)UtcDateTime.Subtract(Epoch).Ticks) / ((ulong)TimeSpan.TicksPerMillisecond);
            }
            set
            {
                UtcDateTime = DateTime.SpecifyKind(Epoch.AddTicks((long)value * (TimeSpan.TicksPerMillisecond)), DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Gets / Sets timeStamp as number of milliseconds since selected epoch
        /// </summary>
        public long TimestampInt64
        {
            get => (long)Timestamp;
            set
            {
                if (value < 0) { throw new ArgumentOutOfRangeException(nameof(TimestampInt64)); }
                ulong newVal = (ulong)value & MASK_DATETIMEMILLIS_RIGHT_ALIGNED;
                if (newVal != (ulong)value) { throw new ArgumentOutOfRangeException(nameof(TimestampInt64)); }
                Timestamp = newVal;
            }
        }

        /// <summary>
        /// Gets snowflakeId.
        /// </summary>
        [CLSCompliant(false)]
        public virtual ulong Id
        {
            get
            {
                ulong timestamp = ((ulong)UtcDateTime.Subtract(Epoch).Ticks) / ((ulong)TimeSpan.TicksPerMillisecond);
                return ((timestamp & MASK_DATETIMEMILLIS_RIGHT_ALIGNED) << BITS_SHIFT_DATETIMEMILLIS)
                                        | ((MachineId & MASK_ESTACION_RIGHT_ALIGNED) << BITS_SHIFT_MACHINE)
                                        | ((Sequence & MASK_SECUENCIA_RIGHT_ALIGNED) << BITS_SHIFT_SEQUENCE);
            }
            private set
            {
                ulong val = value >> BITS_SHIFT_SEQUENCE;
                Sequence = val & MASK_SECUENCIA_RIGHT_ALIGNED;
                val >>= BITS_SHIFT_MACHINE - BITS_SHIFT_SEQUENCE;
                MachineId = val & MASK_ESTACION_RIGHT_ALIGNED;
                val >>= BITS_SHIFT_DATETIMEMILLIS - BITS_SHIFT_MACHINE - BITS_SHIFT_SEQUENCE;
                Timestamp = val & MASK_DATETIMEMILLIS_RIGHT_ALIGNED;
            }
        }

        /// <summary>
        /// Gets snowflakeId from string
        /// </summary>
        public string Code
        {
            get
            {
                return Id.ToString(CultureInfo.InvariantCulture).PadLeft(NumberOfDigits, '0');
            }
            private set
            {
                Id = ulong.Parse(value, CultureInfo.InvariantCulture);
            }
        }


        public void RebaseEpoch(DateTime newEpoch)
        {
            Epoch = newEpoch;
        }


        public void ChangeEpoch(DateTime newEpoch)
        {
            UtcDateTime += newEpoch - Epoch;
            Epoch = newEpoch;
        }



        /// <summary>
        /// Creates a SnowflakeId object from a SnowflakeId code
        /// </summary>
        /// <param name="s">Code</param>
        /// <returns></returns>
        public static Snowflake Parse(string s)
        {
            return new Snowflake() { Code = s };
        }

        /// <summary>
        /// Creates a SnowflakeId object from a SnowflakeId code using a custom epoch
        /// </summary>
        /// <param name="s">Code</param>
        /// <param name="customEpoch">Date to use as epoch</param>
        /// <returns></returns>
        public static Snowflake Parse(string s, DateTime customEpoch)
        {
            return new Snowflake(customEpoch) { Code = s };
        }

        /// <summary>
        /// Creates a SnowflakeId object from a SnowflakeId code
        /// </summary>
        /// <param name="b">Code</param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static Snowflake Parse(ulong b)
        {
            return new Snowflake() { Id = b };
        }

        /// <summary>
        /// Creates a SnowflakeId object from a SnowflakeId code using a custom epoch
        /// </summary>
        /// <param name="b">Code</param>
        /// <param name="customEpoch">Date to use as epoch</param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static Snowflake Parse(ulong b, DateTime customEpoch)
        {
            return new Snowflake(customEpoch) { Id = b };
        }

        /// <summary>
        /// Gets code as string
        /// </summary>
        /// <returns>SnowflakeId code</returns>
        public override string ToString()
        {
            return Code;
        }

        /// <summary>
        /// Checks equality between two SnowflakeId objects
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is not Snowflake other) { return false; }
            return Equals(other);
        }

        /// <summary>
        /// Checks equality between two SnowflakeId objects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool Equals(Snowflake other)
        {
            return other != null && Id == other.Id;
        }

        /// <summary>
        /// Serves as the default hash function. Override of <seealso cref="object.GetHashCode()"/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)(Id >> 32);
        }

        /// <summary>
        /// Implements of <seealso cref="IComparable.CompareTo(object)"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj is not Snowflake other) { return 1; }
            return CompareTo(other);
        }

        /// <summary>
        /// Implements of <seealso cref="IComparable{SnowflakeID}.CompareTo(SnowflakeID)"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Snowflake other)
        {
            if (other == null) { return 1; }
            return Id.CompareTo(other.Id);
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool operator ==(Snowflake s1, Snowflake s2)
        {
            if (s1 is null)
            {
                if (s2 is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return s1.Equals(s2);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool operator !=(Snowflake s1, Snowflake s2) => !(s1 == s2);

        /// <summary>
        /// Greater than operator
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool operator >(Snowflake s1, Snowflake s2) => (!(s1 is null ^ s2 is null)) && s1 != s2 && s1.CompareTo(s2) > 0;

        /// <summary>
        /// Less than operator
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool operator <(Snowflake s1, Snowflake s2) => (!(s1 is null ^ s2 is null)) && s1 != s2 && s1.CompareTo(s2) < 0;

        /// <summary>
        /// Greater than or equal to operator
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool operator >=(Snowflake s1, Snowflake s2) => s1 == s2 || s1 > s2;

        /// <summary>
        /// Less than or equal to operator
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool operator <=(Snowflake s1, Snowflake s2) => s1 == s2 || s1 < s2;
    }
}
