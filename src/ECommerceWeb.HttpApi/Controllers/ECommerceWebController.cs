using ECommerceWeb.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace ECommerceWeb.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class ECommerceWebController : AbpControllerBase
{
    protected ECommerceWebController()
    {
        LocalizationResource = typeof(ECommerceWebResource);
    }
}
