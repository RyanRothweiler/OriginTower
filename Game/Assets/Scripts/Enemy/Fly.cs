﻿using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class Fly : MonoBehaviour 
{

	public Tweener movingTween;
	public Vector3 origPos;
	
	void Start()
	{
		origPos = this.transform.position;
		Vector3 newTarget = origPos + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), Random.Range(-2, 2));
		movingTween = HOTween.To(this.transform, 1f, "position", newTarget);
	}

	void Update () 
	{
		if (this.GetComponent<EnemyController>().alive && !this.GetComponent<EnemyController>().isRunning)
		{
			if (movingTween.isComplete)
			{
				Vector3 newTarget = origPos + new Vector3(Random.Range(-1.5f, 1.5f), 
					Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f));
				movingTween = HOTween.To(this.transform, Random.Range(0.2f, 0.8f), "position", newTarget);
			}
		}
		else
		{
			HOTween.Kill(movingTween);
		}
	}
}
