using System;
using System.Threading.Tasks;
using ECommerceWeb.Categories;
using ECommerceWeb.Events;
using ECommerceWeb.Orders;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;

namespace ECommerceWeb.Products;

public class ProductManager : DomainService
{
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<OrderItem, Guid> _orderItemRepository;
    private readonly ILocalEventBus _localEventBus;

    public ProductManager(
        IRepository<Product, Guid> productRepository,
        IRepository<Category, Guid> categoryRepository,
        IRepository<OrderItem, Guid> orderItemRepository,
        ILocalEventBus localEventBus)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _orderItemRepository = orderItemRepository;
        _localEventBus = localEventBus;
    }

    public async Task<Product> CreateAsync(
        Guid categoryId,
        string name,
        decimal price,
        int stockCount,
        string imageUrl,
        string? description = null)
    {
        await EnsureCategoryIsActiveAsync(categoryId);
        await EnsureNameIsUniqueAsync(name);

        var product = new Product(
            GuidGenerator.Create(),
            categoryId,
            name,
            price,
            stockCount,
            imageUrl,
            description);

        return await _productRepository.InsertAsync(product, autoSave: true);
    }

    public async Task<Product> UpdateAsync(
        Guid id,
        Guid categoryId,
        string name,
        decimal price,
        int stockCount,
        string imageUrl,
        string? description,
        bool isActive)
    {
        var product = await GetAsync(id);
        await EnsureCategoryIsActiveAsync(categoryId);
        await EnsureNameIsUniqueAsync(name, id);

        product.SetCategory(categoryId);
        product.SetName(name);
        product.SetPrice(price);
        product.SetStockCount(stockCount);
        product.SetImageUrl(imageUrl);
        product.SetDescription(description);

        if (isActive)
        {
            product.Activate();
        }
        else
        {
            product.Deactivate();
        }

        return await _productRepository.UpdateAsync(product, autoSave: true);
    }

    public async Task<Product> GetAsync(Guid id)
    {
        return await _productRepository.GetAsync(id);
    }

    public async Task<Product> GetAvailableAsync(Guid id, int quantity)
    {
        var product = await GetAsync(id);
        product.EnsureAvailableForSale(quantity);
        return product;
    }

    public async Task ReserveStockAsync(Guid productId, int quantity)
    {
        var product = await GetAsync(productId);
        product.DecreaseStock(quantity);
        await _productRepository.UpdateAsync(product, autoSave: true);
        await PublishLowStockIfNeededAsync(product);
    }

    public async Task ReleaseStockAsync(Guid productId, int quantity)
    {
        var product = await GetAsync(productId);
        product.IncreaseStock(quantity);
        await _productRepository.UpdateAsync(product, autoSave: true);
    }

    /// <summary>
    /// Yönetim panelinden stok düzeltmesi (+/-). Negatif sonuç oluşturmaz.
    /// </summary>
    public async Task<Product> AdjustStockAsync(Guid productId, int delta)
    {
        if (delta == 0)
        {
            return await GetAsync(productId);
        }

        var product = await GetAsync(productId);

        if (delta > 0)
        {
            product.IncreaseStock(delta);
        }
        else
        {
            var decreaseBy = Math.Abs(delta);
            if (product.StockCount < decreaseBy)
            {
                throw new BusinessException(ECommerceWebDomainErrorCodes.InsufficientStock)
                    .WithData("ProductName", product.Name)
                    .WithData("AvailableStock", product.StockCount)
                    .WithData("RequestedQuantity", decreaseBy);
            }

            product.DecreaseStock(decreaseBy);
        }

        await _productRepository.UpdateAsync(product, autoSave: true);
        await PublishLowStockIfNeededAsync(product);
        return product;
    }

    public async Task DeleteAsync(Guid id)
    {
        if (await _orderItemRepository.AnyAsync(x => x.ProductId == id))
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.ProductInUse);
        }

        await _productRepository.DeleteAsync(id);
    }

    private async Task EnsureCategoryExistsAsync(Guid categoryId)
    {
        if (!await _categoryRepository.AnyAsync(x => x.Id == categoryId))
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.CategoryNotFound);
        }
    }

    private async Task EnsureCategoryIsActiveAsync(Guid categoryId)
    {
        await EnsureCategoryExistsAsync(categoryId);

        var category = await _categoryRepository.GetAsync(categoryId);
        if (!category.IsActive)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.CategoryInactive)
                .WithData("CategoryName", category.Name);
        }
    }

    private async Task EnsureNameIsUniqueAsync(string name, Guid? excludeId = null)
    {
        var exists = excludeId.HasValue
            ? await _productRepository.AnyAsync(x => x.Name == name && x.Id != excludeId.Value)
            : await _productRepository.AnyAsync(x => x.Name == name);

        if (exists)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.ProductNameAlreadyExists)
                .WithData("Name", name);
        }
    }

    private async Task PublishLowStockIfNeededAsync(Product product)
    {
        if (!product.IsLowStock())
        {
            return;
        }

        await _localEventBus.PublishAsync(new ProductLowStockEvent
        {
            ProductId = product.Id,
            ProductName = product.Name,
            CurrentStock = product.StockCount,
            Threshold = Product.LowStockThreshold
        });
    }
}
