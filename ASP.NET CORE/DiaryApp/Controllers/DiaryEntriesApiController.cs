using DiaryApp.Data;
using DiaryApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiaryEntriesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public DiaryEntriesApiController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entries = await _db.DiaryEntries.ToListAsync();
            return Ok(entries);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var entry = await _db.DiaryEntries.FindAsync(id);
            if (entry == null) return NotFound();
            return Ok(entry);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DiaryEntry entry)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _db.DiaryEntries.Add(entry);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DiaryEntry entry)
        {
            var existing = await _db.DiaryEntries.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = entry.Title;
            existing.Content = entry.Content;
            existing.CreatedDate = entry.CreatedDate;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entry = await _db.DiaryEntries.FindAsync(id);
            if (entry == null) return NotFound();

            _db.DiaryEntries.Remove(entry);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
