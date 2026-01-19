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
    public static class ProductHelperClass
    {

        static string VAPIAppId = WebConfigurationManager.AppSettings["VAPIAppId"];
        static string BranchId = WebConfigurationManager.AppSettings["BranchId"];

        public static async Task<Activity> checkProductPrice(Activity activity, Product productToCheck, LUIS luis)
        {




            //manage the state here
            await BotHelperClass.populateChatBotState(activity);


            var reply = activity.CreateReply();
            bool pendingReturnFromUser = false;

            //look for entities detected by LUIS
            var productDetected = luis.entities.Where(x => x.type.ToString().Equals(StaticEnum.Entities.Products) || x.type.ToString().Equals(StaticEnum.Entities.BeautyProduct)|| x.type.ToString().Equals(StaticEnum.Entities.HairProduct));

            //make sure we are not doing with null items
            if (productToCheck != null)
            {
                //do nothing
            }
            else
            {
                productToCheck = new Product();

            }


            if (productDetected.Count() != 0)
            {
                productToCheck.productlist = productDetected.ToArray();
            }
            else
            {

                //if no services is detected from user

                String questionToAskBackUser = "What product would like to check for?  ";
                reply = activity.CreateReply(questionToAskBackUser);

            }

            //if there are services detected from user

            if (productToCheck.productlist !=null)
            {

                StringBuilder sb = new StringBuilder();
                //look up product on database.

                sb.Append(WebServiceHelper(productToCheck.productlist) + Environment.NewLine);
                reply = activity.CreateReply(sb.ToString());

                //clear check in session
                productToCheck.productlist = null;

                // nothing more needed from user 
                pendingReturnFromUser = false;


                
            }
            else
            {
                reply = activity.CreateReply("What Products would you like to check for?");

                // more needed from user 
                pendingReturnFromUser = true;

            }

            //create or update existing check in session
            //save back
            BotHelperClass.ObjectRemoverHelper(new String[] { StaticEnum.pendingObject.CheckProductPrice, StaticEnum.pendingIntent.toCheckProductPrice });
            BotHelperClass.ObjectAdderHelper<Product>(StaticEnum.pendingObject.CheckProductPrice, productToCheck);
            BotHelperClass.ObjectAdderHelper<bool>(StaticEnum.pendingIntent.toCheckProductPrice, pendingReturnFromUser);
            await BotHelperClass.ObjectSave(activity.ChannelId, activity.From.Id, StaticEnum.pendingObject.CheckProductPrice);

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
                    String tempString = checkProductPriceFromWS(productorservicetocheck.entity);

                    //if we find no result
                    if (tempString.Equals(""))
                {
                    sb.Append("While i can't find anything on " + productorservicetocheck.entity + ", let me try more detail search... ");
                    sb.Append(Environment.NewLine);
                        //try split string

                        foreach (String s in productorservicetocheck.entity.Split(' '))
                        {
                            try
                            {
                                tempString = checkProductPriceFromWS(s);
                                //if no results found, nevermind, keep doing
                                if (tempString.Equals(""))
                                {
                                    sb.Append("For more detail search, I can't find anything on " + s + ". ");
                                    sb.Append(Environment.NewLine);

                                }
                                //if results found, then we set false to false
                                else
                                {
                                    sb.Append("Here's something I've found on " + s + ". ");
                                    sb.Append(Environment.NewLine);
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
        /// Web API call to check the product pricing and availablity
        /// </summary>
        /// <param name="productToCheck"></param>
        /// <returns></returns>
        private static string checkProductPriceFromWS(String productToCheck)
        {
            //this string must maintain as "" as the method calling will check if nothing is found
            String returnString = "";

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("AppId", VAPIAppId);
            data.Add("BranchId", BranchId);
            data.Add("ProductName", productToCheck);
            VAPIClient client = new VAPIClient("https://veon3d.azurewebsites.net/VAPI/CheckProductPrice", data);
            VAPIProductResponse response = client.CheckProductPrice();
            if (response.Status == "Success")
            {
                List<VAPIProductResponseModel> productPriceList = response.Data;

                foreach (VAPIProductResponseModel model in productPriceList)
                {
                    returnString += model.ProductName + ": $" + model.ProductPrice + "\n";
                }
            }
            else
            {

                returnString = "We can't find anything matching " + productToCheck + ". ";
            }

            if (returnString == "")
            {
                returnString = "";
            }
            return returnString;
        }
    }
}