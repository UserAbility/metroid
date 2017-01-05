using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyTransform : MonoBehaviour {
	
	public Transform target;
	
	public bool posX = true, posY = true, posZ = true;
	public bool rotX = true, rotY = true, rotZ = true;
	public bool scaleX = true, scaleY = true, scaleZ = true;
	public bool parent = false;
	
	public bool onlyOnStart = false;
	
	// Use this for initialization
	void Start () {
		
		Copy();
		
	}
	
	// Update is called once per frame
	void Update () {
		
		//if we are copying positoin contiunously
		if(!onlyOnStart)
			Copy();
	}
	
	//Copy Transform
	public void Copy()
	{
		if (target == null)
			return;
		
		//Copy position
		this.transform.position = new Vector3(
			posX ? target.position.x : this.transform.position.x,
			posY ? target.position.y : this.transform.position.y,
			posZ ? target.position.z : this.transform.position.z 	
		);
		
		//Copy rotation
		this.transform.eulerAngles = new Vector3(
			rotX ? target.eulerAngles.x : this.transform.eulerAngles.x,
			rotY ? target.eulerAngles.y : this.transform.eulerAngles.y,
			rotZ ? target.eulerAngles.z : this.transform.eulerAngles.z 	
		);
		
		//Copy scale
		this.transform.localScale = new Vector3(
			scaleX ? target.localScale.x : this.transform.localScale.x,
			scaleY ? target.localScale.y : this.transform.localScale.y,
			scaleZ ? target.localScale.z : this.transform.localScale.z 	
		);
		
		//Copy Parent
		if (parent)
		{
			this.transform.SetParent(target.parent);
		}
	}
	
	
}
