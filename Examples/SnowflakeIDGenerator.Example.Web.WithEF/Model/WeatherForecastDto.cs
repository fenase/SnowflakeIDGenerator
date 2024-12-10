using SnowflakeID;

namespace SnowflakeIDGenerator.Example.Web.WithEF.Model
{
    public class WeatherForecastDto
    {
        public string Id { get; set; }

        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF { get; set; }

        public string? Summary { get; set; }

        // DTO mappings
        // In this example it's done manually, but you can use libraries like AutoMapper to do it automatically
        public static explicit operator WeatherForecastDto?(WeatherForecast? weatherForecast)
        {
            if (weatherForecast == null)
            {
                return null;
            }
            return new WeatherForecastDto
            {
                Id = weatherForecast.Id.ToString(),
                Date = weatherForecast.Date,
                TemperatureC = weatherForecast.TemperatureC,
                TemperatureF = weatherForecast.TemperatureF,
                Summary = weatherForecast.Summary
            };
        }

        public static explicit operator WeatherForecast?(WeatherForecastDto? weatherForecastDto)
        {
            if (weatherForecastDto == null)
            {
                return null;
            }
            // It would probably be better not to use Snowflake.Parse here, but to move this to the WeatherForecast class in order not not expose Snowflake outside of the Model namespace
            return new WeatherForecast
            {
                Id = Snowflake.Parse(weatherForecastDto.Id),
                Date = weatherForecastDto.Date,
                TemperatureC = weatherForecastDto.TemperatureC,
                Summary = weatherForecastDto.Summary
            };
        }
    }
}
