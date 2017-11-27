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
using System.IO;
using System.Diagnostics;
using Microsoft.Bot.Builder.Dialogs;

namespace MSA_ContosoBank
{
    public class CallBot : IDisposable, ICallingBot
    {
        private readonly MicrosoftCognitiveSpeechService speechService = new MicrosoftCognitiveSpeechService();

        public CallBot(ICallingBotService callingBotService)
        {
            this.CallingBotService = callingBotService;

            this.CallingBotService.OnIncomingCallReceived += this.OnIncomingCallReceived;
            this.CallingBotService.OnRecordCompleted += this.OnRecordCompleted;
            this.CallingBotService.OnHangupCompleted += OnHangupCompleted;
        }

        public ICallingBotService CallingBotService { get; }

        public void Dispose()
        {
            if (this.CallingBotService != null)
            {
                this.CallingBotService.OnIncomingCallReceived -= this.OnIncomingCallReceived;
                this.CallingBotService.OnRecordCompleted -= this.OnRecordCompleted;
                this.CallingBotService.OnHangupCompleted -= OnHangupCompleted;
            }
        }

        private static Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {
            hangupOutcomeEvent.ResultingWorkflow = null;
            return Task.FromResult(true);
        }

        private static PlayPrompt GetPromptForText(string text)
        {
            var prompt = new Prompt { Value = text, Voice = VoiceGender.Male };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }

        private Task OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            var record = new Record
            {
                OperationId = Guid.NewGuid().ToString(),
                PlayPrompt = new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { new Prompt { Value = "Please leave a message" } } },
                RecordingFormat = RecordingFormat.Wav
            };

            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase> {
                new Answer { OperationId = Guid.NewGuid().ToString() },
                record
            };

            return Task.FromResult(true);
        }

        private async Task OnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            List<ActionBase> actions = new List<ActionBase>();

            var spokenText = string.Empty;
            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success)
            {
                var record = await recordOutcomeEvent.RecordedContent;
                spokenText = await this.speechService.GetTextFromAudioAsync(record);
                actions.Add(new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { new Prompt { Value = "Thanks for leaving the message." }, new Prompt { Value = "You said... " + spokenText } } });
            }
            else
            {
                actions.Add(new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { new Prompt { Value = "Sorry, there was an issue. " } } });
            }

            actions.Add(new Hangup { OperationId = Guid.NewGuid().ToString() }); // hang up the call

            recordOutcomeEvent.ResultingWorkflow.Actions = actions;
            recordOutcomeEvent.ResultingWorkflow.Links = null;
            
        }

        private async Task SendSTTResultToUser(string text, IEnumerable<Participant> participants)
        {
            var to = participants.Single(x => x.Originator);
            var from = participants.First(x => !x.Originator);

            await AgentListener.Resume(to.Identity, to.DisplayName, from.Identity, from.DisplayName, to.Identity, text);
        }

        private async Task<string> GetTextFromAudioAsync(Stream audiostream)
        {
            var text = await this.speechService.GetTextFromAudioAsync(audiostream);
            Debug.WriteLine(text);
            return text;
        }
    }
}