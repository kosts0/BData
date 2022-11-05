using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class AuthorMembers
    {
        public int contestId { get; set; }
        public string participantType { get; set; }
        public bool ghost { get; set; }
        public long startTimeSeconds { get; set; }
        public List<Member> members { get; set; }
    }
}
