using System;
using System.Collections.Generic;
using System.Text;

namespace ShpoNest.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }


        //Fk
        public int CategoryId { get; set; }

        //Nav prop
        public Category Category { get; set; }
        public ICollection<ProductImages> Images { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
