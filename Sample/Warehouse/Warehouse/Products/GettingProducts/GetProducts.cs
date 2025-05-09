﻿using Microsoft.EntityFrameworkCore;
using Warehouse.Core.Queries;

namespace Warehouse.Products.GettingProducts;

internal class HandleGetProducts(IQueryable<Product> products)
    : IQueryHandler<GetProducts, IReadOnlyList<ProductListItem>>
{
    public async ValueTask<IReadOnlyList<ProductListItem>> Handle(GetProducts query, CancellationToken ct)
    {
        var (filter, page, pageSize) = query;

        var filteredProducts = string.IsNullOrEmpty(filter)
            ? products
            : products
                .Where(p =>
                    p.Sku.Value.Contains(query.Filter!) ||
                    p.Name.Contains(query.Filter!) ||
                    p.Description!.Contains(query.Filter!)
                );

        return await filteredProducts
            .Skip(pageSize * (page - 1))
            .Take(pageSize)
            .Select(p => new ProductListItem(p.Id, p.Sku.Value, p.Name))
            .ToListAsync(ct);
    }
}

public record GetProducts
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;

    public string? Filter { get; }

    public int Page { get; }

    public int PageSize { get; }

    private GetProducts(string? filter, int page, int pageSize)
    {
        Filter = filter;
        Page = page;
        PageSize = pageSize;
    }

    public static GetProducts Create(string? filter, int? page, int? pageSize)
    {
        page ??= DefaultPage;
        pageSize ??= DefaultPageSize;

        if (page <= 0)
            throw new ArgumentOutOfRangeException(nameof(page));

        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        return new (filter, page.Value, pageSize.Value);
    }

    public void Deconstruct(out string? filter, out int page, out int pageSize)
    {
        filter = Filter;
        page = Page;
        pageSize = PageSize;
    }
}

public record ProductListItem(
    Guid Id,
    string Sku,
    string Name
);
