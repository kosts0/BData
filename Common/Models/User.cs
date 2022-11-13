using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonElement("_id")]
        public string id { get; set; }
        public string lastName { get; set; }
        public string country { get; set; }
        public long lastOnlineTimeSeconds { get; set; }
        public DateTime lastOnlineTime => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(lastOnlineTimeSeconds);
        public string city { get; set; }
        public int rating { get; set; }
        public int friendOfCount { get; set; }
        public string titlePhoto { get; set; }
        public string handle { get; set; }
        public string avatar { get; set; }    
        public string firstName { get; set; }
        public int contribution { get; set; }
        public string organization { get; set; }
        public string rank { get; set; }
        public int maxRating { get; set; }
        public long registrationTimeSeconds { get; set; }
        public DateTime registrationTime => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(registrationTimeSeconds);
        public string maxRank { get; set; }


    }
}
