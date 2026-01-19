using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GamuraiChatBot
{
    public static class PaymentHelperClass
    {

        public static async Task<Activity> checkPaymentMethod(Activity activity, PaymentMethod paymentMethod, LUIS luis)
        {
            //manage the state here
            await BotHelperClass.populateChatBotState(activity);



            var reply = activity.CreateReply();
            bool pendingReturnFromUser = false;

            //look for entities detected by LUIS
            var PaymentMethodDetected = luis.entities.Where(x => x.type.ToString().Equals(StaticEnum.Entities.PaymentMethods));

            if (PaymentMethodDetected.Count() != 0)
            {
                paymentMethod.paymentMethod = PaymentMethodDetected.ToArray();
            }
            else
            {

                //if no services is detected from user

                String questionToAskBackUser = "What payment method would you like to check for?";
                reply = activity.CreateReply(questionToAskBackUser);

            }

            //if there are services detected from user

            if (paymentMethod.paymentMethod != null)
            {

                StringBuilder sb = new StringBuilder();
              

                foreach (Entity payment in paymentMethod.paymentMethod)
                {
                    try {
                        //primary to do chun siong some logic to look up payment on database.
                        
                        sb.Append(payment.entity);

                    }
                    catch (Exception ex)
                    {
                        BotHelperClass.LogToApplicationInsights(ex);
                    }
                }

                reply = activity.CreateReply(sb.ToString());


             
                //clear check in session
                paymentMethod.paymentMethod = null;

                // nothing more needed from user 
                pendingReturnFromUser = false;

            }
            else
            {
                //  needed from user 
                pendingReturnFromUser = true;



            }
       

            BotHelperClass.ObjectRemoverHelper(new String[] { StaticEnum.pendingObject.CheckPaymentMethod, StaticEnum.pendingIntent.toCheckPaymentMethod });
            BotHelperClass.ObjectAdderHelper <PaymentMethod> (StaticEnum.pendingObject.CheckPaymentMethod, paymentMethod);
            BotHelperClass.ObjectAdderHelper<bool>(StaticEnum.pendingIntent.toCheckPaymentMethod, pendingReturnFromUser);
            await BotHelperClass.ObjectSave(activity.ChannelId, activity.From.Id, StaticEnum.pendingObject.CheckPaymentMethod);

            return reply;
        }
    }
}