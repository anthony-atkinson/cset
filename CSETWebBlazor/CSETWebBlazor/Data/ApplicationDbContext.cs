using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CSETWebBlazor.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add CSET-specific DbSets here
        // public DbSet<Assessment> Assessments { get; set; }
        // public DbSet<Question> Questions { get; set; }
        // public DbSet<Answer> Answers { get; set; }
        // public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure CSET-specific entities here
            // builder.Entity<Assessment>(entity =>
            // {
            //     entity.HasKey(e => e.Id);
            //     entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            //     entity.Property(e => e.CreatedDate).IsRequired();
            // });
        }
    }
} 