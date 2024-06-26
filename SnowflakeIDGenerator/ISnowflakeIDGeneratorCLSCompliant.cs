﻿// Copyright (c) 2022-2024, Federico Seckel.
// Licensed under the BSD 3-Clause License. See LICENSE file in the project root for full license information.

// Ignore Spelling: Cls

using System;

namespace SnowflakeID
{
    /// <summary>
    /// Generator class for <see cref="SnowflakeID"/>.
    /// <para>This keeps track of time, machine number and sequence.</para>
    /// </summary>
    public interface ISnowflakeIDGeneratorClsCompliant
    {
        /// <summary>
        /// Date configured as epoch for the generator
        /// </summary>
        DateTime ConfiguredEpoch { get; }

        /// <summary>
        /// Configured instance id for the generator
        /// </summary>
        int ConfiguredMachineId { get; }

        /// <summary>
        /// Gets next Snowflake as <typeparamref cref="string">string</typeparamref>
        /// </summary>
        /// <returns></returns>
        /// <typeparam cref="string">string</typeparam>
        string GetCodeString();

        /// <summary>
        /// Gets next Snowflake id
        /// </summary>
        /// <returns><typeparamref cref="Snowflake">Snowflake</typeparamref></returns>
        Snowflake GetSnowflake();
    }
}
