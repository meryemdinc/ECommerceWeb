using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ECommerceWeb.Products;

// IApplicationService, ABP'nin bu arayüzü otomatik olarak bir API'ye dönüştürmesini sağlar.
public interface IProductAppService : IApplicationService
{
    // Arayüzdeki ürünleri getirecek olan metodumuz
    Task<List<ProductDto>> GetListAsync();
}