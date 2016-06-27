using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.SocketEngine
{
    public class AsyncUserToken:IDisposable
    {
        private Socket m_Socket;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        bool disposed = false;

        public Socket Socket
        {
            get
            {
                return m_Socket;
            }
            set
            {
                m_Socket = value;
            }
        }


        public string IP { get; set; }
        public int Port { get; set; }

        public AsyncUserToken()
            : this(null)
        {

        }

        public AsyncUserToken(Socket socket)
        {
            m_Socket = socket;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);   
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }
    }
}
