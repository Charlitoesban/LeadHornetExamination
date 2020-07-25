using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeadHornet.Models
{
    public class IpTracker
    {
        [Key]
        public int id { get; set; }
        public string location { get; set; }
        public string Ip { get; set; }
        public string CountryCode { get; set; }
        public string ContinentName { get; set; }
        public DateTime DateTimeVisited { get; set; }
    }
}
