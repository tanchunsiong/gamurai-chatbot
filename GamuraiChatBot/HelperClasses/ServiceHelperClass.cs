using GamuraiChatBot.VAPI;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace GamuraiChatBot
{
    public static class ServiceHelperClass
    {
        static string VAPIAppId = WebConfigurationManager.AppSettings["VAPIAppId"];
        static string BranchId = WebConfigurationManager.AppSettings["BranchId"];

        public static async Task<Activity> checkServicePrice(Activity activity, Service serviceToCheck, LUIS luis)
        {

            //manage the state here
            await BotHelperClass.populateChatBotState(activity);


            var reply = activity.CreateReply();
            var hairstylistServices = luis.entities.Where(x => x.type.ToString().Equals(StaticEnum.Entities.HairstylistServices));
            var beauticianServices = luis.entities.Where(x => x.type.ToString().Equals(StaticEnum.Entities.BeauticianServices));
            bool  pendingReturnFromUser = false;


            //make sure we are not doing with null items
            if (serviceToCheck != null)
            {
                //do nothing
            }
            else
            {
                serviceToCheck = new Service();
            }

            if (hairstylistServices.Count() != 0)
            {
                serviceToCheck.HairstylistService = hairstylistServices.ToArray();
            }

            if (beauticianServices.Count() != 0)
            {
                serviceToCheck.BeauticianService = beauticianServices.ToArray();
            }

            //if no services is detected from user, ask the user
            String questionToAskBackUser = "What is the  ";

            if (serviceToCheck.HairstylistService == null && serviceToCheck.BeauticianService == null)
            {
                if (serviceToCheck.HairstylistService == null)
                {
                    questionToAskBackUser += "hairstylist service, ";

                }
                if (serviceToCheck.BeauticianService == null)
                {

                    questionToAskBackUser += "beautician service, ";
                }

                reply = activity.CreateReply(questionToAskBackUser + " you would like to check for");

            }

            //if there are services detected from user, check with webservice and return to user

            if (serviceToCheck.HairstylistService != null || serviceToCheck.BeauticianService != null)
            {

                StringBuilder sb = new StringBuilder();
                //look up product on database.

                if (serviceToCheck.HairstylistService != null)
                {
                    sb.Append(WebServiceHelper(serviceToCheck.HairstylistService));
                }
                if (serviceToCheck.BeauticianService != null)
                {
                    sb.Append(WebServiceHelper(serviceToCheck.BeauticianService));

                }
              

                //clear check in session
                serviceToCheck.BeauticianService = null;
                serviceToCheck.HairstylistService = null;
                pendingReturnFromUser = false;

                reply = activity.CreateReply(sb.ToString());
            }
            else
            {
                reply = activity.CreateReply("What Services would you like to check for?");
                //  more needed from user 
                pendingReturnFromUser = true;

           
            }
         

            //save back
            BotHelperClass.ObjectRemoverHelper(new String[] { StaticEnum.pendingObject.CheckServicePrice, StaticEnum.pendingIntent.toCheckServicePrice });
            BotHelperClass.ObjectAdderHelper<Service>(StaticEnum.pendingObject.CheckServicePrice, serviceToCheck);
            BotHelperClass.ObjectAdderHelper<bool>(StaticEnum.pendingIntent.toCheckServicePrice, pendingReturnFromUser);
            await BotHelperClass.ObjectSave(activity.ChannelId, activity.From.Id, StaticEnum.pendingObject.CheckServicePrice);


            return reply;
        }


        /// <summary>
        /// This is a web service helper to help return the results from the web service in a proper structure.
        /// There will be attempt to split string for services which the bot doesn't understand.
        /// </summary>
        /// <param name="entitiestocheck"></param>
        /// <returns></returns>
        public static string WebServiceHelper(Entity[] entitiestocheck)
        {
            Boolean noresult = true;
            StringBuilder sb = new StringBuilder("");
            StringBuilder sbItemNotFound = new StringBuilder("");
            foreach (Entity productorservicetocheck in entitiestocheck)
            {
                try
                {
                    //always assume no result
                    noresult = true;
                    String tempString = checkServicePriceFromWS(productorservicetocheck.entity);

                    //if we find no result
                    if (tempString.Equals(""))
                    {
                        sb.Append("While i can't find anything on " + productorservicetocheck.entity + ", let me try more detail search... ");
                        sb.Append("\n\n");
                        //try split string

                        foreach (String s in productorservicetocheck.entity.Split(' '))
                        {
                            try
                            {
                                tempString = checkServicePriceFromWS(s);
                                //if no results found, nevermind, keep doing
                                if (tempString.Equals(""))
                                {
                                    sb.Append("For more detail search, I can't find anything on " + s + ". ");
                                    sb.Append("\n\n");

                                }
                                //if results found, then we set false to false
                                else
                                {
                                    sb.Append("Here's something I've found on " + s + ". ");
                                    sb.Append("\n\n");
                                    sb.Append(tempString);
                                    noresult = false;
                                }

                            }
                            catch (Exception ex)
                            {
                                BotHelperClass.LogToApplicationInsights(ex);
                            }
                        }

                        //end try split string

                        if (noresult)
                        {
                            //get all item listed and put them into 
                            sbItemNotFound.Append("We are unable to find the product/service" + productorservicetocheck.entity);

                        }
                    }
                    else
                    {
                        //result found on first try
                        sb.Append("Here's something I've found on " + productorservicetocheck.entity + ". ");
                        sb.Append("\n\n");
                        sb.Append(tempString);
                        noresult = false;

                    }


                }
                catch (Exception ex)
                {
                    BotHelperClass.LogToApplicationInsights(ex);
                }
            }
            return sb.ToString();


        }



        /// <summary>
        /// this method returns "" if there is nothing is returned
        /// </summary>
        /// <param name="serviceToCheck"></param>
        /// <returns></returns>
        private static string checkServicePriceFromWS(String serviceToCheck)
        {
            String returnString = "";

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("AppId", VAPIAppId);
            data.Add("BranchId", BranchId);
            data.Add("ServiceName", serviceToCheck);
            VAPIClient client = new VAPIClient("https://veon3d.azurewebsites.net/VAPI/CheckServicePrice", data);
            try
            {
                VAPIServiceResponse response = client.CheckServicePrice();
                if (response.Status == "Success")
                {
                    List<VAPIServiceResponseModel> servicePriceList = response.Data;

                    foreach (VAPIServiceResponseModel model in servicePriceList)
                    {                    
                        returnString += model.ServiceName + ": $" + model.ServicePrice + "\n";                    
                    }
                }
            }
            catch (Exception ex)
            {
                BotHelperClass.LogToApplicationInsights(ex);
            }
            
            return returnString;
        }
    }
}