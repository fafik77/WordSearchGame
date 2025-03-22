using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterDisplayScript : MonoBehaviour
{
	private TextMeshProUGUI _textPro;
	private TextMeshProUGUI textPro {
		get {  return _textPro? _textPro : _textPro= GettextPro(); }
		set { if(!textPro) _textPro = GettextPro(); _textPro = value; }
	}
	private TextMeshProUGUI GettextPro() { return this.gameObject.GetComponent<TextMeshProUGUI>(); }

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Awake()
	{
		textPro = GettextPro();
	}

	public void SetLetter(char Letter)
	{
		textPro.text= Letter.ToString();
	}
	public char GetLetter()
	{
		return textPro.text[0];
	}

}
