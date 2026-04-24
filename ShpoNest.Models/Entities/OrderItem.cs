using System;
using System.Collections.Generic;
using System.Text;

namespace ShpoNest.Models.Entities
{
    // OrderItem.cs
    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // FKs
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        // Navigation
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
