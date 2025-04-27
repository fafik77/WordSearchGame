using Mono.Cecil;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterTileScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField] public LayerMask TileLayerMask;

	private LetterDisplayScript letterScript;

	private float lastClickTime;

	void Awake()
	{
		letterScript = this.gameObject.transform.GetComponentInChildren<LetterDisplayScript>();
		lastClickTime = 0;
	}

	public char Letter
	{
		get => GetLetter();
		set => SetLetter(value);
	}
	public void SetLetter(char letter)
	{
		letterScript.SetLetter(letter);
	}
	public char GetLetter()
	{
		return letterScript.GetLetter();
	}

    ///TODO add some highlight
    public void Highlight()
	{
		
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

			//Debug.Log($"OnPointerDown {this.transform.name}");
			Singleton.clickAndDrag.AddClickPoint(this, eventData);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (WasShortClick)
			{
				//Debug.Log($"OnPointerUp-click {this.transform.name}");
				Singleton.clickAndDrag.AddClickPoint(this, eventData, true);
			}
			else
			{
				RaycastHit hit = new RaycastHit();
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out hit, 100, TileLayerMask))
				{
					var asLetterTile = hit.transform.GetComponent<LetterTileScript>();
					if (asLetterTile)
					{
						//Debug.Log("OnPointerUp-drag: " + hit.transform.name); // ensure you picked right object
						Singleton.clickAndDrag.AddClickPoint(asLetterTile, eventData, true); //only the endpoint
						return;
					}
				}
				Singleton.clickAndDrag.CancelClickPoints(this);
			}
		}
	}
}
