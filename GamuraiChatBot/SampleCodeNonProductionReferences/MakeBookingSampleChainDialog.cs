
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
using Microsoft.Bot.Connector;

namespace GamuraiChatBot
{
    public class MakeBookingSampleChainDialog : IDialog<string>
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
                         case "NestedQueries":
                             String textreply = "";

                             //try to get all detected hairstyle services returned from LUIS
                             var Hairstylelistservice = from Entity s in intent.entities
                                                        where s.@type == "HairstylistServices"
                                                        select s;

                             //try to get all detected beautician services returned from LUIS
                             var Beauticianservice = from Entity s in intent.entities
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
                                 
                                     string reply = await res;
                                     return Chain.Return("you said" + reply);
                                 });
                             }



                             return Chain.Return(textreply);
                             break;

                         case "DoublePrompt":
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
                             var query = from x in new PromptDialog.PromptString("What's your phone number?", "p1", 1)
                                         from y in new PromptDialog.PromptConfirm("Are you sure?", "p2", 1)
                                         select string.Join(" ", x, y.ToString());
                             query = query.PostToUser();
                             return Chain.PostToUser(query).ContinueWith<string, string>(async (ctx, res) =>
                             {
                                 //this is the response from customer
                                 string customerphonenumber = await res;
                                 //return Chain.Return(customerphonenumber);

                                 return Chain.Return("hello");
                             });
                       
                             break;

                         case "CheckEntityFromLUIS":
                             textreply = "";

                             //try to get all detected hairstyle services returned from LUIS
                             date = from Entity s in intent.entities
                                    where s.@type == "builtin.datetime.date"
                                    select s;

                             //try to get all detected beautician services returned from LUIS
                             time = from Entity s in intent.entities
                                    where s.@type == "builtin.datetime.time"
                                    select s;

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

        private async Task LogIn(IDialogContext context)
        {
            string token;
            if (!context.PrivateConversationData.TryGetValue("token", out token))
            {
                context.PrivateConversationData.SetValue("persistedCookie", new object());
                var fbLogin = "Go to this url to do something";

                await context.PostAsync(fbLogin);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                context.Done(token);
            }
        }
        //this get's a simple input from the user
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await(argument);
            if (msg.Text.StartsWith("token:"))
            {
                // Dialog is resumed by the OAuth callback and access token
                // is encoded in the message.Text
                var token = msg.Text.Remove(0, "token:".Length);
                context.PrivateConversationData.SetValue("token", token);
                context.Done(token);
            }
            else
            {
                await LogIn(context);
            }
        }

        public async Task StartAsync(IDialogContext context)
        {
            await LogIn(context);
        }
    }

}