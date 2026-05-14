using Microsoft.Extensions.Localization;
using ECommerceWeb.Localization;
using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace ECommerceWeb.Web;

[Dependency(ReplaceServices = true)]
public class ECommerceWebBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<ECommerceWebResource> _localizer;

    public ECommerceWebBrandingProvider(IStringLocalizer<ECommerceWebResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
