using Microsoft.AspNetCore.Mvc;
using ShopNest.BLL.Services.Interfaces;

namespace ShopNest.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public HomeController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var bestsellers = (await _productService.GetActiveAsync())
                .Take(5);

            ViewBag.Bestsellers = bestsellers;
            return View();
        }
    }
}