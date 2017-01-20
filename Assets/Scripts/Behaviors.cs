using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Mediation.Interfaces;
using Mediation.PlanTools;
using System;

public class Behaviors : MonoBehaviour 
{
	private StateManager stateManager;
	private Pathfinding pathfinder;
	private Mediator mediator;

	void Awake ()
	{
		// Find the level game object.
		GameObject level = GameObject.Find("Level");
		
		// Set the state manager.
		stateManager = level.GetComponent<StateManager>();
		
		// Set the pathfinder.
		pathfinder = level.GetComponent<Pathfinding>();
		
		GameObject mediatorGO = GameObject.Find("Mediator");
		
		mediator = mediatorGO.GetComponent<Mediator>();
	}
	
	public void GuardBehavior (NPC npc)
	{
		CheckSight(npc);
	
		if (!npc.ExecutingAction && Time.time - npc.FinishedActionTime > 2f && stateManager.Colocated(npc.name,stateManager.Player))
		{
			System.Random rand = new System.Random();
			int num = rand.Next();
			
			if (num % 2 == 0)
				RandomMove (npc);
			else if (num % 2 == 1)
				RandomLook (npc);
		}
	}
	
	public void RandomLook (NPC npc)
	{	
		System.Random rand = new System.Random();
		int num = rand.Next();
		
		if (num % 4 == 0)
			npc.Direction = new Vector3(1, 0, 0);
		else if (num % 4 == 1)
			npc.Direction = new Vector3(-1, 0, 0);
		else if (num % 4 == 2)
			npc.Direction = new Vector3(0, 1, 0);
		else if (num % 4 == 3)
			npc.Direction = new Vector3(0, -1, 0);
			
		npc.FinishAction();
	}

	public void RandomMove (NPC npc)
	{
		GameObject roomGO = GameObject.Find(stateManager.At(npc.name));
		if (roomGO != null)
		{
			LevelTile[] roomTiles = roomGO.GetComponentsInChildren<LevelTile>();
			List<LevelTile> path = new List<LevelTile>();
			
			System.Random rand = new System.Random();
			
			foreach (LevelTile tile in roomTiles.OrderBy(i => rand.Next()))
			{
				if (tile.getAccessible())
				{
					path = pathfinder.GetPath(npc.FloorTileList.FirstOrDefault(), tile);
					
					if (path.Count > 0)
						npc.SetPath(path);
					
					return;
				}
			}
		}
	}
	
	private void CheckSight (NPC npc)
	{	
		RaycastHit2D hit = Physics2D.Raycast(npc.transform.position, npc.Direction, npc.renderer.bounds.size.x * 20);
		if (hit.collider != null)
			if (hit.collider.gameObject.tag.Equals("Player") && npc.renderer.isVisible && !npc.Freeze)
			{
				npc.Freeze = true;
				GameObject exclamation = Instantiate(Resources.Load("Exclamation", typeof(GameObject)), new Vector3 (npc.transform.position.x, npc.transform.position.y + npc.renderer.bounds.size.y, npc.transform.position.z), Quaternion.identity) as GameObject;
				Exclamation excl = exclamation.GetComponent<Exclamation>();
				excl.attach(npc.gameObject);
				mediator.RestartGame();
			}
	}
	
	private void MouseMovement (NPC npc)
	{
		if (Input.GetMouseButtonDown(0))
		{
			Physics2D.raycastsHitTriggers = true;
			RaycastHit2D hit = Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,Camera.main.ScreenToWorldPoint(Input.mousePosition).y), new Vector2 (0,0), 0f);
			Physics2D.raycastsHitTriggers = false;
			
			List<LevelTile> path = new List<LevelTile>();
			
			if (hit)
			{
				LevelTile goal = hit.transform.gameObject.GetComponent<LevelTile>();
				if (goal != null)
					path = pathfinder.GetPath(npc.FloorTileList.FirstOrDefault(), goal);
			}
		}
	}
}
