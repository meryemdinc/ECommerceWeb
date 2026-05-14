using System.Threading.Tasks;

namespace ECommerceWeb.Data;

public interface IECommerceWebDbSchemaMigrator
{
    Task MigrateAsync();
}
