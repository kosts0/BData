using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Submission
    {
        public Submission(BsonDocument bson)
        {
            id = bson["id"].AsInt64;
            ContestId = bson["contestId"].AsInt64;
            CreationTimeUnix = bson["creationTimeSeconds"].AsInt64;
            ProgrammingLanguage = bson["programmingLanguage"].AsString;
            Verdict = bson["verdict"].AsString;
            MemoryConsumedBytes = bson["memoryConsumedBytes"].AsInt64;
            TimeConsumedMillis = bson["timeConsumedMillis"].AsInt64;
            AuthorHandle = bson["author"].AsBsonDocument["members"].AsBsonArray[0].AsBsonDocument["handle"].AsString;
            problem = BsonSerializer.Deserialize<Problem>(bson["problem"].AsBsonDocument);
            ParticipantType = bson["author"].AsBsonDocument["participantType"].AsString;
            SolutionCode = bson.GetValue("SolutionCode", null)?.AsString ?? "";
        }
        public long id { get; set; }
        [BsonElement("contestId")]
        public long ContestId { get; set; }
        [BsonElement("creationTimeSeconds")]
        private long CreationTimeUnix { get; set; }
        public DateTime CreationTime => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(CreationTimeUnix);
        [BsonElement("programmingLanguage")]
        public string ProgrammingLanguage { get; set; }
        [BsonElement("verdict")]
        public string Verdict { get; set; }
        [BsonElement("memoryConsumedBytes")]
        public long MemoryConsumedBytes { get; set; }
        [BsonElement("timeConsumedMillis")]
        public long TimeConsumedMillis { get; set; }
        [BsonElement("problem")]
        public Problem problem { get; set; }
        [BsonElement("SolutionCode")]
        public string SolutionCode { get; set; }
        public string AuthorHandle { get; set; }
        public string ParticipantType { get; set; }
        public int? SolutionCodeLength => string.IsNullOrEmpty(SolutionCode) ? null : SolutionCode.Split('\n').Length;
    }
}
