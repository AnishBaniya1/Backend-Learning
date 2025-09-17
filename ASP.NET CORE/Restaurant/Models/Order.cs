using System;

namespace Restaurant.Models;

public class Order
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public int UserId { get; set; }//foreign key
    public User? User { get; set; }//navigation property
    public decimal TotalAmount { get; set; }
    public ICollection<OrderItem>? OrderItems { get; set; }
}
