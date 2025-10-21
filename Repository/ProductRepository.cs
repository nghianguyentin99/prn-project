using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCShop.Repository
{
    public class ProductRepository
    {
        PcshopDbContext context;

        public ProductRepository()
        {
            context = new PcshopDbContext();
        }
        public List<Product> GetAll()
        {
            return context.Products.ToList();
        }
        public Product GetById(int id)
        {
            return context.Products.FirstOrDefault(p => p.ProductId == id);
        }
        public void  AddProduct(Product product)
        {
            context.Products.Add(product);
            context.SaveChanges();
        }
        public void DeleteProduct(int id)
        {
            var product = context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                context.Products.Remove(product);
                context.SaveChanges();
            }
        }
    }
}
