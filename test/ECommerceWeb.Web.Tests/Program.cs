using Microsoft.AspNetCore.Builder;
using ECommerceWeb;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();

builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("ECommerceWeb.Web.csproj");
await builder.RunAbpModuleAsync<ECommerceWebWebTestModule>(applicationName: "ECommerceWeb.Web" );

public partial class Program
{
}
