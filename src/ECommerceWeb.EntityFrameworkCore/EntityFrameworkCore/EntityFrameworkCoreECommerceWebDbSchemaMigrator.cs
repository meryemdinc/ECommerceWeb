using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ECommerceWeb.Data;
using Volo.Abp.DependencyInjection;

namespace ECommerceWeb.EntityFrameworkCore;

public class EntityFrameworkCoreECommerceWebDbSchemaMigrator
    : IECommerceWebDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreECommerceWebDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the ECommerceWebDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<ECommerceWebDbContext>()
            .Database
            .MigrateAsync();
    }
}
