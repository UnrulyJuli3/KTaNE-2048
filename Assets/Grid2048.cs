using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Just calling it "Grid" can cause conflicts with default Unity classes
public class Grid2048
{
	public DigTile[,] Cells = new DigTile[4, 4];
	public DigTile[,] StartingGrid = new DigTile[4, 4];

	public Coord RandomAvailableCell()
	{
		if (CellsAvailable()) return AvailableCells().PickRandom();
		return null;
	}

	public List<Coord> AvailableCells()
	{
		List<Coord> cells = new List<Coord>();
		EachCell(delegate (int x, int y, DigTile tile)
		{
			if (tile == null) cells.Add(new Coord(x, y));
		});
		return cells;
	}

	public delegate void EachCellCallback(int x, int y, DigTile tile);
	public void EachCell(EachCellCallback callback, bool startingGrid = false)
	{
		if (callback != null) for (int x = 0; x < 4; x++) for (int y = 0; y < 4; y++) callback(x, y, startingGrid ? StartingGrid[y, x] : Cells[y, x]);
	}

	public bool CellsAvailable()
	{
		return AvailableCells().Count() > 0;
	}

	public bool CellAvailable(Coord cell)
	{
		return CellContent(cell) == null;
	}

	public bool CellOccupied(Coord cell)
	{
		return !CellAvailable(cell);
	}

	public DigTile CellContent(Coord cell)
	{
		if (WithinBounds(cell)) return Cells[cell.y, cell.x];
		return null;
	}

	public void InsertTile(DigTile tile)
	{
		Cells[tile.Y, tile.X] = tile;
	}

	public void RemoveTile(DigTile tile)
	{
		Cells[tile.Y, tile.X] = null;
	}

	public bool WithinBounds(Coord position)
	{
		return position.x.InRange(0, 3) && position.y.InRange(0, 3);
	}
}