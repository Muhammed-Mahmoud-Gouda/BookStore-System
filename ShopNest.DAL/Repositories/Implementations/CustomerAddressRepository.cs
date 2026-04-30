using Microsoft.EntityFrameworkCore;
using ShopNest.DAL.ApplicationDbContext;
using ShopNest.DAL.Repositories.Interfaces;
using ShpoNest.Models.Entities;


namespace ShopNest.DAL.Repositories.Implementations
{
    public class CustomerAddressRepository : GenericRepository<CustomerAddress>, ICustomerAddressRepository
    {
        public CustomerAddressRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<CustomerAddress>> GetByCustomerIdAsync(int customerId)
    => await _dbSet
        .Where(a => a.CustomerId == customerId)
        .OrderByDescending(a => a.IsDefault)
        .ToListAsync();

        public async Task<CustomerAddress?> GetDefaultAddressAsync(int customerId)
            => await _dbSet
                .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.IsDefault);
    }
}
