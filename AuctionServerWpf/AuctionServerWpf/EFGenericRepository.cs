using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionServerWpf
{
    public class EFGenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        AuctionContext _context;
        DbSet<TEntity> _dbSet;
        public EFGenericRepository(AuctionContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }
        public IEnumerable<TEntity> Get()
        {
            return _dbSet.AsNoTracking().ToList();
        }
        public IEnumerable<TEntity> Get(Func<TEntity, bool> predicate)
        {
            return _dbSet.AsNoTracking().Where(predicate).ToList();
        }
        public TEntity FindById(int id)
        {
            return _dbSet.Find(id);
        }
        public void Add(TEntity item)
        {
            _dbSet.Add(item);
        }

        public void Remove(TEntity item)
        {
            _dbSet.Remove(item);
        }

        public void Update(TEntity item)
        {
            _context.Entry(item).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void AttachItem(TEntity entity)
        {
            _dbSet.Attach(entity);
        }

        public int GetCount()
        {
            return _dbSet.Count();
        }

        //public void ChangeItemProperty(TEntity item)
        //{
        //    TEntity entity = _dbSet.Where(n => n.Equals(item)).First();
        //    entity = item;

        //}
    }
}
