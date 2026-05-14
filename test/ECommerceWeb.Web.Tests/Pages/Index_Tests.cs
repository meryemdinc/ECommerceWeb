using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace ECommerceWeb.Pages;

public class Index_Tests : ECommerceWebWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
