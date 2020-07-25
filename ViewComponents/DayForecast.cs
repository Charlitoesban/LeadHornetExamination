using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeadHornet.Models;
using Microsoft.AspNetCore.Mvc;

namespace LeadHornet.ViewComponents
{
    public class DayForecast : ViewComponent
    {
        public IViewComponentResult Invoke(WeatherSummary weatherSummary)
        {
            return View(weatherSummary);
        }
    }
}
