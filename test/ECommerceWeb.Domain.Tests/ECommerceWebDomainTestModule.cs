using Volo.Abp.Modularity;

namespace ECommerceWeb;

[DependsOn(
    typeof(ECommerceWebDomainModule),
    typeof(ECommerceWebTestBaseModule)
)]
public class ECommerceWebDomainTestModule : AbpModule
{

}
