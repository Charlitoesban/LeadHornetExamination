using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using LeadHornet.ForecastAppModels;
using LeadHornet.OpenWeatherMapModels;
using LeadHornet.Repositories;
using LeadHornet.Config;
using IpStack;
using MetOfficeDataPoint;
using MetOfficeDataPoint.Models;
using MetOfficeDataPoint.Models.GeoCoordinate;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using LeadHornet.Models;
using System.Collections.Generic;
using System;
using System.Linq;

namespace LeadHornet.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly IForecastRepository _forecastRepository;
        private readonly IActionContextAccessor _accessor;
        private readonly ILogger<HomeController> _logger;
        private LeadHornetDbContext _leadHornetDbContext;
        public HomeController(ILogger<HomeController> logger, IActionContextAccessor accessor, IForecastRepository forecastAppRepo, IConfiguration config, IHttpContextAccessor httpContextAccessor, LeadHornetDbContext leadHornetDbContext)
        {
            _logger = logger;
            _accessor = accessor;
            _forecastRepository = forecastAppRepo;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _leadHornetDbContext = leadHornetDbContext;
        }

        //GET: ForecastApp/SearchCity
        public IActionResult SearchCity(double longitude, double latitude)
        {
            IpStackClient ipClient = new IpStackClient(_config["Keys:IPStack"]);

            string ipAddress = LeadHornet.Models.weather.GetRequestIP(_httpContextAccessor);

            IpStack.Models.IpAddressDetails location = ipClient.GetIpAddressDetails(ipAddress);

            GeoCoordinate coordinate = new GeoCoordinate();

            // If location is provided then use over IP address
            if (longitude == 0 && latitude == 0)
            {
                coordinate.Longitude = location.Longitude;
                coordinate.Latitude = location.Latitude;
            }
            else
            {
                coordinate.Longitude = longitude;
                coordinate.Latitude = latitude;
            }

            MetOfficeDataPointClient metClient = new MetOfficeDataPointClient(_config["Keys:MetOfficeDataPoint"]);
            MetOfficeDataPoint.Models.Location site = metClient.GetClosestSite(coordinate).Result;
            ForecastResponse3Hourly forecastResponse = metClient.GetForecasts3Hourly(null, site.ID).Result;

            // Create list containing 5 days of forecasts for midday
            List<WeatherSummary> weatherSummaries = new List<WeatherSummary>();

            // Get current minutes after midnight
            int minutes = (int)(DateTime.Now.TimeOfDay.TotalSeconds / 60);

            foreach (ForecastLocation3Hourly forecastLocation in forecastResponse.SiteRep.DV.Location)
            {
                foreach (Period3Hourly period in forecastLocation.Period)
                {
                    if (DateTime.Parse(period.Value).Date == DateTime.Today.Date)
                    {
                        WeatherSummary weatherSummary = new WeatherSummary();
                        weatherSummary.ForecastDay = DateTime.Parse(period.Value);

                        // Find closest forecast to now
                        Rep3Hourly forecast = period.Rep.Aggregate((x, y) => Math.Abs(x.MinutesAfterMidnight - minutes) < Math.Abs(y.MinutesAfterMidnight - minutes) ? x : y);
                        weatherSummary.Icon = LeadHornet.Models.weather.GetWeatherIcon(forecast.WeatherType);
                        // Find min temperature
                        weatherSummary.MinTemperature = (int)period.Rep.Where(x => x.MinutesAfterMidnight >= forecast.MinutesAfterMidnight).Min(x => x.Temperature);
                        // Find current temperature
                        weatherSummary.MaxTemperature = (int)period.Rep.Where(x => x.MinutesAfterMidnight >= forecast.MinutesAfterMidnight).Max(x => x.Temperature);
                        // Get remaing forecasts
                        weatherSummary.Forecasts = period.Rep.Where(x => x.MinutesAfterMidnight >= forecast.MinutesAfterMidnight).ToList();

                        weatherSummaries.Add(weatherSummary);
                    }
                    else
                    {
                        WeatherSummary weatherSummary = new WeatherSummary();
                        weatherSummary.ForecastDay = DateTime.Parse(period.Value);

                        // Get icon for midday
                        weatherSummary.Icon = LeadHornet.Models.weather.GetWeatherIcon(period.Rep.First(x => x.MinutesAfterMidnight == 720).WeatherType);
                        // Find min temperature
                        weatherSummary.MinTemperature = (int)period.Rep.Min(x => x.Temperature);
                        // Find max temperature
                        weatherSummary.MaxTemperature = (int)period.Rep.Max(x => x.Temperature);
                        // Get forecasts
                        weatherSummary.Forecasts = period.Rep;

                        weatherSummaries.Add(weatherSummary);
                    }
                }
            }

            ViewData["WeatherSummaries"] = weatherSummaries;
            ViewData["Location"] = location;
            ViewData["Site"] = site;

            string strLoc = location.CountryName;
            string strIP = location.Ip;
            string strCountryCode = location.CountryCode;
            string strContinentName = location.ContinentName;

            var ipTrackerData = new IpTracker();

            _leadHornetDbContext.IpTrackers.Add(new IpTracker() { 
                location = strLoc,
                Ip = strIP,
                CountryCode = strCountryCode,
                ContinentName = strContinentName,
                DateTimeVisited = DateTime.Now
            });

            _leadHornetDbContext.SaveChanges();

            return View();
        }

      
        public JsonResult City([FromBody] string city)
        {
   
            WeatherResponse weatherResponse = _forecastRepository.GetForecast(city);
            City viewModel = new City();

            if (weatherResponse != null)
            {
                viewModel.Name = weatherResponse.Name;
                viewModel.Humidity = weatherResponse.Main.Humidity;
                viewModel.Pressure = weatherResponse.Main.Pressure;
                viewModel.Temp = weatherResponse.Main.Temp;
                viewModel.Weather = weatherResponse.Weather[0].Main;
                viewModel.Wind = weatherResponse.Wind.Speed;
            }

            return new JsonResult(viewModel);
        }

    }
}
