using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Just calling it "Grid" can cause conflicts with default Unity classes
public class Grid2048
{
	public GameTile[,] Cells = new GameTile[4, 4];


	public List<Coord> AvailableCells()
	{
		List<Coord> cells = new List<Coord>();
		for (int y = 0; y < 4; y++) for (int x = 0; x < 4; x++) if (Cells[y, x] == null) cells.Add(new Coord(x, y));
		return cells;
	}

	public bool CellsAvailable()
	{
		return AvailableCells().Count() > 0;
	}

	public bool CellAvailable(Coord cell)
	{
		return !CellOccupied(cell);
	}

	public bool CellOccupied(Coord cell)
	{
		return CellContent(cell) != null;
	}

	public GameTile CellContent(Coord cell)
	{
		if (WithinBounds(cell)) return Cells[cell.y, cell.x];
		return null;
	}

	public void InsertTile(GameTile tile)
	{
		Cells[tile.Y, tile.X] = tile;
	}

	public void RemoveTile(GameTile tile)
	{
		Cells[tile.Y, tile.X] = null;
	}

	public bool WithinBounds(Coord position)
	{
		return position.x >= 0 && position.x < 4 && position.y >= 0 && position.y < 4;
	}
}