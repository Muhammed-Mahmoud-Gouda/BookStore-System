using ShopNest.BLL.DTOs.Product;

public interface IProductService
{
    Task<IEnumerable<ProductResultDto>> GetAllAsync();
    Task<IEnumerable<ProductResultDto>> GetAllWithCategoryAsync();
    Task<IEnumerable<ProductResultDto>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<ProductResultDto>> GetActiveAsync();
    Task<IEnumerable<ProductResultDto>> GetLowStockAsync(int threshold = 10);
    Task<ProductResultDto?> GetByIdAsync(int id);
    Task<ProductResultDto?> GetByISBNAsync(string isbn);  
    Task CreateAsync(ProductCreateDto dto);
    Task UpdateAsync(ProductUpdateDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ISBNExistsAsync(string isbn);              
}