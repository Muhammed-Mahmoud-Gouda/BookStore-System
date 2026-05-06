using ShopNest.BLL.DTOs.Category;
using ShopNest.BLL.Helper;
using ShopNest.BLL.Helpers;
using ShopNest.BLL.Services.Interfaces;
using ShopNest.DAL.Repositories.Interfaces;
using ShpoNest.Models.Entities;

namespace ShopNest.BLL.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private const string FolderName = "Files/Categories";

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
       
        public async Task<IEnumerable<CategoryResultDto>> GetAllAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllWithProductsAsync();
            return categories.Select(c => MapToResultDto(c));
        }
        
        public async Task<IEnumerable<CategoryResultDto>> GetAllWithProductsAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllWithProductsAsync();
            return categories.Select(c => MapToResultDto(c));
        }
        
        public async Task<CategoryResultDto?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id)
                ?? throw new Exception($"Category with id {id} not found");

            return MapToResultDto(category);
        }
        
        public async Task CreateAsync(CategoryCreateDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            // Upload Image
            if (dto.Image != null)
                category.ImagePath = await UploadHelper.UploadFileAsync(FolderName, dto.Image);

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }
        
        public async Task UpdateAsync(CategoryUpdateDto dto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.Id)
                ?? throw new Exception($"Category with id {dto.Id} not found");

            category.Name = dto.Name;
            category.Description = dto.Description;

            // Upload New Image
            if (dto.Image != null)
            {
                // Remove Old Image
                if (!string.IsNullOrEmpty(category.ImagePath))
                    UploadHelper.RemoveFile(FolderName, category.ImagePath);

                category.ImagePath = await UploadHelper.UploadFileAsync(FolderName, dto.Image);
            }

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();
        }
      
        public async Task DeleteAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id)
                ?? throw new Exception($"Category with id {id} not found");

            
            if (!string.IsNullOrEmpty(category.ImagePath))
                UploadHelper.RemoveFile(FolderName, category.ImagePath);

            _unitOfWork.Categories.Delete(category);
            await _unitOfWork.SaveChangesAsync();
        }
       
        public async Task<bool> ExistsAsync(int id)
            => await _unitOfWork.Categories.ExistsAsync(id);

        
        private static CategoryResultDto MapToResultDto(Category category)
        {
            return new CategoryResultDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImagePath = category.ImagePath,
                IsActive = category.IsActive,
                ProductsCount = category.Products?.Count() ?? 0,
            };
        }
    }
}