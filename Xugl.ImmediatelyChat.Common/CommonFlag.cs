using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Common
{
    public class CommonFlag
    {
        private static string f_ArrangeChars = "ArrangeChars";
        public static string F_ArrangeChars { get { return f_ArrangeChars; } }

        private static string f_DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        public static string F_DateTimeFormat { get { return f_DateTimeFormat; } }

        private static string f_MinDatetime = "1900-01-01 00:00:00.000";
        public static string F_MinDatetime { get { return f_MinDatetime; } }

        private static string f_PSCallMMSStart = "PSCallMMSStart";
        private static string f_PSCallMCSStart = "PSCallMCSStart";
        private static string f_PSCallMDSStart = "PSCallMDSStart";
        private static string f_PSSendMMSUser = "PSSendMMSUser";
        public static string F_PSCallMMSStart { get { return f_PSCallMMSStart; } }
        public static string F_PSCallMCSStart { get { return f_PSCallMCSStart; } }
        public static string F_PSCallMDSStart { get { return f_PSCallMDSStart; } }
        public static string F_PSSendMMSUser { get { return f_PSSendMMSUser; } }

        //private static string f_MMSVerifyMCS = "MCS";
        //private static string f_MMSVerifyMDS = "MDS";
        //private static string f_MMSCallMCSStart = "MCS start";
        //private static string f_MMSCallMDSStart = "MDS start";
        //private static string f_MMSReciveStopMCS = "stopMCS";
        //private static string f_MMSReciveStopMDS = "stopMDS";
        private static string f_MMSVerifyUA = "VerifyUA";
        private static string f_MMSVerifyUAGetUAInfo = "MMSVerifyUAGetUAInfo";
        private static string f_MMSVerifyFBUAGetUAInfo = "MMSVerifyFBUAGetUAInfo";
        private static string f_MMSVerifyMCSFBGetUAInfo = "VerifyMCSFBGetUAInfo";
        private static string f_MMSVerifyUASearch = "MMSVerifyUASearch";
        private static string f_MMSVerifyUAFBSearch = "MMSVerifyUAFBSearch";
        private static string f_MMSVerifyUAAddPerson = "MMSVerifyUAAddPerson";
        private static string f_MMSVerifyUAAddGroup = "MMSVerifyUAAddGroup";
        //public static string F_MMSVerifyMCS { get { return f_MMSVerifyMCS; } }
        //public static string F_MMSVerifyMDS { get { return f_MMSVerifyMDS; } }
        //public static string F_MMSCallMCSStart { get { return f_MMSCallMCSStart; } }
        //public static string F_MMSCallMDSStart { get { return f_MMSCallMDSStart; } }
        //public static string F_MMSReciveStopMCS { get { return f_MMSReciveStopMCS; } }
        //public static string F_MMSReciveStopMDS { get { return f_MMSReciveStopMDS; } }
        public static string F_MMSVerifyUA { get { return f_MMSVerifyUA; } }
        public static string F_MMSVerifyUAGetUAInfo { get { return f_MMSVerifyUAGetUAInfo; } }
        public static string F_MMSVerifyFBUAGetUAInfo { get { return f_MMSVerifyFBUAGetUAInfo; } }
        public static string F_MMSVerifyMCSFBGetUAInfo { get { return f_MMSVerifyMCSFBGetUAInfo; } }
        public static string F_MMSVerifyUASearch { get { return f_MMSVerifyUASearch; } }
        public static string F_MMSVerifyUAFBSearch { get { return f_MMSVerifyUAFBSearch; } }
        public static string F_MMSVerifyUAAddPerson { get { return f_MMSVerifyUAAddPerson; } }
        public static string F_MMSVerifyUAAddGroup { get { return f_MMSVerifyUAAddGroup; } }

        private static string f_MCSVerifyUA = "VerifyAccount";
        private static string f_MCSVerifyUAMSG = "VerifyMSG";
        private static string f_MCSVerifyUAGetMSG = "VerifyGetMSG";
        private static string f_MCSVerfiyMDSMSG = "VerifyMDSMSG";
        private static string f_MCSReceiveUAFBMSG = "VerifyFBMSG";
        private static string f_MCSReceiveUAInfo = "VerifyUAInfo";
        private static string f_MCSReceiveMMSUAUpdateTime = "VerifyUAUpdateTime";
        public static string F_MCSVerifyUA { get { return f_MCSVerifyUA; } }
        public static string F_MCSVerifyUAMSG { get { return f_MCSVerifyUAMSG; } }
        public static string F_MCSVerifyUAGetMSG { get { return f_MCSVerifyUAGetMSG; } }
        public static string F_MCSVerfiyMDSMSG { get { return f_MCSVerfiyMDSMSG; } }
        public static string F_MCSReceiveUAFBMSG { get { return f_MCSReceiveUAFBMSG; } }
        public static string F_MCSReceiveUAInfo { get { return f_MCSReceiveUAInfo; } }
        public static string F_MCSReceiveMMSUAUpdateTime { get { return f_MCSReceiveMMSUAUpdateTime; } }

        private static string f_MDSVerifyMCSMSG = "VerifyMCSMSG";
        private static string f_MDSVerifyMCSGetMSG = "VerifyMCSGetMSG";
        private static string f_MDSReciveMCSFBMSG = "VerifyMCSFBMSG";

        public static string F_MDSVerifyMCSMSG { get { return f_MDSVerifyMCSMSG; } }
        public static string F_MDSVerifyMCSGetMSG { get { return f_MDSVerifyMCSGetMSG; } }
        public static string F_MDSReciveMCSFBMSG { get { return f_MDSReciveMCSFBMSG; } }

        public static object lockobject = new object();
    }
}
