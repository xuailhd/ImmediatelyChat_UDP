using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.Core;

namespace Xugl.ImmediatelyChat.MessageDataServer
{
    class SocketService
    {
        private SocketListener socketListener;
        public void StartConnectMMS()
        {
            ThreadStart threadStart = new ThreadStart(ConnectPS);
            Thread thread = new Thread(threadStart);
            thread.Start();
        }

        private void ConnectPS()
        {
            try
            {
                //1.Post information to PS 
                CommonVariables.LogTool.Log("Begin connect Portal Server:" + CommonVariables.PSIP + ", Port:" + CommonVariables.PSPort.ToString());
                string PSMsg = PostServerInfo();

                if (!string.IsNullOrEmpty(PSMsg))
                {
                    //2.begin MDS service
                    CommonVariables.LogTool.Log("Wait Start Command");
                    CommonVariables.LogTool.Log("IP:" + CommonVariables.MDSIP + "    Port:" + CommonVariables.MDSPort.ToString());

                    socketListener = new SocketListener();
                    socketListener.BeginService();
                    return;
                }
                CommonVariables.LogTool.Log("Post PS Failed");
            }
            catch (Exception ex)
            {
                CommonVariables.LogTool.Log(ex.Message);
            }
        }

        /// <summary>
        /// Get MMS information from PS 
        /// </summary>
        /// <returns></returns>
        private string PostServerInfo()
        {
            StreamReader sReader = null;
            WebResponse webResponse = null;
            Stream stream = null;
            try
            {
                string arrangeStr = CommonVariables.OperateFile.GetConfig(CommonVariables.ConfigFilePath, CommonFlag.F_ArrangeChars);

                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://" + CommonVariables.PSIP
                    + ":" + CommonVariables.PSPort + "/AppServer/CollectMDS?ip=" + CommonVariables.MDSIP
                    + "&&port=" + CommonVariables.MDSPort + "&&arrangeStr=" + arrangeStr);

                myRequest.Method = "Get";
                webResponse = myRequest.GetResponse();
                stream = webResponse.GetResponseStream();
                sReader = new StreamReader(stream, Encoding.GetEncoding("utf-8"));
                string result = sReader.ReadToEnd();

                return result;
            }
            catch (Exception ex)
            {
                CommonVariables.LogTool.Log(ex.Message);
                return null;
            }
            finally
            {
                if (sReader != null) { sReader.Close(); }
                if (stream != null) { stream.Close(); }
                if (webResponse != null) { webResponse.Close(); }
            }
        }

        public void StopMDSService()
        {
            CommonVariables.MessageContorl.StopMainThread();
            //socketListener.CloseListener();
            //socketListener = null;
        }
    }
}
