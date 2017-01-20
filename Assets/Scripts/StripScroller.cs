using UnityEngine;
using System.Collections;

public class StripScroller : MonoBehaviour
{
	public float scrollSpeed;
	
	private Vector3 startPosition;
	private GameObject next;
	
	void Start ()
	{
		startPosition = transform.position;
		next = new GameObject();
		next.transform.position = new Vector3(transform.position.x + gameObject.renderer.bounds.size.x, transform.position.y, transform.position.z);
		next.transform.localScale = transform.localScale;
		SpriteRenderer nextSpriteR = next.AddComponent<SpriteRenderer>();
		SpriteRenderer thisSpriteR = this.GetComponent<SpriteRenderer>();
		nextSpriteR.sprite = thisSpriteR.sprite;
		nextSpriteR.sortingLayerName = thisSpriteR.sortingLayerName;
	}
	
	void Update ()
	{
		if (offScreen())
		{
			StripScroller script = next.AddComponent<StripScroller>();
			script.scrollSpeed = scrollSpeed;
			Destroy(this.gameObject);
		}
			
		float translate = Time.deltaTime * (scrollSpeed / 1.5f);
		transform.Translate(-translate, 0, 0);
		next.transform.Translate(-translate, 0, 0);
	}
	
	private bool offScreen()
	{
		if (transform.position.x <= startPosition.x - gameObject.renderer.bounds.size.x)
			return true;
		return false;
	}
}