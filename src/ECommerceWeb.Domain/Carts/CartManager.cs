using System;
using System.Linq;
using System.Threading.Tasks;
using ECommerceWeb.Data;
using ECommerceWeb.Products;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ECommerceWeb.Carts;

public class CartManager : DomainService
{
    public const int MaxDistinctItems = 50;

    private readonly IRepository<Cart, Guid> _cartRepository;
    private readonly ProductManager _productManager;

    public CartManager(
        IRepository<Cart, Guid> cartRepository,
        ProductManager productManager)
    {
        _cartRepository = cartRepository;
        _productManager = productManager;
    }

    public async Task<Cart> GetOrCreateAsync(Guid customerId)
    {
        var cart = await _cartRepository.FirstOrDefaultAsync(x => x.CustomerId == customerId);
        if (cart != null)
        {
            return cart;
        }

        cart = new Cart(GuidGenerator.Create(), customerId);
        return await _cartRepository.InsertAsync(cart, autoSave: true);
    }

    public async Task<Cart> AddItemAsync(Guid customerId, Guid productId, int quantity)
    {
        Check.Positive(quantity, nameof(quantity));

        var product = await _productManager.GetAvailableAsync(productId, quantity);
        var cart = await GetOrCreateAsync(customerId);

        if (!cart.Items.Any(x => x.ProductId == productId))
        {
            EnsureCartCapacity(cart);
        }

        cart.AddOrUpdateItem(
            product.Id,
            product.Name,
            quantity,
            product.Price,
            ECommerceWebCatalogData.ResolveDisplayImageUrl(product.Name, product.ImageUrl));

        return await _cartRepository.UpdateAsync(cart, autoSave: true);
    }

    public async Task<Cart> UpdateItemQuantityAsync(Guid customerId, Guid productId, int quantity)
    {
        Check.Positive(quantity, nameof(quantity));

        await _productManager.GetAvailableAsync(productId, quantity);

        var cart = await GetOrCreateAsync(customerId);
        cart.UpdateItemQuantity(productId, quantity);

        return await _cartRepository.UpdateAsync(cart, autoSave: true);
    }

    public async Task<Cart> RemoveItemAsync(Guid customerId, Guid productId)
    {
        var cart = await GetOrCreateAsync(customerId);
        cart.RemoveItem(productId);
        return await _cartRepository.UpdateAsync(cart, autoSave: true);
    }

    public async Task<Cart> ClearAsync(Guid customerId)
    {
        var cart = await GetOrCreateAsync(customerId);
        cart.Clear();
        return await _cartRepository.UpdateAsync(cart, autoSave: true);
    }

    public async Task<Cart> GetWithItemsAsync(Guid customerId)
    {
        var queryable = await _cartRepository.WithDetailsAsync(x => x.Items);
        var cart = queryable.FirstOrDefault(x => x.CustomerId == customerId);
        return cart ?? await GetOrCreateAsync(customerId);
    }

    /// <summary>
    /// Sipariş öncesi sepetteki fiyatları güncel ürün fiyatlarıyla senkronize eder.
    /// </summary>
    public async Task<Cart> SyncPricesAsync(Guid customerId)
    {
        var cart = await GetWithItemsAsync(customerId);
        if (cart.IsEmpty)
        {
            return cart;
        }

        var changed = false;
        foreach (var item in cart.Items.ToList())
        {
            var product = await _productManager.GetAvailableAsync(item.ProductId, item.Quantity);
            var displayImageUrl = ECommerceWebCatalogData.ResolveDisplayImageUrl(product.Name, product.ImageUrl);
            if (item.UnitPrice != product.Price
                || item.ProductName != product.Name
                || !string.Equals(item.ImageUrl, displayImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                cart.RefreshItemPrice(product.Id, product.Name, product.Price, displayImageUrl);
                changed = true;
            }
        }

        return changed
            ? await _cartRepository.UpdateAsync(cart, autoSave: true)
            : cart;
    }

    private static void EnsureCartCapacity(Cart cart)
    {
        if (cart.Items.Count >= MaxDistinctItems)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.CartMaxItemsExceeded)
                .WithData("MaxItems", MaxDistinctItems);
        }
    }
}
