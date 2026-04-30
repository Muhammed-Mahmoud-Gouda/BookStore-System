using ShpoNest.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopNest.DAL.Repositories.Interfaces
{
    public interface IOrderItemRepository : IGenericRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
    }
}
