using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Repositories;

public class CRUDRepository<TEntity>: ICRUDRepository<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbContext dbContext;

    protected CRUDRepository(DbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public TEntity Add(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = dbContext.Add(entity);

        return entry.Entity;
    }

    public TEntity Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = dbContext.Update(entity);

        return entry.Entity;
    }

    public TEntity Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = dbContext.Remove(entity);

        return entry.Entity;
    }

    public ValueTask<TEntity?> FindByIdAsync(Guid id, CancellationToken ct) =>
        dbContext.Set<TEntity>().FindAsync([id], ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        dbContext.SaveChangesAsync(ct);

    public IQueryable<TEntity> Query() =>
        dbContext.Set<TEntity>();
}
