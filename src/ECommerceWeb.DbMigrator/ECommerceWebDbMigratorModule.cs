using ECommerceWeb.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace ECommerceWeb.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(ECommerceWebEntityFrameworkCoreModule),
    typeof(ECommerceWebApplicationContractsModule)
    )]
public class ECommerceWebDbMigratorModule : AbpModule
{
}
