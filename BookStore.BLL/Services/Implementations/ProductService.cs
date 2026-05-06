using ShopNest.BLL.DTOs.Product;
using ShopNest.BLL.Helper;
using ShopNest.DAL.Repositories.Interfaces;
using ShpoNest.Models.Entities;

namespace ShopNest.BLL.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private const string FolderName = "Files/Products";

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        
        public async Task<IEnumerable<ProductResultDto>> GetAllAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return products.Select(p => MapToResultDto(p));
        }
       
        public async Task<IEnumerable<ProductResultDto>> GetAllWithCategoryAsync()
        {
            var products = await _unitOfWork.Products.GetAllWithCategoryAsync();
            return products.Select(p => MapToResultDto(p));
        }
       
        public async Task<IEnumerable<ProductResultDto>> GetByCategoryAsync(int categoryId)
        {
            var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);
            return products.Select(p => MapToResultDto(p));
        }
       
        public async Task<IEnumerable<ProductResultDto>> GetActiveAsync()
        {
            var products = await _unitOfWork.Products.GetAllWithCategoryAsync();
            return products
                .Where(p => p.IsActive)
                .Select(p => MapToResultDto(p));
        }
      
        public async Task<IEnumerable<ProductResultDto>> GetLowStockAsync(int threshold = 10)
        {
            var products = await _unitOfWork.Products.GetLowStockAsync(threshold);
            return products.Select(p => MapToResultDto(p));
        }
       
        public async Task<ProductResultDto?> GetByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdWithImagesAsync(id)
                ?? throw new Exception($"Product with id {id} not found");

            return MapToResultDto(product);
        }

        public async Task<ProductResultDto?> GetByISBNAsync(string isbn)
        {
            var product = await _unitOfWork.Products.GetByISBNAsync(isbn)
                ?? throw new Exception($"Product with ISBN {isbn} not found");
            return MapToResultDto(product);
        }

        public async Task<bool> ISBNExistsAsync(string isbn)
                    => await _unitOfWork.Products.ISBNExistsAsync(isbn);

        public async Task CreateAsync(ProductCreateDto dto)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Check ISBN Unique
                if (!string.IsNullOrEmpty(dto.ISBN) && await ISBNExistsAsync(dto.ISBN))
                    throw new Exception($"ISBN {dto.ISBN} already exists");

                var product = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Stock = dto.Stock,
                    CategoryId = dto.CategoryId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Author = dto.Author,
                    Publisher = dto.Publisher,
                    ISBN = dto.ISBN,
                    PublicationYear = dto.PublicationYear,
                    Pages = dto.Pages,
                    Language = dto.Language,
                    Edition = dto.Edition,
                    Format = dto.Format,
                };

                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                // Upload Images
                if (dto.Images != null && dto.Images.Any())
                {
                    bool isFirst = true;
                    foreach (var image in dto.Images)
                    {
                        var fileName = await UploadHelper.UploadFileAsync(FolderName, image);

                        var productImage = new ProductImages
                        {
                            ProductId = product.Id,
                            ImagePath = fileName,
                            IsMain = isFirst,
                            DisplayOrder = dto.Images.IndexOf(image),
                            CreatedAt = DateTime.UtcNow,
                        };

                        await _unitOfWork.ProductImages.AddAsync(productImage);
                        isFirst = false;
                    }

                    await _unitOfWork.SaveChangesAsync();
                }
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(ProductUpdateDto dto)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                Product? product = await _unitOfWork.Products.GetByIdWithImagesAsync(dto.Id)
               ?? throw new Exception($"Product with id {dto.Id} not found");


                if (!string.IsNullOrEmpty(dto.ISBN) &&
                    dto.ISBN != product.ISBN &&
                    await ISBNExistsAsync(dto.ISBN))
                    throw new Exception($"ISBN {dto.ISBN} already exists");

                // Update Fields
                product.Name = dto.Name;
                product.Description = dto.Description;
                product.Price = dto.Price;
                product.Stock = dto.Stock;
                product.CategoryId = dto.CategoryId;
                product.IsActive = dto.IsActive;
                product.Author = dto.Author;
                product.Publisher = dto.Publisher;
                product.ISBN = dto.ISBN;
                product.PublicationYear = dto.PublicationYear;
                product.Pages = dto.Pages;
                product.Language = dto.Language;
                product.Edition = dto.Edition;
                product.Format = dto.Format;

                // Remove Selected Images
                if (dto.RemovedImageIds != null && dto.RemovedImageIds.Any())
                {
                    foreach (var imageId in dto.RemovedImageIds)
                    {
                        var image = await _unitOfWork.ProductImages.GetByIdAsync(imageId);
                        if (image != null)
                        {
                            UploadHelper.RemoveFile(FolderName, image.ImagePath);
                            _unitOfWork.ProductImages.Delete(image);
                        }
                    }
                }

                // Upload New Images
                if (dto.NewImages != null && dto.NewImages.Any())
                {
                    var existingCount = product.Images?.Count ?? 0;
                    bool isFirst = existingCount == 0;

                    foreach (var image in dto.NewImages)
                    {
                        var fileName = await UploadHelper.UploadFileAsync(FolderName, image);

                        var productImage = new ProductImages
                        {
                            ProductId = product.Id,
                            ImagePath = fileName,
                            IsMain = isFirst,
                            DisplayOrder = existingCount + dto.NewImages.IndexOf(image),
                            CreatedAt = DateTime.UtcNow,
                        };

                        await _unitOfWork.ProductImages.AddAsync(productImage);
                        isFirst = false;
                    }
                }

                _unitOfWork.Products.Update(product);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdWithImagesAsync(id)
                ?? throw new Exception($"Product with id {id} not found");

            // Remove All Images
            if (product.Images != null && product.Images.Any())
            {
                foreach (var image in product.Images)
                    UploadHelper.RemoveFile(FolderName, image.ImagePath);
            }

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.SaveChangesAsync();
        }
      
        public async Task<bool> ExistsAsync(int id)
            => await _unitOfWork.Products.ExistsAsync(id);

        private static ProductResultDto MapToResultDto(Product product)
        {
            return new ProductResultDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                Author = product.Author,
                Publisher = product.Publisher,
                ISBN = product.ISBN,
                PublicationYear = product.PublicationYear,
                Pages = product.Pages,
                Language = product.Language,
                Edition = product.Edition,
                Format = product.Format,
                MainImagePath = product.Images?
                    .FirstOrDefault(i => i.IsMain)?.ImagePath,
                Images = product.Images?
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new ProductImageResultDto
                    {
                        Id = i.Id,
                        ImagePath = i.ImagePath,
                        IsMain = i.IsMain,
                        DisplayOrder = i.DisplayOrder,
                    }).ToList() ?? new(),
            };
        }
    }
}