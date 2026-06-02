using Microsoft.AspNetCore.Mvc;
using ShopNest.BLL.DTOs.Customer;
using ShopNest.BLL.Services.Interfaces;
using ShopNest.Web.ViewModels.Customer;

using Microsoft.AspNetCore.Authorization;

namespace ShopNest.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerAddressService _addressService;

        public CustomerController(
            ICustomerService customerService,
            ICustomerAddressService addressService)
        {
            _customerService = customerService;
            _addressService = addressService;
        }

        
        public async Task<IActionResult> Index(
            string? searchTerm,
            bool? isActive,
            int page = 1)
        {
            var customers = await _customerService.GetAllAsync();

           
            if (!string.IsNullOrEmpty(searchTerm))
                customers = customers.Where(c =>
                    c.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (c.Phone != null && c.Phone.Contains(searchTerm)));

            if (isActive.HasValue)
                customers = customers.Where(c => c.IsActive == isActive);

           
            const int pageSize = 10;
            var totalCount = customers.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var paged = customers
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var vm = new CustomerIndexViewModel
            {
                Customers = paged,
                SearchTerm = searchTerm,
                IsActive = isActive,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize,
            };

            return View(vm);
        }

        
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var customer = await _customerService.GetByIdAsync(id);
                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        
        public IActionResult Create()
        {
            return View(new CustomerCreateViewModel());
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var dto = new CustomerCreateDto
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Email = vm.Email,
                    Phone = vm.Phone,
                };

                await _customerService.CreateAsync(dto);
                TempData["Success"] = "Customer created successfully";
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
                var customer = await _customerService.GetByIdAsync(id);

                var vm = new CustomerEditViewModel
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    IsActive = customer.IsActive,
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
        public async Task<IActionResult> Edit(CustomerEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var dto = new CustomerUpdateDto
                {
                    Id = vm.Id,
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Email = vm.Email,
                    Phone = vm.Phone,
                    IsActive = vm.IsActive,
                };

                await _customerService.UpdateAsync(dto);
                TempData["Success"] = "Customer updated successfully";
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
                var customer = await _customerService.GetByIdAsync(id);
                return View(customer);
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
                await _customerService.DeleteAsync(id);
                TempData["Success"] = "Customer deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

       
        public async Task<IActionResult> Addresses(int customerId)
        {
            try
            {
                var customer = await _customerService.GetByIdAsync(customerId);
                var addresses = await _addressService.GetByCustomerIdAsync(customerId);

                ViewBag.CustomerId = customerId;
                ViewBag.CustomerName = customer.FullName;

                return View(addresses);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        
        public IActionResult CreateAddress(int customerId)
        {
            var vm = new CustomerAddressCreateViewModel
            {
                CustomerId = customerId,
            };
            return View(vm);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAddress(CustomerAddressCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var dto = new CustomerAddressCreateDto
                {
                    CustomerId = vm.CustomerId,
                    Label = vm.Label,
                    Street = vm.Street,
                    City = vm.City,
                    PostalCode = vm.PostalCode,
                    IsDefault = vm.IsDefault,
                };

                await _addressService.CreateAsync(dto);
                TempData["Success"] = "Address added successfully";
                return RedirectToAction(nameof(Addresses), new { customerId = vm.CustomerId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

       
        public async Task<IActionResult> EditAddress(int id, int customerId)
        {
            try
            {
                var address = await _addressService.GetByIdAsync(id);

                var vm = new CustomerAddressEditViewModel
                {
                    Id = address.Id,
                    CustomerId = customerId,
                    Label = address.Label,
                    Street = address.Street,
                    City = address.City,
                    PostalCode = address.PostalCode,
                    IsDefault = address.IsDefault,
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Addresses), new { customerId });
            }
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(CustomerAddressEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var dto = new CustomerAddressUpdateDto
                {
                    Id = vm.Id,
                    CustomerId = vm.CustomerId,
                    Label = vm.Label,
                    Street = vm.Street,
                    City = vm.City,
                    PostalCode = vm.PostalCode,
                    IsDefault = vm.IsDefault,
                };

                await _addressService.UpdateAsync(dto);
                TempData["Success"] = "Address updated successfully";
                return RedirectToAction(nameof(Addresses), new { customerId = vm.CustomerId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int id, int customerId)
        {
            try
            {
                await _addressService.DeleteAsync(id);
                TempData["Success"] = "Address deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Addresses), new { customerId });
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefaultAddress(int addressId, int customerId)
        {
            try
            {
                await _addressService.SetDefaultAsync(addressId, customerId);
                TempData["Success"] = "Default address updated successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Addresses), new { customerId });
        }
    }
}