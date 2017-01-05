using UnityEngine;
using System.Collections;

[AddComponentMenu("Devika Learning/Color Gaze")]
public class GazeColor : GazeTarget {
	public GameObject target;
	public Color targetColor;
	
	Renderer renderer;
	Color defualtColor;
	
	// Use this for initialization
	void Start () {
		if(target == null)
			target = this.gameObject;
		
		renderer = target.GetComponent<Renderer>();
		defualtColor = renderer.material.color;
	}
	
	public override bool OnGaze()
	{
		if (!base.OnGaze())
			return false;
		
		if(renderer.material.color == targetColor)
			renderer.material.color = defualtColor;
		else
			renderer.material.color = targetColor;
		
		return true;
	}
	
	public override bool Gazing()
	{
		if (!base.Gazing())
			return false;
		
		if(renderer.material.color == targetColor)
			renderer.material.color = defualtColor;
		else
			renderer.material.color = targetColor;
		
		return true;
		
	}
	
	public override bool OffGaze()
	{
		if (!base.OffGaze())
			return false;
		
		if(renderer.material.color == targetColor)
			renderer.material.color = defualtColor;
		else
			renderer.material.color = targetColor;
		
		return true;
		
	}
}
