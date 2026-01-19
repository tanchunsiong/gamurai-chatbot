using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamuraiChatBot
{
    public class Booking
    {
        /// <summary>
        /// this property is to store the raw entities from user
        /// </summary>
        public List<String> time;
        /// <summary>
        /// this property is to store the raw entities from user
        /// </summary>
        public List<DateTime> date;
        public List<String> staff;
        //public List<String> hairstylist;
        //public List<String> beautician;
        public List<DateTime> combinedPreferredDateAndTiming;



        public string username;
        public string number;

        public void ClearAll()
        {
            staff = null;
            //hairstylist = null;
            //beautician = null;
            time = null;
            date = null;
            combinedPreferredDateAndTiming = null;
            number = null;
            username = null;
        }
    }
}