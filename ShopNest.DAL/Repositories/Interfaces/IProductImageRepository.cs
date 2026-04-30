using ShpoNest.Models.Entities;

namespace ShopNest.DAL.Repositories.Interfaces
{
    public interface IProductImageRepository : IGenericRepository<ProductImages>
    {
        Task<IEnumerable<ProductImages>> GetByProductIdAsync(int productId);
        Task<ProductImages?> GetMainImageAsync(int productId);
    }
}