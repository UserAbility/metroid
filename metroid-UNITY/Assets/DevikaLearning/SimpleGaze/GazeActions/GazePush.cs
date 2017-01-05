using UnityEngine;
using System.Collections;

[AddComponentMenu("Devika Learning/Gaze Push")]
public class GazePush  :GazeTarget {
	
	public GameObject target;
	Rigidbody rigibody;
	public Transform direction;
	public float power;
	
	bool held = false;
	
	
	void Start()
	{
		
		
		if(target == null)
			target = this.gameObject;
		
		rigibody = target.GetComponent<Rigidbody>();
		
	}
	
	public override bool OnGaze()
	{
		if (!base.OnGaze())
			return false;
		
		Push();
		
		return true;
	}
	
	public override bool Gazing()
	{
		if (!base.Gazing())
			return false;
		
		Push();
		
		
		return true;
		
	}
	
	public override bool OffGaze()
	{
		if (!base.OffGaze())
			return false;
		
		Push();
		
		return true;
		
	}
	
	void Push()
	{
		
		if(rigibody)
			rigibody.velocity += direction.forward * power;//.AddForce(direction.forward * power);
	}
}
