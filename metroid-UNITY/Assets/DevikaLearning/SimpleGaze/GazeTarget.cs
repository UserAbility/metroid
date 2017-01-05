using UnityEngine;
using System.Collections;

public class GazeTarget : MonoBehaviour {
	
	public bool UseOnGaze = true, UseGazing = false, UseOffGaze = false;
	[Tooltip("Only works while Gazing")]
	public bool OnlyWhenClick = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public virtual bool OnGaze()
	{
		if (OnlyWhenClick)
			return false;
		return(UseOnGaze);
	}
	
	public virtual bool Gazing()
	{
		if (OnlyWhenClick)
			return(UseGazing && Input.GetMouseButton(0));
		
			
		return(UseGazing);
		
	}
	
	public virtual bool OffGaze()
	{
		
		return(UseOffGaze);
	}
}
