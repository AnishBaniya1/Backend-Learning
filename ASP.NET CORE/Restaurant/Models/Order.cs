using System;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Models;

public class Order
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public int UserId { get; set; }//foreign key
    public User? User { get; set; }//navigation property
    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }
    public ICollection<OrderItem>? OrderItems { get; set; }
}
