using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WirelessCom.Application.Database.Repositories;
using WirelessCom.Infrastructure.Persistence;

namespace WirelessCom.Infrastructure.Database.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ClimateDbContext Context;

    /// <summary>
    ///     Creates a new <see cref="Repository{T}" />.
    /// </summary>
    /// <param name="context">The context that will be used.</param>
    public Repository(ClimateDbContext context)
    {
        Context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(T entity)
    {
        await Context.Set<T>().AddAsync(entity).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Add(T entity)
    {
        Context.Set<T>().Add(entity);
    }

    /// <inheritdoc />
    public Task AddRangeAsync(IEnumerable<T> entities)
    {
        return Context.Set<T>().AddRangeAsync(entities);
    }

    /// <inheritdoc />
    public async Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate)
    {
        return await Context.Set<T>().Where(predicate).ToListAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync(ulong id)
    {
        return await Context.Set<T>().FindAsync(id).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Context.Set<T>().ToListAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Remove(T entity)
    {
        Context.Set<T>().Remove(entity);
    }

    /// <inheritdoc />
    public void RemoveRange(IEnumerable<T> entities)
    {
        Context.Set<T>().RemoveRange(entities);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await Context.Set<T>().CountAsync(predicate).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int> CountAllAsync()
    {
        return await Context.Set<T>().CountAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public int Count(Expression<Func<T, bool>> predicate)
    {
        return Context.Set<T>().Count(predicate);
    }

    /// <inheritdoc />
    public int CountAll()
    {
        return Context.Set<T>().Count();
    }
}