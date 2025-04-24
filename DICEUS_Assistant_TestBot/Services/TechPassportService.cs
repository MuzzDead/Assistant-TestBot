using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICEUS_Assistant_TestBot.Services;

public static class TechPassportService
{
	public static Dictionary<string, string> GenerateFakeTechData()
	{
		return new Dictionary<string, string>
		{
			{ "VIN", "AB1234567890CD" },
			{ "Car Brand", "Volkswagen" },
			{ "Model", "Passat B6" },
			{ "Year", "2007" },
			{ "Fuel Type", "Diesel" },
			{ "Engine Volume", "1968 cm³" },
			{ "Color", "Black" }
		};
	}
}
