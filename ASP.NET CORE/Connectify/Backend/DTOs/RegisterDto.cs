using System;

namespace Backend.DTOs;

public class RegisterDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? UserName { get; set; }
    public IFormFile? ProfileImage { get; set; }
}
