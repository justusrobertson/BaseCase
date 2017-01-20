using UnityEngine;
using System.Collections;

public class CameraFader : MonoBehaviour 
{
	private Mediator mediator;

	// Use this for initialization
	void Start () 
	{
		// Find the mediator's game object.
		GameObject medOB = GameObject.Find("Mediator");
		
		// Store the mediator script.
		mediator = medOB.GetComponent<Mediator>();
		
		iTween.CameraFadeAdd();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (mediator.ValidState())
		{
			foreach (GUIText tex in this.GetComponentsInChildren<GUIText>())
				tex.enabled = false;
			this.GetComponent<GUILayer>().enabled = true;
			iTween.CameraFadeTo (1.0f, 4.0f);
		}
	}
}
