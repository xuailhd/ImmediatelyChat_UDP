using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Model;

namespace Xugl.ImmediatelyChat.IServices
{
    public interface IContactPersonService
    {
        IList<String> GetContactPersonIDListByGroupID(string senderObjectID, string groupID);

        ContactPerson FindContactPersonNoTracking(string objectID);
        ContactPerson FindContactPerson(string objectID);
        ContactPerson FindContactPerson(Expression<Func<ContactPerson, bool>> predicate);
        int InsertNewPerson(ContactPerson contactPerson);
        int UpdateContactPerson(ContactPerson contactPerson);

        ContactPersonList FindContactPersonList(string objectID, string destinationObjectID);
        int InsertContactPersonList(ContactPersonList contactPersonList);
        int UpdateContactPersonList(ContactPersonList contactPersonList);

        ContactGroup FindContactGroup(string groupID);
        int InsertNewGroup(ContactGroup contactGroup);
        int UpdateContactGroup(ContactGroup contactGroup);

        ContactGroupSub FindContactGroupSub(string groupID, string contactPersonObjectID);
        int InsertContactGroupSub(ContactGroupSub contactGroupSub);
        int UpdateContactGroupSub(ContactGroupSub contactGroupSub);

        int InsertDefaultGroup(string objectID);

        IList<ContactGroupSub> GetLastestContactGroupSub(string objectID, string updateTime);
        IList<ContactPersonList> GetLastestContactPersonList(string objectID, string updateTime);
        IList<ContactGroup> GetLastestContactGroup(string objectID, string updateTime);

        IList<ContactGroup> SearchGroup(string objectID, string key);
        IList<ContactPerson> SearchPerson(string objectID, string key);

        int UpdateContactUpdateTimeByGroup(string groupID, string updateTime);

    }
}
