using UnityEngine;
using System.Collections;

public class Exclamation : MonoBehaviour 
{
	GameObject attached = null;
	
	public void attach(GameObject go)
	{
		attached = go;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (attached != null)
			transform.position = new Vector3(attached.transform.position.x, attached.transform.position.y + attached.renderer.bounds.size.y, attached.transform.position.z);
	}
}
