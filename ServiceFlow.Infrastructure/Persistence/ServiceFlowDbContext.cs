using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceFlow.Domain.Customers;
using ServiceFlow.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceFlow.Infrastructure.Persistence
{
    public class ServiceFlowDbContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ServiceFlowDbContext(DbContextOptions<ServiceFlowDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();

        protected override void ConfigureConventions(ModelConfigurationBuilder cb)
        {
            cb.Properties<decimal>().HavePrecision(18, 2);   // importes
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

            builder.ApplyConfigurationsFromAssembly(typeof(ServiceFlowDbContext).Assembly);

            builder.HasSequence<int>("seq_ServiceRequestNumber");
            builder.HasSequence<int>("seq_QuoteNumber");
            builder.HasSequence<int>("seq_WorkOrderNumber");

            // RN-015: sin borrado en cascada (Restrict = NO ACTION en SQL Server)
            foreach (var fk in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
