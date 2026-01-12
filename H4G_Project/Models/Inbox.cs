using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace H4G_Project.Models
{
    public class Inbox
    {
        public int InboxItemID { get; set; }

        public int MemberID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsRead { get; set; }

        public DateTime DateSent { get; set; }
    }
}
