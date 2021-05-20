﻿using KeepCoding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TP2048Script : TPScript<Module2048Script>
{
	private Module2048Script.Direction GetDirection(string phrase)
	{
		switch (phrase)
		{
			case "u":
			case "up":
			case "top":
				return Module2048Script.Direction.Up;
			case "l":
			case "left":
				return Module2048Script.Direction.Left;
			case "r":
			case "right":
				return Module2048Script.Direction.Right;
			case "d":
			case "down":
			case "bottom":
				return Module2048Script.Direction.Down;
		}
		return Module2048Script.Direction.Reset;
	}

	public override IEnumerator ProcessTwitchCommand(string command)
	{
		string[] split = command.ToLowerInvariant().Split();
		switch (split[0])
		{
			case "move":
			case "m":
			case "go":
			case "shift":
				if (split.Length > 1)
				{
					Module2048Script.Direction direction = GetDirection(split[1]);
					if (!direction.Equals(Module2048Script.Direction.Reset))
					{
						yield return null;
						Module.MoveDirection(direction);
					}
				}
				break;
			case "reset":
				yield return null;
				Module.MoveDirection(Module2048Script.Direction.Reset);
				break;
		}
		yield break;
	}

	public override IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
		Module.AddRandomTile(2048);
		Module.Actuate();
		yield break;
	}
}