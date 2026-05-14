using Volo.Abp.Settings;

namespace ECommerceWeb.Settings;

public class ECommerceWebSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(ECommerceWebSettings.MySetting1));
    }
}
