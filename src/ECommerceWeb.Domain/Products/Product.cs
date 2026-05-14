using System;
using ECommerceWeb.Categories;
using Volo.Abp.Domain.Entities.Auditing;

namespace ECommerceWeb.Products;

public class Product : FullAuditedAggregateRoot<Guid>
{
    // Foreign Key (Yabancı Anahtar) - Ürünün hangi kategoriye ait olduğunu tutar
    public Guid CategoryId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public int StockCount { get; set; }

    public string ImageUrl { get; set; }

    // Navigation Property: Ürünün bağlı olduğu Kategori nesnesi
    public Category Category { get; set; }

    protected Product()
    {
    }

    public Product(Guid id, Guid categoryId, string name, decimal price, int stockCount, string imageUrl, string description = null)
        : base(id)
    {
        CategoryId = categoryId;
        Name = name;
        Price = price;
        StockCount = stockCount;
        ImageUrl = imageUrl;
        Description = description;
    }
}