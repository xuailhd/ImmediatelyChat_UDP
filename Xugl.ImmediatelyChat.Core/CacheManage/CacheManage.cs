using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;


namespace Xugl.ImmediatelyChat.Core
{
    public class CacheManage:ICacheManage
    {
        private const string tag="ImmediatelyChatCache";

        public void AddCache<T>(string key, T value)
        {
            AddCache<T>(key,value,0);
        }

        public void AddCache<T>(string key, T value,int timeoutSec)
        {

            if(MemoryCache.Default.Contains(tag + key))
            {
                MemoryCache.Default.Remove(tag + key);
            }

            CacheItem cacheItem=new CacheItem(tag + key,value);
            CacheItemPolicy cacheItemPolicy=new CacheItemPolicy();
            
            if(timeoutSec>0)
            {
                cacheItemPolicy.AbsoluteExpiration=DateTime.Now.AddSeconds(timeoutSec);
            }
            else
            {
                cacheItemPolicy.AbsoluteExpiration = DateTime.MaxValue;
            }

            MemoryCache.Default.Add(cacheItem,cacheItemPolicy);
        }

        public T GetCache<T>(string key)
        {

            if (MemoryCache.Default.Get(tag + key) != null)
            {
                return (T)MemoryCache.Default.Get(tag + key);
            }

            return default(T);
        }
    }
}
