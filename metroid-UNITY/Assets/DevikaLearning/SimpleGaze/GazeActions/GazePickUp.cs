using UnityEngine;
using System.Collections;

[AddComponentMenu("Devika Learning/Gaze Pickup")]
public class GazePickUp :GazeTarget {
	
	Transform originalParent;
	public GameObject target;
	Rigidbody rigibody;
	[Tooltip("Object which picks up target")]
	public Transform holder;
	
	bool held = false;
	
	
	void Start()
	{
		
		
		if(target == null)
			target = this.gameObject;
		
		originalParent = target.transform.parent;
		
		rigibody = target.GetComponent<Rigidbody>();
		
	}
	
	public override bool OnGaze()
	{
		if (!base.OnGaze())
			return false;
		
		//PickUp();
		
		return true;
	}
	
	public override bool Gazing()
	{
		if (!base.Gazing())
			return false;
		
		//PickUp();
		if(Input.GetMouseButtonDown(0))
		{
			if(held)
				Drop();
			else
				PickUp();
		}
			
		
		return true;
		
	}
	
	public override bool OffGaze()
	{
		if (!base.OffGaze())
			return false;
		
		//Drop();
		
		
		return true;
		
	}
	
	public void PickUp()
	{
		held = true;//		Debug.Log("Pickup");
		target.transform.SetParent(holder.transform);
		if(rigibody)
			rigibody.isKinematic = true;
	}
	public void Drop()
	{
		
		held = false;//Debug.Log("Drop");
		target.transform.SetParent(originalParent);
		if(rigibody)
			rigibody.isKinematic = false;
	}
}
