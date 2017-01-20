using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Mediation.PlanTools;
using System.Linq;

public class Computer : MonoBehaviour 
{
	public Font computerFont;
	private GUIStyle computerStyle;
	private GUIStyle toggleStyleOn;
	private GUIStyle toggleStyleOff;

	// Is the computer colliding with the player?
	private bool touchingPlayer = false;
	
	private StateManager stateManager;
	private List<Predicate> localPredicates = new List<Predicate>();
	
	private Mediator mediator;
	
	private bool displayOn = false;
	private void ToggleDisplay()
	{
		if (displayOn)
		{
			displayOn = false;
			stateManager.PlayerTurn = true;
		}
		else
		{
			displayOn = true;
			stateManager.PlayerTurn = false;
		}
	}
	
	private string displayString = "";
	private List<Predicate> toggleList = new List<Predicate>();
	
	private bool[] toggleValues;
	private bool[] lastToggles;

	// Use this for initialization
	void Start () 
	{
		GameObject level = GameObject.Find("Level");
		stateManager = level.GetComponent<StateManager>();
		
		GameObject mediatorGO = GameObject.Find("Mediator");
		mediator = mediatorGO.GetComponent<Mediator>();
		
		computerStyle = new GUIStyle();
		computerStyle.font = computerFont;
		computerStyle.fontSize = 40;
		computerStyle.normal.textColor = Color.green;
		computerStyle.normal.background = MakeTexture(Screen.width - 20, Screen.height - 20, new Color (0.5f,0.5f,0.5f,0.75f));
		
		toggleStyleOn = new GUIStyle();
		toggleStyleOn.font = computerFont;
		toggleStyleOn.fontSize = 40;
		toggleStyleOn.normal.textColor = Color.green;
		toggleStyleOn.normal.background = MakeTexture(Screen.width - 20, Screen.height - 20, new Color (0.5f,0.5f,0.5f,0f));
		
		toggleStyleOff = new GUIStyle();		
		toggleStyleOff.font = computerFont;
		toggleStyleOff.fontSize = 40;
		toggleStyleOff.normal.textColor = new Color (0f,.41f,0f,1f);
		toggleStyleOff.normal.background = MakeTexture(Screen.width - 20, Screen.height - 20, new Color (0.5f,0.5f,0.5f,0f));
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetButtonDown("Use") && touchingPlayer && !displayOn)
		{
			CreateDisplay();
			ToggleDisplay();
		}
		
		if (Input.GetButtonDown("Back") && touchingPlayer && displayOn)
			ToggleDisplay();
			
		if (displayOn)
			if (!Enumerable.SequenceEqual(toggleValues, lastToggles))
			{
				UpdateWorld();
				lastToggles = toggleValues.Clone() as bool[];
			}
	}
	
	void OnGUI ()
	{
		if (displayOn)
			DrawDisplay();	
	}
	
	private void UpdateWorld()
	{
		for (int i = 0; i < toggleValues.Length; i++)
		{
			if (toggleValues[i] != lastToggles[i])
			{
				if (toggleValues[i])
					localPredicates.Add(toggleList[i]);
				else
					localPredicates.Remove(toggleList[i]);
			}
		}
		
		mediator.JumpToState(localPredicates);
	}
	
	private void DrawDisplay()
	{
		int iterator = 0;
		int textHeight = 50;
		GUI.Box (new Rect(20, 20, Screen.width - 40, Screen.height - 40), "", computerStyle);
		foreach (Predicate p in toggleList)
			if (toggleValues[iterator])
				toggleValues[iterator] = GUI.Toggle(new Rect(20, textHeight * (iterator + 1), Screen.width - 40, textHeight), toggleValues[iterator++], p.ToString(), toggleStyleOn);
			else
			toggleValues[iterator] = GUI.Toggle(new Rect(20, textHeight * (iterator + 1), Screen.width - 40, textHeight), toggleValues[iterator++], p.ToString(), toggleStyleOff);
	}
	
	private void CreateDisplay()
	{
		toggleList = new List<Predicate>();
		localPredicates = stateManager.Predicates;
	
		foreach (Predicate p in Prune(localPredicates))
			toggleList.Add(p);
		
		toggleValues = new bool[toggleList.Count];
		for (int i = 0; i < toggleValues.Length; i++)
			toggleValues[i] = true;
			
		lastToggles = toggleValues.Clone() as bool[];
	}
	
	private List<Predicate> Prune (List<Predicate> predicates)
	{
		List<Predicate> pruned = new List<Predicate>();
		foreach (Predicate pred in predicates)
		{
			string name = pred.Name;
			if ((name.Equals("locked") || name.Equals("tied")) && !stateManager.Type(pred.TermAt(0)).Equals("computer") && !pred.TermAt(0).Equals(stateManager.Player))
				pruned.Add(pred);
		}
				
		return pruned;
	}
	
	// Fires on a collision.
	void OnCollisionEnter2D (Collision2D col)
	{
		if (col.gameObject.tag.Equals("Player"))
			touchingPlayer = true;
	}
	
	// Fires on collision exit.
	void OnCollisionExit2D (Collision2D col)
	{
		if (col.gameObject.tag.Equals("Player"))
			touchingPlayer = false;
	}
	
	private Texture2D MakeTexture (int width, int height, Color color)
	{
		Color[] pix = new Color[width * height];
		
		for (int i = 0; i < pix.Length; i++)
			pix[i] = color;
			
		Texture2D result = new Texture2D(width, height);
		result.SetPixels (pix);
		result.Apply ();
		
		return result;
	}
}
