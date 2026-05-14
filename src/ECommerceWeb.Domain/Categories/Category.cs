using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace ECommerceWeb.Categories;

// Kategori ana tablomuz. ABP'nin FullAuditedAggregateRoot sınıfı sayesinde
// Id, CreationTime, CreatorId, IsDeleted gibi alanlar otomatik eklenecek.
public class Category : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; }

    public string Description { get; set; }

    // Navigation Property: Bir kategorinin birden fazla ürünü olabilir
    public ICollection<Products.Product> Products { get; set; }

    // Entity Framework Core'un arka planda çalışabilmesi için boş constructor şarttır
    protected Category()
    {
    }

    // Kod tarafında nesne üretirken kullanacağımız constructor
    public Category(Guid id, string name, string description = null)
        : base(id)
    {
        Name = name;
        Description = description;
        Products = new List<Products.Product>();
    }
}