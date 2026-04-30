using ShopNest.DAL.ApplicationDbContext;
using ShopNest.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopNest.DAL.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public ICategoryRepository Categories { get; }
        public IProductRepository Products { get; }
        public IProductImageRepository ProductImages { get; }
        public ICustomerRepository Customers { get; }
        public ICustomerAddressRepository CustomerAddresses { get; }
        public IOrderRepository Orders { get; }
        public IOrderItemRepository OrderItems { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Categories = new CategoryRepository(context);
            Products = new ProductRepository(context);
            ProductImages = new ProductImageRepository(context);
            Customers = new CustomerRepository(context);
            CustomerAddresses = new CustomerAddressRepository(context);
            Orders = new OrderRepository(context);
            OrderItems = new OrderItemRepository(context);
        }

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
