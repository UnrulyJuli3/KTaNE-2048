using KeepCoding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Module2048Script : ModuleScript
{
	public Transform AnchorsWrapper;
	public GameObject BlankTile;
	public GameTile TileObject;
	public KMSelectable[] DirectionButtons;

	private float tileScale;
	private static readonly float gridSize = 0.048f;
	private Transform[,] anchors;
	//private GameTile[,] currentGrid = new GameTile[4, 4];
	//private readonly GameTile[,] initialState = new GameTile[4, 4];

	private void Start()
	{
		tileScale = TileObject.transform.localScale.x;
		TileObject.gameObject.SetActive(false);
		CreateAnchors();

		//for (int y = 0; y < 4; y++) for (int x = 0; x < 4; x++) initialState[y, x] = currentGrid[y, x];

		for (int i = 0; i < DirectionButtons.Length; i++)
		{
			Direction direction = (Direction)i;
			DirectionButtons[i].OnInteract += delegate
			{
				MoveDirection(direction);
				return false;
			};
		}

		Get<KMSelectable>().Assign(onInteract: OnInteract, onDefocus: OnDefocus);
	}

	private class Traversals
	{
		public List<int> x = new List<int>();
		public List<int> y = new List<int>();

		public static Traversals BuildTraversals(Coord vector)
		{
			Traversals traversals = new Traversals();

			for (int pos = 0; pos < 4; pos++) {
				traversals.x.Add(pos);
				traversals.y.Add(pos);
			}

			if (vector.x > 0) traversals.x.Reverse();
			if (vector.y > 0) traversals.y.Reverse();

			return traversals;
		}
	}


	private static readonly Dictionary<Direction, KeyCode> directionKeys = new Dictionary<Direction, KeyCode>
	{
		{ Direction.Up, KeyCode.UpArrow },
		{ Direction.Left, KeyCode.LeftArrow },
		{ Direction.Right, KeyCode.RightArrow },
		{ Direction.Down, KeyCode.DownArrow },
		{ Direction.Reset, KeyCode.R }
	};

	private readonly Dictionary<Direction, bool> holdingDirections = new Dictionary<Direction, bool>
	{
		{ Direction.Up, false },
		{ Direction.Left, false },
		{ Direction.Right, false },
		{ Direction.Down, false },
		{ Direction.Reset, false }
	};

	private bool isSelected;

	private void OnInteract()
	{
		isSelected = true;
	}

	private void OnDefocus()
	{
		isSelected = false;
	}

	private void Update()
	{
		foreach (KeyValuePair<Direction, KeyCode> pair in directionKeys)
		{
			if (Input.GetKey(pair.Value))
			{
				if (!holdingDirections[pair.Key] && isSelected)
				{
					holdingDirections[pair.Key] = true;
					MoveDirection(pair.Key);
				}
			}
			else if (holdingDirections[pair.Key]) holdingDirections[pair.Key] = false;
		}
	}

	private enum Direction
	{
		Up,
		Left,
		Right,
		Down,
		Reset
	}

	private static Coord GetDirectionalOffset(Direction direction)
	{
		switch (direction)
		{
			case Direction.Up:
				return new Coord(0, -1);
			case Direction.Left:
				return new Coord(-1, 0);
			case Direction.Right:
				return new Coord(1, 0);
		}
		return new Coord(0, 1);
	}

	private void MoveDirection(Direction direction)
	{
		if (direction.Equals(Direction.Reset))
		{
			Log("Reset module");
			//ResetToOriginalState();
			return;
		}
		Log("Moved {0}", direction.ToString().ToLowerInvariant());
		ButtonEffect(DirectionButtons[(int)direction], 0.7f, KMSoundOverride.SoundEffect.ButtonPress);

		Coord vector = GetDirectionalOffset(direction);
		
	}

	private void CreateAnchors()
	{
		anchors = new Transform[4, 4];

		for (int y = 0; y < 4; y++)
		{
			for (int x = 0; x < 4; x++)
			{
				GameObject anchor = new GameObject("anchor{0}{1}".Form(x, y));
				anchor.transform.parent = AnchorsWrapper;
				anchor.transform.localPosition = new Vector3(Mathf.Lerp(-gridSize, gridSize, x / 3f), 0f, Mathf.Lerp(gridSize, -gridSize, y / 3f));
				anchor.transform.localScale = Vector3.one;
				anchors[y, x] = anchor.transform;
			}
		}

		foreach (Transform anchor in anchors)
		{
			GameObject tile = Instantiate(BlankTile, anchor);
			tile.transform.localPosition = new Vector3(0f, -0.0001f, 0f);
		}
		Destroy(BlankTile);
	}

	/* private enum TileAddAnim
	{
		New,
		Merge
	}

	private void CreateTile(int value, int x, int y, TileAddAnim animationType)
	{
		if (currentGrid[y, x] != null) Destroy(currentGrid[y, x].gameObject);

		GameTile tile = Instantiate(TileObject, AnchorsWrapper);
		currentGrid[y, x] = tile;
		tile.gameObject.SetActive(true);
		tile.SetValue(value);
		tile.transform.localPosition = anchors[y, x].localPosition;
		switch (animationType)
		{
			case TileAddAnim.New:
				StartCoroutine(TileAddNew(tile.transform));
				break;
			case TileAddAnim.Merge:
				StartCoroutine(TileAddMerge(tile.transform));
				break;
		}
	}

	private IEnumerator TileAddNew(Transform tile)
	{
		float duration = 0.3f;
		float initialTime = Time.time;
		while (Time.time - initialTime < duration && tile)
		{
			float lerp = (Time.time - initialTime) / duration;
			float scale = Mathf.SmoothStep(0f, tileScale, lerp);
			tile.localScale = new Vector3(scale, scale, scale);
			yield return null;
		}
		if (tile) tile.localScale = new Vector3(tileScale, tileScale, tileScale);
		yield break;
	}

	private IEnumerator TileAddMerge(Transform tile)
	{
		float duration = 0.1f;
		float initialTime = Time.time;
		while (Time.time - initialTime < duration && tile)
		{
			float lerp = (Time.time - initialTime) / duration;
			float scale = Mathf.SmoothStep(tileScale * 0.7f, tileScale * 1.3f, lerp);
			tile.localScale = new Vector3(scale, scale, scale);
			yield return null;
		}
		initialTime = Time.time;
		while (Time.time - initialTime < duration && tile)
		{
			float lerp = (Time.time - initialTime) / duration;
			float scale = Mathf.SmoothStep(tileScale * 1.3f, tileScale, lerp);
			tile.localScale = new Vector3(scale, scale, scale);
			yield return null;
		}
		if (tile) tile.localScale = new Vector3(tileScale, tileScale, tileScale);
		yield break;
	} */



	/* private IEnumerator MoveTileToPosition(int oldX, int oldY, int newX, int newY, int? newValue = null)
	{
		GameTile tile = currentGrid[oldY, oldX];

		Vector3 oldAnchor = anchors[oldY, oldX].localPosition;
		Vector3 newAnchor = anchors[newY, newX].localPosition;

		float duration = 0.3f;
		float initialTime = Time.time;
		while (Time.time - initialTime < duration)
		{
			float lerp = EaseIn((Time.time - initialTime) / duration);
			float xPos = Mathf.Lerp(oldAnchor.x, newAnchor.x, lerp);
			float zPos = Mathf.Lerp(oldAnchor.z, newAnchor.z, lerp);
			tile.transform.localPosition = new Vector3(xPos, 0f, zPos);
			yield return null;
		}
		tile.transform.localPosition = newAnchor;

		if (newValue != null)
		{
			Destroy(tile.gameObject);
			CreateTile((int)newValue, newX, newY, TileAddAnim.Merge);
		}
		yield break;
	}

	private static float EaseIn(float k)
	{
		return k * k * k;
	} */

	private void GameOver()
	{
		Strike("Out of moves!");
	}
}