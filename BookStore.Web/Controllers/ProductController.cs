using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopNest.BLL.DTOs.Product;
using ShopNest.BLL.Services.Interfaces;
using ShopNest.Web.ViewModels.Product;
using ShpoNest.Models.Enums;

using Microsoft.AspNetCore.Authorization;

namespace ShopNest.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

       
        public async Task<IActionResult> Index(
            string? searchTerm,
            int? categoryId,
            bool? isActive,
            int page = 1)
        {
            var products = await _productService.GetAllWithCategoryAsync();
            var categories = await _categoryService.GetAllAsync();

           
            if (!string.IsNullOrEmpty(searchTerm))
                products = products.Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (p.Author != null && p.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (p.ISBN != null && p.ISBN.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));

            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId);

            if (isActive.HasValue)
                products = products.Where(p => p.IsActive == isActive);

           
            const int pageSize = 10;
            var totalCount = products.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var vm = new ProductIndexViewModel
            {
                Products = pagedProducts,
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                IsActive = isActive,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize,
                Categories = new SelectList(categories, "Id", "Name", categoryId),
            };

            return View(vm);
        }

        
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        
        public async Task<IActionResult> Create()
        {
            var vm = await BuildCreateViewModelAsync();
            return View(vm);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await RebuildCreateViewModelAsync(vm);
                return View(vm);
            }

            try
            {
                var dto = MapToCreateDto(vm);
                await _productService.CreateAsync(dto);
                TempData["Success"] = "Product created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await RebuildCreateViewModelAsync(vm);
                return View(vm);
            }
        }

      
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                var vm = await BuildEditViewModelAsync(product);
                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await RebuildEditViewModelAsync(vm);
                return View(vm);
            }

            try
            {
                var dto = MapToUpdateDto(vm);
                await _productService.UpdateAsync(dto);
                TempData["Success"] = "Product updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await RebuildEditViewModelAsync(vm);
                return View(vm);
            }
        }

      
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _productService.DeleteAsync(id);
                TempData["Success"] = "Product deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

       
        private async Task<ProductCreateViewModel> BuildCreateViewModelAsync()
        {
            var categories = await _categoryService.GetAllAsync();
            return new ProductCreateViewModel
            {
                Categories = new SelectList(categories, "Id", "Name"),
                Formats = new SelectList(
                    Enum.GetValues(typeof(BookFormat))
                        .Cast<BookFormat>()
                        .Select(f => new { Id = (int)f, Name = f.ToString() }),
                    "Id", "Name"),
            };
        }

       
        private async Task RebuildCreateViewModelAsync(ProductCreateViewModel vm)
        {
            var categories = await _categoryService.GetAllAsync();
            vm.Categories = new SelectList(categories, "Id", "Name", vm.CategoryId);
            vm.Formats = new SelectList(
                Enum.GetValues(typeof(BookFormat))
                    .Cast<BookFormat>()
                    .Select(f => new { Id = (int)f, Name = f.ToString() }),
                "Id", "Name", (int?)vm.Format);
        }

      
        private async Task<ProductEditViewModel> BuildEditViewModelAsync(ProductResultDto product)
        {
            var categories = await _categoryService.GetAllAsync();
            return new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive,
                Author = product.Author,
                Publisher = product.Publisher,
                ISBN = product.ISBN,
                PublicationYear = product.PublicationYear,
                Pages = product.Pages,
                Language = product.Language,
                Edition = product.Edition,
                Format = product.Format,
                ExistingImages = product.Images,
                Categories = new SelectList(categories, "Id", "Name", product.CategoryId),
                Formats = new SelectList(
                    Enum.GetValues(typeof(BookFormat))
                        .Cast<BookFormat>()
                        .Select(f => new { Id = (int)f, Name = f.ToString() }),
                    "Id", "Name", (int?)product.Format),
            };
        }

     
        private async Task RebuildEditViewModelAsync(ProductEditViewModel vm)
        {
            var categories = await _categoryService.GetAllAsync();
            vm.Categories = new SelectList(categories, "Id", "Name", vm.CategoryId);
            vm.Formats = new SelectList(
                Enum.GetValues(typeof(BookFormat))
                    .Cast<BookFormat>()
                    .Select(f => new { Id = (int)f, Name = f.ToString() }),
                "Id", "Name", (int?)vm.Format);

            // Reload Existing Images
            var product = await _productService.GetByIdAsync(vm.Id);
            vm.ExistingImages = product.Images;
        }

       
        private static ProductCreateDto MapToCreateDto(ProductCreateViewModel vm)
        {
            return new ProductCreateDto
            {
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                Stock = vm.Stock,
                CategoryId = vm.CategoryId,
                Author = vm.Author,
                Publisher = vm.Publisher,
                ISBN = vm.ISBN,
                PublicationYear = vm.PublicationYear,
                Pages = vm.Pages,
                Language = vm.Language,
                Edition = vm.Edition,
                Format = vm.Format,
                Images = vm.Images,
            };
        }

       
        private static ProductUpdateDto MapToUpdateDto(ProductEditViewModel vm)
        {
            return new ProductUpdateDto
            {
                Id = vm.Id,
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                Stock = vm.Stock,
                CategoryId = vm.CategoryId,
                IsActive = vm.IsActive,
                Author = vm.Author,
                Publisher = vm.Publisher,
                ISBN = vm.ISBN,
                PublicationYear = vm.PublicationYear,
                Pages = vm.Pages,
                Language = vm.Language,
                Edition = vm.Edition,
                Format = vm.Format,
                NewImages = vm.NewImages,
                RemovedImageIds = vm.RemovedImageIds,
            };
        }
    }
}