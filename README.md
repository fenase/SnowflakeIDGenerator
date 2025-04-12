# Snowflake Id Generator

Generate unique identifiers based on Twitter's [Snowflake ID](https://en.wikipedia.org/wiki/Snowflake_ID).
Parse a Snowflake to get information about it's creation.

| [More info](https://fenase.github.io/projects/SnowflakeIDGenerator) | [API](https://fenase.github.io/SnowflakeIDGenerator/api/SnowflakeID.html) |
|---|---|


[![Nuget](https://img.shields.io/nuget/v/SnowflakeIDGenerator?logo=nuget)](https://www.nuget.org/packages/SnowflakeIDGenerator)
[![Build Status](https://dev.azure.com/fenase/SnowflakeIDGenerator/_apis/build/status%2FSnowflakeIDGenerator-CI?branchName=master)](https://dev.azure.com/fenase/SnowflakeIDGenerator/_build/latest?definitionId=21)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=fenase_SnowflakeIDGenerator2&metric=alert_status)](https://sonarcloud.io/summary/overall?id=fenase_SnowflakeIDGenerator2)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=fenase_SnowflakeIDGenerator2&metric=ncloc)](https://sonarcloud.io/summary/overall?id=fenase_SnowflakeIDGenerator2)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=fenase_SnowflakeIDGenerator2&metric=coverage)](https://sonarcloud.io/summary/overall?id=fenase_SnowflakeIDGenerator2)


- [Snowflakes](#snowflakes)
- [Usage](#usage)
  - [Generate](#generate)
    - [Using the `SnowflakeIDGenerator` class](#using-the-snowflakeidgenerator-class)
    - [Using the `SnowflakeIDGenerator` class as static](#using-the-snowflakeidgenerator-class-as-static)
    - [Using a non-standard date as epoch](#using-a-non-standard-date-as-epoch)
  - [Parsing an Id](#parsing-an-id)
  - [The Snowflake object](#the-snowflake-object)
  - [Changing Epoch on generated codes](#changing-epoch-on-generated-codes)


# Snowflakes

Snowflakes (or SnoflakeIds) are a form of generating identifiers used in distributed computing that, when used properly,
guarantees uniqueness between systems, since one of it's components refers to the system creating the ID.

The other components refer to the current date and time, in order not to keep track of a long running sequencer
(for example, a sequence in a database),
and a short (in memory) sequence, to allow the generation of several codes in a short amount of time.

| ![SnowflakeId components](https://raw.githubusercontent.com/fenase/SnowflakeIDGenerator/master/ReadmeImages/SnowflakeId.svg) |
|:--| 


# Usage

## Generate

There are 2 ways of using the generator:
* Using the `SnowflakeIDGenerator` class as a static class.
Useful when generating a single code to avoid dealing with constructors and the scope of the generator object.
* Instantiating the `SnowflakeIDGenerator` class. 
Recommended if you plan to generate more than a few codes at the same time.
  > On version 1.4.2024 and up you can use the interface `ISnowflakeIDGenerator`.
  > If you prefer to use dependency injection, the package [SnowflakeIDGenerator.DependencyInjection](https://www.nuget.org/packages/SnowflakeIDGenerator.DependencyInjection)
  > might be helpful.

### Using the `SnowflakeIDGenerator` class

1. Instantiate class `SnowflakeIDGenerator`
```c#
SnowflakeIDGenerator generator = new SnowflakeIDGenerator(machineId);
```
where `machineId` is the number / identifier of the system currently trying to get an id

Starting on version 1.1.2023 you can instruct the generator to use a custom date as epoch 
from which the timestamps are derived for the current date.
```c#
SnowflakeIDGenerator generator = new SnowflakeIDGenerator(machineId, CustomEpoch);
```

2. Using the generator object, there are 3 ways of obtaining the code:
   1. Call `generator.GetSnowflake()` to get a `Snowflake` object
   2. Call `generator.GetCode()` to get an Id in number (ulong) format
   3. Call `generator.GetCodeString()` to get an Id in string format

### Using the `SnowflakeIDGenerator` class as static

If you only need to get a single Id, it's easier to just use the generator class as static.

The method names are the same as when using the generator, except they need the `machineId` as parameter:
   
1. Call `SnowflakeIDGenerator.GetSnowflake(machineId)` to get a `Snowflake` object
2. Call `SnowflakeIDGenerator.GetCode(machineId)` to get an Id in number (ulong) format
3. Call `SnowflakeIDGenerator.GetCodeString(machineId)` to get an Id in string format

> If the generated code is going to be used somewhere without a well-defined ulong type (example, Typescript),
> it is recommended to use the string representation, because those languages usually represent numbers as
> floating-point numbers and must round the code since they can't represent every integer beyond certain value.

### Using a non-standard date as epoch

*version 1.1.2023 and up*

The first component of the codes is the amount of milliseconds elapsed since a set point in time, called epoch.
By default, the generator uses the unix epoch (jan-1-1970 12:00:00am) as starting point to count.

Adding a `DateTime` object as an extra parameter when using the generator allows to change the zero value to be used to count milliseconds to.

```c#
DateTime customEpoch = new DateTime(year: 2020, month: 1, day: 1, hour: 0, minute: 0, second: 0, DateTimeKind.Utc);

// Creating the generator class
SnowflakeIDGenerator generator = new SnowflakeIDGenerator(machineId, customEpoch);

// This works when using as static too!
SnowflakeIDGenerator.GetSnowflake(machineId, customEpoch)
```

## Parsing an Id

Parse a Snowflake either from a string or a number (ulong) in order to get information regarding the generation,
such as the time or the machine that generated the code.

If a custom epoch was used when generating, that DateTime must be passed as second parameter when parsing in order to get the right generation date.

```c#
string s = "06975580616378931208";
ulong n = 6975580821430984519ul;
Snowflake fromString = Snowflake.Parse(s);
var utcDateTimeFromString = fromString.UtcDateTime; // 13/9/2022 22:26:58
var timestampFromString = fromString.Timestamp;     // 1663108018965
var machineIdFromString = fromString.MachineId;     // 477
var sequenceFromString = fromString.Sequence;       // 2056

Snowflake fromNumber = Snowflake.Parse(n);
var utcDateTimeFromNumber = fromNumber.UtcDateTime; // 13/9/2022 22:27:47
var timestampFromNumber = fromNumber.Timestamp;     // 1663108067853
var machineIdFromNumber = fromNumber.MachineId;     // 701
var sequenceFromNumber = fromNumber.Sequence;       // 3911
```

Additionally, starting on version 1.2.2023 you can cast a string or a number (ulong) directly into a Snowflake
without using the `Parse()` method (only when using the default epoch).

```c#
string s = "06975580616378931208";
ulong n = 6975580821430984519ul;
Snowflake fromString = (Snowflake)s;
var utcDateTimeFromString = fromString.UtcDateTime; // 13/9/2022 22:26:58
var timestampFromString = fromString.Timestamp;     // 1663108018965
var machineIdFromString = fromString.MachineId;     // 477
var sequenceFromString = fromString.Sequence;       // 2056

Snowflake fromNumber = (Snowflake)n;
var utcDateTimeFromNumber = fromNumber.UtcDateTime; // 13/9/2022 22:27:47
var timestampFromNumber = fromNumber.Timestamp;     // 1663108067853
var machineIdFromNumber = fromNumber.MachineId;     // 701
var sequenceFromNumber = fromNumber.Sequence;       // 3911
```

## The Snowflake object

While the `SnowflakeIDGenerator` class keeps track of time, machine and sequence,
a `Snowflake` object keeps track of the meaning of the code, and allows to extract information about said code.

For example, as seen on the [parsing an Id](#parsing-an-id) section, when working with a `Snowflake` object
(either parsed or just generated), you can see some information. The available fields are:

* `UtcDateTime`: The creation date (UTC format)
* `Timestamp` and `TimestampInt64` The timestamp component of the code. Amount of milliseconds since the configured epoch
* `MachineId` and `MachineIdInt32`: the machine / terminal / server that created the id
* `Sequence` and `SequenceInt32`: The sequencer. If greater than 0, multiple ids where generated withing the same millisecond
* `Id`: The Snowflake id in `ulong` format
* `Code`: The Snowflake id as `String`


## Changing Epoch on generated codes

If you need to change the epoch on an already generated code,
use `ChangeEpoch()` to change it keeping the same code but changing the represented date,
or use `RebaseEpoch()` to keep the date but changing the final code.
