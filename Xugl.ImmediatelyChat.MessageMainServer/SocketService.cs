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

namespace Xugl.ImmediatelyChat.MessageMainServer
{
    class SocketService
    {

        SocketListener socketListener;

        public void StartMMSService()
        {
            ThreadStart threadStart = new ThreadStart(MMSService);
            Thread thread = new Thread(threadStart);
            thread.Start();
        }

        private void MMSService()
        {
            try
            {
                //1.Post information to PS 
                CommonVariables.LogTool.Log("Begin post server information to portal server " + CommonVariables.PSIP);
                string PSMsg = PostServerInfo();
                if (!string.IsNullOrEmpty(PSMsg))
                {
                    CommonVariables.LogTool.Log("Wait Start Command");
                    CommonVariables.LogTool.Log("IP:" + CommonVariables.MMSIP + "    Port:" + CommonVariables.MMSPort.ToString());

                    socketListener = new SocketListener();
                    socketListener.BeginService();
                    return;
                }
                CommonVariables.LogTool.Log("Post PS Failed");
            }
            catch (Exception ex)
            {
                CommonVariables.LogTool.Log(ex.Message + ex.StackTrace);
            }
        }

        private string PostServerInfo()
        {
            StreamReader sReader = null;
            WebResponse webResponse = null;
            Stream stream=null;
            try
            {
                string arrangeStr = CommonVariables.OperateFile.GetConfig(CommonVariables.ConfigFilePath, CommonFlag.F_ArrangeChars);

                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://" + CommonVariables.PSIP
                    + ":" + CommonVariables.PSPort + "/AppServer/CollectMMS?ip=" + CommonVariables.MMSIP + "&&port=" + CommonVariables.MMSPort
                    + "&&arrangeStr=" + arrangeStr);

                myRequest.Method = "Get";

                webResponse = myRequest.GetResponse();

                stream=webResponse.GetResponseStream();

                sReader = new StreamReader(stream, Encoding.GetEncoding("utf-8"));

                string result = sReader.ReadToEnd();
               
                return result;
            }
            catch(Exception ex)
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

        public void StopMMSService()
        {
            //CommonVariables.UAInfoContorl.StopMainThread();
            //socketListener.CloseListener();
            //socketListener = null;
        }
    }
}
