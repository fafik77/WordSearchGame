using Mono.Cecil;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterTileScript : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField] public char Letter;// { get { return GetLetter(); }  set { SetLetter(value); } }
	private LetterDisplayScript letterScript;

	private float lastClickTime;


	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Awake()
	{
		letterScript = this.gameObject.transform.GetComponentInChildren<LetterDisplayScript>();
		//var LetterObj = this.gameObject.transform.GetComponentInChildren<UnityEngine.UI.Text>();
		//LetterObj.text = Letter.ToString();
		if (Letter != 0x00 && Letter != ' ')
			SetLetter(Letter);
		lastClickTime = 0;
	}

	public void SetLetter(char letter)
	{
		letterScript.SetLetter(letter);
	}
	public char GetLetter()
	{
		return letterScript.GetLetter();
	}

	public void OnDrag(PointerEventData eventData)
	{
		//Debug.Log($"OnDrag {eventData.ToString()}");
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
	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			WasShortClick = true; //set to check for short click

			Singleton.Instance.clickAndDrag.AddClickPoint(this, eventData);
			Debug.Log($"OnPointerDown {this.transform.name}");
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (WasShortClick)
			{
				Debug.Log($"OnPointerUp-click {this.transform.name}");
			}
			else
			{
				Debug.Log($"OnPointerUp-drag {this.transform.name}");
			}
			Singleton.Instance.clickAndDrag.AddClickPoint(this, eventData);
		}
	}
}
