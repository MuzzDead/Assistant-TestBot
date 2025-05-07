using DICEUS_Assistant_TestBot.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DICEUS_Assistant_TestBot.Handlers
{
	public class TextMessageHandler
	{
		private readonly OpenAIService _openAIService;

		public TextMessageHandler(OpenAIService openAIService)
		{
			_openAIService = openAIService;
		}

		public async Task HandleAsync(ITelegramBotClient botClient, Message message, BotState botState, CancellationToken cancellationToken)
		{
			if (message.Text is null)
				return;

			if (IsLikelyQuestion(message.Text) || IsInitiatingMessage(message.Text))
			{
				string stateDescription = GetStateDescription(botState);

				string response = await _openAIService.HandleOffScriptQuestionAsync(message.Text, stateDescription);

				await botClient.SendMessage(
					chatId: message.Chat.Id,
					text: response,
					cancellationToken: cancellationToken
				);
			}
		}

		private static bool IsLikelyQuestion(string text)
		{
			if (string.IsNullOrWhiteSpace(text)) return false;

			text = text.ToLower();

			if (text.Contains("?")) return true;

			string[] questionWords = new[]
			{
				// Ukrainian
				"що", "як", "чому", "навіщо", "для чого", "скільки", "коли", "де", "хто", "який", "чи", "яка",

				// English
				"how", "what", "why", "when", "where", "who", "which", "can", "could", "would", "should",
				"do", "does", "is", "are", "will"
			};

			return questionWords.Any(word => text.StartsWith(word));
		}

		private static bool IsInitiatingMessage(string text)
		{
			string[] greetingsOrStarts = new[]
			{
				"привіт", "почати", "почнемо", "давай розпочнемо", "хай", "запусти", "починай", "старт", "давай",
				"hello", "hi", "let's start", "start", "ready", "go"
			};

			return greetingsOrStarts.Any(phrase => text.Contains(phrase));
		}

		private static string GetStateDescription(BotState botState)
		{
			return botState switch
			{
				BotState.WaitingForPassport => "waiting_for_passport",
				BotState.ConfirmingPassport => "confirming_passport_data",
				BotState.WaitingForTechPassport => "waiting_for_vehicle_document",
				BotState.ConfirmingTechPassport => "confirming_vehicle_data",
				_ => "unknown"
			};
		}
	}
}
