using System;
using Volo.Abp.Application.Dtos;

namespace ECommerceWeb.Products;

// AuditedEntityDto, Domain'deki FullAuditedAggregateRoot'un DTO karşılığıdır.
public class ProductDto : AuditedEntityDto<Guid>
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockCount { get; set; }
    public string ImageUrl { get; set; }
}