using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
	public TextMesh Label;
	public Renderer TileRenderer;

	private float CalculateFontMultiplier(int value)
	{
		switch (value.ToString().Length)
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

	private int CalculateFontSize(int value = 2)
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
