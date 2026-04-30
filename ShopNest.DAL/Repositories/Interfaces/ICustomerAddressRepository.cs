using ShpoNest.Models.Entities;

namespace ShopNest.DAL.Repositories.Interfaces
{
    public interface ICustomerAddressRepository : IGenericRepository<CustomerAddress>
    {
        Task<IEnumerable<CustomerAddress>> GetByCustomerIdAsync(int customerId);
        Task<CustomerAddress?> GetDefaultAddressAsync(int customerId);
    }
}