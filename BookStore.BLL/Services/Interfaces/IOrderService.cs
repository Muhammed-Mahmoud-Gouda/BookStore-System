using ShopNest.BLL.DTOs.Order;
using ShpoNest.Models.Enums;

namespace ShopNest.BLL.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResultDto>> GetAllAsync();
        Task<IEnumerable<OrderResultDto>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<OrderResultDto>> GetByStatusAsync(OrderStatus status);
        Task<OrderResultDto?> GetByIdAsync(int id);
        Task CreateAsync(OrderCreateDto dto);
        Task UpdateStatusAsync(OrderUpdateDto dto);
        Task CancelAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}