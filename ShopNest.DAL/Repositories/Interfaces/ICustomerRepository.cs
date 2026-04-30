using ShpoNest.Models.Entities;

namespace ShopNest.DAL.Repositories.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<Customer?> GetByIdWithAddressesAsync(int id);
        Task<Customer?> GetByEmailAsync(string email);
        Task<IEnumerable<Customer>> GetAllWithOrdersAsync();
    }
}