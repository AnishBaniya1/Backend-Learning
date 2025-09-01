using System;
using Microsoft.EntityFrameworkCore;
using RestApi.Models;

namespace RestApi.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {

    }

    public DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                Id = 1,
                Title = "The Great Gatsby",
                Author = "F. Scott Fitz",
                YearPublished = 1925
            },
            new Book
            {
                Id = 2,
                Title = "To Kill a Bird",
                Author = "F. Scott",
                YearPublished = 1960
            },
              new Book
              {
                  Id = 3,
                  Title = "1984",
                  Author = "George Orwell",
                  YearPublished = 1949
              }

        );
    }

}
