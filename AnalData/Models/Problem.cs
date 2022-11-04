using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalData.Models
{
    public class Problem
    {
        public int contestId { get; set; }
        public int rating { get; set; }
        public int points { get; set; }
        public string name { get; set; }
        public string index { get; set; }
        public string type { get; set; }
        public List<string> tags { get; set; }
    }
}
