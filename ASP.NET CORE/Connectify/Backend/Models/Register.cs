using System;

namespace Backend.Models;

public class Register
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? UserName { get; set; }
    public IFormFile? ProfileImage { get; set; }
}
