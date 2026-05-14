using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace ECommerceWeb.Data;

/* This is used if database provider does't define
 * IECommerceWebDbSchemaMigrator implementation.
 */
public class NullECommerceWebDbSchemaMigrator : IECommerceWebDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
