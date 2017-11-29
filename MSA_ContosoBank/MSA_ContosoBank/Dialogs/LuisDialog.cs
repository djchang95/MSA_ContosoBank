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
    [LuisModel("df58f5a9-0c7f-41cb-9907-db29b13de067", "a0541c7b5e0344c889570c0057845e50")]
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        private User2 userinfo;
        private string balance;


        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hi, Welcome to Contoso Bot Service.");
            PromptDialog.Text(context, this.initial, "Before you begin our service please enter your ID. If you do not have an ID, we will create one for you");
        }
        //Assign customer to our database
        private async Task initial(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string userName = await result;
                List<User2> users = await AzureManager.AzureManagerInstance.GetUserInformation();

                this.userinfo = new User2();
                this.userinfo.userName = userName;
                this.userinfo.userBalance = 0.0;
                this.userinfo.userPassword = string.Empty;

                
                //if user available
                if (users.Count != 0)
                {
                    foreach (User2 u in users)
                    {
                        //if there is matching username
                        if (u.userName.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.userinfo = u;
                            await context.PostAsync($"Welcome back {userName}");
                            PromptDialog.Text(context, this.initialPW, "Before you begin our service please enter your PW.");
                            break;
                        }


                    }
                    await AzureManager.AzureManagerInstance.CreateUser(userinfo);
                    foreach(User2 u in users)
                    {
                        if(u.userName.Equals(userName, StringComparison.InvariantCultureIgnoreCase)){
                            this.userinfo = u;
                        }
                    }
                    await context.PostAsync($"Welcome {this.userinfo.userName}, I have created a new account for you");
                    PromptDialog.Text(context, this.initialPW, "Before you begin our service please enter your Password.");
                }

                //if no user available
                else
                {
                    await AzureManager.AzureManagerInstance.CreateUser(userinfo);

                    await context.PostAsync($"Welcome {userName}, I have created a new account for you");
                    PromptDialog.Text(context, this.initialPW, "Before you begin our service please enter your Password.");
                }
            }
            catch
            {
                await context.PostAsync("Thanks for coming back");
            }
        }

        private async Task initialPW(IDialogContext context, IAwaitable<string> result)
        {
            string userPw = await result;
            List<User2> users = await AzureManager.AzureManagerInstance.GetUserInformation();

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
                Title = "Find out the Exchange rate",
                Value = "Exchange rate"
            };
            CardAction ca5 = new CardAction()
            {
                Title = "Find out the Stock price",
                Value = "Stock price"
            };

            CardAction ca6 = new CardAction()
            {
                Title = "Current News",
                Value = "news"
            };
            CardAction ca7 = new CardAction()
            {
                Title = "Delete account",
                Value = "Delete my account"
            };
            CardAction ca8 = new CardAction()
            {
                Title = "Help",
                Value = "help"
            };

            HeroCard herocard = new HeroCard()
            {
                Title = "Contoso Bank Bot Service",
                Subtitle = "Please select/type one of the services",
                Buttons = new List<CardAction>()
            };
            herocard.Buttons.Add(ca);
            herocard.Buttons.Add(ca2);
            herocard.Buttons.Add(ca3);
            herocard.Buttons.Add(ca4);
            herocard.Buttons.Add(ca5);
            herocard.Buttons.Add(ca6);
            herocard.Buttons.Add(ca7);
            herocard.Buttons.Add(ca8);


            cardmessage.Attachments.Add(herocard.ToAttachment());

            foreach (User2 u in users)
            {
                if (this.userinfo.userPassword == string.Empty)
                {
                    this.userinfo.userPassword = userPw;
                    await AzureManager.AzureManagerInstance.UpdateUser(this.userinfo);

                    await context.PostAsync($"Welcome {this.userinfo.userName}, I have created a new password for you");
                    await context.PostAsync(cardmessage);

                }
                //if there is matching username
                else if (u.userPassword.Equals(userPw, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.userinfo = u;
                    await context.PostAsync("Password confirmed");
                    await context.PostAsync(cardmessage);
                    break;
                }
            }
            if (this.userinfo.userPassword != userPw)
            {
                await context.PostAsync("Your password is incorrect. If you forgot your password, please contact us to find your password");
            }
            await context.PostAsync("If you need help, please say 'help' or call our speech bot service");
            context.Wait(MessageReceived);
        }

        [LuisIntent("GetBalance")]
        public async Task getBalance(IDialogContext context, LuisResult result)
        {
            try
            {
                List<User2> users = await AzureManager.AzureManagerInstance.GetUserInformation();

                foreach (User2 u in users)
                {
                    if (u.userName.Equals(this.userinfo.userName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.userinfo.userBalance = u.userBalance;
                        await context.PostAsync($"Your current balance is ${this.userinfo.userBalance}");
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
                    Title = $"Do you want to deposit ${newbalance}?",
                    Subtitle = "Please select one from above",
                    Buttons = new List<CardAction>()
                };
                herocard.Buttons.Add(ca);
                herocard.Buttons.Add(ca2);
                cardmessage.Attachments.Add(herocard.ToAttachment());

                await context.PostAsync(cardmessage);

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
                    double currentbalance = this.userinfo.userBalance;

                    List<User2> users = await AzureManager.AzureManagerInstance.GetUserInformation();

                    foreach (User2 u in users)
                    {
                        if (u.userName.Equals(this.userinfo.userName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            currentbalance += convertedbalance;
                            this.userinfo.userBalance = currentbalance;
                            await AzureManager.AzureManagerInstance.UpdateUser(this.userinfo);
                            await context.PostAsync($"We have now added funds upon your request. Your balance has now been updated. \n\n Balance: ${this.userinfo.userBalance} \n\n Please visit nearest bank to deposit your money in your account within 7 days");
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
                    Title = $"Do you want to withdraw ${newbalance}?",
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

        private async Task withdrawprompt(IDialogContext context, IAwaitable<string> result)
        {
            if (await result == "Yes" || await result == "yes")
            {
                try
                {
                    double convertedbalance = Convert.ToDouble(this.balance);
                    double currentbalance = this.userinfo.userBalance;

                    var balanceafterwithdraw = currentbalance - convertedbalance;

                    if (currentbalance >= 0 && balanceafterwithdraw >= 0)
                    {
                        List<User2> users = await AzureManager.AzureManagerInstance.GetUserInformation();

                        foreach (User2 u in users)
                        {
                            if (u.userName.Equals(this.userinfo.userName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                currentbalance -= convertedbalance;
                                this.userinfo.userBalance = currentbalance;
                                await AzureManager.AzureManagerInstance.UpdateUser(this.userinfo);
                                await context.PostAsync($"You have requested withdraw service. Your balance has now been updated. \n\n Balance: ${this.userinfo.userBalance} \n\n Please visit nearest bank to get your money within 7 days");
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
                Title = "Do you want to delete account?",
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
                List<User2> users = await AzureManager.AzureManagerInstance.GetUserInformation();
                var loginID = await result;

                foreach (User2 u in users)
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

        [LuisIntent("Stock")]
        public async Task stock(IDialogContext context, LuisResult result)
        {
            if (!(result.Entities.Count == 0))
            {
                string companyname = result.Entities.First().Entity.ToUpper();

                using (var client = new HttpClient())
                {
                    string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={companyname}&outputsize=full&apikey=BRBIL98ZCADVQBFL";
                    var response = await client.GetAsync(url);

                    var responseText = await response.Content.ReadAsStringAsync();
                    var index = responseText.IndexOf("close", 286) + 9;
                    var index2 = responseText.IndexOf('"', index);
                    var indexLength = index2 - index;

                    var stock = responseText.Substring(index, indexLength);

                    await context.PostAsync("Stock price of " + companyname + "=" + stock);
                }
            }
            else
            {
                await context.PostAsync($"Please specify the stock price symbol eg. 'Stock price of MSFT'");
                return;
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

        [LuisIntent("News")] 
        public async Task news (IDialogContext context, LuisResult result)
        {
            string url = $"https://newsapi.org/v2/top-headlines?sources=techcrunch&apiKey=b55936b2626d4d07914ae3f8252b5c77";

            var cardmessage = context.MakeMessage();
            News.Rootobject newsobject;
            HttpClient client = new HttpClient();
            string api = await client.GetStringAsync(new Uri(url));
            newsobject = JsonConvert.DeserializeObject<News.Rootobject>(api);

            cardmessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            cardmessage.Attachments = new List<Attachment>();

            foreach (var news in newsobject.articles)
            {
                
                List<CardAction> cardbutton = new List<CardAction>();
                CardAction ca = new CardAction()
                {
                    Title = "Click for more information",
                    Value = news.url,
                    Type = "openUrl"
                };

                cardbutton.Add(ca);


                HeroCard herocard = new HeroCard()
                {
                    Title = news.title,
                    Text = news.description,
                    Buttons = cardbutton
                };

                cardmessage.Attachments.Add(herocard.ToAttachment());

            }
            await context.PostAsync(cardmessage);

            context.Wait(MessageReceived);
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
                Title = "Find out the Exchange rate",
                Value = "Exchange rate"
            };
            CardAction ca5 = new CardAction()
            {
                Title = "Find out the Stock price",
                Value = "Stock price"
            };

            CardAction ca6 = new CardAction()
            {
                Title = "Current News",
                Value = "news"
            };
            CardAction ca7 = new CardAction()
            {
                Title = "Delete account",
                Value = "Delete my account"
            };
            CardAction ca8 = new CardAction()
            {
                Title = "Help",
                Value = "help"
            };

            HeroCard herocard = new HeroCard()
            {
                Title = "Contoso Bank Bot Service",
                Subtitle = "Please select/type one of the services",
                Buttons = new List<CardAction>()
            };
            herocard.Buttons.Add(ca);
            herocard.Buttons.Add(ca2);
            herocard.Buttons.Add(ca3);
            herocard.Buttons.Add(ca4);
            herocard.Buttons.Add(ca5);
            herocard.Buttons.Add(ca6);
            herocard.Buttons.Add(ca7);
            herocard.Buttons.Add(ca8);


            cardmessage.Attachments.Add(herocard.ToAttachment());

            await context.PostAsync("Hi, I will explain you how to use this chat bot. \n\n If you have logged in, you may see the list of services that we provide. \n\n Please choose or type one of the service and each service will give you detailed instruction to complete the service. \n\n Should you need human help, please contact 0800-000-000 for human help.");
            await context.PostAsync(cardmessage);
        }
    }
}