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
        private IWebHostEnvironment _webHostEnvironment;
        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            products = new Repository<Product>(context);
            ingredients = new Repository<Ingredient>(context);
            categories = new Repository<Category>(context);
            _webHostEnvironment = webHostEnvironment;
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
                Product product = await products.GetByIdAsync(id, new QueryOptions<Product>
                {
                    Includes = "ProductIngredients.Ingredient, Category",
                });
                ViewBag.Operation = "Edit";
                return View(product);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(Product product, int[] ingredientIds, int catId)
        {
            ViewBag.Ingredients = await ingredients.GetAllAsync();
            ViewBag.Categories = await categories.GetAllAsync();
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    //This line defines where to save the uploaded image file.
                    //_webHostEnvironment.WebRootPath gives the path to your wwwroot folder in the project (for example: C:\MyApp\wwwroot).
                    //Path.Combine(..., "images") adds a subfolder named images inside that folder.
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                    //Creates a unique filename to avoid overwriting existing files.
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;

                    // Builds the full file path like:C:\MyApp\wwwroot\images\a7c1e76b-82e9-44c4-8a6f_photo.png
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // This actually creates and saves the file on disk using a FileStream.
                    using (var fileStrem = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(fileStrem);
                    }
                    // Stores the saved image file name in the database model — so you can later display it using something like:
                    product.ImageUrl = uniqueFileName;
                }
                //add product
                if (product.ProductId == 0)
                {

                    product.CategoryId = catId; //Assigns the chosen category to the product.

                    //add ingredients
                    foreach (int id in ingredientIds)
                    {
                        product.ProductIngredients?.Add(new ProductIngredient { IngredientId = id, ProductId = product.ProductId });
                    }
                    await products.AddAsync(product);
                    return RedirectToAction("Index", "Product");
                }
                //existing product
                else
                {//Fetches the existing product (with its ingredients) from the database.
                    var existingProduct = await products.GetByIdAsync(product.ProductId, new QueryOptions<Product>
                    {
                        Includes = "ProductIngredients"
                    });
                    if (existingProduct == null)
                    {//If the product doesn’t exist, show an error message.
                        ModelState.AddModelError("", "Product Not Found");
                        ViewBag.Ingredient = await ingredients.GetAllAsync();
                        ViewBag.Categories = await categories.GetAllAsync();
                        return View(product);
                    }
                    //Updates the product details with new values from the form.
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.Stock = product.Stock;
                    existingProduct.CategoryId = catId;

                    //Removes old ingredients and adds the new selected ones.
                    existingProduct.ProductIngredients?.Clear();
                    foreach (int id in ingredientIds)
                    {
                        existingProduct.ProductIngredients?.Add(new ProductIngredient { IngredientId = id, ProductId = product.ProductId });
                    }
                    try
                    {
                        await products.UpdateAsync(existingProduct);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Erro:{ex.GetBaseException().Message}");
                        ViewBag.Ingredient = await ingredients.GetAllAsync();
                        ViewBag.Categories = await categories.GetAllAsync();
                        return View(product);
                    }
                }
            }
            return RedirectToAction("Index", "Product");


        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await products.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Product Not Found");
                return RedirectToAction("Index");
            }
        }

    }
}
