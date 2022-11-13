using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{

    public class SubmissionWithUsers : Submission
    {
        public SubmissionWithUsers(BsonDocument bson) : base(bson)
        {
        }

        public User User { get; set; }
    }
}
