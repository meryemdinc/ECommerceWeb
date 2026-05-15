using ECommerceWeb.Categories;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ECommerceWeb.Products;

public class Product : FullAuditedAggregateRoot<Guid>
{
    // Foreign Key (Yabancı Anahtar) - Ürünün hangi kategoriye ait olduğunu tutar
    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public int StockCount { get; private set; }

    public string ImageUrl { get; private set; } = string.Empty;

    // Navigation Property: Ürünün bağlı olduğu Kategori nesnesi
    public Category? Category { get; set; }
    //bu methodu EFCore reflection ile veritabanından veri çekerken kullanır,
    //EFCore bu işi kendi hallettiği için bu methodun içinde bişi yazması gerekmez.
    //bu methodun protected olması, sadece bu sınıfın kendisi ve miras alan sınıflar tarafından erişilebilir olmasını sağlar.
    //EfCore veritabından veri çekip c# nesnesi oluştururken c#tarafında boş bir kalıp yaratması gerekir
    //ve bu kalıbı yaratırken de parametresiz bir constructor'a ihtiyaç duyar,parametreli constructor parametrelerine atama bekler ama eşleştirme boş kalıp 
    //oluşturduktan sonra yapılır, bu yüzden parametresiz constructor'a ihtiyaç duyarız.
    protected Product()
    {
    }
    //biz nesne üretirken bu constructor'ı kullanacağız, bu yüzden parametreler ekledik.
    //string name burada zorunlu alan olarak belirtilmiş,üstte ilk başta null başlatsak da 
    public Product(Guid id, Guid categoryId, string name, decimal price, int stockCount, string imageUrl, string? description = null)
        : base(id)
    {
        CategoryId = categoryId;
        // Guard Clauses (Verileri güvenli atama)
        // Nesne doğarken hatalıysa programı durdur (Fail-Fast kuralı)
        SetName(name);
        SetPrice(price);
        SetStockCount(stockCount);

        // ABP'nin hazır metodu: Hem null olmasını hem de sadece boşluk olmasını engeller
        ImageUrl = Check.NotNullOrWhiteSpace(imageUrl, nameof(imageUrl));
        Description = description;
    }
    // 3. ADIM: Davranış Metotları (Değişiklikler sadece bu kapılardan yapılabilir)

    public void SetName(string name)
    {
        // Maksimum uzunluk kuralını bile burada veriyoruz
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: 256);
    }

    public void SetPrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentException("Ürün fiyatı 0'dan küçük olamaz!", nameof(price));
        }
        Price = price;
    }

    private void SetStockCount(int stockCount)
    {
        if (stockCount < 0)
        {
            throw new ArgumentException("Stok adedi eksi olamaz!", nameof(stockCount));
        }
        StockCount = stockCount;
    }

    // Gerçek dünya iş kuralı (Örneğin birisi sipariş verdiğinde bu metot çağrılacak)
    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Düşürülecek stok miktarı 0'dan büyük olmalıdır.");
        }

        if (StockCount - quantity < 0)
        {
            throw new InvalidOperationException($"Yetersiz stok! Sadece {StockCount} adet ürün kaldı.");
        }

        StockCount -= quantity;
    }
}