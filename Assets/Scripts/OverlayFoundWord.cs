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
	private void Start()
	{
		Singleton.clickAndDrag.FinishDrawingLine += ClickAndDrag_FinishDrawingLine;
	}
	private void OnDisable()
	{
		Singleton.clickAndDrag.FinishDrawingLine -= ClickAndDrag_FinishDrawingLine;
	}

	private void ClickAndDrag_FinishDrawingLine(object sender, LetterTileScript[] bothPoints)
	{
		Vector2[] bothPointsPosition = { bothPoints[0].transform.position, bothPoints[1].transform.position };
		Vector2[] bothPointsPositionAbs = { (bothPoints[0].transform.position - myTransform.position).Abs(), (bothPoints[1].transform.position - myTransform.position).Abs() };
		if (ValidateLineCoords(bothPointsPositionAbs[0], bothPointsPositionAbs[1]))
		{
			for (int i = 0; i != Singleton.wordList.list.Count; ++i)
			{
				var item = Singleton.wordList.list[i];
				if (item.found == false && item.CompareTo(bothPointsPositionAbs[0], bothPointsPositionAbs[1]))
				{
					item.Found = true;
					Singleton.wordList.list[i] = item;
					Singleton.boardUiEvents.FoundWord(item.word);
					RenderLine(bothPointsPosition[0], bothPointsPosition[1]);
					break;
				}
			}

			//var found = Singleton.wordList.list.Where(x => { return x.found == false && x.CompareTo(bothPointsPositionAbs[0], bothPointsPositionAbs[1]); });
			//foreach(WordListEntry item in found)
			//{
			//	item.Found = true;
			//		RenderLine(bothPointsPosition[0], bothPointsPosition[1]);
			//	break;
			//}
			//         foreach (var item in Singleton.wordList.list)
			//{
			//	if(item.found==false && item.CompareTo(bothPointsPositionAbs[0], bothPointsPositionAbs[1]))
			//	{
			//		item.Found = true;

			//                 RenderLine(bothPointsPosition[0], bothPointsPosition[1]);
			//		break;
			//	}
			//}
		}
		else
		{
			Debug.Log("Invalid Move!");
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

	public static bool ValidateLineCoords(Vector2 fromPos, Vector2 toPos)
	{
		if (fromPos == toPos)		// ! .
			return false;
		if (fromPos.x == toPos.x)	//  ----
			return true;
		if (fromPos.y == toPos.y)	//  ||
			return true;            //  ||

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
