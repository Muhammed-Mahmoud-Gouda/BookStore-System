using ShpoNest.Models.Enums;

namespace ShpoNest.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }

        //Shipping Snapshot
        public string ShippingStreet { get; set; }
        public string ShippingCity { get; set; }
        public string? ShippingPostalCode { get; set; }

        //FK
        public int CustomerId { get; set; }
        public int? ShippingAddressId { get; set; }

        // Navigation
        public Customer Customer { get; set; }
        public CustomerAddress? ShippingAddress { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }

    }
}
