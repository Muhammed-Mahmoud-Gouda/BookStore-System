using Microsoft.EntityFrameworkCore;
using ShopNest.DAL.ApplicationDbContext;
using ShopNest.DAL.Repositories.Interfaces;
using ShpoNest.Models.Entities;
using ShpoNest.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopNest.DAL.Repositories.Implementations
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetAllWithCustomerAsync()
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
               
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdWithItemsAsync(int id)
        {
            return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.ShippingAddress)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            return await _dbSet
            .Include(o => o.Customer)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
        }
    }
}
