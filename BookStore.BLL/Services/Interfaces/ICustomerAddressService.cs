using ShopNest.BLL.DTOs.Customer;

namespace ShopNest.BLL.Services.Interfaces
{
    public interface ICustomerAddressService
    {
        Task<IEnumerable<CustomerAddressResultDto>> GetByCustomerIdAsync(int customerId);
        Task<CustomerAddressResultDto?> GetByIdAsync(int id);
        Task<CustomerAddressResultDto?> GetDefaultAddressAsync(int customerId);
        Task CreateAsync(CustomerAddressCreateDto dto);
        Task UpdateAsync(CustomerAddressUpdateDto dto);
        Task DeleteAsync(int id);
        Task SetDefaultAsync(int addressId, int customerId);
    }
}