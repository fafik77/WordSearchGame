using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Threading;
using BoardContent;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using System.Threading.Tasks;

public class BoardTiles : MonoBehaviour
{
	///depracteced
	[SerializeField] private int width, height;

	[SerializeField] private GameObject tile;
	private Transform tilesParent, overlayParent;
	private List<GameObject> tilesPool = new List<GameObject>();
	private int reservingTilesAmount;
	private Mutex mutexTilesPool = new Mutex();

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


	protected LetterTileScript[,] tilesSript2D;
	private void Awake()
	{
		tilesParent = this.gameObject.transform.Find("tilesParent");
		overlayParent = this.gameObject.transform.Find("overlayParent");

		ReserveAmountAsync(width * height);
		//CreateBoard(5,5);
	}
	private void Start()
	{
		CreateBoard(10, 10);
		PlaceWords placeWords = new PlaceWords(tilesSript2D);
		placeWords.PlaceWordsOnBoard();
    }

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

	public void CreateBoard(int width, int height)
	{
		ReserveAmountSync(width * height);

		if (width == widthPrev && height == heightPrev)
			return; //no change

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
	}
}
