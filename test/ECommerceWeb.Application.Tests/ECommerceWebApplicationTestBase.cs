using Volo.Abp.Modularity;

namespace ECommerceWeb;

public abstract class ECommerceWebApplicationTestBase<TStartupModule> : ECommerceWebTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
