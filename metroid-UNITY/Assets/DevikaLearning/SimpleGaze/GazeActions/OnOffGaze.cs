using UnityEngine;
using System.Collections;

[AddComponentMenu("Devika Learning/On Off Gaze")]
public class OnOffGaze : GazeTarget {
	
	public GameObject target;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public override bool OnGaze()
	{
		if (!base.OnGaze())
			return false;
		
		target.SetActive(!target.activeSelf);
		
		return true;
	}
	
	public override bool Gazing()
	{
		if (!base.Gazing())
			return false;
		
		target.SetActive(!target.activeSelf);
		
		return true;
		
	}
	
	public override bool OffGaze()
	{
		if (!base.OffGaze())
			return false;
		
		target.SetActive(!target.activeSelf);
		
		return true;
		
	}
}
