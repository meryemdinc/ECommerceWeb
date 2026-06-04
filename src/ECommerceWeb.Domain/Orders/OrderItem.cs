using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ECommerceWeb.Orders;

public class OrderItem : AuditedEntity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public decimal TotalPrice => Quantity * UnitPrice;

    protected OrderItem() { }

    public OrderItem(
        Guid id,
        Guid orderId,
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice)
        : base(id)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = Check.NotNullOrWhiteSpace(productName, nameof(productName), maxLength: 256);
        UpdateQuantity(quantity);
        SetUnitPrice(unitPrice);
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Sipariş miktarı 0'dan büyük olmalıdır.", nameof(quantity));
        }

        Quantity = quantity;
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
