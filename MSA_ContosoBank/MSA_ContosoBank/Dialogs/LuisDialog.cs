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
        private string balance;

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hi, Welcome to Contoso Bot Service.");
            PromptDialog.Text(context, this.initial, "Before you begin our service please enter your ID. If you do not have an ID, we will create one for you");
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

                var cardmessage = context.MakeMessage();
                cardmessage.Attachments = new List<Attachment>();
                CardAction ca = new CardAction()
                {
                    Title = "Deposit",
                    Value = "Deposit"
                };
                CardAction ca2 = new CardAction()
                {
                    Title = "Withdraw",
                    Value = "Withdraw"
                };
                CardAction ca3 = new CardAction()
                {
                    Title = "Check Balance",
                    Value = "My Balance"
                };
                CardAction ca4 = new CardAction()
                {
                    Title = "Find out the exchange rate",
                    Value = "Exchange rate"
                };
                CardAction ca5 = new CardAction()
                {
                    Title = "Delete account",
                    Value = "Delete my account"
                };
                CardAction ca6 = new CardAction()
                {
                    Title = "Help",
                    Value = "Help"
                };

                HeroCard herocard = new HeroCard()
                {
                    Title = "Contoso Bank Bot Service",
                    Subtitle = "We have following services for you. \n\n You can either choose to write or click the keywords. \n\n If you need help, please type or click 'help'",
                    Buttons = new List<CardAction>()
                };
                herocard.Buttons.Add(ca);
                herocard.Buttons.Add(ca2);
                herocard.Buttons.Add(ca3);
                herocard.Buttons.Add(ca4);
                herocard.Buttons.Add(ca5);
                herocard.Buttons.Add(ca6);

                cardmessage.Attachments.Add(herocard.ToAttachment());

                //if user available
                if (users.Count != 0)
                {
                    foreach (User u in users)
                    {
                        //if there is matching username
                        if (u.userName.Equals(userName, StringComparison.InvariantCultureIgnoreCase) && this.userinfo.deleted == false)
                        {
                            this.userinfo = u;

                            await context.PostAsync($"Welcome back {userName}");
                            await context.PostAsync(cardmessage);
                        }
                        //if there is no matching username
                        else
                        {
                            await AzureManager.AzureManagerInstance.CreateUser(userinfo);

                            this.userinfo = u;
                            await context.PostAsync($"Welcome {userName}, I have created a new account for you");
                            await context.PostAsync(cardmessage);
                        }
                    }
                }

                //if no user available
                else
                {
                    await AzureManager.AzureManagerInstance.CreateUser(userinfo);

                    await context.PostAsync($"Welcome {userName}, I have created a new account for you");
                    await context.PostAsync(cardmessage);

                    //context.UserData.SetValue("username", userinfo.userName);
                }
            }
            catch
            {
                await context.PostAsync("Something wrong with the bot");
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetBalance")]
        public async Task getBalance(IDialogContext context, LuisResult result)
        {
            try
            {
                List<User> users = await AzureManager.AzureManagerInstance.GetUserInformation();

                foreach (User u in users)
                {
                    if (u.userName.Equals(this.userinfo.userName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.userinfo.balance = u.balance;
                        await context.PostAsync($"Your current balance is ${this.userinfo.balance}");
                    }
                }
            }
            catch
            {
                await context.PostAsync("Something wrong with getBalance");
            }
            context.Wait(MessageReceived);
        }


        [LuisIntent("Deposit")]
        public async Task deposit(IDialogContext context, LuisResult result)
        {
            if (!(result.Entities.Count == 0))
            {
                this.balance = result.Entities.First().Entity;

                var oldbalance = result.Entities.First().Entity;
                var newbalance = Convert.ToDouble(oldbalance);

                var cardmessage = context.MakeMessage();
                cardmessage.Attachments = new List<Attachment>();
                CardAction ca = new CardAction()
                {
                    Title = "Yes",
                    Value = "Yes"
                };
                CardAction ca2 = new CardAction()
                {
                    Title = "No",
                    Value = "No"
                };
                HeroCard herocard = new HeroCard()
                {
                    Title = $"Are you sure you want to deposit ${newbalance}?",
                    Subtitle = "Please select one from above",
                    Buttons = new List<CardAction>()
                };
                herocard.Buttons.Add(ca);
                herocard.Buttons.Add(ca2);
                cardmessage.Attachments.Add(herocard.ToAttachment());

                await context.PostAsync(cardmessage);

                //try
                //{

                //    List<User> users = await AzureManager.AzureManagerInstance.GetUserInformation();

                //    foreach (User u in users)
                //    {
                //        if (u.userName.Equals(this.userinfo.userName, StringComparison.InvariantCultureIgnoreCase))
                //        {
                //            currentbalance += newbalance;
                //            this.userinfo.balance = currentbalance;
                //            await AzureManager.AzureManagerInstance.UpdateUser(this.userinfo);
                //            await context.PostAsync($"We have now added funds upon your request. Your balance has now been updated. Balance: ${this.userinfo.balance}");
                //        }
                //    }


                //}
                //catch
                //{
                //    await context.PostAsync("Something wrong with deposit");
                //}
                //context.Wait(MessageReceived);
                PromptDialog.Text(context, this.depositprompt, "Please select one from above");
            }
            else
            {
                await context.PostAsync($"Please write the amount eg. Deposit $100");
                return;
            }
        }

        private async Task depositprompt(IDialogContext context, IAwaitable<string> result)
        {
            if (await result == "Yes" || await result == "yes")
            {
                try
                {
                    double convertedbalance = Convert.ToDouble(this.balance);
                    double currentbalance = this.userinfo.balance;

                    List<User> users = await AzureManager.AzureManagerInstance.GetUserInformation();

                    foreach (User u in users)
                    {
                        if (u.userName.Equals(this.userinfo.userName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            currentbalance += convertedbalance;
                            this.userinfo.balance = currentbalance;
                            await AzureManager.AzureManagerInstance.UpdateUser(this.userinfo);
                            await context.PostAsync($"We have now added funds upon your request. Your balance has now been updated. \n\n Balance: ${this.userinfo.balance} \n\n Please visit nearest bank to put your money in your account");
                        }
                    }
                }
                catch
                {
                    await context.PostAsync("Something wrong with deposit. This may have caused as you have deleted your account. \\n Please say hello to create your account");
                }
            }
            else
            {
                await context.PostAsync("You cancelled deposit. Please say 'Hello' to start over from the begining");
            }
            context.Wait(MessageReceived);
        }

        [LuisIntent("Withdraw")]
        public async Task withdraw(IDialogContext context, LuisResult result)
        {
            if (!(result.Entities.Count == 0))
            {
                this.balance = result.Entities.First().Entity;

                var oldbalance = result.Entities.First().Entity;
                var newbalance = Convert.ToDouble(oldbalance);

                var cardmessage = context.MakeMessage();
                cardmessage.Attachments = new List<Attachment>();
                CardAction ca = new CardAction()
                {
                    Title = "Yes",
                    Value = "Yes"
                };
                CardAction ca2 = new CardAction()
                {
                    Title = "No",
                    Value = "No"
                };
                HeroCard herocard = new HeroCard()
                {
                    Title = $"Are you sure you want to withdraw ${newbalance}?",
                    Subtitle = "Please select one from above",
                    Buttons = new List<CardAction>()
                };
                herocard.Buttons.Add(ca);
                herocard.Buttons.Add(ca2);
                cardmessage.Attachments.Add(herocard.ToAttachment());

                await context.PostAsync(cardmessage);

                PromptDialog.Text(context, this.withdrawprompt, "Please select one from above");

            }
            else
            {
                await context.PostAsync($"Please write the amount eg. Withdraw $100");
                return;
            }
        }

        private async Task withdrawprompt (IDialogContext context, IAwaitable<string> result)
        {
            if (await result == "Yes" || await result == "yes")
            {
                try
                {
                    double convertedbalance = Convert.ToDouble(this.balance);
                    double currentbalance = this.userinfo.balance;

                    var balanceafterwithdraw = currentbalance - convertedbalance;

                    if (currentbalance >= 0 && balanceafterwithdraw >= 0)
                    {
                        List<User> users = await AzureManager.AzureManagerInstance.GetUserInformation();

                        foreach (User u in users)
                        {
                            if (u.userName.Equals(this.userinfo.userName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                currentbalance -= convertedbalance;
                                this.userinfo.balance = currentbalance;
                                await AzureManager.AzureManagerInstance.UpdateUser(this.userinfo);
                                await context.PostAsync($"You have requested withdraw service. Your balance has now been updated. \n\n Balance: ${this.userinfo.balance} \n\n Please visit nearest bank to get your money");
                            }
                        }
                    }
                    else
                    {
                        await context.PostAsync("Sorry, your current balance is lower than the withdrawing amount. Please add some funds in your account. Please start from the begining.");
                    }
                }
                catch
                {
                    await context.PostAsync("Something wrong with withdraw service. This may have caused as you have deleted your account. \\n Please say hello to create your account.");
                }
            }
            else
            {
                await context.PostAsync("You cancelled withdraw. Please say 'Hello' to start over from the begining");
            }
            context.Wait(MessageReceived);
        }

        [LuisIntent("DeleteAccount")]
        public async Task deleteaccount(IDialogContext context, LuisResult result)
        {
            
            var cardmessage = context.MakeMessage();
            cardmessage.Attachments = new List<Attachment>();
            CardAction ca = new CardAction()
            {
                Title = "Yes",
                Value = "Yes"
            };
            CardAction ca2 = new CardAction()
            {
                Title = "No",
                Value = "No"
            };
            HeroCard herocard = new HeroCard()
            {
                Title = "Are you sure you want to delete your account?",
                Subtitle = "You will be losing all your account information including your current balance. \n\n if you still want to delete your account, please select one from below",
                Buttons = new List<CardAction>()
            };
            herocard.Buttons.Add(ca);
            herocard.Buttons.Add(ca2);
            cardmessage.Attachments.Add(herocard.ToAttachment());

            await context.PostAsync(cardmessage);

            PromptDialog.Text(context, this.deleteaccountprompt, "Please select one from above");

        }

        private async Task deleteaccountprompt(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var deleteconfirmation = await result;
                if (deleteconfirmation == "yes" | deleteconfirmation == "Yes")
                {
                    PromptDialog.Text(context, this.confirmusername, "Before deleting your account. Please enter your login ID");
                }
                else
                {
                    await context.PostAsync("You cancelled deleting account. Please say 'Hello' to start over from the begining");
                    return;
                }
            }
            catch
            {
                await context.PostAsync("An error occured while deleting your account");
            }
        }

        private async Task confirmusername(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                List<User> users = await AzureManager.AzureManagerInstance.GetUserInformation();
                var loginID = await result;

                foreach (User u in users)
                {
                    if (u.userName.Equals(this.userinfo.userName, StringComparison.InvariantCultureIgnoreCase))
                    {

                        if (loginID == this.userinfo.userName)
                        {
                            this.userinfo.userName = loginID;
                            await context.PostAsync($"Deleting Account Name: '{this.userinfo.userName}' is now in process...");
                            await AzureManager.AzureManagerInstance.DeleteUser(this.userinfo);
                            await context.PostAsync($"Your account is now deleted. Thank you for using our service.");
                        }
                        else
                        {
                            await context.PostAsync("Your login detail is incorrect. Deleting account failed. Please retry eg. Delete my account");
                        }
                    }
                }
            }
            catch
            {
                await context.PostAsync("An error occured while deleting your account");
            }
            context.Wait(MessageReceived);
        }

        [LuisIntent("exchange")]
        public async Task exchange(IDialogContext context, LuisResult result)
        {
            if (!(result.Entities.Count == 0))
            {
                var currency = result.Entities.First().Entity.ToUpper();

                currencyRate.Rootobject exchangeObject;
                HttpClient client = new HttpClient();
                string api = await client.GetStringAsync(new Uri("https://api.fixer.io/latest?base=NZD"));
                exchangeObject = JsonConvert.DeserializeObject<currencyRate.Rootobject>(api);

                switch (currency)
                {
                    case "AUD":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = ${exchangeObject.rates.AUD} {currency}");
                        break;
                    case "BGN":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = {exchangeObject.rates.BGN} {currency}");
                        break;
                    case "CNY":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = {exchangeObject.rates.CNY} {currency}");
                        break;
                    case "BRL":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = {exchangeObject.rates.BRL} {currency}");
                        break;
                    case "HKD":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = ${exchangeObject.rates.HKD} {currency}");
                        break;
                    case "IDR":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = {exchangeObject.rates.IDR} {currency}");
                        break;
                    case "ILS":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = {exchangeObject.rates.ILS} {currency}");
                        break;
                    case "INR":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = {exchangeObject.rates.INR} {currency}");
                        break;
                    case "JPY":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = {exchangeObject.rates.JPY} {currency}");
                        break;
                    case "KRW":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = {exchangeObject.rates.KRW} {currency}");
                        break;
                    case "MXN":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = {exchangeObject.rates.MXN} {currency}");
                        break;
                    case "USD":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = ${exchangeObject.rates.USD} {currency}");
                        break;
                    case "EUR":
                        await context.PostAsync($"Based on the exchange rate: $1 NZD = ${exchangeObject.rates.EUR} {currency}");
                        break;
                    default:
                        await context.PostAsync($"No currency matching. Please specify the exchange rate symbol eg. 'exchange rate of AUD");
                        break;
                }
            }
            else
            {
                await context.PostAsync($"Please specify the exchange rate symbol eg. 'Exchange rate of AUD'");
                return;
            }
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I could not understand. Please rephrase your sentence");
            context.Wait(MessageReceived);
        }

        [LuisIntent("help")]
        public async Task help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("\b Welcome to the Contoso Bank Bot service.\b \n\n I will explain you how to use this chat bot. \n\n First, if you have logged in, you may see the list of services that we provide. \n\n Second, please choose or type one of the service and each service will give you detailed instruction to complete the service. \n\n Should you need human help, please contact 0800-000-000 for human help.  \n\n Please say 'Hello' to begin your chat");
        }
    }
}