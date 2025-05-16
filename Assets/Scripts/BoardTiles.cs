using UnityEngine;
using System.Linq;
using BoardContent;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

public class BoardTiles : MonoBehaviour
{
	/// <summary>
	/// The tile Template to create
	/// </summary>
	[SerializeField] private GameObject tile;
	//[SerializeField] private int awaitTimeoutMs = 1000;


	private Transform tilesParent;
	private OverlayFoundWord overlayFoundWord;
	private List<GameObject> tilesPool = new List<GameObject>();
	private int reservingTilesAmount;
	//private Mutex mutexTilesPool = new Mutex();
	private bool ModyfyingTilesPool = false;
	Camera mainCamera;
	CameraZoom mainCameraZoom;


	private int widthPrev, heightPrev;
	//private int boardMinW = 14, boardMinH = 9;



	protected LetterTileScript[,] tilesSript2D;
	private void Awake()
	{
		tilesParent = this.gameObject.transform.Find("tilesParent");
		overlayFoundWord = this.gameObject.GetComponentInChildren<OverlayFoundWord>();
		mainCamera = Camera.main;
		mainCameraZoom = mainCamera.GetComponent<CameraZoom>();
		string pathSettings = Singleton.settingsPersistent_GetSavePath();
		Singleton.settingsPersistent_loadJson(pathSettings);
	}
	private void OnEnable()
	{
		Singleton.boardUiEvents.BoardSetCaseEvent += BoardUiEvents_BoardSetCaseEventHandler;
		Singleton.boardUiEvents.CreateBoardEvent += BoardUiEvents_CreateBoardEvent;
	}

	private void BoardUiEvents_CreateBoardEvent(bool predef)
	{
		if (predef)
		{
			PlaceContentOnBoard(Singleton.choosenBoard.PredefinedBoard2D, Singleton.choosenBoard.WordsOnBoard);
		}
		else
		{
			PlaceWordsOnBoard(Singleton.choosenBoard.WordsOnBoard);
		}
		Singleton.choosenBoard.WaitingForApply = false;
	}

	private void OnDisable()
	{
		Singleton.boardUiEvents.BoardSetCaseEvent -= BoardUiEvents_BoardSetCaseEventHandler;
		Singleton.boardUiEvents.CreateBoardEvent -= BoardUiEvents_CreateBoardEvent;
	}
	private void Start()
	{
		if (Singleton.choosenBoard.WaitingForApply == true)
		{	///we have board queed
			BoardUiEvents_CreateBoardEvent(Singleton.choosenBoard.PredefinedBoard2D != null);
			return;
		}
		///load default content
		List<string> words = new List<string>() { "barbara", "ania", "Olaf", "kamil", "ola", "ślimak", "Ania", "ara", "abra"
		////"Ktoś", "Silikon", "Cadmium", "Kura", "kurczak", "kaczka", "kasia", "asia", "klaudia"
		};
		////var ss = "loach\r\nloaches\r\nload\r\nloadable\r\nloadage\r\nloaded\r\nloadedness\r\nloaden\r\nloader\r\nloaders\r\nloadinfo\r\nloading\r\nloadings\r\nloadless\r\nloadpenny\r\nloads\r\nloadsome\r\nloadspecs\r\nloadstar\r\nloadstars\r\nloadstone\r\nloadstones\r\nloadum\r\nloaf\r\n";
		////foreach( Match match in Regex.Matches(ss, "\\w+", RegexOptions.IgnoreCase))
		////{
		////	words.Add(match.Value);
		////}
		PlaceWordsOnBoard(words);
	}

	private void BoardUiEvents_BoardSetCaseEventHandler(bool UpperCase)
	{
		if (UpperCase)
		{
			foreach(var tile in tilesSript2D)
				tile.Letter = char.ToUpper(tile.Letter);
		}
		else
		{
			foreach(var tile in tilesSript2D)
				tile.Letter = char.ToLower(tile.Letter);
		}
	}


	public void PlaceContentOnBoard(char[,] content, List<string> wordsToFind)
	{
		overlayFoundWord.RemoveAllHighlights();
		///search through provided content to find wordsToFind
		var widthLocal = content.GetLength(0);
		var heightLocal = content.GetLength(1);
		CreateBoard(widthLocal, heightLocal);
		///find words on board
		var orientations = PlaceWords.FindWordsOnBoard(content, wordsToFind);
		///write onto the screen
		PlaceWords.WriteContentOntoScreen(content, tilesSript2D);
		if (orientations.Contains(WordOrientationEnum.diagonal) || orientations.Contains(WordOrientationEnum.diagonalBack)) Singleton.wordList.diagonalWords = true;
		if (orientations.Contains(WordOrientationEnum.diagonalBack) || orientations.Contains(WordOrientationEnum.horizontalBack) ||
			orientations.Contains(WordOrientationEnum.verticalBack)) Singleton.wordList.reversedWords = true;

		Singleton.boardUiEvents.RefreshBoardUi();
		ZoomCameraOnBoard();
	}
	public void PlaceWordsOnBoard(List<string> words)
	{
		overlayFoundWord.RemoveAllHighlights();

		///delegate logic to separete class
		PlaceWords placeWords = new PlaceWords(words, AspectRatio: new(14, 9), CreateBoard, wordsInReverse: Singleton.settingsPersistent.reversedWords, AdditionalCharsPercent: 1.2f);
		///try to place words on board
		placeWords.PlaceWordsOnBoardThreaded(wordPlaceMaxRetry: 100, maxThreads: 8);

		Singleton.boardUiEvents.RefreshBoardUi();

		ZoomCameraOnBoard();

		//DebugOnlyBoardDump();
	}

	/// <summary>
	/// Exports board into string, where 1-st line is words, next lines are board content
	/// </summary>
	/// <returns></returns>
	public string ExportBoard()
	{
		StringBuilder streamWriter = new StringBuilder();
		foreach (var word in Singleton.wordList.wordsToFind)
		{
			streamWriter.Append(word + " ");
		}
		streamWriter.Append("\n");
		var width = tilesSript2D.GetLength(0);
		var height = tilesSript2D.GetLength(1);
		for (int ii = 0; ii != height; ++ii)
		{
			for (int i = 0; i != width; ++i)
			{
				streamWriter.Append(tilesSript2D[i, ii].Letter);
			}
			streamWriter.Append("\n");
		}
		return streamWriter.ToString();
	}

	public void ZoomCameraOnBoard()
	{
		///14:9 -> 7.5,-4,-10 size = 5
		///8:5 ->  4,-2,-10 size = 2.5
		///18:11 -> 10,-5,-10 size = 6
		//float ratio = 14f / 9f;
		//mainCameraZoom.SetCameraDefaults(new(widthPrev / 2 + 0.5f, -(heightPrev / 2)), ((float)widthPrev) / 2.92f, new(widthPrev, heightPrev));
		mainCameraZoom.SetCameraDefaults(new(widthPrev / 2 + 0.5f, -(heightPrev / 2)), ((float)heightPrev) / 1.92f, new(widthPrev, heightPrev));
	}


	/// <summary>
	/// Creates the board only if provided dimensions are greater than current dimensions of the board
	/// </summary>
	/// <param name="width">new width</param>
	/// <param name="height">new height</param>
	/// <returns>board</returns>
	public LetterTileScript[,] CreateBoardAtLeast(int width, int height)
	{
		if (!(width > widthPrev || height > heightPrev))
			return tilesSript2D;    //double negative
		return CreateBoard(width, height);
	}

	/// <summary>
	/// makes sure there is at least given amount of tiles available
	/// </summary>
	/// <param name="amount">the amount of tiles</param>
	/// <returns>amount added to tiles pool</returns>
	public int ReserveAmountSync(int amount)
	{
		var amountToCreate = amount - tilesPool.Count;
		if (amountToCreate <= 0)
			return 0;

		tilesPool.Capacity = amount + 1; //reserve us some space
		reservingTilesAmount = amount;
		tile.SetActive(false);  //set all new tiles to not render
		var results = InstantiateAsync(tile, amountToCreate, tilesParent);
		results.WaitForCompletion();
		ModyfyingTilesPool = true;
		tilesPool.AddRange(results.Result);
		reservingTilesAmount = 0;
		ModyfyingTilesPool = false;
		return amountToCreate;
	}

	/// <summary>
	/// Creates the board with provided dimensions, and assigns Tiles
	/// </summary>
	/// <param name="width">Board width</param>
	/// <param name="height">Board height</param>
	public LetterTileScript[,] CreateBoard(int width, int height)
	{
		ReserveAmountSync(width * height);
		if (width == widthPrev && height == heightPrev)
			return tilesSript2D; //no change

		widthPrev = width;
		heightPrev = height;
		tilesSript2D = new LetterTileScript[width, height];
		List<GameObject> tilesPoolLocal;

		tilesPoolLocal = tilesPool.ToList();

		var eachTileToHide = tilesPoolLocal.Skip(width * height);
		var tilesStartingPos = tilesParent.position;
		foreach (var tile in eachTileToHide)
		{
			tile.SetActive(false); //hide the rest
			tile.transform.position = tilesStartingPos;
		}
		var tileEnum = tilesPoolLocal.GetEnumerator();

		for (int i = 0; i != width; i++)
		{
			for (int j = 0; j != height; j++)
			{
				tileEnum.MoveNext();
				GameObject spawnedTile = (GameObject)tileEnum.Current;
				spawnedTile.SetActive(true);
				spawnedTile.transform.position = new Vector3(tilesStartingPos.x + i, tilesStartingPos.y - j, tilesStartingPos.z);
				spawnedTile.transform.rotation = Quaternion.identity;
				spawnedTile.name = $"Tile-{i}-{j}";
				tilesSript2D[i, j] = spawnedTile.GetComponent<LetterTileScript>();
				tilesSript2D[i, j].SetLetter('-');
			}
		}
		Singleton.TilesSript2D = tilesSript2D;
		return tilesSript2D;
	}
}
