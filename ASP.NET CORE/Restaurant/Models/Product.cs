using System;

namespace Restaurant.Models;

public class Product
{
    public int ProductId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }//this defines in which category does product belongs to
    public ICollection<OrderItem>? OrderItems { get; set; }//a prodt can be in many order items
    public ICollection<ProductIngredient>? ProductIngredients { get; set; }//defines prodt ingredients list
}
