using Volo.Abp.Modularity;

namespace ECommerceWeb;

/* Inherit from this class for your domain layer tests. */
public abstract class ECommerceWebDomainTestBase<TStartupModule> : ECommerceWebTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
