using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ECommerceWeb.Orders;

public class Order : FullAuditedAggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }
    public string OrderNumber { get; private set; } = null!;
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }

    public string ShippingAddress { get; private set; } = string.Empty;
    public string ShippingCity { get; private set; } = string.Empty;
    public string ShippingPostalCode { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    public ICollection<OrderItem> Items { get; private set; }

    protected Order()
    {
        Items = new List<OrderItem>();
    }

    public Order(Guid id, Guid customerId, string orderNumber)
        : base(id)
    {
        CustomerId = customerId;
        OrderNumber = Check.NotNullOrWhiteSpace(orderNumber, nameof(orderNumber), maxLength: 32);
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        TotalAmount = 0;
        Items = new List<OrderItem>();
    }

    public void SetShippingInfo(string address, string city, string postalCode, string? notes = null)
    {
        ShippingAddress = Check.NotNullOrWhiteSpace(address, nameof(address), maxLength: 512);
        ShippingCity = Check.NotNullOrWhiteSpace(city, nameof(city), maxLength: 128);
        ShippingPostalCode = Check.NotNullOrWhiteSpace(postalCode, nameof(postalCode), maxLength: 32);
        Notes = notes;
    }

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        EnsureModifiable();

        var existingItem = Items.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            Items.Add(new OrderItem(
                Guid.NewGuid(),
                Id,
                productId,
                productName,
                quantity,
                unitPrice));
        }

        RecalculateTotal();
    }

    public void RemoveItem(Guid productId)
    {
        EnsureModifiable();

        var item = Items.FirstOrDefault(x => x.ProductId == productId);
        if (item == null)
        {
            return;
        }

        Items.Remove(item);
        RecalculateTotal();
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        if (Status == newStatus)
        {
            return;
        }

        if (Status == OrderStatus.Cancelled)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.OrderCannotBeModified);
        }

        if (Status == OrderStatus.Completed && newStatus != OrderStatus.Completed)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.InvalidOrderStatusTransition);
        }

        var allowed = (Status, newStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Processing) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Processing, OrderStatus.Shipped) => true,
            (OrderStatus.Processing, OrderStatus.Cancelled) => true,
            (OrderStatus.Shipped, OrderStatus.Completed) => true,
            (OrderStatus.Shipped, OrderStatus.Cancelled) => true,
            _ => false
        };

        if (!allowed)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.InvalidOrderStatusTransition)
                .WithData("CurrentStatus", Status.ToString())
                .WithData("TargetStatus", newStatus.ToString());
        }

        Status = newStatus;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.OrderCannotBeModified);
        }

        Status = OrderStatus.Cancelled;
    }

    public void EnsureHasItems()
    {
        if (!Items.Any())
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.OrderEmpty);
        }
    }

    private void EnsureModifiable()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.OrderCannotBeModified);
        }
    }

    private void RecalculateTotal()
    {
        TotalAmount = Items.Sum(item => item.TotalPrice);
    }
}
