using KeepCoding;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Module2048Script : ModuleScript
{
	public int Size;
	public int Goal;

	public Transform AnchorsWrapper;
	public GameObject BlankTile;
	public GameTile TileObject;
	public KMSelectable[] DirectionButtons;
	public TextMesh VersionLabel;
	public TextMesh ScoreLabel;
	public GameObject Win2048;

	internal Grid2048 grid;

	private float tileScale;
	private static readonly float gridSize = 0.048f;
	private Transform[,] anchors;

	private bool justWon2048;
	private bool hasWon2048;

	private int currentScoreValue;
	private int CurrentScore { get { return currentScoreValue; } set { currentScoreValue = value; ScoreLabel.text = string.Format("{0:n0}", currentScoreValue); } }

	private void Start()
	{
		HideDirectionHighlights(false);

		grid = new Grid2048(Size);

		Win2048.SetActive(false);

		if (!IsEditor) VersionLabel.text = Version;
		//CurrentScore = 0;

		tileScale = TileObject.transform.localScale.x;
		TileObject.gameObject.SetActive(false);
		CreateAnchors();

		//AddStartTiles();
		//initialScore = CurrentScore;

		Reset();

		//Actuate();
		//LogGrid(Direction.Reset);

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

	private DigTile[,] lastLoggedGrid;

	private class GridLog
	{
		public class GridLogTile
		{
			public int value;
			public int x;
			public int y;
			public Coord previousPosition;
			public List<GridLogTile> mergedFrom;

			public GridLogTile(DigTile tile)
			{
				value = tile.Value;
				x = tile.X;
				y = tile.Y;
				previousPosition = tile.PreviousPosition;
				mergedFrom = tile.MergedFrom == null ? null : tile.MergedFrom.Select(t => new GridLogTile(t)).ToList();
			}
		}

		public int score;
		public Direction direction;
		public List<GridLogTile> tiles = new List<GridLogTile>();

		public GridLog(int newScore, Direction moved)
		{
			score = newScore;
			direction = moved;
		}
	}

	private void LogGrid(Direction direction)
	{
		if (lastLoggedGrid != null)
		{
			bool flag = true;
			// dumbass tile check. wrote this when i was drunk or something and dont feel like making it not bad
			for (int y = 0; y < Size; y++) for (int x = 0; x < Size; x++) if (grid.Cells[y, x] == null && lastLoggedGrid[y, x] != null || grid.Cells[y, x] != null && lastLoggedGrid[y, x] == null || grid.Cells[y, x] != null && lastLoggedGrid[y, x] != null && grid.Cells[y, x].Value != lastLoggedGrid[y, x].Value) flag = false;
			if (flag) return;
		}

		lastLoggedGrid = new DigTile[Size, Size];
		for (int y = 0; y < Size; y++) for (int x = 0; x < Size; x++) lastLoggedGrid[y, x] = grid.Cells[y, x] ?? null;


		GridLog log = new GridLog(CurrentScore, direction);
		grid.EachCell(delegate (int x, int y, DigTile tile)
		{
			if (tile != null) log.tiles.Add(new GridLog.GridLogTile(tile));
		});

		Log("Current grid: {0}", JsonConvert.SerializeObject(log, Formatting.None, new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		}));
	}

	private void Reset()
	{
		if (justWon2048)
		{
			justWon2048 = false;
			Win2048.SetActive(false);
			HideDirectionHighlights(false);
			return;
		}

		grid.Cells = new DigTile[Size, Size];
		CurrentScore = 0;
		AddStartTiles();
		Actuate();
		LogGrid(Direction.Reset);
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

	private void HideDirectionHighlights(bool hide)
	{
		List<KMSelectable> children;
		if (hide) children = new List<KMSelectable> { DirectionButtons[4] };
		else children = new List<KMSelectable> { DirectionButtons[0], DirectionButtons[0], DirectionButtons[1], DirectionButtons[2], DirectionButtons[4], DirectionButtons[3] };
		Get<KMSelectable>().Children = children.ToArray();
		Get<KMSelectable>().UpdateChildren(hide ? DirectionButtons[4] : DirectionButtons[0]);
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

	internal enum Direction
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

	internal void MoveDirection(Direction direction)
	{
		if (justWon2048 && !direction.Equals(Direction.Reset)) return;

		ButtonEffect(DirectionButtons[(int)direction], 0.5f, KMSoundOverride.SoundEffect.ButtonPress);

		if (direction.Equals(Direction.Reset))
		{
			Reset();
			return;
		}

		Move(direction);
	}

	private void CreateAnchors()
	{
		anchors = new Transform[Size, Size];

		for (int y = 0; y < Size; y++)
		{
			for (int x = 0; x < Size; x++)
			{
				GameObject anchor = new GameObject("anchor{0}{1}".Form(x, y));
				anchor.transform.parent = AnchorsWrapper;
				anchor.transform.localPosition = new Vector3(Mathf.Lerp(-gridSize, gridSize, (float)x / (Size - 1)), 0f, Mathf.Lerp(gridSize, -gridSize, (float)y / (Size - 1)));
				anchor.transform.localScale = Vector3.one;
				anchor.transform.localRotation = Quaternion.identity;
				anchors[y, x] = anchor.transform;
			}
		}

		foreach (Transform anchor in anchors)
		{
			GameObject tile = Instantiate(BlankTile, anchor);
			tile.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
			tile.transform.localPosition = new Vector3(0f, -0.0001f, 0f);
		}
		BlankTile.SetActive(false);
	}

	private void AddStartTiles()
	{
		/* foreach (int value in startTiles) AddRandomTile(value);
		MoveDirection((Direction)Random.Range(0, 4)); */
		for (int i = 0; i < 2; i++) AddRandomTile();
	}

	internal void AddRandomTile(int? valueOverride = null)
	{
		if (grid.CellsAvailable())
		{
			int value = valueOverride == null ? Random.value < 0.9f ? 2 : 4 : (int)valueOverride;
			DigTile tile = new DigTile(grid.RandomAvailableCell(), value);
			grid.InsertTile(tile);
		}
	}

	private void PrepareTiles()
	{
		grid.EachCell(delegate (int x, int y, DigTile tile)
		{
			if (tile != null)
			{
				tile.MergedFrom = null;
				tile.SavePosition();
			}
		});
	}

	private void MoveTile(DigTile tile, Coord cell)
	{
		grid.Cells[tile.Y, tile.X] = null;
		grid.Cells[cell.y, cell.x] = tile;
		tile.UpdatePosition(cell);
	}

	private void Move(Direction direction)
	{
		Coord cell;
		DigTile tile;

		Coord vector = GetDirectionalOffset(direction);
		Traversals traversals = Traversals.BuildTraversals(Size, vector);
		bool moved = false;

		PrepareTiles();

		foreach (int x in traversals.x)
		{
			foreach (int y in traversals.y)
			{
				cell = new Coord(x, y);
				tile = grid.CellContent(cell);

				if (tile != null)
				{
					FarthestPosition positions = FindFarthestPosition(cell, vector);
					DigTile next = grid.CellContent(positions.Next);

					if (next != null && next.Value == tile.Value && next.MergedFrom == null)
					{
						DigTile merged = new DigTile(positions.Next, tile.Value * 2);
						merged.MergedFrom = new List<DigTile> { tile, next };

						grid.InsertTile(merged);
						grid.RemoveTile(tile);

						tile.UpdatePosition(positions.Next);

						CurrentScore += merged.Value;

						if (merged.Value == Goal) Solve();
						if (merged.Value == 2048 && !hasWon2048)
						{
							Win2048.SetActive(true);
							hasWon2048 = justWon2048 = true;
							HideDirectionHighlights(true);
						}
					}
					else MoveTile(tile, positions.Farthest);

					if (!PositionsEqual(cell, tile)) moved = true;
				}
			}
		}

		if (moved)
		{
			AddRandomTile();

			Actuate();
			LogGrid(direction);

			if (!MovesAvailable()) GameOver();
		}
	}

	private class Traversals
	{
		public List<int> x = new List<int>();
		public List<int> y = new List<int>();

		public static Traversals BuildTraversals(int size, Coord vector)
		{
			Traversals traversals = new Traversals();

			for (int pos = 0; pos < size; pos++)
			{
				traversals.x.Add(pos);
				traversals.y.Add(pos);
			}

			if (vector.x > 0) traversals.x.Reverse();
			if (vector.y > 0) traversals.y.Reverse();

			return traversals;
		}
	}

	private class FarthestPosition
	{
		public Coord Farthest;
		public Coord Next;
		
		public FarthestPosition(Coord farthest, Coord next)
		{
			Farthest = farthest;
			Next = next;
		}
	}

	private FarthestPosition FindFarthestPosition(Coord cell, Coord vector)
	{
		Coord previous;

		do
		{
			previous = cell;
			cell = new Coord(previous.x + vector.x, previous.y + vector.y);
		} while (grid.WithinBounds(cell) && grid.CellAvailable(cell));

		return new FarthestPosition(previous, cell);
	}

	private bool PositionsEqual(Coord first, DigTile second)
	{
		return PositionsEqual(first, new Coord(second.X, second.Y));
	}

	private bool PositionsEqual(Coord first, Coord second)
	{
		return first.x == second.x && first.y == second.y;
	}


	private readonly List<GameTile> ActuateddTiles = new List<GameTile>();

	internal void Actuate()
	{
		foreach (GameTile tile in ActuateddTiles) Destroy(tile.gameObject);
		ActuateddTiles.Clear();

		foreach (DigTile tile in grid.Cells) if (tile != null) ActuateTile(tile);
	}

	private void ActuateTile(DigTile tile)
	{
		GameTile gameTile = Instantiate(TileObject, AnchorsWrapper);
		gameTile.gameObject.SetActive(true);
		gameTile.SetValue(tile.Value);

		Coord anchorPosition = tile.PreviousPosition ?? new Coord(tile.X, tile.Y);

		gameTile.transform.localPosition = anchors[anchorPosition.y, anchorPosition.x].localPosition;

		if (tile.PreviousPosition != null)
		{
			StartCoroutine(ActuateTileToPosition(gameTile.transform, anchorPosition, new Coord(tile.X, tile.Y)));
		}
		else if (tile.MergedFrom != null)
		{
			StartCoroutine(TileAddMerge(gameTile.transform));
			foreach (DigTile merged in tile.MergedFrom) ActuateTile(merged);
			gameTile.transform.localPosition += new Vector3(0f, 0.00015f, 0f);
		}
		else StartCoroutine(TileAddNew(gameTile.transform));

		ActuateddTiles.Add(gameTile);
	}

	private IEnumerator ActuateTileToPosition(Transform tile, Coord oldPosition, Coord newPosition)
	{
		Vector3 oldAnchor = anchors[oldPosition.y, oldPosition.x].localPosition;
		Vector3 newAnchor = anchors[newPosition.y, newPosition.x].localPosition;

		float duration = 0.1f;
		float initialTime = Time.time;
		while (Time.time - initialTime < duration && tile)
		{
			float lerp = (Time.time - initialTime) / duration;
			float xPos = Mathf.SmoothStep(oldAnchor.x, newAnchor.x, lerp);
			float zPos = Mathf.SmoothStep(oldAnchor.z, newAnchor.z, lerp);
			tile.localPosition = new Vector3(xPos, 0f, zPos);
			yield return null;
		}
		if (tile) tile.localPosition = newAnchor;
		yield break;
	}

	private IEnumerator TileAddNew(Transform tile)
	{
		float duration = 0.2f;
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
			float scale = Mathf.SmoothStep(0f, tileScale * 1.3f, lerp);
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
	}

	private void GameOver()
	{
		Strike("Out of moves!");
		Reset();
	}

	private bool MovesAvailable()
	{
		return grid.CellsAvailable() || TileMatchesAvailable();
	}

	private bool TileMatchesAvailable()
	{
		for (int y = 0; y < Size; y++)
		{
			for (int x = 0; x < Size; x++)
			{
				DigTile tile = grid.CellContent(new Coord(x, y));

				if (tile != null)
				{
					for (int direction = 0; direction < 4; direction++)
					{
						Coord vector = GetDirectionalOffset((Direction)direction);
						Coord cell = new Coord(x + vector.x, y + vector.y);

						DigTile other = grid.CellContent(cell);

						if (other != null && other.Value == tile.Value) return true;
					}
				}
			}
		}
		return false;
	}
}