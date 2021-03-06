﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.SocketEngine;

namespace Xugl.ImmediatelyChat.MessageMainServer
{
    public class CommonVariables
    {
        public static string PSIP
        {
            get;
            set;
        }

        public static int PSPort
        {
            get;
            set;
        }

        public static string MMSIP
        {
            get;
            set;
        }

        public static int MMSPort
        {
            get;
            set;
        }

        public static bool IsBeginMessageService { get; set; }

        public static string ArrangeStr { get; set; }

        #region MCSServers

        public static IList<MCSServer> MCSServers
        {
            get
            {
                return SingletonList<MCSServer>.Instance;
            }
        }

        #endregion

        #region Log tool

        public static ICommonLog LogTool
        {
            get
            {
                if (Singleton<ICommonLog>.Instance == null)
                {
                    Singleton<ICommonLog>.Instance = Xugl.ImmediatelyChat.Core.DependencyResolution.ObjectContainerFactory.CurrentContainer.Resolver<ICommonLog>();
                }
                return Singleton<ICommonLog>.Instance;
            }
        }

        #endregion

        #region  OperateFile
        public static IOperateFile OperateFile
        {
            get
            {
                if (Singleton<IOperateFile>.Instance == null)
                {
                    Singleton<IOperateFile>.Instance = Xugl.ImmediatelyChat.Core.DependencyResolution.ObjectContainerFactory.CurrentContainer.Resolver<IOperateFile>();
                }
                return Singleton<IOperateFile>.Instance;
            }
        }


        private static string m_ConfigFilePath;
        public static string ConfigFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(m_ConfigFilePath))
                {
                    m_ConfigFilePath = AppDomain.CurrentDomain.BaseDirectory + "config.txt";
                }
                return m_ConfigFilePath;
            }
        }
        #endregion


        public static ICommonFunctions CommonFunctions
        {
            get
            {
                if (Singleton<ICommonFunctions>.Instance == null)
                {
                    Singleton<ICommonFunctions>.Instance = Xugl.ImmediatelyChat.Core.DependencyResolution.ObjectContainerFactory.CurrentContainer.Resolver<ICommonFunctions>();
                }
                return Singleton<ICommonFunctions>.Instance;
            }
        }

        public static BufferContorl UAInfoContorl
        {
            get
            {
                if (Singleton<BufferContorl>.Instance == null)
                {
                    Singleton<BufferContorl>.Instance = new BufferContorl();
                }
                return Singleton<BufferContorl>.Instance;
            }
        }

        public static UDPSocketListener Listener
        {
            get
            {
                if (Singleton<UDPSocketListener>.Instance == null)
                {
                    Singleton<UDPSocketListener>.Instance = new UDPSocketListener();
                }
                return Singleton<UDPSocketListener>.Instance;
            }
        }
    }
}
