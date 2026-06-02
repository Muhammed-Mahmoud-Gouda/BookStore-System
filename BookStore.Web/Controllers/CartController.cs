using Microsoft.AspNetCore.Mvc;
using ShopNest.BLL.Services.Interfaces;
using ShopNest.BLL.DTOs.Order;
using ShopNest.BLL.DTOs.Customer;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace ShopNest.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICustomerAddressService _customerAddressService;
        private readonly ICustomerService _customerService;

        private const string CartSessionKey = "ShopNestCart";

        public CartController(
            IProductService productService,
            IOrderService orderService,
            ICustomerAddressService customerAddressService,
            ICustomerService customerService)
        {
            _productService = productService;
            _orderService = orderService;
            _customerAddressService = customerAddressService;
            _customerService = customerService;
        }

        public IActionResult Index()
        {
            var cart = GetCartItems();
            return View(cart);
        }

        [HttpPost]
        [Route("Cart/Add/{id}")]
        public async Task<IActionResult> Add(int id, int qty = 1)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null || !product.IsActive)
                {
                    return Json(new { success = false, message = "Book is not available." });
                }

                if (product.Stock < qty)
                {
                    return Json(new { success = false, message = $"Only {product.Stock} copies left in stock." });
                }

                var cart = GetCartItems();
                var cartItem = cart.FirstOrDefault(i => i.ProductId == id);

                if (cartItem != null)
                {
                    if (product.Stock < cartItem.Quantity + qty)
                    {
                        return Json(new { success = false, message = $"Only {product.Stock} copies in stock. You already have {cartItem.Quantity} in cart." });
                    }
                    cartItem.Quantity += qty;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.Id,
                        BookName = product.Name,
                        Author = product.Author ?? "Unknown",
                        Price = product.Price,
                        MainImagePath = product.MainImagePath ?? "/images/default-book.png",
                        Quantity = qty
                    });
                }

                SaveCartItems(cart);
                int totalItems = cart.Sum(i => i.Quantity);

                // Set cart count in session for easy retrieval in layout
                HttpContext.Session.SetInt32("CartCount", totalItems);

                return Json(new { success = true, cartCount = totalItems, message = "Added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int qty)
        {
            if (qty < 1) qty = 1;

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity = qty;
                SaveCartItems(cart);
            }

            int totalItems = cart.Sum(i => i.Quantity);
            HttpContext.Session.SetInt32("CartCount", totalItems);

            decimal subtotal = item != null ? item.Price * item.Quantity : 0;
            decimal total = cart.Sum(i => i.Price * i.Quantity);

            return Json(new { 
                success = true, 
                cartCount = totalItems,
                itemSubtotal = subtotal.ToString("N0"),
                cartTotal = total.ToString("N0")
            });
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(i => i.ProductId == id);
            if (item != null)
            {
                cart.Remove(item);
                SaveCartItems(cart);
            }

            int totalItems = cart.Sum(i => i.Quantity);
            HttpContext.Session.SetInt32("CartCount", totalItems);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var addresses = await _customerAddressService.GetByCustomerIdAsync(userId);
            ViewBag.Addresses = addresses.ToList();
            ViewBag.Cart = cart;
            ViewBag.CartTotal = cart.Sum(i => i.Price * i.Quantity);

            return View(new CheckoutViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                var addresses = await _customerAddressService.GetByCustomerIdAsync(userId);
                ViewBag.Addresses = addresses.ToList();
                ViewBag.Cart = cart;
                ViewBag.CartTotal = cart.Sum(i => i.Price * i.Quantity);
                return View(model);
            }

            try
            {
                int addressId = model.SelectedAddressId;

                // If user opted to add a new address or has no saved address
                if (addressId == 0)
                {
                    var newAddress = new CustomerAddressCreateDto
                    {
                        CustomerId = userId,
                        Label = string.IsNullOrEmpty(model.AddressLabel) ? "Shipping" : model.AddressLabel,
                        Street = model.Street,
                        City = model.City,
                        PostalCode = model.PostalCode,
                        IsDefault = true
                    };

                    await _customerAddressService.CreateAsync(newAddress);

                    // Fetch the default address just created
                    var defaultAddr = await _customerAddressService.GetDefaultAddressAsync(userId);
                    if (defaultAddr != null)
                    {
                        addressId = defaultAddr.Id;
                    }
                    else
                    {
                        // Fallback: get any address
                        var allAddrs = await _customerAddressService.GetByCustomerIdAsync(userId);
                        addressId = allAddrs.LastOrDefault()?.Id ?? 0;
                    }
                }

                if (addressId == 0)
                {
                    ModelState.AddModelError("", "Could not register a valid shipping address. Please select or add an address.");
                    var addresses = await _customerAddressService.GetByCustomerIdAsync(userId);
                    ViewBag.Addresses = addresses.ToList();
                    ViewBag.Cart = cart;
                    ViewBag.CartTotal = cart.Sum(i => i.Price * i.Quantity);
                    return View(model);
                }

                // Map cart items to order items
                var orderItems = cart.Select(i => new OrderItemCreateDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList();

                var orderDto = new OrderCreateDto
                {
                    CustomerId = userId,
                    ShippingAddressId = addressId,
                    Notes = model.Notes,
                    Items = orderItems
                };

                await _orderService.CreateAsync(orderDto);

                // Clear Cart
                HttpContext.Session.Remove(CartSessionKey);
                HttpContext.Session.SetInt32("CartCount", 0);

                TempData["Success"] = "Your order was placed successfully! 🛒🎉";
                return RedirectToAction(nameof(Confirmation));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Order checkout failed: " + ex.Message);
                var addresses = await _customerAddressService.GetByCustomerIdAsync(userId);
                ViewBag.Addresses = addresses.ToList();
                ViewBag.Cart = cart;
                ViewBag.CartTotal = cart.Sum(i => i.Price * i.Quantity);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Confirmation()
        {
            return View();
        }

        // --- Helper Methods ---
        private List<CartItem> GetCartItems()
        {
            var cartSession = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartSession))
            {
                return new List<CartItem>();
            }
            return JsonSerializer.Deserialize<List<CartItem>>(cartSession) ?? new List<CartItem>();
        }

        private void SaveCartItems(List<CartItem> cart)
        {
            var cartSession = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, cartSession);
        }
    }

    // ViewModels used in Cart and Checkout
    public class CartItem
    {
        public int ProductId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string MainImagePath { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }

    public class CheckoutViewModel
    {
        [Display(Name = "Select Shipping Address")]
        public int SelectedAddressId { get; set; }

        [Display(Name = "Address Label (e.g., Home, Office)")]
        public string? AddressLabel { get; set; }

        [Required(ErrorMessage = "Street name is required if adding a new address")]
        [StringLength(200, ErrorMessage = "Street cannot exceed 200 characters")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required if adding a new address")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Postal Code")]
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? PostalCode { get; set; }

        [Display(Name = "Additional Notes (Optional)")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
