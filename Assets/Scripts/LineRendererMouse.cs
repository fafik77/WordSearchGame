using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class LineRendererMouse : MonoBehaviour
{
	private LineRenderer lineRenderer;
	//private Vector3 screenPos;
	//private Vector2 posCam;

	void Awake()
	{
		lineRenderer = this.gameObject.GetComponent<LineRenderer>();
		lineRenderer.enabled = false;
		lineRenderer.positionCount = 2;
		lineRenderer.useWorldSpace = true;
		lineRenderer.widthCurve.keys[0].value = (float)Math.Sqrt(2)/2;

	}
	private void OnEnable()
	{   //subscribe to "click" events
		Singleton.clickAndDrag.StartDrawingLine += ClickAndDrag_StartDrawingLine;
		Singleton.clickAndDrag.FinishDrawingLine += ClickAndDrag_FinishDrawingLine;
		Singleton.clickAndDrag.CancelDrawingLine += ClickAndDrag_CancelDrawingLine;
	}

	private void OnDisable()
	{   //unsubscribe from "click" events
		Singleton.clickAndDrag.StartDrawingLine -= ClickAndDrag_StartDrawingLine;
		Singleton.clickAndDrag.FinishDrawingLine -= ClickAndDrag_FinishDrawingLine;
		Singleton.clickAndDrag.CancelDrawingLine -= ClickAndDrag_CancelDrawingLine;
	}

	private void ClickAndDrag_CancelDrawingLine(object sender, LetterTileScript e)
	{
		lineRenderer.enabled = false;
	}

	private void ClickAndDrag_FinishDrawingLine(object sender, LetterTileScript[] e)
	{
		lineRenderer.enabled = false;
	}

	private void ClickAndDrag_StartDrawingLine(object sender, LetterTileScript e)
	{
		Vector2 posOfTile = e.transform.position;

		lineRenderer.SetPosition(0, posOfTile);
		lineRenderer.enabled = true;
	}

	void Update()
	{
		if (!lineRenderer.enabled) return;

		if (Input.GetMouseButtonDown(1))	//cancel button
		{
			Singleton.clickAndDrag.CancelClickPoints(null);
			return;
		}

		Vector3 screenPos;
		Vector3 posCam;

		screenPos = Input.mousePosition;
		screenPos.z = Camera.main.nearClipPlane + 1;
		posCam = Camera.main.ScreenToWorldPoint(screenPos);

		lineRenderer.SetPosition(1, (Vector2)posCam);
	}

}
