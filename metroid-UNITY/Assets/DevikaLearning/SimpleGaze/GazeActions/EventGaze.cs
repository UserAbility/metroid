using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[AddComponentMenu("Devika Learning/Event Gaze")]
public class EventGaze : GazeTarget {
	
	public UnityEvent myUnityEvent;
	
	public override bool OnGaze()
	{
		if (!base.OnGaze())
			return false;
		
		if (myUnityEvent != null)
			myUnityEvent.Invoke();
		
		return true;
	}
	
	public override bool Gazing()
	{
		if (!base.Gazing())
			return false;
		
		
		if (myUnityEvent != null)
		myUnityEvent.Invoke();
		
		
		return true;
		
	}
	
	public override bool OffGaze()
	{
		if (!base.OffGaze())
			return false;
		
		
		if (myUnityEvent != null)
		myUnityEvent.Invoke();
		
		
		return true;
		
	}
}
