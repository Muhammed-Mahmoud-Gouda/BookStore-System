using Microsoft.EntityFrameworkCore;
using ShopNest.DAL.ApplicationDbContext;
using ShopNest.DAL.Repositories.Interfaces;
using ShpoNest.Models.Entities;


namespace ShopNest.DAL.Repositories.Implementations
{
    public class ProductImageRepository : GenericRepository<ProductImages>, IProductImageRepository
    {
        public ProductImageRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<ProductImages>> GetByProductIdAsync(int productId)
            => await _dbSet
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.DisplayOrder)
                .ToListAsync();

        public async Task<ProductImages?> GetMainImageAsync(int productId)
            => await _dbSet
                .Where(pi => pi.ProductId == productId)
                .OrderByDescending(pi => pi.IsMain)
                .FirstOrDefaultAsync();
    }
}
