using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.IServices;
using Xugl.ImmediatelyChat.Model;
using Xugl.ImmediatelyChat.Core;
using System.Linq.Expressions;
using Xugl.ImmediatelyChat.Common;

namespace Xugl.ImmediatelyChat.Services
{
    public class ContactPersonService : IContactPersonService
    {
        private readonly IRepository<ContactPerson> contactPersonRepository;
        private readonly IRepository<ContactPersonList> contactPersonListRepository;
        private readonly IRepository<ContactGroup> contactGroupRepository;
        private readonly IRepository<ContactGroupSub> contactGroupSubRepository;

        public ContactPersonService(IRepository<ContactPerson> contactPersonRepository, IRepository<ContactGroupSub> contactGroupSubRepository,
            IRepository<ContactGroup> contactGroupRepository, IRepository<ContactPersonList> contactPersonListRepository)
        {
            this.contactPersonRepository = contactPersonRepository;
            this.contactGroupSubRepository = contactGroupSubRepository;
            this.contactGroupRepository = contactGroupRepository;
            this.contactPersonListRepository = contactPersonListRepository;
        }

        public IList<String> GetContactPersonIDListByGroupID(string senderObjectID, string groupID)
        {

            var contactGroupSubs = from aa in contactGroupSubRepository.Table
                                 where aa.ContactGroupID == groupID && aa.ContactPersonObjectID != senderObjectID
                                 select aa.ContactPersonObjectID;
            return contactGroupSubs.ToList();
        }


        public ContactPerson FindContactPerson(string objectID)
        {
            return contactPersonRepository.Find(t => t.ObjectID == objectID);
        }

        public ContactPerson FindContactPerson(Expression<Func<ContactPerson, bool>> predicate)
        {
            return contactPersonRepository.Find(predicate);
        }

        public ContactPerson FindContactPersonNoTracking(string objectID)
        {
            return contactPersonRepository.Table.Where(t => t.ObjectID == objectID).FirstOrDefault();
        }

        public int InsertNewPerson(ContactPerson contactPerson)
        {
            if (contactPerson == null)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(contactPerson.UpdateTime))
            {
                contactPerson.UpdateTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
            }

            if (string.IsNullOrEmpty(contactPerson.LatestTime))
            {
                contactPerson.LatestTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
            }

            return contactPersonRepository.Insert(contactPerson);
        }

        public int UpdateContactPerson(ContactPerson contactPerson)
        {
            if (contactPerson == null)
            {
                return 0;
            }

            return contactPersonRepository.Update(contactPerson);
        }

        public int InsertDefaultGroup(string ObjectID)
        {
            if (string.IsNullOrEmpty(ObjectID))
            {
                return 0;
            }

            ContactGroup contactGroup = contactGroupRepository.Table.Where(t => t.GroupName == "Group1").FirstOrDefault();
            if (contactGroup == null)
            {
                contactGroup = new ContactGroup();
                contactGroup.GroupName = "Group1";
                contactGroup.GroupObjectID = "Group1";
                contactGroup.UpdateTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
                contactGroupRepository.Insert(contactGroup);
            }

            ContactGroupSub contactGroupSub = new ContactGroupSub();
            contactGroupSub.ContactGroupID = contactGroup.GroupObjectID;
            contactGroupSub.ContactPersonObjectID = ObjectID;
            contactGroupSub.UpdateTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
            return contactGroupSubRepository.Insert(contactGroupSub);

        }

        public IList<ContactGroup> GetLastestContactGroup(string objectID, string updateTime)
        {
            if (string.IsNullOrEmpty(objectID))
            {
                return null;
            }
            if (string.IsNullOrEmpty(updateTime))
            {
                updateTime = CommonFlag.F_MinDatetime;
            }

            var query = from aa in contactGroupRepository.Table
                        join bb in contactGroupSubRepository.Table on aa.GroupObjectID equals bb.ContactGroupID
                        where bb.ContactPersonObjectID.Equals(objectID) && bb.UpdateTime.CompareTo(updateTime) > 0
                        select aa;

            return query.ToList();
        }


        public IList<ContactGroupSub> GetLastestContactGroupSub(string objectID, string updateTime)
        {
            if (string.IsNullOrEmpty(objectID))
            {
                return null;
            }
            if (string.IsNullOrEmpty(updateTime))
            {
                updateTime = CommonFlag.F_MinDatetime;
            }


            var query = from aa in contactGroupSubRepository.Table
                        join bb in
                            (from cc in contactGroupSubRepository.Table
                             where cc.ContactPersonObjectID.Equals(objectID) && !cc.IsDelete
                             select cc.ContactGroupID) on aa.ContactGroupID equals bb
                        where aa.UpdateTime.CompareTo(updateTime) > 0
                        select aa;

            return query.ToList();
        }

        public IList<ContactPersonList> GetLastestContactPersonList(string objectID, string updateTime)
        {
            if (string.IsNullOrEmpty(objectID))
            {
                return null;
            }
            if (string.IsNullOrEmpty(updateTime))
            {
                updateTime = CommonFlag.F_MinDatetime;
            }
            return contactPersonListRepository.Table.Where(t => t.ObjectID == objectID
                && t.UpdateTime.CompareTo(updateTime) > 0).ToList();
        }

        public ContactPersonList FindContactPersonList(string objectID, string destinationObjectID)
        {
            if (string.IsNullOrEmpty(objectID) || string.IsNullOrEmpty(destinationObjectID))
            {
                return null;
            }
            return contactPersonListRepository.Find(t => t.ObjectID == objectID && t.DestinationObjectID == destinationObjectID);
        }

        public int InsertContactPersonList(ContactPersonList contactPersonList)
        {
            if (contactPersonList == null)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(contactPersonList.UpdateTime))
            {
                contactPersonList.UpdateTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
            }

            return contactPersonListRepository.Insert(contactPersonList);
        }
        public int UpdateContactPersonList(ContactPersonList contactPersonList)
        {
            if (contactPersonList == null)
            {
                return 0;
            }

            return contactPersonListRepository.Update(contactPersonList);
        }

        public ContactGroup FindContactGroup(string groupID)
        {
            if (string.IsNullOrEmpty(groupID))
            {
                return null;
            }

            return contactGroupRepository.Find(t => t.GroupObjectID == groupID);
        }
        public int InsertNewGroup(ContactGroup contactGroup)
        {
            if (contactGroup == null)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(contactGroup.UpdateTime))
            {
                contactGroup.UpdateTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
            }

            return contactGroupRepository.Insert(contactGroup);
        }
        public int UpdateContactGroup(ContactGroup contactGroup)
        {
            if (contactGroup == null)
            {
                return 0;
            }

            return contactGroupRepository.Update(contactGroup);
        }

        public ContactGroupSub FindContactGroupSub(string groupID, string contactPersonObjectID)
        {
            if (string.IsNullOrEmpty(groupID) || string.IsNullOrEmpty(contactPersonObjectID))
            {
                return null;
            }
            return contactGroupSubRepository.Find(t => t.ContactGroupID == groupID && t.ContactPersonObjectID == contactPersonObjectID);
        }
        public int InsertContactGroupSub(ContactGroupSub contactGroupSub)
        {
            if (contactGroupSub == null)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(contactGroupSub.UpdateTime))
            {
                contactGroupSub.UpdateTime = DateTime.Now.ToString(CommonFlag.F_DateTimeFormat);
            }
            return contactGroupSubRepository.Insert(contactGroupSub);
        }

        public int UpdateContactGroupSub(ContactGroupSub contactGroupSub)
        {
            if (contactGroupSub == null)
            {
                return 0;
            }
            return contactGroupSubRepository.Update(contactGroupSub);
        }


        public IList<ContactGroup> SearchGroup(string objectID,string key)
        {
            ContactGroup contactGroup = (from aa in contactGroupRepository.Table
                                         join bb in contactGroupSubRepository.Table on aa.GroupObjectID equals bb.ContactGroupID
                                         where aa.GroupName == key && bb.ContactPersonObjectID == objectID
                                         select aa).FirstOrDefault();

            if (contactGroup == null)
            {
                return contactGroupRepository.Table.Where(t => t.GroupName == key).ToList();
            }
            else
            {
                return null;
            }
        }

        public IList<ContactPerson> SearchPerson(string objectID, string key)
        {
            //ContactPersonListQuery query = new ContactPersonListQuery();
            //query.ContactPersonName = key;

            ContactPerson contactPerson = (from aa in contactPersonRepository.Table
                                           join bb in contactPersonListRepository.Table on aa.ObjectID equals bb.DestinationObjectID
                                           where bb.ContactPersonName == key && bb.DestinationObjectID == objectID
                                           select aa).FirstOrDefault();

            if (contactPerson == null)
            {
                return contactPersonRepository.Table.Where(t => t.ContactName == key).ToList();
            }
            else
            {
                return null;
            }
        }

        public int UpdateContactUpdateTimeByGroup(string groupID, string updateTime)
        {
            int updateCount = 0;

            IList<ContactPerson> contactPersons = (from aa in contactPersonRepository.Table
                                                   join bb in contactGroupSubRepository.Table on aa.ObjectID equals bb.ContactPersonObjectID
                                                   where bb.ContactGroupID == groupID
                                                   select aa).ToList();

            foreach (ContactPerson contactPerson in contactPersons)
            {
                contactPerson.UpdateTime = updateTime;
            }

            updateCount = contactPersonRepository.BatchUpdate(contactPersons);

            ContactGroup contactGroup = contactGroupRepository.Table.Where(t => t.GroupObjectID == groupID).FirstOrDefault();
            contactGroup.UpdateTime = updateTime;

            updateCount = updateCount + contactGroupRepository.Update(contactGroup);

            return updateCount;
        }
    }
}
