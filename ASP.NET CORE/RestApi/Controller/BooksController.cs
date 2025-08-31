using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestApi.Models;

namespace RestApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        static private List<Book> books = new List<Book>
        {
            new Book{
                Id=1,
                Title="The Great Gatsby",
                Author="F. Scott Fitz",
                YearPublished=1925
            },
            new Book{
                Id=2,
                Title="To Kill a Bird",
                Author="F. Scott",
                YearPublished=1960
            },
              new Book{
                Id=3,
                Title="1984",
                Author="George Orwell",
                YearPublished=1949
            },
        };
        [HttpGet]
        public ActionResult<List<Book>> GetBooks()
        {
            return Ok(books);
        }
        [HttpGet("{id}")]
        public ActionResult<Book> GetBooks(int id)
        {
            var book = books.FirstOrDefault(x => x.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }
    }
}
