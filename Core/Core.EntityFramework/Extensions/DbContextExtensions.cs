using Microsoft.EntityFrameworkCore;

namespace Core.EntityFramework.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="DbContext"/> class to simplify common operations.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Adds a new entity to the database context or updates it if it already exists in the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being added or updated.</typeparam>
    /// <param name="dbContext">The database context.</param>
    /// <param name="entity">The entity to add or update.</param>
    public static void AddOrUpdate<TEntity>(this DbContext dbContext, TEntity entity)
        where TEntity : class
    {
        var dbSet = dbContext.Set<TEntity>();
        var entry = dbContext.Entry(entity);

        if (entry.State == EntityState.Detached)
        {
            // If the entity is not being tracked, add it to the context.
            dbSet.Add(entity);
        }
        // Otherwise, the entity is already being tracked and will be updated during SaveChanges.
    }
}
