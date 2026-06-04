using System;

namespace ECommerceWeb.Events;

public class ProductLowStockEvent
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int Threshold { get; set; }
}
