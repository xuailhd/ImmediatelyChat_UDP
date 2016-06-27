using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
//using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Xugl.ImmediatelyChat.Common;
using Xugl.ImmediatelyChat.Core;
using Xugl.ImmediatelyChat.Core.DependencyResolution;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.SocketEngine;

namespace Xugl.ImmediatelyChat.MessageMainServer
{
    public class BufferContorl
    {

        private AsyncSocketClient asyncSocketClient;
        private readonly IContactPersonService contactPersonService;

        private int _maxSize = 1024;
        private int _maxConnnections = 10;

        public bool IsRunning = false;


        public BufferContorl()
        {
            contactPersonService = ObjectContainerFactory.CurrentContainer.Resolver<IContactPersonService>();
        }

        public IList<ContactData> PreparContactData(string objectID, string updateTime)
        {
            IList<ContactData> tempContactDatas = null;
            ContactData tempContactData;

            ContactPerson contactPerson = contactPersonService.FindContactPerson(objectID);

            if(contactPerson!=null)
            {
                tempContactDatas = new List<ContactData>();
                tempContactData = new ContactData();

                tempContactData.ContactName = contactPerson.ContactName;
                tempContactData.ImageSrc = contactPerson.ImageSrc;
                tempContactData.LatestTime = contactPerson.LatestTime;
                tempContactData.ObjectID = contactPerson.ObjectID;
                tempContactData.UpdateTime = contactPerson.UpdateTime;
                tempContactData.ContactDataID = Guid.NewGuid().ToString();
                tempContactData.DataType = 0;
                tempContactDatas.Add(tempContactData);

                IList<ContactPersonList> contactPersonLists = contactPersonService.GetLastestContactPersonList(objectID, updateTime);
                IList<ContactGroup> contactGroups = contactPersonService.GetLastestContactGroup(objectID, updateTime);
                IList<ContactGroupSub> contactGroupSubs = contactPersonService.GetLastestContactGroupSub(objectID, updateTime);
                if (contactPersonLists != null && contactPersonLists.Count > 0)
                {
                    foreach (ContactPersonList contactPersonList in contactPersonLists)
                    {
                        tempContactData = new ContactData();

                        tempContactData.DestinationObjectID = contactPersonList.DestinationObjectID;
                        tempContactData.ContactPersonName = contactPersonList.ContactPersonName;
                        tempContactData.ObjectID = contactPersonList.ObjectID;
                        tempContactData.IsDelete = contactPersonList.IsDelete;
                        tempContactData.UpdateTime = contactPersonList.UpdateTime;
                        tempContactData.ContactDataID = Guid.NewGuid().ToString();
                        tempContactData.DataType = 1;
                        tempContactDatas.Add(tempContactData);
                    }
                }
                if (contactGroups != null && contactGroups.Count > 0)
                {
                    foreach (ContactGroup contactGroup in contactGroups)
                    {
                        tempContactData = new ContactData();
                        tempContactData.GroupObjectID = contactGroup.GroupObjectID;
                        tempContactData.GroupName = contactGroup.GroupName;
                        tempContactData.IsDelete = contactGroup.IsDelete;
                        tempContactData.UpdateTime = contactGroup.UpdateTime;
                        tempContactData.ContactDataID = Guid.NewGuid().ToString();
                        tempContactData.DataType = 2;
                        tempContactData.ObjectID = objectID;
                        tempContactDatas.Add(tempContactData);
                    }
                }
                if (contactGroupSubs != null && contactGroupSubs.Count > 0)
                {
                    foreach (ContactGroupSub contactGroupSub in contactGroupSubs)
                    {
                        tempContactData = new ContactData();
                        tempContactData.ContactGroupID = contactGroupSub.ContactGroupID;
                        tempContactData.ContactPersonObjectID = contactGroupSub.ContactPersonObjectID;
                        tempContactData.IsDelete = contactGroupSub.IsDelete;
                        tempContactData.UpdateTime = contactGroupSub.UpdateTime;
                        tempContactData.ContactDataID = Guid.NewGuid().ToString();
                        tempContactData.DataType = 3;
                        tempContactData.ObjectID = objectID;
                        tempContactDatas.Add(tempContactData);
                    }
                }
                return tempContactDatas;
            }
            return null;
        }

        public void StopMainThread()
        {
            IsRunning = false;
        }

    }

}

