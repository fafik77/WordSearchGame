using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class Singleton : MonoBehaviour
{
	public static Singleton Instance { get; private set; }
	public struct ClickAndDrag
	{
		private float lastClickTime;
		public event EventHandler<LetterTileScript> StartDrawingLine;

		/**
			the event args are more of a hint and not the actuall position
		*/
		public event EventHandler<LetterTileScript> FinishDrawingLine;

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
			//FinishDrawingLine.Invoke(this, tileEnd);
			await Task.Delay(100);
			Singleton.Instance.clickAndDrag.tileStart = null;
            Singleton.Instance.clickAndDrag.clickLocked = false;
		}

		private bool WasShortClick
		{
			get
			{
				if ((Time.fixedTime - lastClickTime) < 0.2)
				{
					lastClickTime = Time.fixedTime;
					return true;
				}
				return false;
			}
			set => lastClickTime = Time.fixedTime;
		}
		private bool clickLocked;
		public void AddClickPoint(LetterTileScript letterTile, PointerEventData eventData)
		{
			if (clickLocked) return;

			if (!tileStart || WasShortClick)
			{
				tileStart = letterTile;
				WasShortClick = true;
				tileEnd = null;

				//Thread thread = new Thread(new ThreadStart(_StartDrawingLineAsync));
				//thread.Start();
				StartDrawingLine.Invoke(this, tileStart);
			}
			else if (!WasShortClick)
			{
				tileEnd = letterTile;
				clickLocked = true;
				FinishDrawingLine.Invoke(this, tileEnd);
				Thread thread = new Thread(new ThreadStart(_FinishLineDelayed));
				thread.Start();
				//tileStart = null;
			}
		}
	}
	public ClickAndDrag clickAndDrag;

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
			DontDestroyOnLoad(Instance);
		}
		else if (Instance != this)
			Destroy(this);
	}

}
