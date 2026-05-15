using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;


namespace ECommerceWeb.Products;

public class ProductAppService : ApplicationService, IProductAppService
{
    // IRepository, ABP'nin bize sunduğu hazır veritabanı erişim aracıdır (Generic Repository)
    private readonly IRepository<Product, System.Guid> _productRepository;

    public ProductAppService(IRepository<Product, System.Guid> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<ProductDto>> GetListAsync()
    {
        // 1. Veritabanından tüm ürünleri çek
        var products = await _productRepository.GetListAsync();

        // 2. Çekilen Product (Domain) listesini ProductDto listesine çevirip döndür
        return ObjectMapper.Map<List<Product>, List<ProductDto>>(products);
    }
}