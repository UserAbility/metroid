using UnityEngine;
using System.Collections;

[AddComponentMenu("Devika Learning/Gaze Pointer")]
public class GazePointer : MonoBehaviour {
	
	public LayerMask layers;
	
	GazeTarget[] gazeTargets;
	RaycastHit rayhit;
	GameObject lastGazedObject = null;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		
		if(Physics.Raycast(this.transform.position, this.transform.forward, out rayhit, Mathf.Infinity, layers))
		{
			if(rayhit.transform.gameObject != null && lastGazedObject != null && lastGazedObject != rayhit.transform.gameObject)
			{
				if(gazeTargets != null)
				{
					if(gazeTargets.Length != 0)
					{
						for(int i = 0; i < gazeTargets.Length; i++)
						{
							gazeTargets[i].OffGaze();
						}
					}
				}
			}
			
			gazeTargets = rayhit.transform.gameObject.GetComponents<GazeTarget>();
			//Debug.Log(rayhit.transform.gameObject.name + "   " + gazeTargets.Length);
			
			//Debug.Log();
			if(gazeTargets.Length != 0)
			{
				if(lastGazedObject != rayhit.transform.gameObject)
				{
					for(int i = 0; i < gazeTargets.Length; i++)
					{
						
						//Debug.Log(rayhit.transform.gameObject.name + " being hit  " + i);
						gazeTargets[i].OnGaze();
					}
				}else 
				{
					for(int i = 0; i < gazeTargets.Length; i++)
					{
						gazeTargets[i].Gazing();
					}
				}
			}
			
			lastGazedObject = rayhit.transform.gameObject;
			
		}else
		{
			if(lastGazedObject != null)
			{
				if(gazeTargets != null)
				{
					if(gazeTargets.Length != 0)
					{
						for(int i = 0; i < gazeTargets.Length; i++)
						{
							gazeTargets[i].OffGaze();
						}
					}
				}
			}
			lastGazedObject = null;
		}
	
	}
}
