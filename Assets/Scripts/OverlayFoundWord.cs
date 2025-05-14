using BoardContent;
using System;
using System.Linq;
using UnityEngine;

public class OverlayFoundWord : MonoBehaviour
{
	[SerializeField] GameObject LineRendererWordFoundPrefab;
	private Transform myTransform;

	private void Awake()
	{
		myTransform = this.gameObject.transform;
	}
	private void OnEnable()
	{
		Singleton.clickAndDrag.FinishDrawingLine += ClickAndDrag_FinishDrawingLine;
	}

	/// <summary>
	/// Or RemoveAllHighlights
	/// </summary>
	public void Clear()
	{
		int i = 0;
		//Array to hold all child obj
		GameObject[] allChildren = new GameObject[transform.childCount];
		//Find all child obj and store to that array
		foreach (Transform child in transform)
		{
			allChildren[i] = child.gameObject;
			i += 1;
		}
		//Now destroy them
		foreach (GameObject child in allChildren)
		{
			DestroyImmediate(child.gameObject);
		}
	}
	public void RemoveAllHighlights() { Clear(); }

	private void OnDisable()
	{
		Singleton.clickAndDrag.FinishDrawingLine -= ClickAndDrag_FinishDrawingLine;
	}

	private void ClickAndDrag_FinishDrawingLine(object sender, LetterTileScript[] bothPoints)
	{
		Vector2[] bothPointsPosition = { bothPoints[0].transform.position, bothPoints[1].transform.position };
		Vector2[] bothPointsPositionAbs = { (bothPoints[0].transform.position - myTransform.position).Abs(), (bothPoints[1].transform.position - myTransform.position).Abs() };
		if (!ValidateLineCoords(bothPointsPositionAbs[0], bothPointsPositionAbs[1]))
		{
			Debug.Log("Invalid Move!");
			return;
		}
		///from here the selection points are valid
		bool existsAsIntended = false;
		for (int i = 0; i != Singleton.wordList.list.Count; ++i)
		{
			var item = Singleton.wordList.list[i];
			if (item.CompareTo(bothPointsPositionAbs[0], bothPointsPositionAbs[1]))
			{
				existsAsIntended = true;
				if (item.found == false)
				{
					item.Found = true;
					Singleton.wordList.list[i] = item;
					Singleton.boardUiEvents.FoundWord(item.word);
					RenderLine(bothPointsPosition[0], bothPointsPosition[1]);
					break;
				}
			}
		}///searched through all placed words and did not find the word

		if (existsAsIntended == true)
			return;
		
		Debug.Log("extending to listUnintended:");
		foreach (var item in Singleton.wordList.listUnintended)
		{
			if (item.CompareTo(bothPointsPositionAbs[0], bothPointsPositionAbs[1]))
				return; ///this was already found
		}
		string findUnknownOrient = string.Empty;
		var orient = bothPointsPositionAbs[1] - bothPointsPositionAbs[0];
		var orientNorm = orient.normalized;
		if (orientNorm.x != 0 && orientNorm.y != 0) orientNorm *= (float)Math.Sqrt(2);

		for (var move = bothPointsPositionAbs[0]; move != bothPointsPositionAbs[1] + orientNorm; move += orientNorm)
		{
			findUnknownOrient += Singleton.TilesSript2D[(int)move.x, (int)move.y].Letter;
		}
		Debug.Log($"adding '{findUnknownOrient}' to listUnintended:");

		string findForward, findBackward = string.Empty;
		if (orient.x > 1 || orient.y > 1)
			findForward = findUnknownOrient;
		else
			findForward = findUnknownOrient.Reverse().ToString();


		if (Singleton.wordList.reversedWords) findBackward = findForward.Reverse().ToString();
		foreach (var word in Singleton.wordList.wordsToFind)
		{
			var wordLoc = word.ToLower();
			if (wordLoc == findForward || wordLoc == findBackward)
			{
				WordListEntry wordListEntry = new WordListEntry() { found = true, word = wordLoc, posFrom = bothPointsPositionAbs[0], posTo = bothPointsPositionAbs[1] };
				Singleton.wordList.listUnintended.Add(wordListEntry);
				Singleton.boardUiEvents.FoundWord(wordLoc);
				RenderLine(bothPointsPosition[0], bothPointsPosition[1]);
				return;
			}
		}
	}

	public void RenderLine(Vector2 fromPos, Vector2 toPos)
	{
		var newLine = Instantiate(LineRendererWordFoundPrefab, fromPos, Quaternion.identity, myTransform);
		if (!newLine)
		{
			Debug.LogError("OverlayFoundWord/ LineRendererWordFoundPrefab Not set!!!");
			return;
		}
		var newLRF = newLine.GetComponent<LineRendereFound>();
		newLRF.SetPositions(fromPos, toPos);
	}

	public static bool ValidateLineCoords(Vector2 fromPos, Vector2 toPos, bool allowDiagonal=true)
	{
		if (fromPos == toPos)		// ! .
			return false;
		if (fromPos.x == toPos.x)	//  ----
			return true;
		if (fromPos.y == toPos.y)	//  ||
			return true;            //  ||

		if (allowDiagonal == false) return false;
		var xalign = fromPos.x - toPos.x;
		if (xalign < 0) xalign *= -1;

		var yalign = fromPos.y - toPos.y;
		if (yalign < 0) yalign *= -1;
		//   /
		//  /
		if ((int)xalign == (int)yalign)
			return true;

		return false;
	}


}
