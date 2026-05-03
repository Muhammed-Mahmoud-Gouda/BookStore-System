using ShopNest.BLL.DTOs.Customer;
using ShopNest.BLL.Services.Interfaces;
using ShopNest.DAL.Repositories.Interfaces;
using ShpoNest.Models.Entities;

namespace ShopNest.BLL.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        
        public async Task<IEnumerable<CustomerResultDto>> GetAllAsync()
        {
            var customers = await _unitOfWork.Customers.GetAllWithOrdersAsync();
            return customers.Select(c => MapToResultDto(c));
        }
      
        public async Task<CustomerResultDto?> GetByIdAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdWithAddressesAsync(id)
                ?? throw new Exception($"Customer with id {id} not found");

            return MapToResultDto(customer);
        }
     
        public async Task<CustomerResultDto?> GetByEmailAsync(string email)
        {
            var customer = await _unitOfWork.Customers.GetByEmailAsync(email)
                ?? throw new Exception($"Customer with email {email} not found");

            return MapToResultDto(customer);
        }
       
        public async Task CreateAsync(CustomerCreateDto dto)
        {
            // Check Email Unique
            if (await EmailExistsAsync(dto.Email))
                throw new Exception($"Email {dto.Email} already exists");

            var customer = new Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
        }
      
        public async Task UpdateAsync(CustomerUpdateDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.Id)
                ?? throw new Exception($"Customer with id {dto.Id} not found");

            
            if (dto.Email != customer.Email && await EmailExistsAsync(dto.Email))
                throw new Exception($"Email {dto.Email} already exists");

            customer.FirstName = dto.FirstName;
            customer.LastName = dto.LastName;
            customer.Email = dto.Email;
            customer.Phone = dto.Phone;
            customer.IsActive = dto.IsActive;

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id)
                ?? throw new Exception($"Customer with id {id} not found");

            _unitOfWork.Customers.Delete(customer);
            await _unitOfWork.SaveChangesAsync();
        }
  
        public async Task<bool> ExistsAsync(int id)
            => await _unitOfWork.Customers.ExistsAsync(id);

        public async Task<bool> EmailExistsAsync(string email)
        {
            var customer = await _unitOfWork.Customers.GetByEmailAsync(email);
            return customer != null;
        }

        
        private static CustomerResultDto MapToResultDto(Customer customer)
        {
            return new CustomerResultDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                IsActive = customer.IsActive,
                OrdersCount = customer.Orders?.Count() ?? 0,
                Addresses = customer.Addresses?
                    .Select(a => new CustomerAddressResultDto
                    {
                        Id = a.Id,
                        Label = a.Label,
                        Street = a.Street,
                        City = a.City,
                        PostalCode = a.PostalCode,
                        IsDefault = a.IsDefault,
                    }).ToList() ?? new(),
            };
        }
    }
}