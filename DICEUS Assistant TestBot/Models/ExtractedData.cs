using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICEUS_Assistant_TestBot.Models;

public class ExtractedData
{
	public string? DocumentNumber { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Sex { get; set; }
	public DateTime BirthDate { get; set; }
	public string? Nationality { get; set; }
	public string? PersonalNumber { get; set; }
	public DateTime ExpirationDate { get; set; }
}
