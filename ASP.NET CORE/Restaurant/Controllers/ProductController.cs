using Microsoft.AspNetCore.Mvc;
using Restaurant.Data;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class ProductController : Controller
    {
        private Repository<Product> products;
        public ProductController(ApplicationDbContext context)
        {
            products = new Repository<Product>(context);
        }

        // GET: ProductController
        public async Task<IActionResult> Index()
        {
            return View(await products.GetAllAsync());
        }

    }
}
