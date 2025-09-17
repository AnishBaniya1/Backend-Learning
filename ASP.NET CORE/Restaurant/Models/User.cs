using System;

namespace Restaurant.Models;

public class User
{
    public int Id { get; set; }   // int instead of string like IdentityUser
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<Order>? Orders { get; set; }
}
