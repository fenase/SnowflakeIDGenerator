using Microsoft.AspNetCore.Mvc;
using SnowflakeID;
using SnowflakeIDGenerator.Example.Web.WithEF.dbContext;
using SnowflakeIDGenerator.Example.Web.WithEF.Model;

namespace SnowflakeIDGenerator.Example.Web.WithEF.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly WeatherDbContext _dbContext;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, WeatherDbContext weatherDbContext)
        {
            _logger = logger;
            _dbContext = weatherDbContext;
        }

        [HttpGet("get/{id}")]
        public WeatherForecastDto? Get(string id)
        {
            return (WeatherForecastDto?)_dbContext.WeatherForecasts.Find((Snowflake)id); //When using custom epoch, you must use Snowflake.Parse(id, epoch) to parse the id instead of casting it to Snowflake
        }

        [HttpGet("getAll")]
        public IEnumerable<WeatherForecastDto> Get()
        {
            return _dbContext.WeatherForecasts.Cast<WeatherForecastDto>();
        }

        [HttpPost("create")]
        public WeatherForecastDto Create()
        {
            WeatherForecast weatherForecast = new()
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = new Random().Next(-20, 55),
                Summary = Summaries[new Random().Next(Summaries.Length)]
            };
            _dbContext.WeatherForecasts.Add(weatherForecast);
            _dbContext.SaveChanges();
            return (WeatherForecastDto)weatherForecast!;
        }
    }
}
