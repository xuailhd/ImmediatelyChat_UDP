using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.SocketEngine
{
    internal class SocketAsyncEventArgsPool<T> where T:IDisposable,new ()
    {
        Stack<T> _Pool;

        public SocketAsyncEventArgsPool()
        {

            _Pool = new Stack<T>();

        }

        public void Push(T item)
        {

            if (item == null) return;

            lock (_Pool)
            {

                _Pool.Push(item);

            }

        }

        public T PopOrNew()
        {

            //if (Count == 0)

            //    return new T();

            return Pop();

        }

        public T Pop()
        {

            lock (_Pool)
            {

                return _Pool.Pop();

            }

        }

        public int Count
        {

            get { return _Pool.Count; }

        }

        public void Clear()
        {

            while (Count > 0)
            {

                Pop().Dispose();

            }

        }
    }
}
