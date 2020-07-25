using MetOfficeDataPoint.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeadHornet.Models
{
    public class WeatherSummary
    {
        public WeatherSummary()
        {
            Forecasts = new List<Rep3Hourly>();
        }

        public DateTime ForecastDay { get; set; }
        public int MinTemperature { get; set; }
        public int MaxTemperature { get; set; }
        public WeatherIcon Icon { get; set; }
        public List<Rep3Hourly> Forecasts { get; set; }
    }
}
