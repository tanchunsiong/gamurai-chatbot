using GamuraiChatBot.HelperClasses;
using GamuraiChatBot.VAPI;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace GamuraiChatBot
{
    public static class BookingHelperClass
    {
        //these are helper objects
        static List<Entity> dateEntitiesFromLuis = new List<Entity>();
        static List<Entity> timeEntitiesFromLuis = new List<Entity>();
        //static List<String> hairStylistsFromLuis = new List<String>();
        //static List<String> beauticiansFromLuis = new List<String>();
        static List<String> staffsFromLuis = new List<String>();
        static string number = "";

        static string VAPIAppId = WebConfigurationManager.AppSettings["VAPIAppId"];
        static string BranchId = WebConfigurationManager.AppSettings["BranchId"];


        /// <summary>
        /// Read from LUIS object and populate all the helper objects
        /// </summary>
        /// <param name="luis"></param>
        private static void populateEntitiesFromLuis(LUIS luis)
        {
            try
            {
                dateEntitiesFromLuis = luis.entities.Where(x => x.type.ToString().Equals(StaticEnum.Entities.date)).ToList<Entity>();
                timeEntitiesFromLuis = luis.entities.Where(x => x.type.ToString().Equals(StaticEnum.Entities.time)).ToList<Entity>();
                //hairStylistsFromLuis = (from x in luis.entities
                //                        where (x.type.ToString().Equals(StaticEnum.Entities.Hairstylist))
                //                        select x.entity).ToList<String>();
                //beauticiansFromLuis = (from x in luis.entities
                //                       where (x.type.ToString().Equals(StaticEnum.Entities.Beautician))
                //                       select x.entity).ToList<String>();

                staffsFromLuis = (from x in luis.entities
                                  where (x.type.ToString().Equals(StaticEnum.Entities.Hairstylist) || x.type.ToString().Equals(StaticEnum.Entities.Beautician))
                                  select x.entity).ToList<String>();
                var numberentity = luis.entities.Where(x => x.type.ToString().Equals(StaticEnum.Entities.number)).FirstOrDefault();
                if (numberentity != null)
                {

                    number = numberentity.entity.ToString();
                }
                else
                {

                    number = "";
                }
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
            }
        }

        public static async Task<Activity> CheckStaffNames(Activity activity, LUIS luis)
        {
            //manage the state here
            await BotHelperClass.populateChatBotState(activity);

            //prepare the reply
            var reply = activity.CreateReply();
            StringBuilder returnStringSB = new StringBuilder();


            //read from LUIS object and populate these helper objects
            populateEntitiesFromLuis(luis);


            //returnStringSB.Append(checkStaffNamesStylistWS());
            //returnStringSB.Append(checkStaffNamesBeauticianWS());
            List<StylistInfoModel> stylistList = checkStaffNamesStylistWS();
            reply = activity.CreateReply();
            if (stylistList.Count > 0)
            {
                foreach (StylistInfoModel stylist in stylistList)
                {
                    reply.Attachments.Add(new Attachment("image/" + stylist.StylistPicSrc.Substring(stylist.StylistPicSrc.Length - 3), stylist.StylistPicSrc, null, stylist.StylistName, null));
                }
                //reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;//it keeps spamming the photos to user
            }

            //this is the response if user did not complete the necessary fields
            //reply = activity.CreateReply(returnStringSB.ToString());

            try
            {
                await BotHelperClass.ObjectSave(activity.ChannelId, activity.From.Id, StaticEnum.pendingObject.CheckStaffName);
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
                return reply;
            }

            return reply;
        }
        public static async Task<Activity> makebooking(Activity activity, Booking booking, LUIS luis)
        {
            //manage the state here
            await BotHelperClass.populateChatBotState(activity);

            //prepare the reply
            var reply = activity.CreateReply();
            StringBuilder returnStringSB = new StringBuilder();
            Boolean pendingReturnFromUser = false;

            //read from LUIS object and populate these helper objects
            populateEntitiesFromLuis(luis);

            booking = consolidateUserEnteredDateTime(booking);
            booking = consolidateStaffNamesEntered(booking);
            booking = populateUserName(booking, activity);
            booking = populatePhoneNumber(booking);

            ArrayList missingFields = new ArrayList();

            //make booking needs date, time, staff, username and phonenumber
            missingFields = checkForMissingDateField(missingFields, booking);
            missingFields = checkForMissingTimeField(missingFields, booking);
            missingFields = checkForMissingStaffField(missingFields, booking);
            missingFields = checkForMissingUserNameField(missingFields, booking);
            missingFields = checkForMissingNumberField(missingFields, booking);


            if (missingFields.Count > 0)
            {
                if (missingFields.Contains("stylist/beautician"))
                {
                    reply = activity.CreateReply("Please select a stylist for your booking:");
                    //show thumbnail card for selection of stylist/beautician
                    List<StylistInfoModel> stylistList = checkStaffNamesStylistWS();
                    
                    
                    foreach (StylistInfoModel stylist in stylistList)
                    {
                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: stylist.StylistPicSrc));
                        List<CardAction> cardButtons = new List<CardAction>();
                        CardAction plButton = new CardAction()
                        {
                            Value = stylist.StylistName,
                            Type = "imBack",
                            Title = stylist.StylistName
                        };
                        cardButtons.Add(plButton);
                        ThumbnailCard plCard = new ThumbnailCard()
                        {
                            Title = "Stylist",
                            Subtitle = stylist.StylistName,
                            Images = cardImages,
                            Buttons = cardButtons
                            //Tap = plButton
                        };
                        Attachment att = plCard.ToAttachment();
                        att.ContentType = "application/vnd.microsoft.card.thumbnail";
                        reply.Attachments.Add(att);
                        
                    }                    
                }
                else
                {
                    for (int i = 0; i <= missingFields.Count - 1; i++)
                    {
                        //not last case
                        if (i < missingFields.Count - 1)
                        {
                            returnStringSB.Append(missingFields[i].ToString() + ", ");

                        }
                        else
                        {
                            //last case
                            returnStringSB.Append(missingFields[i].ToString() + " for your booking?");
                            returnStringSB.Insert(0, "Could you share the ");
                        }
                    }
                    reply = activity.CreateReply(returnStringSB.ToString());
                }


                //this is the response if user did not complete the necessary fields                
                pendingReturnFromUser = true;
            }


            //else if all fields provided
            else
            {
                booking = checkForvaliddaysandslots(booking);
                //call web service here
                //chun siong testing method
                returnStringSB.Clear();

                //for beautician and stylist
                if (booking.staff != null)
                {
                    foreach (String staff in booking.staff)
                    {
                        foreach (DateTime dt in booking.combinedPreferredDateAndTiming)
                        {
                            try
                            {

                                //ask user which slot before booking                  
                                String status = makeBookingWS(staff, booking.username, booking.number, dt);
                                if (status.ToLower().Equals("success"))
                                {
                                    returnStringSB.Append("I've successfully booked your appointment with " + staff + " at " + dt.ToShortDateString() + " " + dt.ToShortTimeString() + Environment.NewLine);
                                    pendingReturnFromUser = false;
                                    booking.ClearAll();
                                    break;
                                }

                            }
                            catch (Exception ex)
                            {
                                BotHelperClass.LogToApplicationInsights(ex);
                            }
                        }
                    }
                }


                reply = activity.CreateReply(returnStringSB.ToString());


            }

            //save back
            BotHelperClass.ObjectRemoverHelper(new String[] { StaticEnum.pendingObject.MakeBooking, StaticEnum.pendingIntent.toMakeBooking });
            BotHelperClass.ObjectAdderHelper<Booking>(StaticEnum.pendingObject.MakeBooking, booking);
            BotHelperClass.ObjectAdderHelper<bool>(StaticEnum.pendingIntent.toMakeBooking, pendingReturnFromUser);
            try
            {
                await BotHelperClass.ObjectSave(activity.ChannelId, activity.From.Id, StaticEnum.pendingObject.MakeBooking);
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
                return reply;
            }

            return reply;
        }

        private static Task ResumeAndPromptSummaryAsync(IDialogContext context, IAwaitable<string> result)
        {
            throw new NotImplementedException();
        }

        private static Booking checkForvaliddaysandslots(Booking booking)
        {
            if (booking.combinedPreferredDateAndTiming != null && booking.combinedPreferredDateAndTiming.Count != 0)
            {
                //primary to do cleanup code, check for hours of operations
                List<DateTime> returnDatetime = new List<DateTime>();
                foreach (DateTime d in booking.combinedPreferredDateAndTiming)
                {
                    try
                    {

                        if (booking.staff.Count >= 1)
                        {
                            foreach (String name in booking.staff)
                            {
                                if (d.DayOfWeek != DayOfWeek.Tuesday)
                                {
                                    var result = checkAvailabilityWS(name, d.ToShortDateString(), d.ToShortTimeString());
                                    if (result == true)
                                    {
                                        returnDatetime.Add(d);
                                    }
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        BotHelperClass.LogToApplicationInsights(ex);
                    }

                }

            }
            return booking;
        }


        private static Booking populatePhoneNumber(Booking booking)
        {
            booking.number = number;
            return booking;
        }

        private static Booking populateUserName(Booking booking, Activity activity)
        {
            try
            {
                booking.username = activity.From.Name;

            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
            }
            return booking;
        }

        private static ArrayList checkForMissingNumberField(ArrayList missingFields, Booking booking)
        {
            if (booking.number == null || booking.number == "")
            {
                missingFields.Add("phone number");
            }
            return missingFields;
        }

        private static ArrayList checkForMissingUserNameField(ArrayList missingFields, Booking booking)
        {

            if (booking.username == null || booking.username == "")
            {
                missingFields.Add("username");
            }
            return missingFields;
        }

        private static ArrayList checkForMissingStaffField(ArrayList missingFields, Booking booking)
        {
            if (booking.staff == null)
            {
                missingFields.Add("stylist/beautician");


            }
            return missingFields;
        }

        private static ArrayList checkForMissingTimeField(ArrayList missingFields, Booking booking)
        {
            if (booking.time == null || booking.time.Count == 0)
            {
                missingFields.Add("time");

            }
            return missingFields;
        }

        private static ArrayList checkForMissingDateField(ArrayList missingFields, Booking booking)
        {


            if (booking.date == null || booking.date.Count == 0)
            {
                missingFields.Add("date");

            }
            return missingFields;
        }

        private static Booking consolidateStaffNamesEntered(Booking booking)
        {
            //need to check if they exist
            if (staffsFromLuis != null && staffsFromLuis.Count != 0)
            {
                booking.staff = staffsFromLuis;
            }

            return booking;
        }

        private static Booking consolidateUserEnteredDateTime(Booking booking)
        {
            //if user has entered a detected dateEntities(s)
            if (dateEntitiesFromLuis != null && dateEntitiesFromLuis.Count != 0)
            {
                //standardize the date and put into the booking object stored in State
                booking.date = BotHelperClass.DateTimeStandardizer(dateEntitiesFromLuis);

                //check that it cannot exist in past
                booking.date = DateTimeStandardizerHelper.selectOnlyDistinctPresentOrFutureDates(booking.date);


            }

            //if user has entered a detected timeEntities(s)
            if (timeEntitiesFromLuis != null && timeEntitiesFromLuis.Count != 0)
            {
                //standardize the time and put into the booking object stored in State
                booking.time = BotHelperClass.TimeStandardizer(timeEntitiesFromLuis);

                //we only want distinct timing as sometimes user will have duplicated timing request
                try
                {
                    var distincttime = (from t in booking.time
                                        select t).Distinct().ToList<String>();

                    booking.time = distincttime;

                }
                catch (Exception ex)
                {
                    BotHelperClass.LogToApplicationInsights(ex);
                }

                //sometimes LUIS will return Dates within Time
                if (DateTimeStandardizerHelper.thereAreDatesInTimeEntities(timeEntitiesFromLuis))
                {

                    foreach (DateTime dt in DateTimeStandardizerHelper.getDatesInTimeEntities(timeEntitiesFromLuis))
                    {
                        try
                        {
                            if (booking.date == null)
                            {
                                booking.date = new List<DateTime>();
                            }
                            booking.date.Add(dt);

                        }
                        catch (Exception ex)
                        {
                            BotHelperClass.LogToApplicationInsights(ex);
                        }
                    }

                    //we only want distinct dates btw.
                    booking.date = DateTimeStandardizerHelper.selectOnlyDistinctPresentOrFutureDates(booking.date);
                }


            }
            //if both processed times and dates have values and are valid
            if (booking.date != null && booking.time != null && booking.date.Count > 0 && booking.time.Count > 0)
            {

                List<DateTime> combinedPreferredDateAndTiming = new List<DateTime>();

                //now add in timing and confirm no "past again", but don't need to warn user anymore
                foreach (String timestring in booking.time)
                {
                    foreach (DateTime dt in booking.date)
                    {
                        try
                        {
                            int hour = 0;
                            int.TryParse(timestring.Split(':')[0], out hour);
                            int minute = 0;
                            int.TryParse(timestring.Split(':')[1], out minute);

                            DateTime returnDT = new DateTime(dt.Year, dt.Month, dt.Day, hour, minute, 0);
                            combinedPreferredDateAndTiming.Add(returnDT);

                        }
                        catch (Exception ex)
                        {
                            BotHelperClass.LogToApplicationInsights(ex);
                        }
                    }

                }
                //check for past and duplicates again

                //we only want distinct dates btw
                combinedPreferredDateAndTiming = DateTimeStandardizerHelper.selectOnlyDistinctPresentOrFutureDates(combinedPreferredDateAndTiming);


                if (combinedPreferredDateAndTiming.Count > 0)
                {
                    booking.combinedPreferredDateAndTiming = combinedPreferredDateAndTiming;
                }
            }

            return booking;
        }

        public static async Task<Activity> checkbooking(Activity activity, Booking booking, LUIS luis)
        {

            //manage the state here
            await BotHelperClass.populateChatBotState(activity);

            //prepare the reply
            var reply = activity.CreateReply();
            StringBuilder sb = new StringBuilder();
            Boolean pendingReturnFromUser = false;

            //read from LUIS object and populate these helper objects
            populateEntitiesFromLuis(luis);

 
            booking = populatePhoneNumber(booking);

            ArrayList missingFields = new ArrayList();

            //make booking needs date, time, staff, username and phonenumber
            missingFields = checkForMissingNumberField(missingFields, booking);
            if (missingFields.Count > 0)
            {

                for (int i = 0; i <= missingFields.Count - 1; i++)
                {
                    //not last case
                    if (i < missingFields.Count - 1)
                    {
                        sb.Append(missingFields[i].ToString() + ", ");

                    }
                    else
                    {
                        //last case
                        sb.Append(missingFields[i].ToString() + " for lookup request?");
                        sb.Insert(0, "Could you share the ");
                    }
                }


                //this is the response if user did not complete the necessary fields
                reply = activity.CreateReply(sb.ToString());
                pendingReturnFromUser = true;
            }

           else
            {
    

                sb.Append(checkBookingWS("", booking.number));
                reply = activity.CreateReply(sb.ToString());
                // nothing more needed from user 
                pendingReturnFromUser = false;
                booking.ClearAll();
            }



            //save back
            BotHelperClass.ObjectRemoverHelper(new String[] { StaticEnum.pendingObject.CheckBooking, StaticEnum.pendingIntent.toCheckBooking
});
            BotHelperClass.ObjectAdderHelper<Booking>(StaticEnum.pendingObject.CheckBooking, booking);
            BotHelperClass.ObjectAdderHelper<bool>(StaticEnum.pendingIntent.toCheckBooking, pendingReturnFromUser);
            try
            {
                await BotHelperClass.ObjectSave(activity.ChannelId, activity.From.Id, StaticEnum.pendingObject.CheckBooking);
            }
            catch(Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
                return reply;
            }


            return reply;
        }

        public static async Task<Activity> updatebooking(Activity activity, Booking booking, LUIS luis)
        {

            //manage the state here
            await BotHelperClass.populateChatBotState(activity);


            //prepare the reply
            var reply = activity.CreateReply();
            StringBuilder returnStringSB = new StringBuilder();
            Boolean pendingReturnFromUser = false;

            //read from LUIS object and populate these helper objects
            populateEntitiesFromLuis(luis);

            booking = consolidateUserEnteredDateTime(booking);
            booking = consolidateStaffNamesEntered(booking);
            booking = populateUserName(booking, activity);
            booking = populatePhoneNumber(booking);

            ArrayList missingFields = new ArrayList();

            //make booking needs date, time, staff, username and phonenumber
            missingFields = checkForMissingDateField(missingFields, booking);
            missingFields = checkForMissingTimeField(missingFields, booking);
            missingFields = checkForMissingStaffField(missingFields, booking);
            missingFields = checkForMissingUserNameField(missingFields, booking);
            missingFields = checkForMissingNumberField(missingFields, booking);


            if (missingFields.Count > 0)
            {

                for (int i = 0; i <= missingFields.Count - 1; i++)
                {
                    //not last case
                    if (i < missingFields.Count - 1)
                    {
                        returnStringSB.Append(missingFields[i].ToString() + ", ");

                    }
                    else
                    {
                        //last case
                        returnStringSB.Append(missingFields[i].ToString() + " for your updated booking?");
                        returnStringSB.Insert(0, "Could you share the ");
                    }
                }


                //this is the response if user did not complete the necessary fields
                reply = activity.CreateReply(returnStringSB.ToString());
                pendingReturnFromUser = true;
            }


            //else if all fields provided
            else
            {
                booking = checkForvaliddaysandslots(booking);
                //call web service here
                //chun siong testing method
                returnStringSB.Clear();

                //for beautician and stylist
                if (booking.staff != null)
                {
                    foreach (String staff in booking.staff)
                    {
                        foreach (DateTime dt in booking.combinedPreferredDateAndTiming)
                        {
                            try
                            {

                                //ask user which slot before booking                  
                                String status = updateBookingWS(staff, "", booking.number, dt);
                                if (status.ToLower().Contains("appointment updated."))
                                {
                                    returnStringSB.Append("I've successfully updated your appointment with " + staff + " at " + dt.ToShortDateString() + " " + dt.ToShortTimeString() + Environment.NewLine);
                                    pendingReturnFromUser = false;
                                    booking.ClearAll();
                                    break;
                                }
                                else
                                {
                                    returnStringSB.Append("Unable to update your appointment");
                                    BotHelperClass.LogToApplicationInsightsString("Something went wrong with updateBookingWS");
                                    pendingReturnFromUser = false;
                                    booking.ClearAll();
                                    break;
                                }

                            }
                            catch (Exception ex)
                            {
                                BotHelperClass.LogToApplicationInsights(ex);
                            }
                        }
                    }
                }


                reply = activity.CreateReply(returnStringSB.ToString());


            }
            //save back

            BotHelperClass.ObjectRemoverHelper(new String[] { StaticEnum.pendingObject.UpdateBooking, StaticEnum.pendingIntent.toUpdateBooking});
            BotHelperClass.ObjectAdderHelper<Booking>(StaticEnum.pendingObject.UpdateBooking, booking);
            BotHelperClass.ObjectAdderHelper<bool>(StaticEnum.pendingIntent.toUpdateBooking, pendingReturnFromUser);
            try
            {
                await BotHelperClass.ObjectSave(activity.ChannelId, activity.From.Id, StaticEnum.pendingObject.UpdateBooking);
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
                return reply;
            }



            return reply;
        }

        //primary to do chun siong not completed yet
        public static async Task<Activity> cancelbooking(Activity activity, Booking booking, LUIS luis)
        {
            //manage the state here
            await BotHelperClass.populateChatBotState(activity);

            //prepare the reply
            var reply = activity.CreateReply();
            StringBuilder sb = new StringBuilder();

            Boolean pendingReturnFromUser = false;

            //read from LUIS object and populate these helper objects
            populateEntitiesFromLuis(luis);




            booking = consolidateUserEnteredDateTime(booking);
            booking = populateUserName(booking, activity);
            booking = populatePhoneNumber(booking);

            ArrayList missingFields = new ArrayList();

            //make booking needs date, time, staff, username and phonenumber
            missingFields = checkForMissingDateField(missingFields, booking);
            missingFields = checkForMissingTimeField(missingFields, booking);
            missingFields = checkForMissingUserNameField(missingFields, booking);
            missingFields = checkForMissingNumberField(missingFields, booking);


            if (missingFields.Count > 0)
            {

                for (int i = 0; i <= missingFields.Count - 1; i++)
                {
                    //not last case
                    if (i < missingFields.Count - 1)
                    {
                        sb.Append(missingFields[i].ToString() + ", ");

                    }
                    else
                    {
                        //last case
                        sb.Append(missingFields[i].ToString() + " for cancellation request?");
                        sb.Insert(0, "Could you share the ");
                    }
                }


                //this is the response if user did not complete the necessary fields
                reply = activity.CreateReply(sb.ToString());
                pendingReturnFromUser = true;
            }

            else
            {
                int count = 0;
                // nothing more needed from user 
                foreach (DateTime dt in booking.combinedPreferredDateAndTiming)
                {
                 String status=   cancelBookingWS(booking.username, booking.number, dt);

                    if (status.Equals("Appointment deleted")){
                        sb.Append("Successfully cancelled appointment with at " + dt.ToShortDateString() + " at " + dt.ToShortTimeString());
                        count++;
                       
                    }
                
                }
                if (count >= 1)
                {
                    pendingReturnFromUser = false;
                    booking.ClearAll();

                }
                else {

                    sb.Append("Unable to find your appointment for cancellation");
                    pendingReturnFromUser = false;
                    booking.ClearAll();
                }
              
                reply = activity.CreateReply(sb.ToString());
                //secondary to do prompt user for booking
            }
            //save back
            BotHelperClass.ObjectRemoverHelper(new String[] { StaticEnum.pendingObject.CancelBooking, StaticEnum.pendingIntent.toCancelBooking
});
            BotHelperClass.ObjectAdderHelper<Booking>(StaticEnum.pendingObject.CancelBooking, booking);
            BotHelperClass.ObjectAdderHelper<bool>(StaticEnum.pendingIntent.toCancelBooking, pendingReturnFromUser);
            try
            {
                await BotHelperClass.ObjectSave(activity.ChannelId, activity.From.Id, StaticEnum.pendingObject.CancelBooking);
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
                return reply;
            }

            return reply;

        }


        /// <summary>
        /// Make new appointment with hairstylist/beautician at dateTime
        /// </summary>
        /// <param name="hairstylistOrBeauticianName">Name of hairstylist/beautician</param>
        /// <param name="name">Name of user</param>
        /// <param name="phone">Phone of user</param>
        /// <param name="dateTime">Datetime of new appointment</param>
        /// <returns></returns>
        private static string makeBookingWS(String hairstylistOrBeauticianName, String name, String phone, DateTime dateTime)
        {
            String returnString = "";

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("AppId", VAPIAppId);
            data.Add("BranchId", BranchId);
            data.Add("HairstylistName", hairstylistOrBeauticianName == null ? "" : hairstylistOrBeauticianName);
            data.Add("Date", dateTime.ToString("dd MMM yyyy"));
            data.Add("Time", dateTime.ToString("hh:mm tt"));
            data.Add("CustomerName", name);
            data.Add("CustomerPhone", phone);
            VAPIClient client = new VAPIClient("https://veon3d.azurewebsites.net/VAPI/BookAppointment", data);
            try
            {
                VAPIHairstylistAppointmentResponse response = client.BookAppointment();
                if (response.Status == "Success")
                {
                    //TODO: provide alternative time slots..response.Data empty currently
                    List<VAPIHairstylistAppointmentResponseModel> alternativeTimeslotList = response.Data;

                    //TODO: beautify returnString
                    returnString = response.Msg;
                }
                else
                {
                    //TODO: beautify returnString
                    returnString = response.Msg;
                }
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
                return returnString;
            }

            return returnString;
        }

        /// <summary>
        /// Update upcoming appointment with new hairstylist/beautician, at new datetime.
        /// </summary>
        /// <param name="hairstylistOrBeauticianName">Optional. Retain appointment's hairstylist if null</param>
        /// <param name="name">Name of user</param>
        /// <param name="phone">Phone of user</param>
        /// <param name="dateTime">New datetime for the appointment</param>
        /// <returns></returns>
        private static string updateBookingWS(String hairstylistOrBeauticianName, String name, String phone, DateTime dateTime)
        {
            String returnString = "";

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("AppId", VAPIAppId);
            data.Add("BranchId", BranchId);
            data.Add("HairstylistName", hairstylistOrBeauticianName == null ? "" : hairstylistOrBeauticianName);
            data.Add("Date", dateTime.ToString("dd MMM yyyy"));
            data.Add("Time", dateTime.ToString("hh:mm tt"));
            data.Add("CustomerName", name);
            data.Add("CustomerPhone", phone);
            VAPIClient client = new VAPIClient("https://veon3d.azurewebsites.net/VAPI/UpdateBooking", data);
            try
            {
                VAPIUpdateBookingResponse response = client.UpdateBooking();
                if (response.Status == "Success")
                {
                    //TODO: provide alternative time slots..response.Data empty currently
                    VAPIUpdateBookingResponseModel alternativeTimeslotList = response.Data;

                    //TODO: beautify returnString
                    returnString = response.Msg;
                }
                else
                {
                    //TODO: beautify returnString
                    returnString = response.Msg;
                }
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
                return returnString;
            }

            return returnString;
        }

        /// <summary>
        /// Attempt to cancel/delete appointment. Return one of the following messages:
        /// (1) User not found
        /// (2) No appointment found..
        /// (3) Appointment deleted
        /// </summary>
        /// <param name="name">Name of user</param>
        /// <param name="phone">Phone of user</param>
        /// <param name="dateTime">Datetime of appointment to cancel/delete</param>
        /// <returns></returns>
        private static string cancelBookingWS(String name, String phone, DateTime dateTime)
        {
            String returnString = "";

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("AppId", VAPIAppId);
            data.Add("BranchId", BranchId);
            data.Add("CustomerName", name);
            data.Add("CustomerPhone", phone);
            data.Add("DateTime", dateTime.ToString("dd MMM yyyy hh:mm tt"));
            VAPIClient client = new VAPIClient("https://veon3d.azurewebsites.net/VAPI/DeleteBooking", data);
            try
            {
                VAPIGenericResponse response = client.DeleteBooking();
                returnString = response.Msg;
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
                return returnString;
            }

            return returnString;
        }
        /// <summary>
        /// Check if slot is free
        /// </summary>
        /// <param name="name">name of stylist or beautician</param>
        /// <param name="datetime">date and timing to check</param>
        /// <returns></returns>
        private static bool checkAvailabilityWS(String name, String date, String time)
        {
            bool returnString = false;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("AppId", VAPIAppId);
            data.Add("BranchId", BranchId);
            data.Add("HairstylistName", name);
            data.Add("Date", date);
            data.Add("Time", time);
            VAPIClient client = new VAPIClient("https://veon3d.azurewebsites.net/VAPI/CheckHairstylistAvailability", data);
            try
            {
                VAPIHairstylistAvailabilityResponse response = client.CheckHairstylistAvailability();
                if (response.Status == "Success")
                {
                    VAPIHairstylistAvailabilityResponseModel responseData = response.Data;
                    if (response != null && responseData != null)
                    {


                        foreach (String s in responseData.AvailableTimeSlot)
                        {
                            if (s.ToLower().Trim().Equals(time.ToLower().Trim()) || s.ToLower().Trim().Equals(time.Insert(0, "0").ToLower().Trim()))
                            {
                                return true;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
                return returnString;
            }

            return returnString;
        }
        /// <summary>
        /// Return all pending appointments of user
        /// </summary>
        /// <param name="name">Name of user *not impt</param>
        /// <param name="phone">Phone number of user</param>
        /// <returns></returns>
        private static string checkBookingWS(String name, String phone)
        {
            String returnString = "";

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("AppId", VAPIAppId);
            data.Add("BranchId", BranchId);
            data.Add("CustomerName", name);
            data.Add("CustomerPhone", phone);
            VAPIClient client = new VAPIClient("https://veon3d.azurewebsites.net/VAPI/CheckBooking", data);
            try
            {
                VAPICheckBookingResponse response = client.CheckBooking();
                if (response.Status == "Success")
                {
                    List<VAPICheckBookingResponseModel> pendingApptList = response.Data;
                    if (pendingApptList.Count > 0)
                    {
                        returnString += "You have an appointment with :\n";
                    }
                    foreach (VAPICheckBookingResponseModel model in pendingApptList)
                    {
                        returnString += model.HairstylistName + " at " + model.DateTime.ToString("dd MMM yyyy hh:mm tt") + "\n";
                    }
                }
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
                return returnString;
            }
            if (returnString == "")
            {
                returnString = "Sorry I can't find your booking. You can make a booking by saying 'I want to make a booking'";

            }
            return returnString;
        }

        /// <summary>
        /// Return all stylist staff names
        /// </summary>
        /// <returns></returns>
        private static List<StylistInfoModel> checkStaffNamesStylistWS()
        {
            String returnString = "";
            List<StylistInfoModel> stylistList = new List<StylistInfoModel>();

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("AppId", VAPIAppId);
            data.Add("BranchId", BranchId);

            //VAPIClient client = new VAPIClient("https://veon3d.azurewebsites.net/VAPI/GetStylistNames", data);
            VAPIClient client = new VAPIClient("https://veon3d.azurewebsites.net/VAPI/GetStylistInfo", data);
            try
            {
                VAPIStylistInfoResponse response = client.GetStylistInfo();
                if (response.Status == "Success")
                {
                    stylistList = response.Data;
                    //if (stylistList.Count == 0)
                    //{
                    //    returnString += "There are no stylist";
                    //}
                    //else { returnString += "Stylist: \n\n"; }
                    //foreach (StylistInfoModel stylist in stylistList)
                    //{
                    //    returnString += name + "\n\n";
                    //}
                    return stylistList;
                }
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
                return stylistList;
            }

            return stylistList;
        }

        /// <summary>
        /// Return all beautician staff names
        /// </summary>
        /// <returns></returns>
        private static string checkStaffNamesBeauticianWS()
        {
            String returnString = "";

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("AppId", VAPIAppId);
            data.Add("BranchId", BranchId);

            VAPIClient client = new VAPIClient("https://veon3d.azurewebsites.net/VAPI/GetBeauticianNames", data);
            try
            {
                VAPIStylistNamesResponse response = client.GetStylistNames();
                if (response.Status == "Success")
                {
                    List<String> nameList = response.Data;
                    if (nameList.Count == 0)
                    {
                        returnString += "There are no Beautician";
                    }
                    else { returnString += "Beautician: \n\n"; }
                    foreach (String name in nameList)
                    {
                        returnString += name + "\n\n";
                    }
                }
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
                return returnString;
            }

            return returnString;
        }
    }
}