using Microsoft.EntityFrameworkCore;
using ShopNest.DAL.ApplicationDbContext;
using ShopNest.DAL.Repositories.Interfaces;
using ShpoNest.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopNest.DAL.Repositories.Implementations
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetAllWithProductsAsync()
        {
            return await _dbSet
                .Include(c => c.Products)
                    .ThenInclude(p => p.Images)
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdWithProductsAsync(int id)
        {
            return await _dbSet
            .Include(c => c.Products)
                .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
