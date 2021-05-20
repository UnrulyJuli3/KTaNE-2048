using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
	public TextMesh Label;
	public Renderer TileRenderer;

	[HideInInspector]
	public int Value;
	[HideInInspector]
	public int X;
	[HideInInspector]
	public int Y;
	[HideInInspector]
	public Coord PreviousPosition;
	[HideInInspector]
	public List<GameTile> MergedFrom;

	public void SavePosition()
	{
		PreviousPosition = new Coord(X, Y);
	}

	public void UpdatePosition(Coord position)
	{
		X = position.x;
		Y = position.y;
	}

	private float CalculateFontMultiplier()
	{
		switch (Value.ToString().Length)
		{
			case 3:
				return 0.8f;
			case 4:
				return 0.63f;
			case 5:
				return 0.5f;
			case 6:
				return 0.425f;
		}
		return 1f;
	}

	private int CalculateFontSize()
	{
		return Mathf.RoundToInt(80f * CalculateFontMultiplier());
	}

	public void SetValue(int value)
	{
		Value = value;
		Label.text = Value.ToString();
		Label.fontSize = CalculateFontSize();

		TileStyle style = TileStyles.GetStyleForValue(Value);
		TileRenderer.material.color = style.Color;
		Label.color = style.TextColor;
	}
}
