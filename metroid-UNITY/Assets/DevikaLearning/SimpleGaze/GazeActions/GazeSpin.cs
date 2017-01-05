using UnityEngine;
using System.Collections;

[AddComponentMenu("Devika Learning/Gaze Spin")]
public class GazeSpin : GazeTarget {
	
	public GameObject target;
	Rigidbody rigibody;
	public Vector3 direction;
	public float power;
	public bool RandomDirection = true;
	[Tooltip("Uses power as a maximum")]
	public bool RandomPower;
	
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
		
		Spin();
		
		return true;
	}
	
	public override bool Gazing()
	{
		if (!base.Gazing())
			return false;
		
		Spin();
		
		
		return true;
		
	}
	
	public override bool OffGaze()
	{
		if (!base.OffGaze())
			return false;
		
		Spin();
		
		return true;
		
	}
	
	void Spin()
	{
		float _power = power;
		if(RandomDirection)
			direction = new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f));
		
		if(RandomPower)
			_power = Random.Range(0,power);
		
		if(rigibody)
			rigibody.angularVelocity += direction * _power;//.AddForce(direction.forward * power);
	}
}
