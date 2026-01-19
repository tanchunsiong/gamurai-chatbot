using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamuraiChatBot.HelperClasses
{
    public static class DateTimeStandardizerHelper
    {
        /// <summary>
        /// check if the user entered date is in the past or not
        /// </summary>
        /// <returns></returns>
        public static bool ifDateIsInPast(List<DateTime> dateList)
        {
            bool returnvalue = false;

            try
            {
                DateTime now = DateTime.Now;

                //this will return true if there is anything in the entered date(s) which are in the past
                returnvalue = ((from x in dateList
                                where (x < now)
                                select x).Count() > 0);

            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
            }
            return returnvalue;

        }

        /// <summary>
        /// check if the user entered date is in the past or not
        /// </summary>
        /// <returns></returns> 
        public static List<DateTime> selectOnlyDistinctPresentOrFutureDates(List<DateTime> dateList)
        {
            List<DateTime> returnvalue = new List<DateTime>();
            try
            {
                DateTime now = DateTime.Now;

                //this will return true if there is anything in the entered date(s) which are in the past
                returnvalue = (from y in dateList
                               where (y > now)
                               select y).Distinct().ToList<DateTime>();

            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
            }

            return returnvalue;

        }

        public static bool thereAreDatesInTimeEntities(List<Entity> timeEntitiesFromLuis)
        {
            bool returnvalue = false;
            try
            {
                //this is to check for values such as date+time
                returnvalue = ((from y in timeEntitiesFromLuis
                                where ((y.resolution.time.ToString().Count() == 13) && y.resolution.time.ToString().Split('T')[1].Count() == 2)
                                select DateTime.Parse(y.resolution.time.Split('T')[0])).Distinct().Count() > 0);

            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
            }
            return returnvalue;
        }
        public static List<DateTime> getDatesInTimeEntities(List<Entity> timeEntitiesFromLuis)
        {
            List<DateTime> returnvalue = new List<DateTime>();
            try
            {
                //this is to check for values such as date+time
                returnvalue = (from y in timeEntitiesFromLuis
                               where ((y.resolution.time.ToString().Count() == 13) && y.resolution.time.ToString().Split('T')[1].Count() == 2)
                               select DateTime.Parse(y.resolution.time.Split('T')[0]).AddHours(23)).Distinct().ToList<DateTime>();

            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
            }

            //maybe need to fill up timing
            return returnvalue;
        }

       
    }
}