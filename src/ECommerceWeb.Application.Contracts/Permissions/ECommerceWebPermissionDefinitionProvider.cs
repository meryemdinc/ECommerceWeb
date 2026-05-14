using ECommerceWeb.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace ECommerceWeb.Permissions;

public class ECommerceWebPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ECommerceWebPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(ECommerceWebPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ECommerceWebResource>(name);
    }
}
