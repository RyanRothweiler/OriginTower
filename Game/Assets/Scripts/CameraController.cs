using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{

	public GameObject focalPoint;
	private GameObject player;

	void Start () 
	{
		player = PlayerController.instance.gameObject;	
	}
	
	void Update () 
	{
        Vector3 newPos = Vector3.Lerp(this.transform.position, focalPoint.transform.position, Time.deltaTime * 6);
        newPos.z = -10;
        this.transform.position = newPos;
	}
}