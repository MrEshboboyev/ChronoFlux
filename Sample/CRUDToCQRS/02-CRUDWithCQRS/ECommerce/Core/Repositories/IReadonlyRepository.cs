using ECommerce.Core.Entities;

namespace ECommerce.Core.Repositories;

public interface IReadonlyRepository<TEntity> where TEntity : class, IEntity
{
    Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    IQueryable<TEntity> Query();
}
