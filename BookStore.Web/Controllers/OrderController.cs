using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopNest.BLL.DTOs.Order;
using ShopNest.BLL.Services.Interfaces;
using ShopNest.Web.ViewModels.Order;
using ShpoNest.Models.Enums;

using Microsoft.AspNetCore.Authorization;

namespace ShopNest.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ICustomerAddressService _addressService;

        public OrderController(
            IOrderService orderService,
            ICustomerService customerService,
            IProductService productService,
            ICustomerAddressService addressService)
        {
            _orderService = orderService;
            _customerService = customerService;
            _productService = productService;
            _addressService = addressService;
        }

        
        public async Task<IActionResult> Index(
            string? searchTerm,
            OrderStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1)
        {
            var orders = await _orderService.GetAllAsync();

       
            if (!string.IsNullOrEmpty(searchTerm))
                orders = orders.Where(o =>
                    o.CustomerName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    o.Id.ToString().Contains(searchTerm));

            if (status.HasValue)
                orders = orders.Where(o => o.Status == status);

            if (fromDate.HasValue)
                orders = orders.Where(o => o.OrderDate >= fromDate);

            if (toDate.HasValue)
                orders = orders.Where(o => o.OrderDate <= toDate);

       
            const int pageSize = 10;
            var totalCount = orders.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var paged = orders
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var vm = new OrderIndexViewModel
            {
                Orders = paged,
                SearchTerm = searchTerm,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize,
                Statuses = new SelectList(
                    Enum.GetValues(typeof(OrderStatus))
                        .Cast<OrderStatus>()
                        .Select(s => new { Id = (int)s, Name = s.ToString() }),
                    "Id", "Name", (int?)status),
            };

            return View(vm);
        }

     
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);

                var vm = new OrderDetailsViewModel
                {
                    Id = order.Id,
                    CustomerName = order.CustomerName,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    ShippingStreet = order.ShippingStreet,
                    ShippingCity = order.ShippingCity,
                    ShippingPostalCode = order.ShippingPostalCode,
                    Notes = order.Notes,
                    Items = order.Items,
                };

                return View(vm);
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
        public async Task<IActionResult> Create(OrderCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await RebuildCreateViewModelAsync(vm);
                return View(vm);
            }

            try
            {
                var dto = new OrderCreateDto
                {
                    CustomerId = vm.CustomerId,
                    ShippingAddressId = vm.ShippingAddressId,
                    Notes = vm.Notes,
                    Items = vm.Items.Select(i => new OrderItemCreateDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                    }).ToList(),
                };

                await _orderService.CreateAsync(dto);
                TempData["Success"] = "Order created successfully";
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
                var order = await _orderService.GetByIdAsync(id);

                var vm = new OrderEditViewModel
                {
                    Id = order.Id,
                    Status = order.Status,
                    Notes = order.Notes,
                    CustomerName = order.CustomerName,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    ShippingStreet = order.ShippingStreet,
                    ShippingCity = order.ShippingCity,
                    ShippingPostalCode = order.ShippingPostalCode,
                    Items = order.Items,
                    Statuses = BuildStatusSelectList(order.Status),
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
        public async Task<IActionResult> Edit(OrderEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Statuses = BuildStatusSelectList(vm.Status);
                return View(vm);
            }

            try
            {
                var dto = new OrderUpdateDto
                {
                    Id = vm.Id,
                    Status = vm.Status,
                    Notes = vm.Notes,
                };

                await _orderService.UpdateStatusAsync(dto);
                TempData["Success"] = "Order status updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                vm.Statuses = BuildStatusSelectList(vm.Status);
                return View(vm);
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _orderService.CancelAsync(id);
                TempData["Success"] = "Order cancelled successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAddressesByCustomer(int customerId)
        {
            try
            {
                var addresses = await _addressService.GetByCustomerIdAsync(customerId);
                return Json(addresses.Select(a => new
                {
                    id = a.Id,
                    label = $"{a.Label} — {a.Street}, {a.City}",
                }));
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        
        private async Task<OrderCreateViewModel> BuildCreateViewModelAsync()
        {
            var customers = await _customerService.GetAllAsync();
            var products = await _productService.GetActiveAsync();

            return new OrderCreateViewModel
            {
                Customers = new SelectList(customers, "Id", "FullName"),
                Products = new SelectList(products, "Id", "Name"),
                Items = new List<OrderItemViewModel> { new() },
            };
        }

        
        private async Task RebuildCreateViewModelAsync(OrderCreateViewModel vm)
        {
            var customers = await _customerService.GetAllAsync();
            var products = await _productService.GetActiveAsync();
            var addresses = vm.CustomerId > 0
                ? await _addressService.GetByCustomerIdAsync(vm.CustomerId)
                : new List<ShopNest.BLL.DTOs.Customer.CustomerAddressResultDto>();

            vm.Customers = new SelectList(customers, "Id", "FullName", vm.CustomerId);
            vm.Products = new SelectList(products, "Id", "Name");
            vm.Addresses = new SelectList(addresses, "Id", "Street", vm.ShippingAddressId);
        }

       
        private static SelectList BuildStatusSelectList(OrderStatus selected)
        {
            return new SelectList(
                Enum.GetValues(typeof(OrderStatus))
                    .Cast<OrderStatus>()
                    .Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id", "Name", (int)selected);
        }
    }
}