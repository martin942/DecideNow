using System;
using System.Collections.Generic;
using System.Text;

namespace Group
{
    public class Group
    {
        public string GroupId { get; set; }
        public string Admin { get; set; }
        public string Name { get; set; }
        public string Visibility { get; set; }

        public Group(string groupId, string admin, string name, string visibility)
        {
            GroupId = groupId;
            Admin = admin;
            Name = name;
            Visibility = visibility;
        }

        public Group(string admin, string name, string visibility)
        {
            Admin = admin;
            Name = name;
            Visibility = visibility;
            GroupId = Guid.NewGuid().ToString();
        }

        public Group()
        {
        }
    }
}
