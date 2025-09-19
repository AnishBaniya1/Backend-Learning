using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Models;

public class Product
{
    public int ProductId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    [Precision(18, 2)]
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
    public string ImageUrl { get; set; } = "https://via.placeholder.com/150";
    [ValidateNever]
    public Category? Category { get; set; }//this defines in which category does product belongs to
    [ValidateNever]
    public ICollection<OrderItem>? OrderItems { get; set; }//a prodt can be in many order items
    [ValidateNever]
    public ICollection<ProductIngredient>? ProductIngredients { get; set; }//defines prodt ingredients list
}
