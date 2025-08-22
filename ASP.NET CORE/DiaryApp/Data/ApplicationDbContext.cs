using DiaryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Data
{
    //iherit from DbContext which is part of EF CORE
    public class ApplicationDbContext : DbContext
    {
        //in constructor pass DbContextOptions of type<ApplicationDbContext>
        // it will pass the options to base class dbcontext and own class applicationdbcontext
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<DiaryEntry> DiaryEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DiaryEntry>().HasData(
                new DiaryEntry
                {
                    Id = 1,
                    Title = "Went Hiking",
                    Content = "Went hiking with Joe!",
                    CreatedDate = new DateTime(2025, 8, 20)
                },

                new DiaryEntry
                {
                    Id = 2,
                    Title = "Went Shopping",
                    Content = "Bought Laptop!",
                    CreatedDate = new DateTime(2025, 8, 22)
                }


            );
        }

    }
}