using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CopyImage : MonoBehaviour {
	
	public Image targetImage;
	
	Image myImage;
	
	public bool onlyColor = true;
	
	// Use this for initialization
	void Start () {
		myImage = this.GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
		
		
		myImage.color = targetImage.color;
		
		if (!onlyColor)
			myImage.sprite = targetImage.sprite;
		
	}
}
