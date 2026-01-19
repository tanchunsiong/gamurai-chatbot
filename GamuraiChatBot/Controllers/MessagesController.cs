using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.FormFlow;
using System.Collections.Generic;

namespace GamuraiChatBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// sample of a dialog getting called from MessageContoller
        /// </summary>
        /// <returns></returns>
        //internal static IDialog<SandwichOrder> MakeRootDialog()
        //{
        //    return Chain.From(() => FormDialog.FromForm(SandwichOrder.BuildForm));
        //}


        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {

                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                StateClient stateClient = activity.GetStateClient();
                BotState botState = new BotState(stateClient);
                BotData botData = await botState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                
                //if data did not exist before
                if (botData == null || botData.Data == null)
                {
                    botData = new BotData(eTag: "*");
          

                }
                //manage the state here
                await BotHelperClass.populateChatBotState(activity);
                
                //BotHelperClass.LogToApplicationInsightsString("myEvent", activity);
                //secondary to do  feature chun siong maybe can do multiple intentions
                String ResponseString;
                LUIS luis = await GetEntityFromLUIS(activity.Text);

                //if there is intent found from LUIS
                if (luis.intents.Count() > 0)
                {

                    var reply = activity.CreateReply();

                    //secondary to do chun siong, maybe we can move "Switch" the below. sorry for the pun

                    //look for continuation of previous intent
                    reply = await SwitchPreviousIntention(luis, activity, botData);

                    if (reply.Text == null || reply.Text == "")
                    {
                        reply = await SwitchIntention(luis, activity, botData);
                    }


                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                //if not intent found from LUIS
                else
                {
                    ResponseString = "Sorry, I am not getting you...";
                    activity.CreateReply(ResponseString);
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        /// <summary>
        /// This method is to handle continuation of conversation with customer, when they do some action/intention, but are missing out some fields.
        /// The methods below will parse in the values from customer, and will try to complete the intention
        /// </summary>
        /// <param name="luis"></param>
        /// <param name="activity"></param>
        /// <param name="botData"></param>
        /// <returns></returns>
        private static async Task<Activity> SwitchPreviousIntention(LUIS luis, Activity activity, BotData botData)
        {
            //prepare reply
            var reply = activity.CreateReply();

            //injection of intent
            #region injection of intent
            String lastintent = ""; //simple variable to store intent and compare

            //try to get the last action / intention from user
            try
            {
                lastintent = botData.GetProperty<String>(StaticEnum.Flow.previousIntent);
                if (lastintent == null) {

                    lastintent = "";
                }
            }
            catch (Exception ex)
            {
                lastintent = "";
                BotHelperClass.LogToApplicationInsights(ex);
            }

            //try to get entities from user's input
            #region populate entities from session
            var servicesEntities = luis.entities.Where(x => x.type == StaticEnum.Entities.HairstylistServices || x.type == StaticEnum.Entities.BeauticianServices);
            var productsEntities = luis.entities.Where(x => x.type == StaticEnum.Entities.Products);
            var paymentEntities = luis.entities.Where(x => x.type == StaticEnum.Entities.PaymentMethods);
            var contactinfoEntities = luis.entities.Where(x => x.type == StaticEnum.Entities.ContactInfo);
            var bookingEntities = luis.entities.Where(x => x.type == "builtin.datetime.date" || x.type == "builtin.datetime.time" || x.type == StaticEnum.Entities.Hairstylist || x.type == StaticEnum.Entities.Beautician || x.type == StaticEnum.Entities.number);
            var checkBookingEntities = luis.entities.Where(x =>  x.type == StaticEnum.Entities.number);
            var cancelBookingEntities = luis.entities.Where(x => x.type == "builtin.datetime.date" || x.type == "builtin.datetime.time" || x.type == StaticEnum.Entities.number);
            var updateBookingEntities = luis.entities.Where(x => x.type == "builtin.datetime.date" || x.type == "builtin.datetime.time" || x.type == StaticEnum.Entities.Hairstylist || x.type == StaticEnum.Entities.Beautician || x.type == StaticEnum.Entities.number);
            #endregion
            
            //regex
            var greetingsRegex = new Regex("^(.*hi$|.*hi!$|.*hai$|.*hai!$|hi there!$|hi there$|hello$|good morning$|good afternoon$|good evening$|good day$|good evening$)");
            var helpegex = new Regex("^help");
            #region checking of intention from session

            //check if customer is greeting
            if (greetingsRegex.Match(activity.Text).Success)
            {
                String textreply = StaticEnum.StandardBotResponse.Greetings;
                reply = activity.CreateReply(textreply);
            }

            //if customer is asking for help
            else if (helpegex.Match(activity.Text).Success)
            {
                String textreply = StaticEnum.StandardBotResponse.Help;

                reply = activity.CreateReply(textreply);
            }

            //if customer previously asked about price of services or products
            else if ((lastintent.Equals(StaticEnum.pendingObject.CheckServicePrice) || lastintent.Equals(StaticEnum.pendingObject.CheckServicePrice)) && (servicesEntities.Count() >= 1 || productsEntities.Count() >= 1))

            {
                //redirect them to either check product or check service
                Service serviceToCheck = new Service();
                Product productToCheck = new Product();

                //always try catch

              
                try
                {
                    serviceToCheck = botData.GetProperty<Service>(StaticEnum.pendingObject.CheckServicePrice);
                }
                catch (Exception ex)
                {
                    BotHelperClass.LogToApplicationInsights(ex);
                }

                try
                {
                    productToCheck = botData.GetProperty<Product>(StaticEnum.pendingObject.CheckProductPrice);
                }
                catch (Exception ex)
                {
                    BotHelperClass.LogToApplicationInsights(ex);
                }

                //done try catch

                //if composite request
                    //dumb down version of below
                    //if customer asked for services + products, append response
                        //else only return services
                        //else only return products               
                if (servicesEntities.Count() >= 1 && productsEntities.Count() >= 1)
                {

                    Activity reply1 = await ServiceHelperClass.checkServicePrice(activity, serviceToCheck, luis);
                    Activity reply2 = await ProductHelperClass.checkProductPrice(activity, productToCheck, luis);
                    reply = activity.CreateReply(reply1.Text + "\n\n" + reply2.Text);
                }
                else if (servicesEntities.Count() >= 1)
                {
                    reply = await ServiceHelperClass.checkServicePrice(activity, serviceToCheck, luis);
                }
                else if (productsEntities.Count() >= 1)
                {

                    reply = await ProductHelperClass.checkProductPrice(activity, productToCheck, luis);
                }

            }

            //if customer previously asked about about payment method
            else if (lastintent.Equals(StaticEnum.pendingObject.CheckPaymentMethod) && paymentEntities.Count() >= 1)
            {
                PaymentMethod currentCheckPaymentMethod = new PaymentMethod();

                try
                {
                    currentCheckPaymentMethod = botData.GetProperty<PaymentMethod>(StaticEnum.pendingObject.CheckPaymentMethod);
                }
                catch (Exception ex)
                {
                    BotHelperClass.LogToApplicationInsights(ex);
                }

                reply = await PaymentHelperClass.checkPaymentMethod(activity, currentCheckPaymentMethod, luis);

            }
            //if customer previously asked about contact
            else if (lastintent.Equals(StaticEnum.pendingObject.CheckContactInfo) && contactinfoEntities.Count() >= 1)
            {

                Contact currentCheckContactInfo = new Contact();

                try
                {
                    currentCheckContactInfo = botData.GetProperty<Contact>(StaticEnum.pendingObject.CheckContactInfo);
                }
                catch (Exception ex)
                {
                    BotHelperClass.LogToApplicationInsights(ex);
                }

                reply = await ContactHelperClass.checkContactInfo(activity, currentCheckContactInfo, luis);

 
            }

            //if customer previously asked about booking request
            //and they must have entered something to complete the request, such as stylist name, date, and time in bookingEntities
            else if (lastintent.Equals(StaticEnum.pendingObject.MakeBooking) && botData.GetProperty<bool>(StaticEnum.pendingIntent.toMakeBooking) && bookingEntities.Count() >= 1)
            {

                Booking pendingBooking = new Booking();

                try
                {
                    pendingBooking = botData.GetProperty<Booking>(StaticEnum.pendingObject.MakeBooking);
                }
                catch (Exception ex)
                {
                    BotHelperClass.LogToApplicationInsights(ex);
                }
                reply = await BookingHelperClass.makebooking(activity, pendingBooking, luis);

            }
            //check if they have pending cancellation request
         
            else if (lastintent.Equals(StaticEnum.pendingObject.CancelBooking) && botData.GetProperty<bool>(StaticEnum.pendingIntent.toCancelBooking) && cancelBookingEntities.Count() >= 1)
            {

                Booking pendingBooking = new Booking();

                try
                {
                    pendingBooking = botData.GetProperty<Booking>(StaticEnum.pendingObject.CancelBooking);
                }
                catch (Exception ex)
                {
                    BotHelperClass.LogToApplicationInsights(ex);
                }
                reply = await BookingHelperClass.cancelbooking(activity, pendingBooking, luis);

            }
            //check if they have pending update booking request
         
            else if (lastintent.Equals(StaticEnum.pendingObject.UpdateBooking) && botData.GetProperty<bool>(StaticEnum.pendingIntent.toUpdateBooking) && updateBookingEntities.Count() >= 1)
            {

                Booking pendingBooking = new Booking();
                try
                {
                    pendingBooking = botData.GetProperty<Booking>(StaticEnum.pendingObject.UpdateBooking);
                }
                catch (Exception ex)
                {
                    BotHelperClass.LogToApplicationInsights(ex);
                }

                reply = await BookingHelperClass.updatebooking(activity, pendingBooking, luis);

            }
            //check if customer has previous done a checkbooking previously
            else if (lastintent.Equals(StaticEnum.pendingObject.CheckBooking) && botData.GetProperty<bool>(StaticEnum.pendingIntent.toCheckBooking) && checkBookingEntities.Count() >= 1)
            {

                Booking pendingBooking = new Booking();
                try
                {
                    pendingBooking = botData.GetProperty<Booking>(StaticEnum.pendingObject.CheckBooking);
                }
                catch (Exception ex)
                {
                    BotHelperClass.LogToApplicationInsights(ex);
                }
                reply = await BookingHelperClass.checkbooking(activity, pendingBooking, luis);

            }
            //else if injection doesn't work, flow back to "original waterfall
            #endregion

            #endregion injection of intent
            return reply;
        }
        /// <summary>
        /// This is the main intention detection algorithm to try to understand what the user wants to be doing.
        /// The fallback if LUIS cannot understand the user is detected as "none"
        /// </summary>
        /// <param name="luis"></param>
        /// <param name="activity"></param>
        /// <param name="botData"></param>
        /// <returns></returns>
        private static async Task<Activity> SwitchIntention(LUIS luis, Activity activity, BotData botData)
        {

            var reply = activity.CreateReply();

            //check out all the different intentions.
            switch (luis.intents[0].intent)
            {
                //case customer is greeting us
                case "Greetings":
                    String textreply = StaticEnum.StandardBotResponse.Greetings;
                    reply = activity.CreateReply(textreply);
                    break;

                //case customer is asking for help from us
                case "Help":
                    textreply = StaticEnum.StandardBotResponse.Help;
                    reply = activity.CreateReply(textreply);
                    break;

                //case customer is trying to check price of services
                case StaticEnum.Intents.CheckServicePrice:
              
                    //if they have previously ask to check service, might be natural to check price
                    Service pendingCheckServicePrice = botData.GetProperty<Service>(StaticEnum.pendingObject.CheckServicePrice);
                    if (pendingCheckServicePrice != null)
                    {
                        reply = await ServiceHelperClass.checkServicePrice(activity, pendingCheckServicePrice, luis);
                    }
                    else
                    {
                        pendingCheckServicePrice = new Service();
                        reply = await ServiceHelperClass.checkServicePrice(activity, pendingCheckServicePrice, luis);
                    }
                    break;

                //case customer is cancel booking
                case StaticEnum.Intents.CancelBooking:
                    Booking currentCancelBooking = botData.GetProperty<Booking>(StaticEnum.pendingObject.CancelBooking);
                    if (currentCancelBooking != null)
                    {
                        reply = await BookingHelperClass.cancelbooking(activity, currentCancelBooking, luis);
                    }
                    else
                    {
                        currentCancelBooking = new Booking();
                        reply = await BookingHelperClass.cancelbooking(activity, currentCancelBooking, luis);
                    }
                    break;

                //case customer is checking payment method
                case StaticEnum.Intents.CheckPaymentMethod:
                    PaymentMethod currentCheckPaymentMethod = botData.GetProperty<PaymentMethod>(StaticEnum.pendingObject.CheckPaymentMethod);
                    if (currentCheckPaymentMethod != null)
                    {
                        reply = await PaymentHelperClass.checkPaymentMethod(activity, currentCheckPaymentMethod, luis);
                    }
                    else
                    {
                        currentCheckPaymentMethod = new PaymentMethod();
                        reply = await PaymentHelperClass.checkPaymentMethod(activity, currentCheckPaymentMethod, luis);
                    }
                    break;


       
                //case customer is updating booking
                case StaticEnum.Intents.UpdateBooking:
                    Booking currentUpdateBooking = botData.GetProperty<Booking>(StaticEnum.pendingObject.UpdateBooking);
                    if (currentUpdateBooking != null)
                    {
                        reply = await BookingHelperClass.updatebooking(activity, currentUpdateBooking, luis);
                    }
                    else
                    {
                        currentUpdateBooking = new Booking();
                        reply = await BookingHelperClass.updatebooking(activity, currentUpdateBooking, luis);
                    }
                    break;

                //case customer is check booking
                case StaticEnum.Intents.CheckBooking:
                    Booking currentCheckBooking = botData.GetProperty<Booking>(StaticEnum.pendingObject.CheckBooking);
                    if (currentCheckBooking != null)
                    {
                        reply = await BookingHelperClass.checkbooking(activity, currentCheckBooking, luis);
                    }
                    else
                    {
                        currentCheckBooking = new Booking();
                        reply = await BookingHelperClass.checkbooking(activity, currentCheckBooking, luis);
                    }
                    break;

                //case customer is make booking
                case StaticEnum.Intents.MakeBooking:
                    {

                        Booking pendingBooking = botData.GetProperty<Booking>(StaticEnum.pendingObject.MakeBooking);
                        if (pendingBooking != null)
                        {
                            reply = await BookingHelperClass.makebooking(activity, pendingBooking, luis);
                        }
                        else
                        {
                            pendingBooking = new Booking();
                            reply = await BookingHelperClass.makebooking(activity, pendingBooking, luis);
                        }
                        break;
                    }

                //case customer is checking promotion
                case StaticEnum.Intents.CheckPromotion:
                    //secondary to do feature chun siong not done yet
                    reply = activity.CreateReply("CheckPromotion");
                    break;

                //case customer is checking for contact info
                case StaticEnum.Intents.CheckContactInfo:
                    Contact currentCheckContactInfo = botData.GetProperty<Contact>(StaticEnum.pendingObject.CheckContactInfo);
                    if (currentCheckContactInfo != null)
                    {
                        reply = await ContactHelperClass.checkContactInfo(activity, currentCheckContactInfo, luis);
                    }
                    else
                    {
                        currentCheckContactInfo = new Contact();
                        reply = await ContactHelperClass.checkContactInfo(activity, currentCheckContactInfo, luis);
                    }


                    break;

                //case customer is trying to check for product price
                case StaticEnum.Intents.CheckProductPrice:

                    //if they have previously ask to check service, might be natural to check price
                    Product pendingCheckProductPrice = botData.GetProperty<Product>(StaticEnum.pendingObject.CheckProductPrice);
                    if (pendingCheckProductPrice != null)
                    {
                        reply = await ProductHelperClass.checkProductPrice(activity, pendingCheckProductPrice, luis);
                    }
                    else
                    {
                        pendingCheckProductPrice = new Product();
                        reply = await ProductHelperClass.checkProductPrice(activity, pendingCheckProductPrice, luis);
                    }
                    break;

               //case customer is asking for staff name
                case StaticEnum.Intents.CheckStaffNames:
                    {
                        reply = await BookingHelperClass.CheckStaffNames(activity, luis);
                        break;
                    }
            
                //case we can't find the intent
                case "":
                case StaticEnum.Intents.None:
                default:
                    {
                        String ResponseString = "Sorry, I can't understand you right now.";
                        reply = activity.CreateReply(ResponseString);
                        //secondary to do chun siong here is the place to get human involved
                        if (BotHelperClass.ObjectGetterHelper<int?>(StaticEnum.Flow.frustrationLevel) != null)
                        {
                            int frustrationLevel = BotHelperClass.ObjectGetterHelper<int>(StaticEnum.Flow.frustrationLevel);
                            if (frustrationLevel == 1)
                            {   
                                //await BotHelperClass.ObjectSave(activity.ChannelId, activity.From.Id);
                                reply = await ContactHelperClass.getHumanAttention(activity);

                                //clear all previous, reset frustration level, get human help
                                BotHelperClass.ObjectRemoverAllHelper();
                            }
                        }
                        else
                        {
                            //add new instance of frustration level
                            BotHelperClass.ObjectAdderHelper(StaticEnum.Flow.frustrationLevel, 1);
                            //await BotHelperClass.ObjectSave(activity.ChannelId, activity.From.Id);
                            reply = activity.CreateReply(ResponseString);
                        }
                        await BotHelperClass.ObjectSave(activity.ChannelId, activity.From.Id);
                        break;
                    }

                    ////sample
                    //case "CallingFormFromMessageController":
                    //    {
                    //        await Conversation.SendAsync(activity, MakeRootDialog);
                    //        break;
                    //    }
                    //    //sample
                    //case "CallingDialogFromMessageController":
                    //    {
                    //        //this will trigger startasync in the method
                    //        await Conversation.SendAsync(activity, () => MakeBookingSampleChainDialog.dialog);
                    //        break;
                    //    }

            }
            return reply;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private static async Task<LUIS> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            LUIS Data = new LUIS();
            using (HttpClient client = new HttpClient())
            {
                // TODO: Move these to Web.config appSettings
                string luisAppId = System.Configuration.ConfigurationManager.AppSettings["LuisAppId"];
                string luisSubscriptionKey = System.Configuration.ConfigurationManager.AppSettings["LuisSubscriptionKey"];
                string RequestURI = $"https://api.projectoxford.ai/luis/v1/application?id={luisAppId}&subscription-key={luisSubscriptionKey}&q=" + Query;

                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();

                    //always remember to standardize return value from LUIS
                    Data = JsonConvert.DeserializeObject<LUIS>(JsonDataResponse);


                }
            }
            return Data;
        }

    }
}