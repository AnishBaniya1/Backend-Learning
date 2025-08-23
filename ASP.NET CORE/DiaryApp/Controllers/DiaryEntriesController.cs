using DiaryApp.Data;
using DiaryApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace DiaryApp.Controllers
{

    public class DiaryEntriesController : Controller
    {
        //this is dependency injection
        private readonly ApplicationDbContext _db;
        public DiaryEntriesController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET: DiaryEntriesController
        public ActionResult Index()
        {
            //to get data
            List<DiaryEntry> entries = _db.DiaryEntries.ToList();
            //passing value
            return View(entries);
        }

    }
}
