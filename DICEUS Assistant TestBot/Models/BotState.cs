using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICEUS_Assistant_TestBot.Models;

public enum BotState
{
	WaitingForPassport,
	ConfirmingPassport,
	WaitingForTechPassport,
	ConfirmingTechPassport
}
