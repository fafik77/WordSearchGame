using System;
using UnityEngine;

public class BoardTiles : MonoBehaviour
{
	[SerializeField] private int with, height;
	[SerializeField] private GameObject tile;
	private Transform tilesParent, overlayParent;

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

	void CreateTiles(int with, int height)
	{
        var random = new System.Random();
        var thisPos = this.transform.position;
		if (tilesSript2D != null) CleanupTiles();
		tilesSript2D = new LetterTileScript[with, height];
		for (int i = 0; i != with; i++) {
			for (int j = 0; j != height; j++)
			{
				var spawnedTile = Instantiate(tile, new Vector3(thisPos.x + i, thisPos.y + j, thisPos.z), Quaternion.identity, tilesParent);
				spawnedTile.name = $"Tile-{i}-{j}";
                tilesSript2D[i, j] = spawnedTile.GetComponent<LetterTileScript>();
				tilesSript2D[i, j].SetLetter(((char)random.Next('A', 'Z')));
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
