using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MSA_ContosoBank
{
    public class AgentListener
    {
        // This will send an adhoc message to the user
        public static async Task Resume(
            string toId,
            string toName,
            string fromId,
            string fromName,
            string conversationId,
            string message,
            string serviceUrl = "https://smba.trafficmanager.net/apis/",
            string channelId = "skype")
        {
            if (!MicrosoftAppCredentials.IsTrustedServiceUrl(serviceUrl))
            {
                MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);
            }

            try
            {
                var userAccount = new ChannelAccount(fromId, fromName);
                var botAccount = new ChannelAccount(toId, toName);
                var connector = new ConnectorClient(new Uri(serviceUrl));

                IMessageActivity activity = Activity.CreateMessageActivity();

                if (!string.IsNullOrEmpty(conversationId) && !string.IsNullOrEmpty(channelId))
                {
                    activity.ChannelId = channelId;
                }
                else
                {
                    conversationId = (await connector.Conversations.CreateDirectConversationAsync(userAccount, botAccount)).Id;
                }

                activity.From = userAccount;
                activity.Recipient = botAccount;
                activity.Conversation = new ConversationAccount(id: conversationId);
                activity.Text = message;
                activity.Locale = "en-Us";
                await connector.Conversations.SendToConversationAsync((Activity)activity);

                await Conversation.SendAsync(activity, () => new Dialogs.LuisDialog());

            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp);
            }
        }
    }
}
