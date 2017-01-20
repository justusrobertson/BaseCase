using UnityEngine;
using System.Collections;

public class StartGame : MonoBehaviour {

	public GUIText title;
	public GUIText sub;

	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.anyKey)
		{
			title.enabled = false;
			sub.enabled = false;
			Application.LoadLevel("Game");
		}
	}
}
