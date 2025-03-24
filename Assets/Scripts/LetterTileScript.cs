using Mono.Cecil;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterTileScript : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField] public char Letter;// { get { return GetLetter(); }  set { SetLetter(value); } }
	[SerializeField] public LayerMask TileLayerMask;

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

			Singleton.clickAndDrag.AddClickPoint(this, eventData);
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
				Singleton.clickAndDrag.AddClickPoint(this, eventData, true);
			}
			else
			{
				//Vector3 screenPos;
				//Vector3 posCam;

				//screenPos = Input.mousePosition;
				//screenPos.z = Camera.main.nearClipPlane + 1;
				//posCam = Camera.main.ScreenToWorldPoint(screenPos);
				//Debug.DrawRay(transform.position, posCam - transform.position, Color.blue);

				RaycastHit hit = new RaycastHit();
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out hit, 100, TileLayerMask))
				{
					var asLetterTile = hit.transform.GetComponent<LetterTileScript>();
					if (asLetterTile)
					{
						Debug.Log("OnPointerUp-drag: " + hit.transform.name); // ensure you picked right object
						Singleton.clickAndDrag.AddClickPoint(asLetterTile, eventData, true); //only the endpoint
						return;
					}
				}
				Singleton.clickAndDrag.CancelClickPoints(this);
			}
			//Singleton.clickAndDrag.AddClickPoint(this, eventData);
		}
	}
}
