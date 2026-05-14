using Volo.Abp.Modularity;

namespace ECommerceWeb;

[DependsOn(
    typeof(ECommerceWebApplicationModule),
    typeof(ECommerceWebDomainTestModule)
)]
public class ECommerceWebApplicationTestModule : AbpModule
{

}
