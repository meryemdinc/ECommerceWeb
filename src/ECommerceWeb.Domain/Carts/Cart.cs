using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ECommerceWeb.Carts;

public class Cart : FullAuditedAggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }

    public ICollection<CartItem> Items { get; private set; }

    public decimal TotalAmount => Items.Sum(x => x.TotalPrice);

    protected Cart()
    {
        Items = new List<CartItem>();
    }

    public Cart(Guid id, Guid customerId)
        : base(id)
    {
        CustomerId = customerId;
        Items = new List<CartItem>();
    }

    public CartItem AddOrUpdateItem(Guid productId, string productName, int quantity, decimal unitPrice, string imageUrl)
    {
        Check.Positive(quantity, nameof(quantity));

        var existing = Items.FirstOrDefault(x => x.ProductId == productId);
        if (existing != null)
        {
            existing.UpdateQuantity(existing.Quantity + quantity);
            return existing;
        }

        var item = new CartItem(
            Guid.NewGuid(),
            Id,
            productId,
            productName,
            quantity,
            unitPrice,
            imageUrl);

        Items.Add(item);
        return item;
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        Check.Positive(quantity, nameof(quantity));

        var item = Items.FirstOrDefault(x => x.ProductId == productId)
            ?? throw new BusinessException(ECommerceWebDomainErrorCodes.CartItemNotFound);

        item.UpdateQuantity(quantity);
    }

    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(x => x.ProductId == productId)
            ?? throw new BusinessException(ECommerceWebDomainErrorCodes.CartItemNotFound);

        Items.Remove(item);
    }

    public void Clear()
    {
        Items.Clear();
    }

    public bool IsEmpty => !Items.Any();

    public void RefreshItemPrice(Guid productId, string productName, decimal unitPrice, string imageUrl)
    {
        var item = Items.FirstOrDefault(x => x.ProductId == productId)
            ?? throw new BusinessException(ECommerceWebDomainErrorCodes.CartItemNotFound);

        item.RefreshSnapshot(productName, unitPrice, imageUrl);
    }
}
