using UnityEngine;
using System.Collections;

[AddComponentMenu("Devika Learning/Move Gaze")]
public class MoveGaze : GazeTarget {
	public Transform targetToMove;
	public Transform targetPosition;
	public bool rotation = true;
	
	
	// Use this for initialization
	void Start () {
		
		if(targetPosition == null)
			targetPosition = this.transform;
		if(targetToMove == null)
			targetToMove = this.transform;
		
	}
	
	
	public override bool OnGaze()
	{
		if (!base.OnGaze())
			return false;
		
		targetToMove.transform.position = targetPosition.position;
		
		if(rotation)
		{
			targetToMove.rotation = targetPosition.rotation;
		}
		
		return true;
	}
	
	public override bool Gazing()
	{
		if (!base.Gazing())
			return false;
		
		
		targetToMove.transform.position = targetPosition.position;
		
		if(rotation)
		{
			targetToMove.rotation = targetPosition.rotation;
		}
		
		
		return true;
		
	}
	
	public override bool OffGaze()
	{
		if (!base.OffGaze())
			return false;
		
		
		targetToMove.transform.position = targetPosition.position;
		
		if(rotation)
		{
			targetToMove.rotation = targetPosition.rotation;
		}
		
		
		return true;
		
	}
}