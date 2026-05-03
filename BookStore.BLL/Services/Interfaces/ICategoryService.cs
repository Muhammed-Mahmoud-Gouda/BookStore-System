using ShopNest.BLL.DTOs.Category;

namespace ShopNest.BLL.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResultDto>> GetAllAsync();
        Task<IEnumerable<CategoryResultDto>> GetAllWithProductsAsync();
        Task<CategoryResultDto?> GetByIdAsync(int id);
        Task CreateAsync(CategoryCreateDto dto);
        Task UpdateAsync(CategoryUpdateDto dto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}