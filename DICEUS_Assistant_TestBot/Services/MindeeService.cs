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

	public async Task<ExtractedPassportData> ExtractDataFromInternationalIdAsync(Stream stream, string fileName)
	{
		// Generate a temporary filename
		var tempFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
		var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

		// Save the input stream to a local temporary file
		using (var fileStream = File.Create(tempFilePath))
		{
			stream.Position = 0;
			await stream.CopyToAsync(fileStream);
		}

		// Create a Mindee input source from the local file
		var inputSource = new LocalInputSource(tempFilePath);

		// Enqueue the document and wait for it to be parsed by Mindee
		var response = await _client.EnqueueAndParseAsync<InternationalIdV2>(inputSource);

		var prediction = response.Document.Inference.Prediction;

		// Map the extracted fields to our data model
		return new ExtractedPassportData
		{
			FirstName = prediction.GivenNames.FirstOrDefault()?.Value ?? "Unknown",
			LastName = prediction.Surnames.FirstOrDefault()?.Value ?? "Unknown",
			DocumentNumber = prediction.DocumentNumber?.Value ?? "Unknown",
			Sex = prediction.Sex?.Value ?? "Unknown",
			Nationality = prediction.Nationality?.Value ?? "Unknown",

			BirthDate = DateTime.TryParse(prediction.BirthDate?.Value, out var birthDate)
				? birthDate
				: DateTime.MinValue,

			ExpirationDate = DateTime.TryParse(prediction.ExpiryDate?.Value, out var expiryDate)
				? expiryDate
				: DateTime.MinValue
		};
	}
}
