using Microsoft.AspNetCore.Mvc;
using Restaurant.Data;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class ProductController : Controller
    {
        private Repository<Product> products;
        private Repository<Ingredient> ingredients;
        private Repository<Category> categories;
        public ProductController(ApplicationDbContext context)
        {
            products = new Repository<Product>(context);
            ingredients = new Repository<Ingredient>(context);
            categories = new Repository<Category>(context);
        }

        // GET: ProductController
        public async Task<IActionResult> Index()
        {
            return View(await products.GetAllAsync());
        }

        public async Task<IActionResult> AddEdit(int id)
        {
            ViewBag.Ingredients = await ingredients.GetAllAsync();
            ViewBag.Categories = await categories.GetAllAsync();
            if (id == 0)//That means the user clicked something like “Add Product”, not “Edit”.
            {
                ViewBag.Operation = "Add";//used in the view to show “Add Product” in the title/button text.
                return View(new Product());//passes a new, empty Product object to the view, so the form fields are blank.
            }
            else//That means the user clicked “Edit” for a specific product.
            {
                ViewBag.Operation = "Edit";
                return View();
            }
        }

    }
}
