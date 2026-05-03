using Microsoft.Extensions.Logging;
using ShopNest.DAL.ApplicationDbContext;
using ShopNest.DAL.Repositories.Interfaces;
using System.Runtime.CompilerServices;

namespace ShopNest.DAL.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    
    private readonly AppDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    

    public ICategoryRepository Categories { get; }
    public IProductRepository Products { get; }
    public IProductImageRepository ProductImages { get; }
    public ICustomerRepository Customers { get; }
    public ICustomerAddressRepository CustomerAddresses { get; }
    public IOrderRepository Orders { get; }
    public IOrderItemRepository OrderItems { get; }

    public UnitOfWork(AppDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
        
        Categories = new CategoryRepository(context);
        Products = new ProductRepository(context);
        ProductImages = new ProductImageRepository(context);
        Customers = new CustomerRepository(context);
        CustomerAddresses = new CustomerAddressRepository(context);
        Orders = new OrderRepository(context);
        OrderItems = new OrderItemRepository(context);
    }

    public async Task<int> SaveChangesAsync()
    {
        // Begin an explicit transaction
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Your pending changes (e.g., _context.Categories.Add(), .Update(), etc.)
            // Note: Your original line `var tr = _context.Categories=` was incomplete.
            // Place any entity operations BEFORE SaveChangesAsync() here.

            var result = await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw; // Preserves the original stack trace
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
