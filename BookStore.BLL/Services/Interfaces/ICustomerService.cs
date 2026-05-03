using ShopNest.BLL.DTOs.Customer;

namespace ShopNest.BLL.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerResultDto>> GetAllAsync();
        Task<CustomerResultDto?> GetByIdAsync(int id);
        Task<CustomerResultDto?> GetByEmailAsync(string email);
        Task CreateAsync(CustomerCreateDto dto);
        Task UpdateAsync(CustomerUpdateDto dto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email);
    }
}