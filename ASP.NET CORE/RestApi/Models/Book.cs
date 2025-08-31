using System;
using System.ComponentModel.DataAnnotations;

namespace RestApi.Models;

public class Book
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int YearPublished { get; set; }
}
