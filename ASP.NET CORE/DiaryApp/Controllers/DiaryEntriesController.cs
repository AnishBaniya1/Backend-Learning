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

            if (obj != null && obj.Title.Length < 3)
            {
                ModelState.AddModelError("Title", "Title too short");
            }
            if (ModelState.IsValid)
            {
                _db.DiaryEntries.Add(obj!);//adds new diary entry to db
                _db.SaveChanges();//saves the chnage4s to the db
                return RedirectToAction("Index");
            }
            return View(obj);

        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            DiaryEntry? diaryEntry = _db.DiaryEntries.Find(id);
            if (diaryEntry == null)
            {
                return NotFound();
            }
            return View(diaryEntry);
        }
        [HttpPost]
        public IActionResult Edit(DiaryEntry obj)
        {

            if (obj != null && obj.Title.Length < 3)
            {
                ModelState.AddModelError("Title", "Title too short");
            }
            if (ModelState.IsValid)
            {
                _db.DiaryEntries.Update(obj!);//updates the diary entry to db
                _db.SaveChanges();//saves the chnage4s to the db
                return RedirectToAction("Index");
            }
            return View(obj);

        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            DiaryEntry? diaryEntry = _db.DiaryEntries.Find(id);
            if (diaryEntry == null)
            {
                return NotFound();
            }
            return View(diaryEntry);
        }
        [HttpPost]
        public IActionResult Delete(DiaryEntry obj)
        {
            _db.DiaryEntries.Remove(obj!);//removes the diary entry to db
            _db.SaveChanges();//saves the chnage4s to the db
            return RedirectToAction("Index");

        }

    }
}
