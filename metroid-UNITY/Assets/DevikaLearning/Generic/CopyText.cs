using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CopyText : MonoBehaviour {
	
	public Text targetText;
	
	public bool automatically;
	
	Text myText;
	
	// Use this for initialization
	void Start () {
		
		myText = this.GetComponent<Text>();
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (automatically)
			UpdateText();
		
	}
	
	public void UpdateText()
	{
		myText.text = targetText.text;
	}
}
