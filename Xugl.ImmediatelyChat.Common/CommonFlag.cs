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

        private static string f_MMSVerifyUA = "VerifyUA";
        private static string f_MMSVerifyUAGetUAInfo = "MMSVerifyUAGetUAInfo";
        private static string f_MMSVerifyFBUAGetUAInfo = "MMSVerifyFBUAGetUAInfo";
        private static string f_MMSVerifyMCSGetUAInfo = "MMSVerifyMCSGetUAInfo";
        private static string f_MMSVerifyMCSFBGetUAInfo = "MMSVerifyMCSFBGetUAInfo";
        private static string f_MMSVerifyUASearch = "MMSVerifyUASearch";
        private static string f_MMSVerifyUAFBSearch = "MMSVerifyUAFBSearch";
        private static string f_MMSVerifyUAAddPerson = "MMSVerifyUAAddPerson";
        private static string f_MMSVerifyUAAddGroup = "MMSVerifyUAAddGroup";
        public static string F_MMSVerifyUA { get { return f_MMSVerifyUA; } }
        public static string F_MMSVerifyUAGetUAInfo { get { return f_MMSVerifyUAGetUAInfo; } }
        public static string F_MMSVerifyFBUAGetUAInfo { get { return f_MMSVerifyFBUAGetUAInfo; } }
        public static string F_MMSVerifyMCSGetUAInfo { get { return f_MMSVerifyMCSGetUAInfo; } }
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
        private static string f_MCSVerifyUAInfo = "MCSVerifyUAInfo";
        private static string f_MCSReceiveMMSUAUpdateTime = "VerifyUAUpdateTime";
        private static string f_MCSVerifyMMSUpdateGroupSub = "MCSVerifyMMSUpdateGroupSub";
        public static string F_MCSVerifyUA { get { return f_MCSVerifyUA; } }
        public static string F_MCSVerifyUAMSG { get { return f_MCSVerifyUAMSG; } }
        public static string F_MCSVerifyUAGetMSG { get { return f_MCSVerifyUAGetMSG; } }
        public static string F_MCSVerfiyMDSMSG { get { return f_MCSVerfiyMDSMSG; } }
        public static string F_MCSReceiveUAFBMSG { get { return f_MCSReceiveUAFBMSG; } }
        public static string F_MCSVerifyUAInfo { get { return f_MCSVerifyUAInfo; } }
        public static string F_MCSReceiveMMSUAUpdateTime { get { return f_MCSReceiveMMSUAUpdateTime; } }
        public static string F_MCSVerifyMMSUpdateGroupSub { get { return f_MCSVerifyMMSUpdateGroupSub; } }

        private static string f_MDSVerifyMCSMSG = "VerifyMCSMSG";
        private static string f_MDSVerifyMCSGetMSG = "VerifyMCSGetMSG";
        private static string f_MDSReciveMCSFBMSG = "VerifyMCSFBMSG";
        public static string F_MDSVerifyMCSMSG { get { return f_MDSVerifyMCSMSG; } }
        public static string F_MDSVerifyMCSGetMSG { get { return f_MDSVerifyMCSGetMSG; } }
        public static string F_MDSReciveMCSFBMSG { get { return f_MDSReciveMCSFBMSG; } }


        private static string f_UAVerifyUAInfo = "UAVerifyUAInfo";
        private static string f_UAVerifyMCSMSG = "UAVerifyMCSMSG";
        private static string f_UAVerifyPersonSearch = "UAVerifyPersonSearch";
        private static string f_UAVerifyGroupSearch = "UAVerifyGroupSearch";
        public static string F_UAVerifyUAInfo { get { return f_UAVerifyUAInfo; } }
        public static string F_UAVerifyMCSMSG { get { return f_UAVerifyMCSMSG; } }
        public static string F_UAVerifyPersonSearch { get { return f_UAVerifyPersonSearch; } }
        public static string F_UAVerifyGroupSearch { get { return f_UAVerifyGroupSearch; } }

        public static object lockobject = new object();
    }
}
