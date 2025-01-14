using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using zad2.Models;

namespace zad2.Services
{
    public class JsonFileProductService
    {
        private readonly string _filePath = "wwwroot/data/products.json"; // Ścieżka do pliku JSON

        // Pobiera listę produktów z pliku JSON
        public List<Product> GetProducts()
        {
            if (!File.Exists(_filePath)) // Jeśli plik nie istnieje, zwraca pustą listę
                return new List<Product>();

            var jsonData = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<List<Product>>(jsonData);
        }

        // Zapisuje listę produktów do pliku JSON
        public void SaveProducts(List<Product> products)
        {
            var jsonData = JsonConvert.SerializeObject(products, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_filePath, jsonData);
        }

        public bool UpdateProduct(Product updatedProduct)
        {
            var products = GetProducts();
            var existingProduct = products.FirstOrDefault(p => p.Id == updatedProduct.Id);

            if (existingProduct != null)
            {
                existingProduct.Name = updatedProduct.Name;
                existingProduct.Category = updatedProduct.Category;
                existingProduct.Price = updatedProduct.Price;
                existingProduct.Quantity = updatedProduct.Quantity;

                // Zapisz zaktualizowane produkty do pliku
                SaveProducts(products);
                return true;
            }

            return false;
        }

        public Product GetProductById(int id)
        {
            var products = GetProducts(); // Pobierz wszystkie produkty z pliku
            return products.FirstOrDefault(p => p.Id == id); // Zwróć produkt, którego ID pasuje do podanego
        }
    }
}
