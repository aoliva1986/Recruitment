using Microsoft.EntityFrameworkCore;
using Recruitment.Models;

namespace Recruitment.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }
        public DbSet<Role> Role { get; set; }

    }
}
