using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Grid
{
	public GameTile[,] Cells = new GameTile[4, 4];


	public List<Coord> AvailableCells()
	{
		List<Coord> cells = new List<Coord>();
		for (int y = 0; y < 4; y++) for (int x = 0; x < 4; x++) if (Cells[y, x] == null) cells.Add(new Coord(x, y));
		return cells;
	}
}