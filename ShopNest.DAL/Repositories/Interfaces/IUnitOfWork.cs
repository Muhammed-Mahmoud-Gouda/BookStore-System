namespace ShopNest.DAL.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        IProductImageRepository ProductImages { get; }
        ICustomerRepository Customers { get; }
        ICustomerAddressRepository CustomerAddresses { get; }
        IOrderRepository Orders { get; }
        IOrderItemRepository OrderItems { get; }

        Task<int> SaveChangesAsync();
    }

}
