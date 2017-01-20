using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Mediation.Interfaces;
using Mediation.PlanTools;
using System;

public class RipleyActionLibrary : MonoBehaviour 
{
	private Pathfinding pathfinder;

	void Start ()
	{
		// Find the level game object.
		GameObject level = GameObject.Find("Level");
		
		// Set the pathfinder.
		pathfinder = level.GetComponent<Pathfinding>();
	}
	
	private void moveto (NPC npc, IOperator action, string room)
	{
		GameObject roomGO = GameObject.Find(room);
		LevelTile[] roomTiles = roomGO.GetComponentsInChildren<LevelTile>();
		List<LevelTile> path = new List<LevelTile>();
		
		System.Random rand = new System.Random();
		
		foreach (LevelTile tile in roomTiles.OrderBy(i => rand.Next()))
		{
			if (tile.getAccessible())
			{
				path = pathfinder.GetPath(npc.FloorTileList.FirstOrDefault(), tile);
				
				if (path.Count > 0)
				{
					npc.SetPath(path);
					npc.CompleteInstruction(action);
				}
				
				return;
			}
		}
	}

	public void movelocation (NPC npc, IOperator action)
	{
		moveto (npc, action, action.Predicate.Terms.ElementAt(1));
	}
	
	public void movelocationdoor (NPC npc, IOperator action)
	{
		moveto (npc, action, action.Predicate.Terms.ElementAt(2));
	}
}
