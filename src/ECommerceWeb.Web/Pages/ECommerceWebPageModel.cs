using ECommerceWeb.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace ECommerceWeb.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class ECommerceWebPageModel : AbpPageModel
{
    protected ECommerceWebPageModel()
    {
        LocalizationResourceType = typeof(ECommerceWebResource);
    }
}
