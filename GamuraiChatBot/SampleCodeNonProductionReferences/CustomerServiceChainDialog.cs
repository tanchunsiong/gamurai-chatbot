
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Luis.Models;
using GamuraiChatBot.VAPI;
using System.Web.Configuration;

namespace GamuraiChatBot
{
    public class CustomerServiceChainDialog
    {
        static string VAPIAppId = WebConfigurationManager.AppSettings["VAPIAppId"];
        static string BranchId = WebConfigurationManager.AppSettings["BranchId"];
        public static readonly IDialog<string> dialog = Chain
            .PostToChain()
            .Select(msg => msg.Text)

            .Switch(
               //case 1
               //as this is a waterfall model, you might want to do things earlier here.
               new Case<string, IDialog<string>>(text =>
                {
                    var regex = new Regex("^(.*hi$|.*hi!$|.*hai$|.*hai!$|hi there!$|hi there$|hello$|good morning$|good afternoon$|good evening$|good day$|good evening$)");
                    return regex.Match(text.ToLower()).Success;
                }, (context, txt) =>
                {
                    String textreply = @"Hi there! I'm a chatbot and I'm here to assist you 
                                        I'm able to perform some functions to help you today.
                                        If you need help, just type ""help""
                                        ";

                    return Chain.Return(textreply);

                }),
                //case 2, let's try to catch user's request for help
                //as this is a waterfall model, you might want to do things earlier here.
                new Case<string, IDialog<string>>(text =>
                {
                    var regex = new Regex("^help");
                    return regex.Match(text).Success;
                }, (context, txt) =>
                {
                    String textreply = @"Here are some features I can perform! 
                                       I can help to make an appointment and check the availablity of services and products
                                        provided by the saloon
                                        ";
                    return Chain.Return(textreply);

                }),
                 //case 3
                 new Case<string, IDialog<string>>(text =>
                 {
                     var regex = new Regex(".*");
                     return regex.Match(text).Success;
                 }, (context, txt) =>
                 {
                     //call LUIS and switch the intents
                     LUIS intent = GetEntityFromLUISSync(txt);

                     switch (intent.intents[0].intent)
                     {

                         //this is case customer is trying to check price of services
                         case "CheckServicePrice":
                             String textreply = "";

                             //try to get all detected hairstyle services returned from LUIS
                             var Hairstylelistservice = from GamuraiChatBot.Entity s in intent.entities
                                                        where s.@type == "HairstylistServices"
                                                        select s;

                             //try to get all detected beautician services returned from LUIS
                             var Beauticianservice = from GamuraiChatBot.Entity s in intent.entities
                                                     where s.@type == "BeauticianServices"
                                                     select s;

                             //if somehow customer missed out the service name, ask them for the specific product
                             if (Beauticianservice.Count() == 0 && Hairstylelistservice.Count() == 0)
                             {
                                 //this will be a text prompt to customer
                                 return Chain.From(() => new PromptDialog.PromptString("What product would you like to check for?",
                                 "Didn't get that!", 3)).ContinueWith<string, string>(async (ctx, res) =>
                                 {
                                     //this is the response from customer
                                     string returnString = "";
                                     string reply = await res;

                                     //to do some logic to look up product on database.
                                     Dictionary<string, string> data = new Dictionary<string, string>();
                                     data.Add("AppId", VAPIAppId);
                                     data.Add("BranchId", BranchId);
                                     data.Add("ServiceName", reply);
                                     VAPIClient client = new VAPIClient("https://veon3d.azurewebsites.net/VAPI/CheckServicePrice", data);
                                     VAPIServiceResponse response = client.CheckServicePrice();
                                     if (response.Status == "Success")
                                     {
                                         List<VAPIServiceResponseModel> servicePriceList = response.Data;

                                         foreach (VAPIServiceResponseModel model in servicePriceList)
                                         {
                                             returnString += model.ServiceName + ": $" + model.ServicePrice + "\n";
                                         }
                                     }


                                     return Chain.Return(returnString);
                                 });
                             }



                             return Chain.Return(textreply);
                             break;

                         case "CancelBooking":
                             textreply = "";

                             //try to get all detected hairstyle services returned from LUIS
                             var date = from GamuraiChatBot.Entity s in intent.entities
                                        where s.@type == "builtin.datetime.date"
                                        select s;

                             //try to get all detected beautician services returned from LUIS
                             var time = from GamuraiChatBot.Entity s in intent.entities
                                        where s.@type == "builtin.datetime.time"
                                        select s;

                             //just double confirm customer's phone number

                             //this will be a text prompt to customer
                             var query = from x in new PromptDialog.PromptString("p1", "p1", 1)
                                         from y in new PromptDialog.PromptConfirm("p2", "p2", 1)
                                         select string.Join(" ", x, y.ToString());
                             query = query.PostToUser();
                             return Chain.PostToUser(query).ContinueWith<string, string>(async (ctx, res) =>
                             {
                                 //this is the response from customer
                                 string customerphonenumber = await res;
                                 //return Chain.Return(customerphonenumber);

                                 return Chain.Return("hellow");
                             });
                             //.WaitToBot()
                             //.Select(a => a.Text);
                       
                             break;



                         case "CheckPaymentMethod":
                             textreply = "You are attempting to do a Check Payment";
                             return Chain.Return(textreply);
                             break;
                         case "CheckPromotion":
                             textreply = "You are attempting to do a Check Promotion";
                             return Chain.Return(textreply);
                             break;
                         case "UpdateBooking":
                             textreply = "You are attempting to do a Update Booking";
                             return Chain.Return(textreply);
                             break;
                         case "CheckServices":
                             textreply = "You are attempting to do a Check Services";
                             return Chain.Return(textreply);
                             break;
                         case "CheckContactInfo":
                             textreply = "You are attempting to do a Check ContactInfo";
                             return Chain.Return(textreply);
                             break;
                         case "CheckProductAvailability":
                             textreply = "You are attempting to do a Check Product Availability";
                             return Chain.Return(textreply);
                             break;
                         case "MakeBooking":
                             textreply = "";

                             //try to get all detected hairstyle services returned from LUIS
                             date = from GamuraiChatBot.Entity s in intent.entities
                                    where s.@type == "builtin.datetime.date"
                                    select s;

                             //try to get all detected beautician services returned from LUIS
                             time = from GamuraiChatBot.Entity s in intent.entities
                                    where s.@type == "builtin.datetime.time"
                                    select s;

                             //try to get all detected beautician services returned from LUIS
                             var hairstylist = from GamuraiChatBot.Entity s in intent.entities
                                               where s.@type == "hairstylist"
                                               select s;

                             //work in progress, need to ask customer for 3 values
                             //this will be a text prompt to customer

                             return Chain.Return(textreply);
                             break;

                         case "CheckProductPrice":
                             textreply = "You are attempting to do a Check Product Price";
                             return Chain.Return(textreply);
                             break;
                         case "CheckBooking":
                             textreply = "You are attempting to do a Check Payment";
                             return Chain.Return(textreply);
                             break;
                         case "":
                             textreply = "None";
                             return Chain.Return(textreply);
                             break;
                         default:
                             return Chain.Return("");
                             break;
                     }//end switch
                 
                 //end case 2
                 }),

                //variation of case, case 3
                new RegexCase<IDialog<string>>(new Regex("^help", RegexOptions.IgnoreCase), (context, txt) =>
                {
                    return Chain.Return("I am a simple echo dialog with a counter! Reset my counter by typing \"reset\"!");
                }),
                new DefaultCase<string, IDialog<string>>((context, txt) =>
                {
                    int count;
                    context.UserData.TryGetValue("count", out count);
                    context.UserData.SetValue("count", ++count);
                    string reply = string.Format("{0}: You said {1}", count, txt);
                    return Chain.Return(reply);
                }))
            .Unwrap()
            .PostToUser();

        private static async Task<LUIS> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            LUIS Data = new LUIS();
            using (HttpClient client = new HttpClient())
            {
                string luisAppId = System.Configuration.ConfigurationManager.AppSettings["LuisAppId"];
                string luisSubscriptionKey = System.Configuration.ConfigurationManager.AppSettings["LuisSubscriptionKey"];
                string RequestURI = $"https://api.projectoxford.ai/luis/v1/application?id={luisAppId}&subscription-key={luisSubscriptionKey}&q=" + Query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<LUIS>(JsonDataResponse);
                }
            }
            return Data;
        }

        private static LUIS GetEntityFromLUISSync(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            LUIS Data = new LUIS();
            using (HttpClient client = new HttpClient())
            {
                string luisAppId = System.Configuration.ConfigurationManager.AppSettings["LuisAppId"];
                string luisSubscriptionKey = System.Configuration.ConfigurationManager.AppSettings["LuisSubscriptionKey"];
                string RequestURI = $"https://api.projectoxford.ai/luis/v1/application?id={luisAppId}&subscription-key={luisSubscriptionKey}&q=" + Query;
                HttpResponseMessage msg = client.GetAsync(RequestURI).Result;

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = msg.Content.ReadAsStringAsync().Result;
                    Data = JsonConvert.DeserializeObject<LUIS>(JsonDataResponse);
                }
            }
            return Data;
        }
    }

}