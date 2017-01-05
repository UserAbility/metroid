using UnityEngine;
using System.Collections;

[AddComponentMenu("Devika Learning/Visible Gaze")]
public class VisibleOnOffGaze : GazeTarget {
	public GameObject target;
	
	Renderer renderer;
	
	// Use this for initialization
	void Start () {
		if(target == null)
			target = this.gameObject;
		
		renderer = target.GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public override bool OnGaze()
	{
		if (!base.OnGaze())
			return false;
		
		renderer.enabled = !renderer.enabled;
		
		return true;
	}
	
	public override bool Gazing()
	{
		if (!base.Gazing())
			return false;
		
		renderer.enabled = !renderer.enabled;
		
		return true;
		
	}
	
	public override bool OffGaze()
	{
		if (!base.OffGaze())
			return false;
		
		renderer.enabled = !renderer.enabled;
		
		return true;
		
	}
}
