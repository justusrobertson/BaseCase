using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour 
{
	public AudioClip[] intros;
	public AudioClip[] mains;
	private bool playing;
	private Mediator mediator;

	// Use this for initialization
	void Start () 
	{
		audio.clip = intros[0];
		audio.loop = false;
		playing = false;
		
		// Find the mediator's game object.
		GameObject medOB = GameObject.Find("Mediator");
		
		// Store the mediator script.
		mediator = medOB.GetComponent<Mediator>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!playing)
		{
			playing = true;
			audio.Play();
		}
	
		if (mediator.ValidState() && audio.volume > 0)
			FadeOut();
	
		if (!audio.isPlaying && playing)
		{
			audio.clip = mains[0];
			audio.loop = true;
			audio.Play();
		}
	}
	
	void FadeOut ()
	{
		audio.volume = audio.volume - (.05f * Time.deltaTime);
	}
}
