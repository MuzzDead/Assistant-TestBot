using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICEUS_Assistant_TestBot.Models;

public class UserSession
{
	public string? PassportFileId { get; set; }
	public string? TechPassportFileId { get; set; }

	public bool IsPassportConfirmed { get; set; }
	public bool IsTechPassportConfirmed { get; set; }

	public BotState CurrentState { get; set; } = BotState.WaitingForPassport;

	public string? ExtractedFirstName { get; set; }
	public string? ExtractedLastName { get; set; }

	public Dictionary<string, string>? FakeTechPassportData { get; set; }
	public bool IsPriceConfirmed { get; set; }

}
