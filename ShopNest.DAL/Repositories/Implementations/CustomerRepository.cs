using Microsoft.EntityFrameworkCore;
using ShopNest.DAL.ApplicationDbContext;
using ShopNest.DAL.Repositories.Interfaces;
using ShpoNest.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopNest.DAL.Repositories.Implementations
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext context) : base(context) { }

        public async Task<Customer?> GetByIdWithAddressesAsync(int id)
    => await _dbSet
        .Include(c => c.Addresses)
        .Include(c => c.Orders)
            .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Images)
        .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Customer?> GetByEmailAsync(string email)
            => await _dbSet
                .FirstOrDefaultAsync(c => c.Email == email);

        public async Task<IEnumerable<Customer>> GetAllWithOrdersAsync()
            => await _dbSet
                .Include(c => c.Orders)
                .ToListAsync();
    }
}
