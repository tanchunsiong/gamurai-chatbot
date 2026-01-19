using GamuraiChatBot.VAPI;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace GamuraiChatBot
{
    public static class ContactHelperClass
    {
        static string VAPIAppId = WebConfigurationManager.AppSettings["VAPIAppId"];
        static string BranchId = WebConfigurationManager.AppSettings["BranchId"];

        public static async Task<Activity> checkContactInfo(Activity activity, Contact contactinfo, LUIS luis)
        {
            //manage the state here
            await BotHelperClass.populateChatBotState(activity);

            //prepare the reply
            var reply = activity.CreateReply();
            var contactinfoentities = luis.entities.Where(x => x.type.ToString().Equals(StaticEnum.Entities.ContactInfo) || x.type.ToString().Equals(StaticEnum.Entities.Address) || x.type.ToString().Equals(StaticEnum.Entities.ContactNumber) || x.type.ToString().Equals(StaticEnum.Entities.Email) || x.type.ToString().Equals(StaticEnum.Entities.OperatingHours) || x.type.ToString().Equals(StaticEnum.Entities.date) || x.type.ToString().Equals(StaticEnum.Entities.datetime));
            bool pendingReturnFromUser = true;

            //make sure we are not doing with null items
            if (contactinfo != null)
            {
                //do nothing
            }
            else
            {
                contactinfo = new Contact();

            }

            if (contactinfoentities.Count() != 0)
            {
                StringBuilder sb = new StringBuilder();

                //return something here.
                foreach (Entity e in contactinfoentities)
                {
                    try
                    {
                        if (e.type.Equals(StaticEnum.Entities.Address))
                        {
                            sb.Append("We are located at ");
                            sb.Append("\n\r");
                            sb.Append(contactinfo.addressline1);
                            sb.Append("\n\r");
                            sb.Append(contactinfo.addressline2);
                            sb.Append("\n\n");
                        }
                        else if (e.type.Equals(StaticEnum.Entities.ContactNumber))
                        {

                            sb.Append("Our contact number is " + contactinfo.phonenumber + ". ");
                            sb.Append("\n\n");
                        }
                        else if (e.type.Equals(StaticEnum.Entities.Email))
                        {
                            sb.Append("We are contactable via email at" + contactinfo.email + ". ");
                            sb.Append("\n\n");

                        }
                        else if (e.type.Equals(StaticEnum.Entities.ContactInfo))
                        {
                            sb.Append("Our contact number is " + contactinfo.phonenumber + ". ");
                            sb.Append("\n\n");
                            sb.Append("We are contactable via email at" + contactinfo.email + ". ");
                            sb.Append("\n\n");

                        }

                        else if (e.type.Equals(StaticEnum.Entities.OperatingHours))
                        {
                            sb.Append("Our operating hours are " + contactinfo.operatinghours + ". ");
                            sb.Append("\n\n");
                        }
                        else if (e.type.Equals(StaticEnum.Entities.date) || e.entity.Equals(StaticEnum.Entities.datetime))
                        {
                            sb.Append("Our rest day is Tuesday and we are opening from Wednesday to Monday.");
                            sb.Append("\n\n");
                        }

                    }
                    catch (Exception ex)
                    {
                        BotHelperClass.LogToApplicationInsights(ex);
                    }
                }
                reply = activity.CreateReply(sb.ToString());

                // nothing more needed from user 
                pendingReturnFromUser = false;


            }

            else
            {

                reply = activity.CreateReply("What contact details would you like to check for?");


                //  needed from user 
                pendingReturnFromUser = true;
            }

            BotHelperClass.ObjectRemoverHelper(new String[] { StaticEnum.pendingObject.CheckContactInfo, StaticEnum.pendingIntent.toCheckContactInfo });
            BotHelperClass.ObjectAdderHelper<Contact>(StaticEnum.pendingObject.CheckContactInfo, contactinfo);
            BotHelperClass.ObjectAdderHelper<bool>(StaticEnum.pendingIntent.toCheckContactInfo, pendingReturnFromUser);
            await BotHelperClass.ObjectSave(activity.ChannelId, activity.From.Id, StaticEnum.pendingObject.CheckContactInfo);


       

       
            return reply;
        }

        public static async Task<Activity> getHumanAttention(Activity activity)
        {
            //manage the state here
            await BotHelperClass.populateChatBotState(activity);
            StringBuilder returnStringSB = new StringBuilder();

            //prepare the reply            
            returnStringSB.Append("\nI'm trying to get human assistance for you.");
            returnStringSB.Append("\n" + getHumanAttentionWS(activity.ChannelId, activity.From.Name));
            var reply = activity.CreateReply(returnStringSB.ToString());                        

            return reply;
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
        private static string getHumanAttentionWS(String channel, String info)
        {
            String returnString = "";

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("AppId", VAPIAppId);
            data.Add("BranchId", BranchId);
            data.Add("Channel", channel);
            data.Add("Name", info);
            //data.Add("DateTime", dateTime.ToString("dd MMM yyyy hh:mm tt"));
            VAPIClient client = new VAPIClient("https://veon3d.azurewebsites.net/VAPI/GetHumanAttention", data);
            try
            {
                VAPIGenericResponse response = client.GetHumanAttention();
                returnString = response.Msg;
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