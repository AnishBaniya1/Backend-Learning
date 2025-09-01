using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestApi.Data;
using RestApi.Models;

namespace RestApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ApiDbContext _db;
        public BooksController(ApiDbContext db)
        {
            _db = db;
        }
        // static private List<Book> books = new List<Book>
        // {
        //     new Book{
        //         Id=1,
        //         Title="The Great Gatsby",
        //         Author="F. Scott Fitz",
        //         YearPublished=1925
        //     },
        //     new Book{
        //         Id=2,
        //         Title="To Kill a Bird",
        //         Author="F. Scott",
        //         YearPublished=1960
        //     },
        //       new Book{
        //         Id=3,
        //         Title="1984",
        //         Author="George Orwell",
        //         YearPublished=1949
        //     },
        // };
        [HttpGet]
        public async Task<ActionResult<List<Book>>> GetBooks()
        {
            var books = await _db.Books.ToListAsync();
            return Ok(books);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBookById(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }
        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(Book newBook)
        {
            if (newBook == null)
            {
                return BadRequest();
            }
            _db.Books.Add(newBook);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBookById), new { id = newBook.Id }, newBook);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooks(int id, Book updatedbook)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            book.Id = updatedbook.Id;
            book.Title = updatedbook.Title;
            book.Author = updatedbook.Author;
            book.YearPublished = updatedbook.YearPublished;
            await _db.SaveChangesAsync();
            return Ok(book);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooks(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null)
                return NotFound();
            _db.Remove(book);
            await _db.SaveChangesAsync();
            return NoContent();
        }

    }
}
