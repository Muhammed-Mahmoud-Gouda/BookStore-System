using ShpoNest.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopNest.DAL.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllWithCategoryAsync();
        Task<Product?> GetByIdWithImagesAsync(int id);
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10);
        Task<Product?> GetByISBNAsync(string isbn);
        Task<bool> ISBNExistsAsync(string isbn);
    }
}
