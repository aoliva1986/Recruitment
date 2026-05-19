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
        public DbSet<Company> Company { get; set; }

        public DbSet<Selection_Pipeline> Selection_Pipeline { get; set; }
        public DbSet<Questionnaire> Questionnaire { get; set; }

    }
}
