using System;
using System.Collections.Generic;
using System.Text;

namespace ShpoNest.Models.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public string PasswordHash { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;

        // Navigation
        public ICollection<CustomerAddress> Addresses { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
