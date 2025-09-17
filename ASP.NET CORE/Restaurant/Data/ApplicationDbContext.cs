using System;
using System.Formats.Tar;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductIngredient> ProductIngredients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //defines composite key and relationships for productIngredient
        modelBuilder.Entity<ProductIngredient>()
        .HasKey(pi => new { pi.ProductId, pi.IngredientId });

        modelBuilder.Entity<ProductIngredient>()
        .HasOne(pi => pi.Product).WithMany(p => p.ProductIngredients)
        .HasForeignKey(pi => pi.ProductId);

        modelBuilder.Entity<ProductIngredient>()
        .HasOne(pi => pi.Ingredient).WithMany(i => i.ProductIngredients)
        .HasForeignKey(pi => pi.IngredientId);

    }
}
