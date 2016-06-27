using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Core
{
    /// <summary>
    /// Provides access to all "singletons" stored by <see cref="Singleton{T}"/>.
    /// </summary>
    public class Singleton
    {
        static readonly IDictionary<Type, object> allSingletons;
        static Singleton()
        {
            allSingletons = new Dictionary<Type, object>();
        }

        public static IDictionary<Type,object> ALlSingletons
        {
            get { return allSingletons; }
        }
    }

    public class Singleton<T>:Singleton
    {
        static T instance;

        public static T Instance
        {
            get { return instance; }
            set 
            {
                instance = value;
                ALlSingletons[typeof(T)] = value;
            }
        }
    }

    public class SingletonList<T>:Singleton<IList<T>>
    {
        static SingletonList()
        {
            Singleton<IList<T>>.Instance = new List<T>();
        }

        public new static IList<T> Instance
        {
            get { return Singleton<IList<T>>.Instance; }
        }
    }

    public class SingletonDictionary<Tkey,TValue>:Singleton<IDictionary<Tkey,TValue>>
    {
        static SingletonDictionary()
        {
            Singleton<IDictionary<Tkey, TValue>>.Instance = new Dictionary<Tkey, TValue>();
        }

        public new static IDictionary<Tkey,TValue> Instance
        {
            get { return Singleton<IDictionary<Tkey, TValue>>.Instance; }
        }
    }
}
