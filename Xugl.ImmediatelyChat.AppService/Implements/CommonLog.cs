using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.AppService.Interface;

namespace Xugl.ImmediatelyChat.AppService
{
    public class CommonLog:ICommonLog
    {
        private StringBuilder _LogMsgs = new StringBuilder();

        public string LogMsg
        {
            get
            {
                lock (this)
                {
                    return _LogMsgs.ToString();
                }
            }
        }


        private bool _isRecord;
        public bool IsRecord {
            get {
                lock (this)
                {
                    return _isRecord;
                }
            }
            set
            {
                lock (this)
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
            lock (this)
            {
                _isRecord = false;
                if (_LogMsgs.Length > 0)
                {
                    _LogMsgs.Append("\r\n" + msg);
                }
                else
                {
                    _LogMsgs.Append(msg);
                }
            }
        }
    }
}
