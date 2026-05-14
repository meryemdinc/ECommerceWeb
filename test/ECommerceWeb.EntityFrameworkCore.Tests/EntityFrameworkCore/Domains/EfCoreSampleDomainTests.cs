using ECommerceWeb.Samples;
using Xunit;

namespace ECommerceWeb.EntityFrameworkCore.Domains;

[Collection(ECommerceWebTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<ECommerceWebEntityFrameworkCoreTestModule>
{

}
