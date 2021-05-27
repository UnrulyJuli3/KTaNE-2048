using KeepCoding;
using System.Collections;

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


	// i will make an actual autosolver eventually
	public override IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
		
		// free up cells if there are none to put the goal tile
		if (!Module.grid.CellsAvailable()) Module.grid.Cells = new DigTile[Module.Size, Module.Size];

		// add goal tile
		Module.AddRandomTile(Module.Goal);

		// display
		Module.Actuate();

		// actuate doesn't actually do the solving calculations so... solve manually
		Module.Solve();
	}
}