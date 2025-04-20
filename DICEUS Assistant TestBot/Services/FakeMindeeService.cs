using DICEUS_Assistant_TestBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICEUS_Assistant_TestBot.Services;

public class FakeMindeeService
{
	public static ExtractedPassportData ExtractDataFromPassport(string filePath)
	{
		return new ExtractedPassportData
		{
			FirstName = "Ivan",
			LastName = "Petrenko"
		};
	}
}
