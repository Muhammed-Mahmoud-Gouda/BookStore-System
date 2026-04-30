using Microsoft.EntityFrameworkCore;
using ShopNest.DAL.ApplicationDbContext;
using ShopNest.DAL.Repositories.Interfaces;
using ShpoNest.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopNest.DAL.Repositories.Implementations
{
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
            => await _dbSet
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Images)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
    }
}
