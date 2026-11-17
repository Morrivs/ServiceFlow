using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceFlow.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceFlow.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<ServiceFlowDbContext>(o =>
                o.UseSqlServer(config.GetConnectionString("Default")));
            return services;
        }
    }
}
