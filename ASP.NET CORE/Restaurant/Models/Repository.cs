using System;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;

namespace Restaurant.Models;

public class Repository<T> : IRepository<T> where T : class
{
    //So the two fields aren’t random — 
    // they’re what make your generic repository work for any entity without hardcoding Customer, Order, etc.
    protected ApplicationDbContext? _context { get; set; }
    //If T = Customer, then _dbSet is basically the Customers table inside your database.
    private DbSet<T> _dbSet { get; set; }

    public Repository(ApplicationDbContext context)
    {
        //When you create a repository, you inject the ApplicationDbContext.
        //_context stores it for later.
        _context = context;
        //Get the correct table from the database for this entity
        _dbSet = context.Set<T>();
    }
    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        T entity = await _dbSet.FindAsync(id);
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id, QueryOptions<T> options)
    {
        IQueryable<T> query = _dbSet;
        if (options.HasWhere)
        {
            query = query.Where(options.Where!);
        }
        if (options.HasOrderBy)
        {
            query = query.OrderBy(options.OrderBy!);
        }
        foreach (string include in options.GetIncludes())
        {
            query = query.Include(include);
        }
        var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.FirstOrDefault();
        string primaryKeyName = key?.Name;
        return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, primaryKeyName) == id);

    }

    public async Task UpdateAsync(T entity)
    {
        _context.Update(entity);
        await _context.SaveChangesAsync();
    }
}

