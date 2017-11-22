using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using MSA_ContosoBank.DataModel;

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
            PromptDialog.Text(context, this.Prompt, "Hello, Welcome to Contoso Bank, before you begin our service please enter your ID");
        }

        private async Task Prompt(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var userID = await result;
                await context.PostAsync("Welcome {username}");

                await context.PostAsync("There are several options \n 1.Deposit \n 2.Withdraw \n 3.Get Balacne \n 4.Find out the exchange rate \n 5.Find out the stock price \n 7.Help ");

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