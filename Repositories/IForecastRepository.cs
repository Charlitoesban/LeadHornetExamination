using LeadHornet.OpenWeatherMapModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeadHornet.Repositories
{
    public interface IForecastRepository
    {
        WeatherResponse GetForecast(string city);
    }
}
