using Microsoft.AspNetCore.Mvc;
using ShopNest.BLL.Services.Interfaces;
using ShopNest.BLL.DTOs.Customer;
using ShopNest.Web.ViewModels.Account;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ShopNest.BLL.Helper;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerAddressService _addressService;
        private readonly IOrderService _orderService;

        public AccountController(
            ICustomerService customerService,
            ICustomerAddressService addressService,
            IOrderService orderService)
        {
            _customerService = customerService;
            _addressService = addressService;
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index" , "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var customer = await _customerService.GetByEmailAsync(model.Email);
                if (customer == null || !PasswordHasher.VerifyPassword(model.Password, customer.PasswordHash))
                {
                    ModelState.AddModelError("", "Incorrect email or password.");
                    return View(model);
                }

                if (!customer.IsActive)
                {
                    ModelState.AddModelError("", "Your account has been deactivated. Please contact support.");
                    return View(model);
                }

                // Sign in the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                    new Claim(ClaimTypes.Name, customer.FullName),
                    new Claim(ClaimTypes.Email, customer.Email),
                    new Claim(ClaimTypes.Role, customer.IsAdmin ? "Admin" : "Customer")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                TempData["Success"] = "Welcome back, " + customer.FirstName + "! 👋";

                if (customer.IsAdmin)
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Incorrect email or password.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (await _customerService.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                    return View(model);
                }

                var customerDto = new CustomerCreateDto
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = model.Password
                };

                await _customerService.CreateAsync(customerDto);

                // Auto-login after registration
                var newCustomer = await _customerService.GetByEmailAsync(model.Email);
                if (newCustomer != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, newCustomer.Id.ToString()),
                        new Claim(ClaimTypes.Name, newCustomer.FullName),
                        new Claim(ClaimTypes.Email, newCustomer.Email),
                        new Claim(ClaimTypes.Role, "Customer")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    TempData["Success"] = "Welcome to ShopNest, " + newCustomer.FirstName + "! 📚🎉";
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Registration failed: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["Success"] = "You have logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            ViewBag.Message = "If this email exists, a password reset link has been sent.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction(nameof(Login));
            }

            try
            {
                var customer = await _customerService.GetByIdAsync(userId);
                var orders = await _orderService.GetByCustomerIdAsync(userId);

                ViewBag.Orders = orders.OrderByDescending(o => o.OrderDate).ToList();
                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading profile: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(string label, string street, string city, string? postalCode)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(city))
            {
                TempData["Error"] = "Street and City are required fields.";
                return RedirectToAction(nameof(Profile));
            }

            try
            {
                var dto = new CustomerAddressCreateDto
                {
                    CustomerId = userId,
                    Label = string.IsNullOrWhiteSpace(label) ? "Shipping" : label,
                    Street = street,
                    City = city,
                    PostalCode = postalCode,
                    IsDefault = false
                };

                await _addressService.CreateAsync(dto);
                TempData["Success"] = "Address added successfully! 📍";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to add address: " + ex.Message;
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction(nameof(Login));
            }

            try
            {
                var address = await _addressService.GetByIdAsync(id);
                if (address != null)
                {
                    await _addressService.DeleteAsync(id);
                    TempData["Success"] = "Address deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete address: " + ex.Message;
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction(nameof(Login));
            }

            try
            {
                await _addressService.SetDefaultAsync(id, userId);
                TempData["Success"] = "Default address updated.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to set default address: " + ex.Message;
            }

            return RedirectToAction(nameof(Profile));
        }
    }
}
