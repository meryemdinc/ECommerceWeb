using Xunit;

namespace ECommerceWeb.EntityFrameworkCore;

[CollectionDefinition(ECommerceWebTestConsts.CollectionDefinitionName)]
public class ECommerceWebEntityFrameworkCoreCollection : ICollectionFixture<ECommerceWebEntityFrameworkCoreFixture>
{

}
