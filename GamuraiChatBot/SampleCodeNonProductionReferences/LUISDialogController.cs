using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace GamuraiChatBot
{

    // TODO: Replace with actual LUIS App ID and Subscription Key from Web.config
    // Example: [LuisModel("YOUR_LUIS_APP_ID", "YOUR_SUBSCRIPTION_KEY")]
    [LuisModel("REPLACE_WITH_YOUR_LUIS_APP_ID", "REPLACE_WITH_YOUR_SUBSCRIPTION_KEY")]
    [Serializable]
    public class LUISDialogController : LuisDialog<object>
    {

        [LuisIntent("CheckServicePrice")]
        public async Task CheckServicePrice(IDialogContext context, LuisResult result)
        {
            //go check on database on price of product. luis will give you the entity

            EntityRecommendation serviceToFind;
            result.TryFindEntity("BeauticianServices", out serviceToFind);
            result.TryFindEntity("HairstylistServices", out serviceToFind);
            String strServiceToFind = "";
            if (serviceToFind!=null && serviceToFind.Entity != null)
            {
                strServiceToFind = serviceToFind.Entity.ToString();
                await context.PostAsync("You are attempting to do a CheckServicePrice for " + strServiceToFind);
                context.Wait(MessageReceived);
            }
            else {
                await context.PostAsync("You are attempting to do a CheckServicePrice for an unknown service");
                context.Wait(MessageReceived);
            }

         
        }

        [LuisIntent("CancelBooking")]
        public async Task CancelBooking(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("CancelBooking");
            context.Wait(MessageReceived);
        }

        [LuisIntent("CheckPaymentMethod")]
        public async Task CheckPaymentMethod(IDialogContext context, LuisResult result)
        {

            EntityRecommendation serviceToFind;
            result.TryFindEntity("PaymentMethods", out serviceToFind);
            String strPaymentMethodToFind = "";
            if (serviceToFind != null && serviceToFind.Entity != null)
            {
                strPaymentMethodToFind = serviceToFind.Entity.ToString();
                await context.PostAsync("You are attempting to do a CheckPaymentMethod for " + strPaymentMethodToFind);
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync("You are attempting to do a CheckPaymentMethod for an PaymentMethod");
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("CheckPromotion")]
        public async Task CheckPromotion(IDialogContext context, LuisResult result)
        {
            EntityRecommendation serviceToFind;
            result.TryFindEntity("DiscountType", out serviceToFind);
            String strDiscountType = "";
            if (serviceToFind != null && serviceToFind.Entity != null)
            {
                strDiscountType = serviceToFind.Entity.ToString();
                await context.PostAsync("You are attempting to do a CheckPromotion for " + strDiscountType);
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync("You are attempting to do a CheckPromotion for an unknown discount type");
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("UpdateBooking")]
        public async Task UpdateBooking(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("UpdateBooking");
            context.Wait(MessageReceived);
        }
        [LuisIntent("CheckServices")]
        public async Task CheckServices(IDialogContext context, LuisResult result)
        {
            EntityRecommendation serviceToFind;
            result.TryFindEntity("BeauticianServices", out serviceToFind);
            result.TryFindEntity("HairstylistServices", out serviceToFind);
            String strServiceToFind = "";
            if (serviceToFind != null && serviceToFind.Entity != null)
            {
                strServiceToFind = serviceToFind.Entity.ToString();
                await context.PostAsync("You are attempting to do a CheckServices for " + strServiceToFind);
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync("You are attempting to do a CheckServices for an unknown service");
                context.Wait(MessageReceived);
            }
        }
        [LuisIntent("CheckContactInfo")]
        public async Task CheckContactInfo(IDialogContext context, LuisResult result)
        {
            String response = "279, Tanjong Katong Road\n\n";
            response += "Singapore - 437 062\n\n";
            response += " Call: 65 - 63480212\n\n";
            response += " Info: info @jawedhabib.com.sg\n\n";
            response += "  Business Hours:\n\n";
            response += "  Mon,  Wed - Fri: 11am to 8pm | Sat / Sun: 10am to 8pm\n\n";
            response += "  Closed on Tuesday\n\n";


            await context.PostAsync(response);
            context.Wait(MessageReceived);
        }
        [LuisIntent("CheckProductAvailability")]
        public async Task CheckProductAvailability(IDialogContext context, LuisResult result)
        {
            EntityRecommendation serviceToFind;
            result.TryFindEntity("Product", out serviceToFind);
            String strProductToFind = "";
            if (serviceToFind != null && serviceToFind.Entity != null)
            {
                strProductToFind = serviceToFind.Entity.ToString();
                await context.PostAsync("You are attempting to do a CheckProductAvailability for " + strProductToFind);
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync("You are attempting to do a CheckProductAvailability for an unknown product");
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("None");
            context.Wait(MessageReceived);
        }
        public async Task ResumeAndPromptSummaryAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var description = await argument;
            await context.PostAsync(description);
            context.Wait(this.MessageReceived);
        }
        [LuisIntent("MakeBooking")]
        public async Task MakeBooking(IDialogContext context, LuisResult result)
        {
            //WebClient wc = new WebClient();
            //wc.DownloadString
            EntityRecommendation timeToFind;
            EntityRecommendation dateToFind;
            EntityRecommendation hairStylistToFind;
            result.TryFindEntity("builtin.datetime.time", out timeToFind);
            result.TryFindEntity("builtin.datetime.date", out dateToFind);
            result.TryFindEntity("HairStylist", out hairStylistToFind);

            if (timeToFind == null || timeToFind.Entity == null) {
                //prompt user for time

               PromptDialog.Text(context, ResumeAndPromptSummaryAsync, "What time would you like your appointment time to be?");
            }
            if (dateToFind == null || dateToFind.Entity == null)
            {
                //prompt user for date
                //PromptDialog.Text(context, ResumeAndPromptSummaryAsync, "What time would you like your date to be?");
            }
            if (hairStylistToFind == null || hairStylistToFind.Entity == null)
            {
                //prompt user for hairstylist
                //PromptDialog.Text(context, ResumeAndPromptSummaryAsync, "who would you like your stylist to be?");
            }

             //await context.PostAsync("MakeBooking");
             //context.Wait(MessageReceived);
        }
        [LuisIntent("CheckProductPrice")]
        public async Task CheckProductPrice(IDialogContext context, LuisResult result)
        {
            EntityRecommendation serviceToFind;
            result.TryFindEntity("Product", out serviceToFind);
            String strProductToFind = "";
            if (serviceToFind != null && serviceToFind.Entity != null)
            {
                strProductToFind = serviceToFind.Entity.ToString();
                await context.PostAsync("You are attempting to do a CheckProductPrice for " + strProductToFind);
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync("You are attempting to do a CheckProductPrice for an unknown product");
                context.Wait(MessageReceived);
            }
        }
        [LuisIntent("CheckBooking")]
        public async Task CheckBooking(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("CheckBooking");
            context.Wait(MessageReceived);
        }
      

    }
}