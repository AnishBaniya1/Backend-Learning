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

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(DiaryEntry obj)
        {
            _db.DiaryEntries.Add(obj);//adds new diary entry to db
            _db.SaveChanges();//saves the chnage4s to the db
            return RedirectToAction("Index");
        }

    }
}
