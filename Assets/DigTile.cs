using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigTile
{
	public int Value;
	public int X;
	public int Y;
	public Coord PreviousPosition;
	public List<DigTile> MergedFrom;

	public void SavePosition()
	{
		PreviousPosition = new Coord(X, Y);
	}

	public void UpdatePosition(Coord position)
	{
		X = position.x;
		Y = position.y;
	}

	public DigTile(Coord position, int value)
	{
		X = position.x;
		Y = position.y;
		Value = value;
	}

	public DigTile(DigTile tileToClone)
	{
		Value = tileToClone.Value;
		X = tileToClone.X;
		Y = tileToClone.Y;
		PreviousPosition = tileToClone.PreviousPosition;
		MergedFrom = tileToClone.MergedFrom;
	}
}
