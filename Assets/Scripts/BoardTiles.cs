using UnityEngine;
using System.Linq;
using System.Collections;
using System.Threading;
using BoardContent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Exceptions;

public class BoardTiles : MonoBehaviour
{
	///depracteced
	[SerializeField] private int width, height;

	[SerializeField] private GameObject tile;
	private Transform tilesParent, overlayParent;
	private List<GameObject> tilesPool = new List<GameObject>();
	private int reservingTilesAmount;
	private Mutex mutexTilesPool = new Mutex();
	Camera mainCamera;
	CameraZoom mainCameraZoom;

	//sizes for camera Projection.Size	(to take up the entire screen)
	//Size: width, height
	//1: 3-2
	//2: 7-4
	//3: 11-6
	//5: 18-10
	//10: 34-20
	//w,h
	//n = 3.5 * n - 2 * n

	private int widthPrev, heightPrev;
	//private int boardMinW = 14, boardMinH = 9;



	protected LetterTileScript[,] tilesSript2D;
	private void Awake()
	{
		tilesParent = this.gameObject.transform.Find("tilesParent");
		overlayParent = this.gameObject.transform.Find("overlayParent");
		mainCamera = Camera.main;
		mainCameraZoom = mainCamera.GetComponent<CameraZoom>();

		StartCoroutine(ReserveAmountAsyncEnum());
	}
	private void OnEnable()
	{
		Singleton.boardUiEvents.BoardSetCaseEvent += BoardUiEvents_BoardSetCaseEventHandler;
		Singleton.boardUiEvents.CreateBoardEvent += BoardUiEvents_CreateBoardEvent;
	}

	private void BoardUiEvents_CreateBoardEvent(bool predef)
	{
		PlaceWordsOnBoard(Singleton.choosenBoard.wordsOnBoard);
	}

	private void OnDisable()
	{
		Singleton.boardUiEvents.BoardSetCaseEvent -= BoardUiEvents_BoardSetCaseEventHandler;
		Singleton.boardUiEvents.CreateBoardEvent -= BoardUiEvents_CreateBoardEvent;
	}
	private void Start()
	{
		//ReserveAmountAsync(1200);   //width * height

		//CreateBoard(10, 10);
		//CreateBoard(14, 9); //camera zoom = int(height/2)+1	(16 x 9 - 2x0 for UI)

		//List<string> words = new List<string>() { "barbara", "ania", "Olaf", "kamil", "ola", "ślimak", "Ania", "ara", "abra"
		////"Ktoś", "Silikon", "Cadmium", "Kura", "kurczak", "kaczka", "kasia", "asia", "klaudia"
		//};
		////var ss = "loach\r\nloaches\r\nload\r\nloadable\r\nloadage\r\nloaded\r\nloadedness\r\nloaden\r\nloader\r\nloaders\r\nloadinfo\r\nloading\r\nloadings\r\nloadless\r\nloadpenny\r\nloads\r\nloadsome\r\nloadspecs\r\nloadstar\r\nloadstars\r\nloadstone\r\nloadstones\r\nloadum\r\nloaf\r\n";
		////foreach( Match match in Regex.Matches(ss, "\\w+", RegexOptions.IgnoreCase))
		////{
		////	words.Add(match.Value);
		////}
		//PlaceWordsOnBoard(words);
		StartCoroutine(PlaceWordsOnBoardEnum());
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

	IEnumerator ReserveAmountAsyncEnum()
	{
		yield return null;	//delay by one tick?
		ReserveAmountAsync(1200);   //width * height
		yield return null;
	}
	IEnumerator PlaceWordsOnBoardEnum()
	{
		List<string> words = new List<string>() { "barbara", "ania", "Olaf", "kamil", "ola", "ślimak", "Ania", "ara", "abra"
		//"Ktoś", "Silikon", "Cadmium", "Kura", "kurczak", "kaczka", "kasia", "asia", "klaudia"
		};
		//var ss = "loach\r\nloaches\r\nload\r\nloadable\r\nloadage\r\nloaded\r\nloadedness\r\nloaden\r\nloader\r\nloaders\r\nloadinfo\r\nloading\r\nloadings\r\nloadless\r\nloadpenny\r\nloads\r\nloadsome\r\nloadspecs\r\nloadstar\r\nloadstars\r\nloadstone\r\nloadstones\r\nloadum\r\nloaf\r\n";
		//foreach( Match match in Regex.Matches(ss, "\\w+", RegexOptions.IgnoreCase))
		//{
		//	words.Add(match.Value);
		//}
		yield return new WaitForSeconds(0.1f);
		PlaceWordsOnBoard(words);
		yield return null;
	}



	public void PlaceContentOnBoard(char[,] content, List<string> wordsToFind)
	{
		///search through provided content to find wordsToFind
		var width = content.GetLength(0);
		var height = content.GetLength(1);
		CreateBoard(width, height);
	}
	public void PlaceWordsOnBoard(List<string> words)
	{
		//delegate logic to separete class
		PlaceWords placeWords = new PlaceWords(words, new(14, 9), CreateBoardAtLeast, wordsInReverse: true, AdditionalCharsPercent: 1.2f);
		//try to place words on board
		placeWords.PlaceWordsOnBoardThreaded(wordPlaceMaxRetry: 100, maxThreads: 8);
		Singleton.boardUiEvents.RefreshBoardUi();

		ZoomCameraOnBoard();
	}

	public void ZoomCameraOnBoard()
	{
		///14:9 -> 7.5,-4,-10 size = 5
		///8:5 ->  4,-2,-10 size = 2.5
		float ratio = 14f / 9f;
		mainCameraZoom.SetCameraDefaults(new(widthPrev / 2, -(heightPrev / 2)), ((float)widthPrev) / (ratio * 2), new(widthPrev, heightPrev));
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
			return tilesSript2D;	//double negative
		return CreateBoard(width, height);
	}

	///// <summary>
	///// Creates the board only if provided dimensions are greater than min dimensions for the board
	///// </summary>
	///// <param name="width">new width</param>
	///// <param name="height">new height</param>
	///// <returns>board</returns>
	//public LetterTileScript[,] CreateBoardNoSmallerThanMin(int width, int height)
	//{
	//	if (!(width > boardMinW || height > boardMinH))
	//		return tilesSript2D;	//double negative
	//	return CreateBoard(width, height);
	//}


	public int ReserveAmountSync(int amount) => ReserveAmount(amount).Result;
	public async void ReserveAmountAsync(int amount) => await ReserveAmount(amount);
	/// <summary>
	/// makes sure there is at least given amount of tiles available
	/// </summary>
	/// <param name="amount">the amount of tiles</param>
	/// <returns>amount added to tiles pool</returns>
	/// <exception cref="ResourceAcquisitionException">thrown when mutex cant be acquired</exception>
	public async Task<int> ReserveAmount(int amount)
	{
		bool ourMutex = mutexTilesPool.WaitOne(100); //make sure we have something to work with, and dont invalidate the list
		if(!ourMutex) throw new ResourceAcquisitionException(reservingTilesAmount, $"Already Reserving {reservingTilesAmount} Tiles");
		var amountToCreate = amount - tilesPool.Count;
		if (amountToCreate <= 0)
		{
			mutexTilesPool.ReleaseMutex();
			return 0; //no need to make more
		}
		tilesPool.Capacity = amount + 1; //reserve us some space
		reservingTilesAmount = amount;
		tile.SetActive(false);  //set all new tiles to not render
		var results = InstantiateAsync(tile, amountToCreate, tilesParent); //now using async
		await results;
		tilesPool.AddRange(results.Result);
		reservingTilesAmount = 0;
		mutexTilesPool.ReleaseMutex();
		return amountToCreate;
	}

	/// <summary>
	/// Creates the board with provided dimensions, assigns Tiles
	/// </summary>
	/// <param name="width">width</param>
	/// <param name="height">height</param>
	public LetterTileScript[,] CreateBoard(int width, int height)
	{
		ReserveAmountSync(width * height);
		if (width == widthPrev && height == heightPrev)
			return tilesSript2D; //no change

		widthPrev = width;
		heightPrev = height;
		tilesSript2D = new LetterTileScript[width, height];
		List<GameObject> tilesPoolLocal;
		mutexTilesPool.WaitOne(100);
		try
		{
			tilesPoolLocal = tilesPool.ToList();
		}
		finally
		{
			mutexTilesPool.ReleaseMutex();
		}
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
