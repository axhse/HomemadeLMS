using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace HomemadeLMS.Services.Data
{
    public class StorageException : Exception
    {
        public StorageException(Exception innerException) : base(string.Empty, innerException)
        { }
    }

    public class Storage<TPrimaryKey, TEntity> : IStorage<TPrimaryKey, TEntity>
        where TEntity : class
    {
        private readonly GenericContext<TEntity> context;

        public Storage(GenericContext<TEntity> context)
        {
            this.context = context;
        }

        public async Task<TEntity?> Find(TPrimaryKey key)
        {
            if (key is null)
            {
                return null;
            }
            try
            {
                return await context.Items.FindAsync(key);
            }
            catch (Exception exception)
            {
                throw new StorageException(exception);
            }
        }

        public async Task<List<TEntity>> Select(Expression<Func<TEntity, bool>> selector)
        {
            if (selector is null)
            {
                return new List<TEntity>();
            }
            try
            {
                return await context.Items.Where(selector).ToListAsync();
            }
            catch (Exception exception)
            {
                throw new StorageException(exception);
            }
        }

        public async Task<bool> TryDelete(TEntity entity)
        {
            EntityEntry<TEntity>? entry = null;
            if (entity is null)
            {
                return false;
            }
            try
            {
                entry = context.Remove(entity);
                await context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException)
            {
                Detach(entry);
                return false;
            }
            catch (DbUpdateConcurrencyException)
            {
                Detach(entry);
                return false;
            }
            catch (Exception exception)
            {
                throw new StorageException(exception);
            }
        }

        public async Task<bool> TryInsert(TEntity entity)
        {
            EntityEntry<TEntity>? entry = null; 
            if (entity is null)
            {
                return false;
            }
            try
            {
                entry = await context.AddAsync(entity);
                await context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException)
            {
                Detach(entry);
                return false;
            }
            catch (DbUpdateException)
            {
                Detach(entry);
                return false;
            }
            catch (Exception exception)
            {
                throw new StorageException(exception);
            }
        }

        public async Task Update(TEntity entity)
        {
            if (entity is null)
            {
                return;
            }
            try
            {
                context.Update(entity);
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                throw new StorageException(exception);
            }
        }

        public async Task Update(TPrimaryKey key, TEntity entity)
        {
            await Update(entity);
        }

        private static void Detach(EntityEntry<TEntity>? entry)
        {
            if (entry is not null)
            {
                entry.State = EntityState.Detached;
            }
        }
    }
}