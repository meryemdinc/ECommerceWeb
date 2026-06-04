using System;
using System.Linq;
using System.Threading.Tasks;
using ECommerceWeb.Carts;
using ECommerceWeb.Events;
using ECommerceWeb.Products;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;

namespace ECommerceWeb.Orders;

public class OrderManager : DomainService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly CartManager _cartManager;
    private readonly ProductManager _productManager;
    private readonly ILocalEventBus _localEventBus;

    public OrderManager(
        IRepository<Order, Guid> orderRepository,
        CartManager cartManager,
        ProductManager productManager,
        ILocalEventBus localEventBus)
    {
        _orderRepository = orderRepository;
        _cartManager = cartManager;
        _productManager = productManager;
        _localEventBus = localEventBus;
    }

    [UnitOfWork]
    public async Task<Order> PlaceOrderFromCartAsync(
        Guid customerId,
        string shippingAddress,
        string shippingCity,
        string shippingPostalCode,
        string? notes = null)
    {
        ValidateShippingInfo(shippingAddress, shippingCity, shippingPostalCode);

        await _cartManager.SyncPricesAsync(customerId);

        var cart = await _cartManager.GetWithItemsAsync(customerId);
        if (cart.IsEmpty)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.OrderEmpty);
        }

        var order = new Order(
            GuidGenerator.Create(),
            customerId,
            GenerateOrderNumber());

        order.SetShippingInfo(shippingAddress, shippingCity, shippingPostalCode, notes);

        foreach (var cartItem in cart.Items.ToList())
        {
            var product = await _productManager.GetAvailableAsync(cartItem.ProductId, cartItem.Quantity);
            await _productManager.ReserveStockAsync(product.Id, cartItem.Quantity);

            order.AddItem(
                product.Id,
                product.Name,
                cartItem.Quantity,
                product.Price);
        }

        order.EnsureHasItems();
        await _orderRepository.InsertAsync(order, autoSave: true);
        await _cartManager.ClearAsync(customerId);

        await PublishOrderPlacedAsync(order);

        return order;
    }

    [UnitOfWork]
    public async Task<Order> ChangeStatusAsync(Guid orderId, OrderStatus newStatus)
    {
        var order = await _orderRepository.GetAsync(orderId);
        var oldStatus = order.Status;

        if (newStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
        {
            await RestoreStockForOrderAsync(order);
        }

        order.ChangeStatus(newStatus);
        await _orderRepository.UpdateAsync(order, autoSave: true);

        await _localEventBus.PublishAsync(new OrderStatusChangedEvent
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            OldStatus = oldStatus.ToString(),
            NewStatus = order.Status.ToString()
        });

        return order;
    }

    public async Task<Order> CancelAsync(Guid orderId)
    {
        return await ChangeStatusAsync(orderId, OrderStatus.Cancelled);
    }

    /// <summary>
    /// Müşterinin kendi siparişini iptal etmesi (yalnızca Pending/Processing).
    /// </summary>
    [UnitOfWork]
    public async Task<Order> CancelForCustomerAsync(Guid orderId, Guid customerId)
    {
        var order = await GetWithDetailsAsync(orderId);
        EnsureCustomerOwnsOrder(order, customerId);
        EnsureCustomerCanCancel(order);

        return await ChangeStatusAsync(orderId, OrderStatus.Cancelled);
    }

    public async Task<Order> GetWithDetailsAsync(Guid id)
    {
        var queryable = await _orderRepository.WithDetailsAsync(x => x.Items);
        var order = queryable.FirstOrDefault(x => x.Id == id);
        if (order == null)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.OrderNotFound);
        }

        return order;
    }

    private async Task PublishOrderPlacedAsync(Order order)
    {
        await _localEventBus.PublishAsync(new OrderPlacedEvent
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(x => new OrderPlacedItemEvent
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice
            }).ToList()
        });
    }

    private async Task RestoreStockForOrderAsync(Order order)
    {
        foreach (var item in order.Items)
        {
            await _productManager.ReleaseStockAsync(item.ProductId, item.Quantity);
        }
    }

    private static void ValidateShippingInfo(string address, string city, string postalCode)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.OrderShippingAddressRequired);
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.OrderShippingCityRequired);
        }

        if (string.IsNullOrWhiteSpace(postalCode))
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.OrderShippingPostalCodeRequired);
        }
    }

    private static void EnsureCustomerOwnsOrder(Order order, Guid customerId)
    {
        if (order.CustomerId != customerId)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.OrderAccessDenied);
        }
    }

    private static void EnsureCustomerCanCancel(Order order)
    {
        if (order.Status is not (OrderStatus.Pending or OrderStatus.Processing))
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.OrderCannotBeCancelledByCustomer)
                .WithData("CurrentStatus", order.Status.ToString());
        }
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
}
