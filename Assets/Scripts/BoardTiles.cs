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
	private Mutex mutexReservingTiles = new Mutex();

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
        CreateBoard(5, 5);
    }

    public int ReserveAmountSync(int amount) => ReserveAmount(amount).Result;
    public async void ReserveAmountAsync(int amount) => await ReserveAmount(amount);
    public async Task<int> ReserveAmount(int amount)
	{
		var amountToCreate = amount - tilesPool.Count;
		if (amountToCreate <= 0) return 0; //no need to make more

		mutexReservingTiles.WaitOne(); //make sure we have something to work with, and dont invalidate the list
			reservingTilesAmount = amount;
			tile.SetActive(false);  //set all new tiles to not render
			var results = InstantiateAsync(tile, amountToCreate, tilesParent); //now using async
			await results;
			tilesPool.AddRange(results.Result);
			reservingTilesAmount = 0;
		mutexReservingTiles.ReleaseMutex();
		return amount;
	}

	public void CreateBoard(int width, int height)
	{
		ReserveAmountSync(width * height);

		if (width == widthPrev && height == heightPrev)
			return; //no change

		widthPrev = width;
		heightPrev = height;
		tilesSript2D = new LetterTileScript[width, height];
		var eachTile = tilesPool.Skip(width * height);
		foreach (var tile in eachTile)
			tile.SetActive(false); //hide the rest
		var tilesStartingPos = tilesParent.position;
		var tileEnum = tilesPool.GetEnumerator();
		//TODO: change this into tiles pool
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
