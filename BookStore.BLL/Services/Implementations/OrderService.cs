using ShopNest.BLL.DTOs.Order;
using ShopNest.BLL.Services.Interfaces;
using ShopNest.DAL.Repositories.Interfaces;
using ShpoNest.Models.Entities;
using ShpoNest.Models.Enums;
using System.Text;

namespace ShopNest.BLL.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<OrderResultDto>> GetAllAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllWithCustomerAsync();
            return orders.Select(o => MapToResultDto(o));
        }

        public async Task<IEnumerable<OrderResultDto>> GetByCustomerIdAsync(int customerId)
        {

            var customerExists = await _unitOfWork.Customers.ExistsAsync(customerId);
            if (!customerExists)
                throw new Exception($"Customer with id {customerId} not found");

            var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(customerId);
            return orders.Select(o => MapToResultDto(o));
        }

        public async Task<IEnumerable<OrderResultDto>> GetByStatusAsync(OrderStatus status)
        {
            var orders = await _unitOfWork.Orders.GetByStatusAsync(status);
            return orders.Select(o => MapToResultDto(o));
        }

        public async Task<OrderResultDto?> GetByIdAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(id)
                ?? throw new Exception($"Order with id {id} not found");

            return MapToResultDto(order);
        }

        public async Task CreateAsync(OrderCreateDto dto)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                
                var customerExists = await _unitOfWork.Customers.ExistsAsync(dto.CustomerId);
                if (!customerExists)
                    throw new Exception($"Customer with id {dto.CustomerId} not found");

                
                var shippingAddress = await _unitOfWork.CustomerAddresses
                    .GetByIdAsync(dto.ShippingAddressId)
                    ?? throw new Exception($"Address with id {dto.ShippingAddressId} not found");

                
                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in dto.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId)
                        ?? throw new Exception($"Product with id {item.ProductId} not found");

                  
                    if (product.Stock < item.Quantity)
                        throw new Exception($"Insufficient stock for product {product.Name}");

                    product.Stock -= item.Quantity;
                    _unitOfWork.Products.Update(product);

                    var unitPrice = product.Price;
                    totalAmount += unitPrice * item.Quantity;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice,
                    });
                }

                
                var order = new Order
                {
                    CustomerId = dto.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    TotalAmount = totalAmount,
                    Notes = dto.Notes,
                    ShippingAddressId = shippingAddress.Id,
                    ShippingStreet = shippingAddress.Street,
                    ShippingCity = shippingAddress.City,
                    ShippingPostalCode = shippingAddress.PostalCode,
                };

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                
                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                    await _unitOfWork.OrderItems.AddAsync(item);
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateStatusAsync(OrderUpdateDto dto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.Id)
                ?? throw new Exception($"Order with id {dto.Id} not found");


            if (order.Status == OrderStatus.Cancelled)
                throw new Exception("Cannot update a cancelled order");

            if (order.Status == OrderStatus.Delivered)
                throw new Exception("Cannot update a delivered order");

            order.Status = dto.Status;
            order.Notes = dto.Notes;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(id)
                ?? throw new Exception($"Order with id {id} not found");


            if (order.Status == OrderStatus.Delivered)
                throw new Exception("Cannot cancel a delivered order");

            if (order.Status == OrderStatus.Cancelled)
                throw new Exception("Order is already cancelled");

            foreach (var item in order.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock += item.Quantity;
                    _unitOfWork.Products.Update(product);
                }
            }

            order.Status = OrderStatus.Cancelled;
           

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
            => await _unitOfWork.Orders.ExistsAsync(id);

      
        private static OrderResultDto MapToResultDto(Order order)
        {
            return new OrderResultDto
            {
                Id = order.Id,
                CustomerName = $"{order.Customer?.FirstName} {order.Customer?.LastName}",
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                ShippingStreet = order.ShippingStreet,
                ShippingCity = order.ShippingCity,
                ShippingPostalCode = order.ShippingPostalCode,
                Notes = order.Notes,
                Items = order.OrderItems?
                    .Select(oi => new OrderItemResultDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name ?? string.Empty,
                        MainImagePath = oi.Product?.Images?
                            .FirstOrDefault(i => i.IsMain)?.ImagePath,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                    }).ToList() ?? new(),
            };
        }

    }
}