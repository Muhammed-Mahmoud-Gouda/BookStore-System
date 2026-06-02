using Microsoft.AspNetCore.Mvc;
using ShopNest.BLL.Services.Interfaces;
using ShopNest.Web.ViewModels.Product;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ShopNest.Web.Controllers
{
    public class ShopController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ShopController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(
            string? q,
            int? categoryId,
            string? author,
            string? sortBy,
            int page = 1)
        {
            var allProducts = await _productService.GetAllWithCategoryAsync();
            var activeProducts = allProducts.Where(p => p.IsActive);
            var categories = await _categoryService.GetAllAsync();

            // Search
            if (!string.IsNullOrEmpty(q))
            {
                activeProducts = activeProducts.Where(p =>
                    p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (p.Author != null && p.Author.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Description != null && p.Description.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (p.ISBN != null && p.ISBN.Contains(q, StringComparison.OrdinalIgnoreCase)));
            }

            // Category Filter
            if (categoryId.HasValue)
            {
                activeProducts = activeProducts.Where(p => p.CategoryId == categoryId);
            }

            // Author Filter
            if (!string.IsNullOrEmpty(author))
            {
                activeProducts = activeProducts.Where(p => p.Author != null && p.Author.Equals(author, StringComparison.OrdinalIgnoreCase));
            }

            // Sorting
            activeProducts = sortBy switch
            {
                "price_asc" => activeProducts.OrderBy(p => p.Price),
                "price_desc" => activeProducts.OrderByDescending(p => p.Price),
                "newest" => activeProducts.OrderByDescending(p => p.Id),
                _ => activeProducts.OrderBy(p => p.Name)
            };

            // Pagination
            const int pageSize = 12;
            var totalCount = activeProducts.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var pagedProducts = activeProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Categories = categories.Where(c => c.IsActive).ToList();
            ViewBag.SelectedCategoryName = categoryId.HasValue 
                ? categories.FirstOrDefault(c => c.Id == categoryId)?.Name 
                : "All Categories";
            
            ViewBag.SearchQuery = q;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedSortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(pagedProducts);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null || !product.IsActive)
                {
                    TempData["Error"] = "Product not found or is unavailable.";
                    return RedirectToAction(nameof(Index));
                }

                // Get related books (same category, excluding current book)
                var allProducts = await _productService.GetAllWithCategoryAsync();
                var relatedBooks = allProducts
                    .Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.Id != product.Id)
                    .Take(4)
                    .ToList();

                ViewBag.RelatedBooks = relatedBooks;
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading book details: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
