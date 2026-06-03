using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using ECommerceWeb.Products;

namespace ECommerceWeb.Categories;

public class CategoryManager : DomainService
{
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Product, Guid> _productRepository;

    public CategoryManager(
        IRepository<Category, Guid> categoryRepository,
        IRepository<Product, Guid> productRepository)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
    }

    public async Task<Category> CreateAsync(string name, string? description = null)
    {
        await EnsureNameIsUniqueAsync(name);

        var category = new Category(GuidGenerator.Create(), name, description);
        return await _categoryRepository.InsertAsync(category, autoSave: true);
    }

    public async Task<Category> UpdateAsync(Guid id, string name, string? description, bool isActive)
    {
        await EnsureNameIsUniqueAsync(name, id);

        var category = await _categoryRepository.GetAsync(id);
        var wasActive = category.IsActive;

        category.SetName(name);
        category.SetDescription(description);

        if (isActive)
        {
            category.Activate();
        }
        else
        {
            category.Deactivate();
            if (wasActive)
            {
                await DeactivateProductsInCategoryAsync(id);
            }
        }

        return await _categoryRepository.UpdateAsync(category, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (await _productRepository.AnyAsync(x => x.CategoryId == id))
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.CategoryHasProducts);
        }

        await _categoryRepository.DeleteAsync(id);
    }

    public async Task<Category> GetAsync(Guid id)
    {
        return await _categoryRepository.GetAsync(id);
    }

    private async Task EnsureNameIsUniqueAsync(string name, Guid? excludeId = null)
    {
        var exists = excludeId.HasValue
            ? await _categoryRepository.AnyAsync(x => x.Name == name && x.Id != excludeId.Value)
            : await _categoryRepository.AnyAsync(x => x.Name == name);

        if (exists)
        {
            throw new BusinessException(ECommerceWebDomainErrorCodes.CategoryNameAlreadyExists)
                .WithData("Name", name);
        }
    }

    private async Task DeactivateProductsInCategoryAsync(Guid categoryId)
    {
        var products = await _productRepository.GetListAsync(x => x.CategoryId == categoryId && x.IsActive);
        foreach (var product in products)
        {
            product.Deactivate();
            await _productRepository.UpdateAsync(product);
        }
    }
}
