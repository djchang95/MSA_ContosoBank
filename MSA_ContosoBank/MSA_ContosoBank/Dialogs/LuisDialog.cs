using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using MSA_ContosoBank.DataModel;
using System.Collections.Generic;

namespace MSA_ContosoBank.Dialogs
{
    [LuisModel("a5a30735-cacd-4d13-8b25-918d27d7ebeb", "1db6495ff375456bb4b71aa80a7097ec")]
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        private User userinfo;

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(context, this.initial, "Hello, Welcome to Contoso Bank, before you begin our service please enter your ID");
        }
        //Assign new customer to our database
        private async Task initial(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var userName = await result;
                List<User> users = await AzureManager.AzureManagerInstance.GetUserInformation();
                if (users.Count == 0)
                {
                    foreach (User u in users)
                    {
                        if (!(u.userName.Equals(userName, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            this.userinfo = new User();
                            this.userinfo.userName = userName;
                            this.userinfo.balance = 0.0;
                            //await AzureManager.AzureManagerInstance.CreateUser(userinfo);

                            await context.PostAsync($"welcome {userName}");
                        }
                    }
                }
                else
                {
                    await context.PostAsync($"Welcome back {userName}");

                    await context.PostAsync($"There are several options 1.Deposit 2.Withdraw \n 3.Get Balacne \n 4.Find out the exchange rate \n 5.Find out the stock price \n 7.Help ");
                }
            }
            catch
            {
                throw new NotImplementedException();
            }

        }



        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I could not understand. Please rephrase your sentence");
            context.Wait(MessageReceived);
        }
        
        //public Task StartAsync(IDialogContext context)
        //{
        //    context.Wait(MessageReceivedAsync);

        //    return Task.CompletedTask;
        //}

        //private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        //{
        //    var activity = await result as Activity;

        //    // calculate something for us to return
        //    int length = (activity.Text ?? string.Empty).Length;

        //    // return our reply to the user
        //    await context.PostAsync($"You sent {activity.Text} which was {length} characters");

        //    context.Wait(MessageReceivedAsync);
        //}
    }
}