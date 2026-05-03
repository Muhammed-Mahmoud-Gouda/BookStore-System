using ShopNest.BLL.DTOs.Customer;
using ShopNest.BLL.Services.Interfaces;
using ShopNest.DAL.Repositories.Interfaces;
using ShpoNest.Models.Entities;

namespace ShopNest.BLL.Services.Implementations
{
    public class CustomerAddressService : ICustomerAddressService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerAddressService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        
        public async Task<IEnumerable<CustomerAddressResultDto>> GetByCustomerIdAsync(int customerId)
        {
            
            var customerExists = await _unitOfWork.Customers.ExistsAsync(customerId);
            if (!customerExists)
                throw new Exception($"Customer with id {customerId} not found");

            var addresses = await _unitOfWork.CustomerAddresses.GetByCustomerIdAsync(customerId);
            return addresses.Select(a => MapToResultDto(a));
        }

       
        public async Task<CustomerAddressResultDto?> GetByIdAsync(int id)
        {
            var address = await _unitOfWork.CustomerAddresses.GetByIdAsync(id)
                ?? throw new Exception($"Address with id {id} not found");

            return MapToResultDto(address);
        }

        
        public async Task<CustomerAddressResultDto?> GetDefaultAddressAsync(int customerId)
        {
            var address = await _unitOfWork.CustomerAddresses.GetDefaultAddressAsync(customerId);

            
            if (address == null) return null;

            return MapToResultDto(address);
        }

       
        public async Task CreateAsync(CustomerAddressCreateDto dto)
        {
            
            var customerExists = await _unitOfWork.Customers.ExistsAsync(dto.CustomerId);
            if (!customerExists)
                throw new Exception($"Customer with id {dto.CustomerId} not found");

            
            if (dto.IsDefault)
                await RemoveCurrentDefaultAsync(dto.CustomerId);

            var address = new CustomerAddress
            {
                CustomerId = dto.CustomerId,
                Label = dto.Label,
                Street = dto.Street,
                City = dto.City,
                PostalCode = dto.PostalCode,
                IsDefault = dto.IsDefault,
                CreatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.CustomerAddresses.AddAsync(address);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(CustomerAddressUpdateDto dto)
        {
            var address = await _unitOfWork.CustomerAddresses.GetByIdAsync(dto.Id)
                ?? throw new Exception($"Address with id {dto.Id} not found");

            
            if (dto.IsDefault && !address.IsDefault)
                await RemoveCurrentDefaultAsync(dto.CustomerId);

            address.Label = dto.Label;
            address.Street = dto.Street;
            address.City = dto.City;
            address.PostalCode = dto.PostalCode;
            address.IsDefault = dto.IsDefault;

            _unitOfWork.CustomerAddresses.Update(address);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var address = await _unitOfWork.CustomerAddresses.GetByIdAsync(id)
                ?? throw new Exception($"Address with id {id} not found");

            
            if (address.IsDefault)
            {
                var addresses = await _unitOfWork.CustomerAddresses
                    .GetByCustomerIdAsync(address.CustomerId);

                var newDefault = addresses.FirstOrDefault(a => a.Id != id);
                if (newDefault != null)
                {
                    newDefault.IsDefault = true;
                    _unitOfWork.CustomerAddresses.Update(newDefault);
                }
            }

            _unitOfWork.CustomerAddresses.Delete(address);
                await _unitOfWork.SaveChangesAsync();
        }

        public async Task SetDefaultAsync(int addressId, int customerId)
        {
            var address = await _unitOfWork.CustomerAddresses.GetByIdAsync(addressId)
                ?? throw new Exception($"Address with id {addressId} not found");

            
            await RemoveCurrentDefaultAsync(customerId);

            
            address.IsDefault = true;
            _unitOfWork.CustomerAddresses.Update(address);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task RemoveCurrentDefaultAsync(int customerId)
        {
            var currentDefault = await _unitOfWork.CustomerAddresses
                .GetDefaultAddressAsync(customerId);

            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
                _unitOfWork.CustomerAddresses.Update(currentDefault);
            }
        }

        private static CustomerAddressResultDto MapToResultDto(CustomerAddress address)
        {
            return new CustomerAddressResultDto
            {
                Id = address.Id,
                Label = address.Label,
                Street = address.Street,
                City = address.City,
                PostalCode = address.PostalCode,
                IsDefault = address.IsDefault,
            };
        }
    }
}