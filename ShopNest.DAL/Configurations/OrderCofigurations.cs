using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShpoNest.Models.Entities;
using ShpoNest.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopNest.DAL.Configurations
{
    // OrderConfiguration.cs
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(o => o.Status)
                .IsRequired()
                .HasDefaultValue(OrderStatus.Pending);
                
            builder.Property(o => o.TotalAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(o => o.Notes)
                .HasMaxLength(1000);

            // Shipping Snapshot
            builder.Property(o => o.ShippingStreet)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(o => o.ShippingCity)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.ShippingPostalCode)
                .HasMaxLength(20);

            
            builder.Property(o => o.ShippingAddressId)
                .IsRequired(false);

            // Relationship
            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
