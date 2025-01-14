using Microsoft.AspNetCore.Mvc;
using zad2.Models;
using zad2.Services;
using System.Linq;
using System.Collections.Generic;

namespace zad2.Controllers
{
    public class ProductController : Controller
    {
        private readonly JsonFileProductService _productService;

        public ProductController(JsonFileProductService productService)
        {
            _productService = productService;
        }

        // Akcja przeglądania listy produktów z filtrowaniem i sortowaniem
        public IActionResult Index(string searchTerm, string category, string sortOrder)
        {
            var products = _productService.GetProducts();

            // Filtrowanie po nazwie, jeśli podano `searchTerm`
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Filtrowanie po kategorii, jeśli podano `category`
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category == category).ToList();
            }

            // Sortowanie po nazwie lub cenie
            products = sortOrder switch
            {
                "name_desc" => products.OrderByDescending(p => p.Name).ToList(),
                "price" => products.OrderBy(p => p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                _ => products.OrderBy(p => p.Name).ToList()
            };

            return View(products);
        }

        // Akcja dodania nowego produktu
        [HttpPost]
        public IActionResult Create(Product product)
        {
            var products = _productService.GetProducts();
            product.Id = products.Any() ? products.Max(p => p.Id) + 1 : 1; // Nowe ID
            products.Add(product);
            _productService.SaveProducts(products);
            return RedirectToAction("Index");
        }

        // Akcja edytowania produktu
        [HttpPost]
        public IActionResult Edit(Product updatedProduct)
        {
            var products = _productService.GetProducts();
            var product = products.FirstOrDefault(p => p.Id == updatedProduct.Id);
            if (product != null)
            {
                product.Name = updatedProduct.Name;
                product.Category = updatedProduct.Category;
                product.Price = updatedProduct.Price;
                product.Quantity = updatedProduct.Quantity;
                _productService.SaveProducts(products);
            }
            return RedirectToAction("Index");
        }

        // Akcja usunięcia produktu
        public IActionResult Delete(int id)
        {
            var products = _productService.GetProducts();
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                products.Remove(product);
                _productService.SaveProducts(products);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetProductsJson()
        {
            try
            {
                var productsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "products.json");

                // Upewnij się, że plik istnieje
                if (!System.IO.File.Exists(productsFilePath))
                {
                    return StatusCode(500, "Plik products.json nie został znaleziony.");
                }

                var jsonData = System.IO.File.ReadAllText(productsFilePath);

                // Zwróć dane jako JSON
                return Content(jsonData, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Wystąpił błąd serwera: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult SearchProducts(string query)
        {
            // Pobierz wszystkie produkty
            var products = _productService.GetProducts();

            // Jeśli zapytanie jest puste, zwróć wszystkie produkty
            if (string.IsNullOrEmpty(query))
            {
                return Json(products);
            }

            // Filtruj produkty na podstawie nazwy (ignorując wielkość liter)
            var filteredProducts = products
                .Where(p => p.Name.ToLower().Contains(query.ToLower()))
                .ToList();

            return Json(filteredProducts);
        }

        [HttpGet]
        public IActionResult GetProductById(int id)
        {
            // Pobierz produkt po ID z serwisu
            var product = _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return Json(product);  // Zwróć produkt w formacie JSON
        }

        [HttpGet("GetProducts")] // Endpoint do pobrania wszystkich produktów
        public IActionResult GetProducts()
        {
            var products = _productService.GetProducts();  // Zakładając, że masz taką metodę w serwisie
            return Json(products);
        }

        [HttpPost]
        public IActionResult UpdateProduct([FromBody] Product updatedProduct)
        {
            if (updatedProduct == null)
            {
                return BadRequest();
            }

            var success = _productService.UpdateProduct(updatedProduct);

            if (success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }


    }
}
