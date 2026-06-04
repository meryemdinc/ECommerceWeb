using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ECommerceWeb.Categories;

public class Category : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<Products.Product> Products { get; private set; }

    protected Category()
    {
        Products = new List<Products.Product>();
    }

    public Category(Guid id, string name, string? description = null)
        : base(id)
    {
        SetName(name);
        Description = description;
        Products = new List<Products.Product>();
        IsActive = true;
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: 128);
    }

    public void SetDescription(string? description)
    {
        Description = description;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
