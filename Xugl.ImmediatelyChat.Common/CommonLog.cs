using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Common
{
    public class CommonLog:ICommonLog
    {
        private StringBuilder _LogMsgs = new StringBuilder();
        private object temp = new object();

        public string GetLogMsg
        {
            get
            {

                return _LogMsgs.ToString(0, _LogMsgs.Length > 5000 ? 5000 : _LogMsgs.Length);
            }
        }


        private bool _isRecord;
        public bool IsRecord
        {
            get
            {
                lock (temp)
                {
                    return _isRecord;
                }
            }
            set
            {
                lock (temp)
                {
                    //if (value)
                    //{
                    //    _LogMsgs.Clear();
                    //}
                    _isRecord = value;
                }
            }
        }

        public void Log(string msg)
        {
            if (_LogMsgs.Length>5000)
            {
                _LogMsgs.Clear();
            }

            _LogMsgs.Append("\r\n" + msg);
        }
    }
}
