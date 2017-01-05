using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool : MonoBehaviour {

	/// <summary>
	/// The object to be pool.
	/// </summary>
	public GameObject ObjectToBePool;

	/// <summary>
	/// The size of the pool.
	/// </summary>
	public int poolSize = 100;

	[HideInInspector]
	public List<GameObject> activePool = new List<GameObject>();

	/// <summary>
	/// The empty prefab.
	/// </summary>
	public GameObject EmptyPrefab;
	public static GameObject pf_empty;

	/// <summary>
	/// The pool parent.
	/// </summary>
	[HideInInspector]
	public Transform poolParent;

	/// <summary>
	/// The object pool.
	/// </summary>
	protected List<GameObject> objectPool = new List<GameObject>();

	protected virtual void Awake()
	{

		if (EmptyPrefab != null)
			pf_empty = EmptyPrefab;

	}

	protected virtual void Start()
	{
		PoolObjects ();
	}

	/// <summary>
	/// Creates the pool parent.
	/// </summary>
	protected virtual void CreatePoolParent()
	{
		poolParent = ((GameObject)Instantiate (pf_empty)).transform;
		poolParent.SetParent (this.transform);
		poolParent.name = ObjectToBePool.gameObject.name + " Pool";
	}

	/// <summary>
	/// Creates the object pool.
	/// </summary>
	protected virtual void CreateObjectPool()
	{
		//Create array to store unit pools
		objectPool = new List<GameObject>();

		//populate pool
		GameObject tmpObject;

		//For size of pool
		for (int i = 0; i < poolSize; ++i) 
		{

			/*//Create unit
			tmpObject = (GameObject)Instantiate (ObjectToBePool);

			//store reference to item in pool
			objectPool.Add (tmpObject);

			//parent item to pool
			tmpObject.transform.SetParent(poolParent);

			//turn off
			tmpObject.SetActive (false);*/
			AddObjectToPool();

		}
	}

	public virtual void PoolObjects()
	{
		CreatePoolParent ();

		CreateObjectPool ();
	}

	public GameObject GetObjectFromPool()
	{
		//Debug.Log (poolSize);
		//Debug.Log (objectPool.Count);
		for(int i = 0; i < poolSize; ++i)
		{
			if(!objectPool[i].gameObject.activeInHierarchy)
			{
				objectPool[i].gameObject.SetActive(true);

				activePool.Add (objectPool [i]);

				return objectPool[i];
			}
		}

		return null;
	}

	public void DisablePool(){
		for (int i = 0; i < objectPool.Count; ++i) {
			objectPool [i].gameObject.SetActive (false);
		}
	}

	public void DeactivePoolObject(GameObject obj)
	{
		activePool.Remove (obj);
	}
	
	public void AddObjectToPool()
	{
		//Create unit
		GameObject tmpObject = (GameObject)Instantiate (ObjectToBePool);
		
			//store reference to item in pool
		objectPool.Add (tmpObject);
		
			//parent item to pool
		tmpObject.transform.SetParent(poolParent);
		
			//turn off
		tmpObject.SetActive (false);
	}

}
