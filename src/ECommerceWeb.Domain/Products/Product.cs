using System;
using ECommerceWeb.Categories;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ECommerceWeb.Products;

public class Product : FullAuditedAggregateRoot<Guid>
{
    public const int LowStockThreshold = 10;

    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockCount { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public Category? Category { get; private set; }

    protected Product() { }

    public Product(
        Guid id,
        Guid categoryId,
        string name,
        decimal price,
        int stockCount,
        string imageUrl,
        string? description = null)
        : base(id)
    {
        CategoryId = categoryId;
        SetName(name);
        SetPrice(price);
        SetStockCount(stockCount);
        ImageUrl = Check.NotNullOrWhiteSpace(imageUrl, nameof(imageUrl), maxLength: 512);
        Description = description;
        IsActive = true;
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: 256);
    }

    public void SetDescription(string? description)
    {
        Description = description;
    }

    public void SetPrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentException("Ürün fiyatı 0'dan küçük olamaz!", nameof(price));
        }

        Price = price;
    }

    public void SetCategory(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public void SetImageUrl(string imageUrl)
    {
        ImageUrl = Check.NotNullOrWhiteSpace(imageUrl, nameof(imageUrl), maxLength: 512);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void SetStockCount(int stockCount)
    {
        if (stockCount < 0)
        {
            throw new ArgumentException("Stok adedi eksi olamaz!", nameof(stockCount));
        }

        StockCount = stockCount;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Düşürülecek stok miktarı 0'dan büyük olmalıdır.");
        }

        if (StockCount - quantity < 0)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.InsufficientStock)
                .WithData("ProductName", Name)
                .WithData("AvailableStock", StockCount)
                .WithData("RequestedQuantity", quantity);
        }

        StockCount -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Artırılacak stok miktarı 0'dan büyük olmalıdır.");
        }

        StockCount += quantity;
    }

    public bool IsLowStock() => StockCount <= LowStockThreshold;

    public void EnsureAvailableForSale(int quantity)
    {
        if (!IsActive)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.ProductNotFound)
                .WithData("ProductName", Name);
        }

        if (StockCount < quantity)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.InsufficientStock)
                .WithData("ProductName", Name)
                .WithData("AvailableStock", StockCount)
                .WithData("RequestedQuantity", quantity);
        }
    }
}
