using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class Singleton
{
	public static Singleton Instance { get; private set; }
	public struct ClickAndDrag
	{
		private float lastClickTime;
		public event EventHandler<LetterTileScript> StartDrawingLine;
		public static float ClickFromDragRegisterSeconds = 0.2f;
		public static float ReClickRegisterDelaySeconds = 0.3f;

		/**
			the event args are more of a hint and not the actuall position
		*/
		public event EventHandler<LetterTileScript> FinishDrawingLine;
		//for when there was an
		public event EventHandler<LetterTileScript> CancelDrawingLine;

		//public Vector3 posStart;
		//public Vector3 posEnd;
		private LetterTileScript tileStart;
		public LetterTileScript tileEnd;

		public LetterTileScript TileStart
		{
			get => tileStart;
			set
			{
				if (!tileStart)
					tileStart = value;
			}
		}

		static private async void _FinishLineDelayed()
		{
			//FinishDrawingLine?.Invoke(this, tileEnd);
			await Task.Delay((int)ReClickRegisterDelaySeconds * 1000);
			Singleton.clickAndDrag.tileStart = null;
			Singleton.clickAndDrag.clickLocked = false;
		}

		private bool WasShortClick
		{
			get
			{
				if ((Time.fixedTime - lastClickTime) < ClickFromDragRegisterSeconds)
				{
					lastClickTime = Time.fixedTime;
					return true;
				}
				return false;
			}
			set => lastClickTime = Time.fixedTime;
		}
		private bool clickLocked;
		public void AddClickPoint(LetterTileScript letterTileTouched, PointerEventData eventData, bool endpointOnly = false)
		{
			if (clickLocked) return;

			if (!tileStart || WasShortClick)
			{
				tileStart = letterTileTouched;
				WasShortClick = true;
				tileEnd = null;

				//Thread thread = new Thread(new ThreadStart(_StartDrawingLineAsync));
				//thread.Start();
				StartDrawingLine?.Invoke(this, tileStart);
			}
			else if (!WasShortClick)
			{
				tileEnd = letterTileTouched;
				clickLocked = true;
				FinishDrawingLine?.Invoke(this, tileEnd);
				Thread thread = new Thread(new ThreadStart(_FinishLineDelayed));
				thread.Start();
				//tileStart = null;
			}
		}
		public void CancelClickPoints(LetterTileScript requester)
		{
            CancelDrawingLine?.Invoke(this, requester);

            Singleton.clickAndDrag.tileStart = null;
            Singleton.clickAndDrag.clickLocked = false;

        }

	}
	public static ClickAndDrag clickAndDrag;

	//private Singleton()
	//{
	//	if (Instance==null)
	//	{
	//		Instance = this;
	//		DontDestroyOnLoad(Instance);
	//	}
	//	else if (Instance && Instance != this)
	//		Destroy(this);
	//}

}
