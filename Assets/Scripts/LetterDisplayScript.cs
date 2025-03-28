using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterDisplayScript : MonoBehaviour
{
	private TextMeshPro _textPro;
	private TextMeshPro textPro {
		get { return _textPro ? _textPro : _textPro = GettextPro(); }
		set { if(!textPro) _textPro = GettextPro(); _textPro = value; }
	}
	private TextMeshPro GettextPro() => this.gameObject.GetComponent<TextMeshPro>();

	private char letter;
	public char Letter
    {
        get => letter;
        set
        {
            letter = value;
            textPro.text = value.ToString();
        }
    }

    void Awake()
	{
		textPro = GettextPro();
		letter = textPro.text[0];
	}

	public void SetLetter(char Letter) => this.Letter = Letter;
	

	public char GetLetter() => Letter;

}
