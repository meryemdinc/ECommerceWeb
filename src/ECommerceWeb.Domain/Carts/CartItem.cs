using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ECommerceWeb.Carts;

public class CartItem : AuditedEntity<Guid>
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;

    public decimal TotalPrice => Quantity * UnitPrice;

    protected CartItem() { }

    public CartItem(
        Guid id,
        Guid cartId,
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice,
        string imageUrl)
        : base(id)
    {
        CartId = cartId;
        ProductId = productId;
        ProductName = Check.NotNullOrWhiteSpace(productName, nameof(productName), maxLength: 256);
        ImageUrl = Check.NotNullOrWhiteSpace(imageUrl, nameof(imageUrl), maxLength: 512);
        UpdateQuantity(quantity);
        SetUnitPrice(unitPrice);
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Sepet miktarı 0'dan büyük olmalıdır.", nameof(quantity));
        }

        Quantity = quantity;
    }

    public void RefreshSnapshot(string productName, decimal unitPrice, string imageUrl)
    {
        ProductName = Check.NotNullOrWhiteSpace(productName, nameof(productName), maxLength: 256);
        ImageUrl = Check.NotNullOrWhiteSpace(imageUrl, nameof(imageUrl), maxLength: 512);
        SetUnitPrice(unitPrice);
    }

    private void SetUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw new ArgumentException("Birim fiyat 0'dan küçük olamaz.", nameof(unitPrice));
        }

        UnitPrice = unitPrice;
    }
}
