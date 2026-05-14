using ECommerceWeb.Samples;
using Xunit;

namespace ECommerceWeb.EntityFrameworkCore.Applications;

[Collection(ECommerceWebTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<ECommerceWebEntityFrameworkCoreTestModule>
{

}
