using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MSA_ContosoBank.Controllers
{
    [BotAuthentication]
    [RoutePrefix("api/calling")]
    public class CallingController : ApiController
    {
        public CallingController() : base()
        {
            CallingConversation.RegisterCallingBot(CallingBotService => new CallBot(CallingBotService));
        }

        [Route("call")]
        public async Task<HttpResponseMessage> ProcessIncomingCallAsync()
        {
            return await CallingConversation.SendAsync(this.Request, CallRequestType.IncomingCall);
        }

        [Route("callback")]
        public async Task<HttpResponseMessage> ProcessCallingEventAsync()
        {
            return await CallingConversation.SendAsync(this.Request, CallRequestType.CallingEvent);
        }
    }
}
