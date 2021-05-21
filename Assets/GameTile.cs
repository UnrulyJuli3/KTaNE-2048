using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
	public TextMesh Label;
	public Renderer TileRenderer;

	private static float CalculateFontMultiplier(int value)
	{
		switch (value.ToString().Length)
		{
			case 0:
			case 1:
			case 2:
				return 1f;
			case 3:
				return 0.8f;
			case 4:
				return 0.63f;
		}
		return 0.5f;
	}

	private static int CalculateFontSize(int value = 2)
	{
		return Mathf.RoundToInt(80f * CalculateFontMultiplier(value));
	}

	public void SetValue(int value)
	{
		Label.text = value.ToString();
		Label.fontSize = CalculateFontSize(value);

		TileStyle style = TileStyles.GetStyleForValue(value);
		TileRenderer.material.color = style.Color;
		Label.color = style.TextColor;
	}
}
