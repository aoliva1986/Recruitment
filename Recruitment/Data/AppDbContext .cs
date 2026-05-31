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

        public DbSet<Job_Position> Job_Position { get; set; }
        public DbSet<Candidate> Candidate { get; set; }
        public DbSet<Documents> Documents { get; set; }
        public DbSet<Move> Move { get; set; }
        public DbSet<Pipeline_Stage> Pipeline_Stage { get; set; }



    }
}
