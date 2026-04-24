using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShpoNest.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopNest.DAL.Configurations
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImages>
    {
        public void Configure(EntityTypeBuilder<ProductImages> builder)
        {
            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.ImagePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(pi => pi.IsMain)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(pi => pi.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pi => pi.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        }

    }
}
