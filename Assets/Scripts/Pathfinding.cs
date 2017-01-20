using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Pathfinding : MonoBehaviour 
{
	public List<LevelTile> GetPath (LevelTile start, LevelTile goal)
	{
		return AStar(start, goal);
	}
	
	public List<LevelTile> AStar (LevelTile start, LevelTile goal)
	{
		HashSet<LevelTile> closed = new HashSet<LevelTile>();
		HashSet<LevelTile> open = new HashSet<LevelTile>();
		Dictionary<LevelTile,LevelTile> cameFrom = new Dictionary<LevelTile,LevelTile>();
		
		Dictionary<LevelTile,int> g = new Dictionary<LevelTile,int>();
		Dictionary<LevelTile,int> f = new Dictionary<LevelTile,int>();
		
		open.Add(start);
		g[start] = 0;
		f[start] = g[start] + heuristic(start, goal);
		
		while (open.Count > 0)
		{
			LevelTile current = getLowest(open, f);
			
			if (current.Equals(goal))
				return path(cameFrom, goal);
			
			open.Remove(current);
			closed.Add(current);
			
			foreach (LevelTile neighbor in current.getNeighbors())
			{
				if (closed.Contains(neighbor))
					continue;
				
				int tentG = g[current] + 1;
				if (!open.Contains(neighbor) || tentG < g[neighbor])
				{
					cameFrom[neighbor] = current;
					g[neighbor] = tentG;
					f[neighbor] = tentG + heuristic(neighbor, goal);
					if (!open.Contains(neighbor))
						open.Add(neighbor);
				}
			}
		}
		
		return new List<LevelTile>();
	}
	
	private List<LevelTile> path (Dictionary<LevelTile,LevelTile> cameFrom, LevelTile goal)
	{
		List<LevelTile> path = new List<LevelTile>();
		LevelTile current = goal;
		path.Add(goal);
		
		while (cameFrom.ContainsKey(current))
		{
			current = cameFrom[current];
			path.Add(current);
		}
		
		path.Reverse();		
		return path;
	}
	
	private int heuristic (LevelTile start, LevelTile goal)
	{
		return (int)System.Math.Ceiling(Vector3.Distance(start.getLocation(), goal.getLocation()));
	}
	
	private LevelTile getLowest (HashSet<LevelTile> open, Dictionary<LevelTile,int> f)
	{
		LevelTile current = null;
		int lowest = int.MaxValue;
		
		foreach (LevelTile tile in open)
			if (f[tile] < lowest)
			{
				lowest = f[tile];
				current = tile;
			}
				
		return current;
	}
}
