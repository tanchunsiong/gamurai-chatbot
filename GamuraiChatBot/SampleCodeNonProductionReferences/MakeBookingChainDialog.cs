
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
    public class MakeBookingChainDialog : IDialog<string>
    {
        static string VAPIAppId = WebConfigurationManager.AppSettings["VAPIAppId"];
        static string BranchId = WebConfigurationManager.AppSettings["BranchId"];
        public static readonly IDialog<string> dialog = Chain
                  .PostToChain()
                  .Switch(
                      new Case<IMessageActivity, IDialog<string>>((msg) =>
                      {
                          var regex = new Regex("^login", RegexOptions.IgnoreCase);
                          return regex.IsMatch(msg.Text);
                      }, (ctx, msg) =>
                      {
                    // User wants to login, send the message to Facebook Auth Dialog
                    return Chain.ContinueWith(new PromptDialog.PromptString("c1","c1",1),
                                      async (context, res) =>
                                      {
                                    // The Facebook Auth Dialog completed successfully and returend the access token in its results
                                    var token = await res;
                                      
                                         
                                          return Chain.Return($"Your are logged in as:");
                                      });
                      }),
                      new Case<IMessageActivity, IDialog<string>>((msg) =>
                      {
                          var regex = new Regex("^logout", RegexOptions.IgnoreCase);
                          return regex.IsMatch(msg.Text);
                      }, (ctx, msg) =>
                      {
                    // Clearing user related data upon logout
                
                          return Chain.Return($"Your are logged out!");
                      }),
                      new DefaultCase<IMessageActivity, IDialog<string>>((ctx, msg) =>
                      {
                          string token;
                          string name = string.Empty;
                   
                          return Chain.Return("Say \"login\" when you want to login to Facebook!");
                      })
                  ).Unwrap().PostToUser();

        public async Task StartAsync(IDialogContext context)
        {
            await LogIn(context);
        }

        //this gets a simple input from the user
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await (argument);

            //if you want another prompt here's sample
            PromptDialog.Confirm(
                context,
                AfterResetAsync,
                "Are you sure you want to reset the count?",
                "Didn't get that!",
                promptStyle: PromptStyle.None);

            context.Done(msg);
         
          
             
        }

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                int count = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            //is this looping?
            context.Wait(MessageReceivedAsync);
        }

        private async Task LogIn(IDialogContext context)
        {
            string token;
       
                context.Done("hi");
            
        }
    }
}

