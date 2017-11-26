using Microsoft.Bot.Builder.Calling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Calling.Events;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using MSA_ContosoBank.Services;

namespace MSA_ContosoBank
{
    public class CallBot : IDisposable, ICallingBot
    {

        private readonly MicrosoftCognitiveSpeechService speechService = new MicrosoftCognitiveSpeechService();

        public CallBot(ICallingBotService callingBotService)
        {
            this.CallingBotService = callingBotService;

            this.CallingBotService.OnIncomingCallReceived += this.onIncomingCallReceived;
            this.CallingBotService.OnRecordCompleted += this.onRecordCompleted;
            this.CallingBotService.OnHangupCompleted += onHangupCompleted;
        }

        private static Task onHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {
            hangupOutcomeEvent.ResultingWorkflow = null;
            return Task.FromResult(true);
        }

        private async Task onRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            List<ActionBase> actions = new List<ActionBase>();
            var spokenText = string.Empty;
            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success)
            {
                var record = await recordOutcomeEvent.RecordedContent;
                spokenText = await this.speechService.GetTextFromAudioAsync(record);
                actions.Add(new PlayPrompt
                {
                    OperationId = Guid.NewGuid().ToString(),
                    Prompts = new List<Prompt> {
                    new Prompt {Value = "Thanks for leaving the message" },
                    new Prompt {Value = "You said..." + spokenText} } });
            }
            else
            {
                actions.Add(new PlayPrompt { OperationId = Guid.NewGuid().ToString(),
                Prompts = new List<Prompt>
                {
                    new Prompt{Value = "Sorry, there was an issue. "}
                }
                });

                actions.Add(new Hangup { OperationId = Guid.NewGuid().ToString() });

                recordOutcomeEvent.ResultingWorkflow.Actions = actions;
                recordOutcomeEvent.ResultingWorkflow.Links = null;
            }
        }

        private Task onIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            var record = new Record
            {
                OperationId = Guid.NewGuid().ToString(),
                PlayPrompt = new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { new Prompt { Value = "Please leave a message" } } },
                RecordingFormat = Microsoft.Bot.Builder.Calling.ObjectModel.Misc.RecordingFormat.Wav
            };

            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
            {
                new Answer {OperationId = Guid.NewGuid().ToString()},
                record
            };

            return Task.FromResult(true);
        }

        public ICallingBotService CallingBotService { get; }

        public void Dispose()
        {
            if (this.CallingBotService != null)
            {
                this.CallingBotService.OnIncomingCallReceived -= this.onIncomingCallReceived;
                this.CallingBotService.OnRecordCompleted -= this.onRecordCompleted;
                this.CallingBotService.OnHangupCompleted -= onHangupCompleted;
            }
        }
    }
}