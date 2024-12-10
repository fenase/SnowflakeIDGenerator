using Microsoft.EntityFrameworkCore.Infrastructure;
using SnowflakeID;

namespace SnowflakeIDGenerator.Example.Web.WithEF.dbGenerators
{
    public class SnowflakeValueGenerator : Microsoft.EntityFrameworkCore.ValueGeneration.ValueGenerator<Snowflake>
    {
        public override bool GeneratesTemporaryValues => false;

        public override Snowflake Next(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            return entry.Context.GetService<ISnowflakeIDGenerator>().GetSnowflake();
        }
    }
}
