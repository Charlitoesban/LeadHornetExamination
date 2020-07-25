using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LeadHornet.Models;
namespace LeadHornet.Models
{
    public class LeadHornetDbContext : DbContext
    {
        public LeadHornetDbContext(DbContextOptions<LeadHornetDbContext> options) : base(options)
        {         
        }
        public DbSet<IpTracker> IpTrackers { get; set; }
    }
}
