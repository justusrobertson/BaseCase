using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelTile : MonoBehaviour 
{
	private Vector3 location;
	public LevelTile up, down, left, right = null;
	private bool unlocked = false;
	private List<LevelTile> neighbors = new List<LevelTile>();
	private HashSet<string> colliding = new HashSet<string>();

	public Vector3 getLocation ()
	{
		return transform.position;
	}
	
	public void setUp (LevelTile up)
	{
		if (this.up == null)
		{
			this.up = up;
			neighbors.Add(up);
			up.setDown(this);
		}
	}
	
	public bool hasUp ()
	{
		return up != null;
	}
	
	public void setDown (LevelTile down)
	{
		if (this.down == null)
		{
			this.down = down;
			neighbors.Add(down);
			down.setUp(this);
		}
	}
	
	public void setLeft (LevelTile left)
	{
		if (this.left == null)
		{
			this.left = left;
			neighbors.Add(left);
			left.setRight(this);
		}
	}
	
	public bool hasLeft ()
	{
		return left != null;
	}
	
	public void setRight (LevelTile right)
	{
		if (this.right == null)
		{
			this.right = right;
			neighbors.Add(right);
			right.setLeft(this);
		}
	}
	
	public bool getAccessible ()
	{
		return (this.gameObject.tag == "Navigation" || this.gameObject.tag == "DoorNav");
	}
	
	public void unlock ()
	{
		if (!unlocked)
		{
			unlocked = true;
		
			if (getAccessible())
				this.collider2D.isTrigger = true;
			
			if (up != null)
				up.unlock();
				
			if (down != null)
				down.unlock();
				
			if (left != null)
				left.unlock();
				
			if (right != null)
				right.unlock();
		}
	}
	
	public List<LevelTile> getNeighbors()
	{
		List<LevelTile> accessible = new List<LevelTile>();
		foreach (LevelTile neighbor in neighbors)
			if (neighbor.getAccessible())
				accessible.Add(neighbor);
				
		return accessible;
	}
	
	public string EqualityCheck()
	{
		string output = this.name;
		
		if (up != null)
			output += up.name;
		
		if (down != null)
			output += down.name;
		
		if (left != null)
			output += left.name;
		
		if (right != null)
			output += right.name;
			
		return output;
	}
	
	public override bool Equals(object other)
	{
		LevelTile tile = other as LevelTile;
		return EqualityCheck().Equals(tile.EqualityCheck());
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		colliding.Add(other.gameObject.name);
		this.tag = "DisabledNavigation";
	}
	
	void OnTriggerExit2D(Collider2D other)
	{
		colliding.Remove(other.gameObject.name);
		if (colliding.Count == 0)
			this.tag = "Navigation";
	}
}
