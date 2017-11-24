using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using MSA_ContosoBank.DataModel;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

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
                string userName = await result;
                List<User> users = await AzureManager.AzureManagerInstance.GetUserInformation();

                this.userinfo = new User();
                this.userinfo.userName = userName;
                this.userinfo.balance = 0.0;

                //if user available
                if (users.Count != 0)
                {
                    foreach (User u in users)
                    {
                        //if there is matching username
                        if (u.userName.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.userinfo = u;

                            await context.PostAsync($"Welcome back {userName}");
                            //context.UserData.SetValue("username", userinfo);
                        }
                        //if there is no matching username
                        else
                        {
                            await AzureManager.AzureManagerInstance.CreateUser(userinfo);

                            this.userinfo = u;
                            await context.PostAsync($"Welcome {userName}, I have created a new account for you");
                        }
                    }
                }

                //if no user available
                else
                {
                    await AzureManager.AzureManagerInstance.CreateUser(userinfo);

                    await context.PostAsync($"Welcome {userName}, I have created a new account for you");

                    //context.UserData.SetValue("username", userinfo.userName);
                }
            }
            catch
            {
                await context.PostAsync("Something wrong with the bot");
            }

            await context.PostAsync($"There are several options 1.Deposit 2.Withdraw \n 3.Get Balacne \n 4.Find out the exchange rate \n 5.Find out the stock price \n 7.Help ");

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetBalance")]
        public async Task getBalance(IDialogContext context, LuisResult result)
        {
            //List<User> users = await AzureManager.AzureManagerInstance.GetUserInformation();
            //context.UserData.TryGetValue("username", out userName);
            try
            {
                await context.PostAsync($"{this.userinfo.balance}");
            }
            catch
            {
                await context.PostAsync("Something wrong with getBalance");
            }
            context.Wait(MessageReceived);

        }

        [LuisIntent("exchange")]
        public async Task exchange(IDialogContext context, LuisResult result)
        {
            var currency = result.Entities.First().Entity.ToUpper();

            currencyRate.Rootobject exchangeObject;
            HttpClient client = new HttpClient();
            string api = await client.GetStringAsync(new Uri("https://api.fixer.io/latest?base=NZD"));
            exchangeObject = JsonConvert.DeserializeObject<currencyRate.Rootobject>(api);

            switch (currency)
            {
                case "AUD":
                    await context.PostAsync($"NZD to {currency} Exchange rate of AUD is {exchangeObject.rates.AUD}");
                    break;
                case "BGN":
                    await context.PostAsync($"NZD to {currency} Exchange rate of BGN is {exchangeObject.rates.BGN}");
                    break;
                case "CNY":
                    await context.PostAsync($"NZD to {currency} Exchange rate of CNY is {exchangeObject.rates.CNY}");
                    break;
                case "BRL":
                    await context.PostAsync($"NZD to {currency} Exchange rate of BRL is {exchangeObject.rates.BRL}");
                    break;
                case "HKD":
                    await context.PostAsync($"NZD to {currency} Exchange rate of HKD is {exchangeObject.rates.HKD}");
                    break;
                case "IDR":
                    await context.PostAsync($"NZD to {currency} Exchange rate of IDR is {exchangeObject.rates.IDR}");
                    break;
                case "ILS":
                    await context.PostAsync($"NZD to {currency} Exchange rate of ILS is {exchangeObject.rates.ILS}");
                    break;
                case "INR":
                    await context.PostAsync($"NZD to {currency} Exchange rate of INR is {exchangeObject.rates.INR}");
                    break;
                case "JPY":
                    await context.PostAsync($"NZD to {currency} Exchange rate of JPY is {exchangeObject.rates.JPY}");
                    break;
                case "KRW":
                    await context.PostAsync($"NZD to {currency} Exchange rate of KRW is {exchangeObject.rates.KRW}");
                    break;
                case "MXN":
                    await context.PostAsync($"NZD to {currency} Exchange rate of MXN is {exchangeObject.rates.MXN}");
                    break;
                case "USD":
                    await context.PostAsync($"NZD to {currency} Exchange rate of USD is {exchangeObject.rates.USD}");
                    break;
                case "EUR":
                    await context.PostAsync($"NZD to {currency} Exchange rate of EUR is {exchangeObject.rates.EUR}");
                    break;
                default:
                    await context.PostAsync($"No currency matching. Please specify the national rate code eg. 'exchange rate of AUD'");
                    break;
            }
            context.Wait(this.MessageReceived);
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