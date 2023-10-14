﻿// Copyright (c) 2022-2023, Federico Seckel.
// Licensed under the BSD 3-Clause License. See LICENSE file in the project root for full license information.

using System;

namespace SnowflakeID.Helpers
{
    internal static class DateTimeHelper
    {
        internal static ulong TimestampMillisFromEpoch(DateTime date, DateTime epoch)
        {
            return (ulong)date.Subtract(epoch).Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
