using System;
using System.Collections.Generic;
using System.Text;
using ECommerceWeb.Localization;
using Volo.Abp.Application.Services;

namespace ECommerceWeb;

/* Inherit your application services from this class.
 */
public abstract class ECommerceWebAppService : ApplicationService
{
    protected ECommerceWebAppService()
    {
        LocalizationResource = typeof(ECommerceWebResource);
    }
}
