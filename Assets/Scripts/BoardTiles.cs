using System;
using UnityEngine;
using System.Linq;
using System.Collections;

public class BoardTiles : MonoBehaviour
{
	[SerializeField] private int with, height;
	[SerializeField] private GameObject tile;
	private Transform tilesParent, overlayParent;

//sizes for camera Projection.Size	(to take up the entire screen)
//Size: width, height
//1: 3-2
//2: 7-4
//3: 11-6
//5: 18-10
//10: 34-20
//w,h
//n = 3.5 * n - 2 * n




	protected LetterTileScript[,] tilesSript2D;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		tilesParent = this.gameObject.transform.Find("tilesParent");
		overlayParent = this.gameObject.transform.Find("overlayParent");

		CreateTiles(with, height);
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	async void CreateTiles(int with, int height)
	{
		var random = new System.Random();
		var thisPos = this.transform.position;
		if (tilesSript2D != null) CleanupTiles();
		tilesSript2D = new LetterTileScript[with, height];

		//now using async
		var results = InstantiateAsync(tile, with * height, tilesParent);
		await results;

		var tileEnum = results.Result.GetEnumerator();
		
		for (int i = 0; i != with; i++) {
			for (int j = 0; j != height; j++)
			{
				tileEnum.MoveNext();
				GameObject spawnedTile = (GameObject)tileEnum.Current;
				//var spawnedTile = Instantiate(tile, new Vector3(thisPos.x + i, thisPos.y - j, thisPos.z), Quaternion.identity, tilesParent);
				spawnedTile.transform.position = new Vector3(thisPos.x + i, thisPos.y - j, thisPos.z);
				spawnedTile.transform.rotation = Quaternion.identity;

				spawnedTile.name = $"Tile-{i}-{j}";
				tilesSript2D[i, j] = spawnedTile.GetComponent<LetterTileScript>();
				//tilesSript2D[i, j].SetLetter(((char)random.Next('A', 'Z')));
			}
		}
	}
	void CleanupTiles()
	{
		//foreach (Transform child in tilesParent)
		//{
		//}

	}
}
