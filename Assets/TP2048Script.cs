using KeepCoding;
using System.Collections;
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

	public override IEnumerator Process(string command)
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

	private Module2048Script.PredictedMove GetCurrentPredictedMove()
	{
		return new Module2048Script.PredictedMove(Module.CurrentScore, Module.grid, Module2048Script.Direction.Reset);
	}

	public override IEnumerator ForceSolve()
	{
		yield return null;
		if (Module.lastMovedDirection != Module2048Script.Direction.Reset) Module.MoveDirection(Module2048Script.Direction.Reset);
		while (!Module.IsSolved)
		{
			yield return new WaitForSecondsRealtime(0.1f);
			Module2048Script.PredictedMove currentMove = GetCurrentPredictedMove();
			Module2048Script.PredictedMove bestMove = null;
			for (int i = 0; i < 4; i++)
			{
				Module2048Script.PredictedMove newMove = Module2048Script.CalculateMove(Module.Size, currentMove, (Module2048Script.Direction)i);
				if (bestMove == null || newMove.NewScore > bestMove.NewScore) bestMove = newMove;
			}
			Module.MoveDirection(bestMove.MoveDirection);
		}
	}
}