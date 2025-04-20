using DICEUS_Assistant_TestBot.Models;
using Mindee;
using Mindee.Input;
using Mindee.Product.InternationalId;

namespace DICEUS_Assistant_TestBot.Services;

public class MindeeService
{
	private readonly MindeeClient _client;

	public MindeeService(string apiKey)
	{
		_client = new MindeeClient(apiKey);
	}

	public async Task<ExtractedData> ExtractDataFromInternationalIdAsync(Stream stream, string fileName)
	{
		var tempFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
		var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

		using (var fileStream = File.Create(tempFilePath))
		{
			stream.Position = 0;
			await stream.CopyToAsync(fileStream);
		}

		var inputSource = new LocalInputSource(tempFilePath);
		var response = await _client.EnqueueAndParseAsync<InternationalIdV2>(inputSource);
		var prediction = response.Document.Inference.Prediction;

		return new ExtractedData
		{
			FirstName = prediction.GivenNames.FirstOrDefault()?.Value ?? "Unknown",
			LastName = prediction.Surnames.FirstOrDefault()?.Value ?? "Unknown",
			DocumentNumber = prediction.DocumentNumber?.Value ?? "Unknown",
			Sex = prediction.Sex?.Value ?? "Unknown",
			Nationality = prediction.Nationality?.Value ?? "Unknown",
			PersonalNumber = prediction.PersonalNumber.Value ?? "Unknown",
			BirthDate = DateTime.TryParse(prediction.BirthDate?.Value, out var birthDate)
				? birthDate
				: DateTime.MinValue,
			ExpirationDate = DateTime.TryParse(prediction.ExpiryDate?.Value, out var expiryDate)
				? expiryDate
				: DateTime.MinValue
		};
	}
}
