using Microsoft.AspNetCore.Mvc;

namespace DiaryApp.Controllers
{
    public class DiaryEntriesController : Controller
    {
        // GET: DiaryEntriesController
        public ActionResult Index()
        {
            return View();
        }

    }
}
