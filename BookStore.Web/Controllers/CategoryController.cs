using Microsoft.AspNetCore.Mvc;
using ShopNest.BLL.DTOs.Category;
using ShopNest.BLL.Services.Interfaces;
using ShopNest.Web.ViewModels.Category;

using Microsoft.AspNetCore.Authorization;

namespace ShopNest.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }


        public async Task<IActionResult> Index(string? searchTerm)
        {
            var categories = string.IsNullOrEmpty(searchTerm)
                ? await _categoryService.GetAllAsync()
                : await _categoryService.SearchAsync(searchTerm);

            var vm = new CategoryIndexViewModel
            {
                Categories = categories,
                SearchTerm = searchTerm,
                TotalCount = categories.Count(),
            };

            return View(vm);
        }


        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

      
        public IActionResult Create()
        {
            return View(new CategoryCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var dto = new CategoryCreateDto
                {
                    Name = vm.Name,
                    Description = vm.Description,
                    Image = vm.Image,
                };

                await _categoryService.CreateAsync(dto);
                TempData["Success"] = "Category created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);

                var vm = new CategoryEditViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ExistingImagePath = category.ImagePath,
                    IsActive = category.IsActive,
                };

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
        public async Task<IActionResult> Edit(CategoryEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var dto = new CategoryUpdateDto
                {
                    Id = vm.Id,
                    Name = vm.Name,
                    Description = vm.Description,
                    ExistingImagePath = vm.ExistingImagePath,
                    Image = vm.Image,
                };

                await _categoryService.UpdateAsync(dto);
                TempData["Success"] = "Category updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                return View(category);
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
                await _categoryService.DeleteAsync(id);
                TempData["Success"] = "Category deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}