using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceFlow.Domain.Customers;
using ServiceFlow.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceFlow.Infrastructure.Persistence.Configurations
{
    internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> b)
        {
            b.ToTable("Customers");
            b.HasKey(c => c.Id);

            b.Property(c => c.FullName).HasMaxLength(150).IsRequired();
            b.Property(c => c.IdentificationNumber).HasMaxLength(30).IsRequired();
            b.Property(c => c.Email).HasMaxLength(256).IsRequired();
            b.Property(c => c.Phone).HasMaxLength(30).IsRequired();
            b.Property(c => c.Address).HasMaxLength(250);
            b.Property(c => c.RowVersion).IsRowVersion();

            b.HasIndex(c => c.IdentificationNumber).IsUnique().HasDatabaseName("UX_Customers_IdentificationNumber");
            b.HasIndex(c => c.Email).IsUnique().HasDatabaseName("UX_Customers_Email");
            b.HasIndex(c => c.UserId).IsUnique()
                .HasFilter("[UserId] IS NOT NULL")
                .HasDatabaseName("UX_Customers_UserId");

            b.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
