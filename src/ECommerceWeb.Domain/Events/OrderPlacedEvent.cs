using System;
using System.Collections.Generic;

namespace ECommerceWeb.Events;

public class OrderPlacedEvent
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderPlacedItemEvent> Items { get; set; } = new();
}

public class OrderPlacedItemEvent
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
