using System;
using System.Collections.Generic;
using System.Text;

namespace ShpoNest.Models.Entities
{
    public class CustomerAddress
    {
        public int Id { get; set; }
        public string? Label { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string? PostalCode { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }

        //Fk From Customer Table
        public int CustomerId { get; set; }

        // Navigation
        public Customer Customer { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
