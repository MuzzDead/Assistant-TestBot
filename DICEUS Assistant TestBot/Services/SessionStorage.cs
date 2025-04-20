using DICEUS_Assistant_TestBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICEUS_Assistant_TestBot.Services;

public static class SessionStorage
{
	private static readonly Dictionary<long, UserSession> _sessions = new();

	public static UserSession GetOrCreate(long userId)
	{
		if (!_sessions.ContainsKey(userId))
			_sessions[userId] = new UserSession();

		return _sessions[userId];
	}

	public static void Reset(long userId)
	{
		_sessions[userId] = new UserSession();
	}
}
