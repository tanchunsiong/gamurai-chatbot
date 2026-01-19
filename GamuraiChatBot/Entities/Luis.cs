using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamuraiChatBot
{

    public class LUIS
    {
        public string query { get; set; }
        public Intent[] intents { get; set; }
        public Entity[] entities { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public float score { get; set; }
        public Resolution resolution { get; set;}
    }

    public class Resolution {

        public String date { get; set; }
        public String comment { get; set; }
        public String time { get; set; }
        public String duration { get; set; }
        public String resolution_type { get; set; }
    }

}