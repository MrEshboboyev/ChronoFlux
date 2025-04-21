using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Repositories;

public abstract class Repository<TEntity>(DbContext dbContext): IRepository<TEntity>
    where TEntity : class, IEntity
{
    public void Add(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        dbContext.Add(entity);
    }

    public void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        dbContext.Update(entity);
    }

    public void Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        dbContext.Remove(entity);
    }

    public ValueTask<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Set<TEntity>().FindAsync(
            [id],
            cancellationToken: cancellationToken
        );

    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
}
