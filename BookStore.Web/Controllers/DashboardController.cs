using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ShopNest.BLL.Services.Interfaces;
using ShpoNest.Models.Enums;

namespace ShopNest.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;

        public DashboardController(
            IProductService productService,
            ICategoryService categoryService,
            ICustomerService customerService,
            IOrderService orderService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _customerService = customerService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();
            var customers = await _customerService.GetAllAsync();
            var orders = await _orderService.GetAllAsync();

            // Stats
            ViewBag.TotalProducts = products.Count();
            ViewBag.TotalCategories = categories.Count();
            ViewBag.TotalCustomers = customers.Count();
            ViewBag.TotalOrders = orders.Count();

            // Revenue calculation (ignore cancelled orders)
            var validOrders = orders.Where(o => o.Status != OrderStatus.Cancelled);
            ViewBag.TotalRevenue = validOrders.Sum(o => o.TotalAmount);

            // Recent Lists
            ViewBag.RecentOrders = orders
                .OrderByDescending(o => o.Id)
                .Take(5)
                .ToList();

            ViewBag.RecentProducts = products
                .OrderByDescending(p => p.Id)
                .Take(5)
                .ToList();

            return View();
        }
    }
}
