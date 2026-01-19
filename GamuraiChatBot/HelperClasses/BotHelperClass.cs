using GamuraiChatBot.HelperClasses;
using Microsoft.ApplicationInsights;
using Microsoft.Bot.Connector;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace GamuraiChatBot
{
    public static class BotHelperClass
    {
        static StateClient stateClient1;
        static BotState botState;
        static BotData botData1;

        public static async Task populateChatBotState(Activity activity)
        {
            try
            {
                //manage the state here
                stateClient1 = activity.GetStateClient();
                botState = new BotState(stateClient1);
                botData1 = await botState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
            }

        }
        public static void ObjectAdderHelper<T>(String property, T obj)
        {
            botData1.SetProperty<T>(property, obj);
        }

        public static T ObjectGetterHelper<T>(String property)
        {
          return  botData1.GetProperty<T>(property);
        }

        public async static Task ObjectSave(String channelid, String channelaccountid, String currentIntent)
        {
            try
            {
                ObjectRemoverHelper(new string[] { StaticEnum.Flow.previousIntent });
                //TestRemoveProperty(new string[] { StaticEnum.Flow.previousIntent });
                BotHelperClass.ObjectAdderHelper<String>(StaticEnum.Flow.previousIntent, currentIntent);

                await stateClient1.BotState.SetUserDataAsync(channelid, channelaccountid, botData1);
            }
            catch (HttpOperationException ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
            }
        }

        public async static Task ObjectSave(String channelid, String channelaccountid)
        {
            try
            {
                await stateClient1.BotState.SetUserDataAsync(channelid, channelaccountid, botData1);
            }
            catch (HttpOperationException ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
            }
        }

        public static BotData TestRemoveProperty(String[] propertyToRemove)
        {

            //to do chun siong this is not working
            try
            {
                if ((Newtonsoft.Json.Linq.JContainer)botData1.Data != null && ((Newtonsoft.Json.Linq.JContainer)botData1.Data).First != null)
                {
                    JToken first = ((Newtonsoft.Json.Linq.JContainer)botData1.Data).First;

                    foreach (String s in propertyToRemove)
                    {
                        try
                        {
                            first = ((Newtonsoft.Json.Linq.JContainer)botData1.Data).First;
                            while (true)
                            {
                                if (((Newtonsoft.Json.Linq.JProperty)first).Name.Equals(s))
                                {
                                    //first.Remove();
                                    first = first.Next;
                                    break;
                                }
                                else
                                {
                                    //last item
                                    if (first == null || first.Next == null)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        first = first.Next;
                                    }
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            BotHelperClass.LogToApplicationInsights(ex);
                        }
                    }

                    return botData1;
                }
            }
            catch (Exception ex) {
                BotHelperClass.LogToApplicationInsights(ex);
            }
            return botData1; 
        }

        public static BotData ObjectRemoverHelper(String[] propertyToRemove)
        {
            JContainer container = (Newtonsoft.Json.Linq.JContainer)botData1.Data;
            List<JToken> tokenToRemove = new List<JToken>();
            
            if (container != null && container.First != null)
            {
                for (int i = 0; i < container.Count; i++)
                {
                    if (Array.IndexOf(propertyToRemove, container.Children().ElementAt(i).Path) >= 0)
                    {
                        tokenToRemove.Add(container.Children().ElementAt(i));
                    }
                }
            }
            foreach(JToken jt in tokenToRemove)
            {
                jt.Remove();
            }
            
            return botData1;
        }

        public static BotData ObjectRemoverAllHelper()
        {
            JContainer container = (Newtonsoft.Json.Linq.JContainer)botData1.Data;
            List<JToken> tokenToRemove = new List<JToken>();

            if (container != null && container.First != null)
            {
                tokenToRemove = container.Children().ToList();
                foreach (JToken jt in tokenToRemove)
                {
                    jt.Remove();
                }
            }
            

            return botData1;
        }

        //secondary to do feature chun siong, this might still not be 100% comprehensive
        public static List<DateTime> DateTimeStandardizer(List<Entity> entity)


        {
            List<DateTime> returnDateTimeArray = new List<DateTime>();

            string normaldatepattern = @"\d{4}-\d{2}-\d{2}";
            string onlydayenteredpattern = @"X{4}-WX{2}-[1-7]";
            string onlyweekenteredpattern = @"\d{4}-W\d{2}";
            string onlyweekenteredpattern2 = @"X{4}-\d{2}";
            string weekofmonthpattern = @"X{4}-\d{2}-\d{2}";
            string onlydayenteredwithtimepattern = @"X{4}-WX{2}-[1-7]Td{1,2}";
            Regex rgx1 = new Regex(normaldatepattern, RegexOptions.IgnoreCase);
            Regex rgx2 = new Regex(onlydayenteredpattern, RegexOptions.IgnoreCase);
            Regex rgx3 = new Regex(onlyweekenteredpattern, RegexOptions.IgnoreCase);

            Regex rgx4 = new Regex(weekofmonthpattern, RegexOptions.IgnoreCase);
            Regex rgx5 = new Regex(onlydayenteredwithtimepattern, RegexOptions.IgnoreCase);
            Regex rgx6 = new Regex(onlyweekenteredpattern2, RegexOptions.IgnoreCase);
            foreach (Entity e in entity)
            {

                try
                {
                    //something like "2015-08-15"
                    if (rgx1.IsMatch(e.resolution.date))
                    {
                        DateTime returnDate = new DateTime();
                        DateTime.TryParse(e.resolution.date, out returnDate);
                        returnDate = addTiming(returnDate);
                        returnDateTimeArray.Add(returnDate);
                    }
                    //something like  "XXXX-WXX-1":
                    else if (rgx2.IsMatch(e.resolution.date))
                    {
                        DateTime returnDate = new DateTime();
                        returnDate = DateTime.Now;
                        String[] datearraystr = e.resolution.date.Split('-');
                        int testDay = 0;
                        int.TryParse(datearraystr[2], out testDay);

                        int daysToAdd = 0;
                        //if already pass, add
                        if ((int)returnDate.DayOfWeek >= testDay)
                        {
                            daysToAdd = testDay - (int)returnDate.DayOfWeek;

                            if (daysToAdd < 0)
                            {
                                daysToAdd += 7;
                            }
                            returnDate = returnDate.AddDays(daysToAdd);
                            returnDate = addTiming(returnDate);
                        }

                        returnDateTimeArray.Add(returnDate);
                    }
                    //something like   "2015-W34":
                    else if (rgx3.IsMatch(e.resolution.date))
                    {
                        if (e.entity.Contains("next") || e.entity.Contains("this") || e.entity.Contains("last"))
                        {
                            String[] datearraystr = e.resolution.date.Split('-');
                            int testWeek = 0;
                            int.TryParse(datearraystr[1].Substring(1, 2), out testWeek);

                            DateTime firstdate = FirstDateOfWeekISO8601(DateTime.Now.Year, testWeek);
                            DateTime returnDate = new DateTime();

                            int count = 0;
                            for (int i = 0; i <= 6; i++)
                            {
                                returnDate = firstdate.AddDays(count);
                                returnDate = addTiming(returnDate);

                                returnDateTimeArray.Add(returnDate);
                                count++;
                            }

                        }
                    }

                    //something like XXXX-01

                    //something like     "XXXX-09-30":
                    else if (rgx4.IsMatch(e.resolution.date))
                    {
                        String[] datearraystr = e.resolution.date.Split('-');
                        datearraystr[0] = DateTime.Now.Year.ToString();


                        DateTime firstdate = new DateTime(int.Parse(datearraystr[0]), int.Parse(datearraystr[1]), int.Parse(datearraystr[2]));
                        DateTime returnDate = new DateTime();

                        int count = 0;
                        for (int i = 0; i <= 6; i++)
                        {
                            returnDate = firstdate.AddDays(count);
                            returnDate = addTiming(returnDate);

                            returnDateTimeArray.Add(returnDate);
                            count++;
                        }

                    }
                    else if (rgx6.IsMatch(e.resolution.date))
                    {
                        String[] datearraystr = e.resolution.date.Split('-');
                        int testMonth = 0;
                        int.TryParse(datearraystr[1], out testMonth);

                        DateTime firstdate = new DateTime(DateTime.Now.Year, testMonth, 1, 23, 00, 00);
                        DateTime returnDate = new DateTime();

                        int count = 0;
                        for (int i = 0; i <= 6; i++)
                        {
                            returnDate = firstdate.AddDays(count);
                            returnDate = addTiming(returnDate);

                            returnDateTimeArray.Add(returnDate);
                            count++;
                        }


                    }
                    //something like  "XXXX-WXX-1TXX":
                    else if (rgx5.IsMatch(e.resolution.date))
                    {
                        DateTime returnDate = new DateTime();
                        returnDate = DateTime.Now;
                        String[] datearraystr = e.resolution.date.Split('-');
                        int testDay = 0;
                        int.TryParse(datearraystr[2], out testDay);
                        int hour = int.Parse(datearraystr[2].Split('T')[1]);
                        int daysToAdd = 0;
                        //if already pass, add
                        if ((int)returnDate.DayOfWeek >= testDay)
                        {
                            daysToAdd = testDay - (int)returnDate.DayOfWeek;

                            if (daysToAdd < 0)
                            {
                                daysToAdd += 7;
                            }
                            returnDate = returnDate.AddDays(daysToAdd);
                            //special case
                            returnDate = new DateTime(returnDate.Year, returnDate.Month, returnDate.Day, hour, 0, 0);
                            addTiming(returnDate);
                        }

                        returnDateTimeArray.Add(returnDate);
                    }


                }
                catch (Exception ex)
                {
                    BotHelperClass.LogToApplicationInsights(ex);
                }
            }

            //return only dates after today
            try
            {
                returnDateTimeArray = DateTimeStandardizerHelper.selectOnlyDistinctPresentOrFutureDates(returnDateTimeArray);
            }
            catch (Exception ex) {
                BotHelperClass.LogToApplicationInsights(ex);

            }
            return returnDateTimeArray;
        }

        /// <summary>
        /// Sometimes LUIS does not return timing, hence the date might exist in the past, therefore we need to add in the timing
        /// </summary>
        /// <param name="returnDate"></param>
        /// <returns></returns>
        private static DateTime addTiming(DateTime returnDate)
        {
            return new DateTime(returnDate.Year, returnDate.Month, returnDate.Day, 23, 59, 59);

        }
        private static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result;
        }
        /// <summary>
        /// this method will return the timing in xx:xx format example 15:00 or 08:00 format
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static List<String> TimeStandardizer(List<Entity> entity)
        {
            List<String> timeStringList = new List<String>();


            //matches "T03:00"
            string patternone = @"T\d{2}:\d{2}";

            //matches "T16"
            string patterntwo = @"T\d{1,2}";

            //matches "2015-08-15TMO" or  "2015-08-14TNI"
            string patternthree = @"\d{4}-\d{2}-\d{2}[A-Z]{3}";

            //matches "TMO" or  "TNI"
            string patternfour = @"T(MO|MI|AF|EV|NI)";

            Regex rgx1 = new Regex(patternone, RegexOptions.IgnoreCase);
            Regex rgx2 = new Regex(patterntwo, RegexOptions.IgnoreCase);
            Regex rgx3 = new Regex(patternthree, RegexOptions.IgnoreCase);
            Regex rgx4 = new Regex(patternfour, RegexOptions.IgnoreCase);
            foreach (Entity e in entity)
            {
                try
                {
                    if (rgx1.IsMatch(e.resolution.time))
                    {
                        //remove the T
                        timeStringList.Add(e.resolution.time.Substring(1));
                    }
                    else if (rgx2.IsMatch(e.resolution.time))
                    {
                        //get only last 2 digits
                        String testerString = e.resolution.time.Replace("T", "");

                        if (testerString.Length == 1)
                        {
                            testerString = "0" + testerString;
                        }
                        //add the minutes
                        timeStringList.Add(testerString + ":00");

                    }
                    //matches "2015-08-15TMO" or  "2015-08-14TNI"
                    else if (rgx3.IsMatch(e.resolution.time))
                    {
                        String testerString = e.resolution.time.Split('T')[1];
                        switch (testerString)
                        {
                            case "MO":
                                timeStringList.Add("08:00");
                                timeStringList.Add("09:00");
                                timeStringList.Add("10:00");
                                timeStringList.Add("11:00");

                                break;
                            case "MI":
                                timeStringList.Add("12:00");
                                break;
                            case "AF":

                                timeStringList.Add("13:00");
                                timeStringList.Add("14:00");
                                timeStringList.Add("15:00");
                                timeStringList.Add("16:00");
                                timeStringList.Add("17:00");
                                timeStringList.Add("18:00");


                                break;
                            case "EV":

                                timeStringList.Add("19:00");
                                timeStringList.Add("20:00");
                                timeStringList.Add("21:00");
                                timeStringList.Add("22:00");
                                timeStringList.Add("23:00");
                                timeStringList.Add("00:00");

                                break;
                            case "NI":

                                timeStringList.Add("19:00");
                                timeStringList.Add("20:00");
                                timeStringList.Add("21:00");
                                timeStringList.Add("22:00");
                                timeStringList.Add("23:00");
                                timeStringList.Add("00:00");
                                break;
                        }

                    }
                    else if (rgx4.IsMatch(e.resolution.time))
                    {
                        String testerString = e.resolution.time.Replace("T", "");
                        switch (testerString)
                        {
                            case "MO":
                                timeStringList.Add("08:00");
                                timeStringList.Add("09:00");
                                timeStringList.Add("10:00");
                                timeStringList.Add("11:00");

                                break;
                            case "MI":
                                timeStringList.Add("12:00");
                                break;
                            case "AF":

                                timeStringList.Add("13:00");
                                timeStringList.Add("14:00");
                                timeStringList.Add("15:00");
                                timeStringList.Add("16:00");
                                timeStringList.Add("17:00");
                                timeStringList.Add("18:00");


                                break;
                            case "EV":

                                timeStringList.Add("19:00");
                                timeStringList.Add("20:00");
                                timeStringList.Add("21:00");
                                timeStringList.Add("22:00");
                                timeStringList.Add("23:00");
                                timeStringList.Add("00:00");

                                break;
                            case "NI":

                                break;
                        }

                    }

                }
                catch (Exception ex)
                {
                    BotHelperClass.LogToApplicationInsights(ex);
                }
            }
            return timeStringList;
        }

        public static void LogToApplicationInsights(Exception ex)
        {
            TelemetryClient telemetry = new TelemetryClient();
            telemetry.TrackException(ex);
        }

        public static void LogToApplicationInsightsString(String s)
        {            
            TelemetryClient telemetry = new TelemetryClient();
            telemetry.TrackEvent(s);
        }

        public static void LogToApplicationInsightsString(String s, Activity activity)
        {
            //log is only available 3~4 minutes after actual timestamp
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("action", activity.Action);            
            data.Add("channelId", activity.ChannelId);//facebook/emulator etc..
            data.Add("conversationId", activity.Conversation.Id);
            data.Add("conversationIsGrp", activity.Conversation.IsGroup.ToString());
            data.Add("conversationName", activity.Conversation.Name);
            data.Add("fromId", activity.From.Id);//userId??
            data.Add("fromName", activity.From.Name);//user's display name
            data.Add("activityId", activity.Id);
            data.Add("locale", activity.Locale);
            data.Add("recipientId", activity.Recipient.Id);//chatbot Id
            data.Add("recipientName", activity.Recipient.Name);//chatbot name
            data.Add("replyToId", activity.ReplyToId);            
            data.Add("serviceUrl", activity.ServiceUrl);
            data.Add("summary", activity.Summary);
            data.Add("textFormat", activity.TextFormat);            
            data.Add("text", activity.Text);//text from user
            
            if (activity.Timestamp != null)
            {
                data.Add("timestamp", ((DateTime)(activity.Timestamp)).ToString("dd MMM yyyy HH:mm:ss"));//GMT +0
            }
            data.Add("topicName", activity.TopicName);
            data.Add("type", activity.Type);
            TelemetryClient telemetry = new TelemetryClient();
            telemetry.TrackEvent(s, data);
        }
    }
}