using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Core;

namespace Xugl.ImmediatelyChat.Data.EF
{
    public class Repository<T>:IRepository<T> where T:class
    {
        private readonly IDbContext _context;
        private IDbSet<T> _entites;


        public Repository(IDbContext context)
        {
            _context = context;
        }

        protected IDbSet<T> Entites
        {
            get {
                if (_entites == null)
                    _entites = _context.Set<T>();
                return _entites;
            }
        }

        public virtual IQueryable<T> Table
        {
            get { return this.Entites; }
        }

        //public virtual IQueryable<T> TableNoTracking
        //{
        //    get { return this.Entites.AsNoTracking<T>(); }
        //}

        public T Find(params object[] keyValues)
        {
            return this.Entites.Find(keyValues);
        }

        public virtual T Find(Expression<Func<T, bool>> predicate)
        {
            return this.Entites.Where(predicate).FirstOrDefault();
        }

        public int Insert(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException();
            }
            int resultCount=0;
            try
            {
                this.Entites.Add(entity);
                resultCount=_context.SaveChanges();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return resultCount;
        }


        public int BatchInsert(IList<T> entitys)
        {
            if(entitys==null)
            {
                throw new ArgumentNullException();
            }

            int resultCount = 0;
            try
            {
                foreach(T entity in entitys)
                {
                    this.Entites.Add(entity);
                }
                
                resultCount = _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resultCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TEntity"></param>
        /// <returns></returns>
        public int Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException();
            }
            int resultCount=0;
            try
            {
                
                this.Entites.Attach(entity);
                this._context.Entry<T>(entity).State = EntityState.Modified;
                resultCount = _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resultCount;       
        }



        public int BatchUpdate(IList<T> entitys)
        {
            if (entitys == null)
            {
                throw new ArgumentNullException();
            }

            if(entitys.Count==0)
            {
                return 0;
            }

            int resultCount = 0;
            try
            {
                foreach (T entity in entitys)
                {
                    this.Entites.Attach(entity);
                    this._context.Entry<T>(entity).State = EntityState.Modified;
                }
                resultCount = _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resultCount;   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TEntity"></param>
        /// <returns></returns>
        public int Delete(T entity)
        {
            if(entity==null)
            {
                throw new ArgumentNullException();
            }
            int resultCount = 0;
            try
            {
                this.Entites.Remove(entity);
                resultCount = _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resultCount;  
        }

        public int BatchDelete(IList<T> entitys)
        {
            int resultCount = 0;
            try
            {
                foreach(T entity in entitys)
                {
                    this.Entites.Remove(entity);
                }
                resultCount = _context.SaveChanges();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return resultCount;
        }


      
    }
}
