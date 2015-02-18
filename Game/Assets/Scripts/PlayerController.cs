using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class PlayerController : MonoBehaviour 
{
	public static PlayerController instance;
	public Capturer capturer;

	public GameObject cameraFocalPoint;

	public bool canCapture;
	public GameObject objCaptured;
	public bool isPulling;
	public bool killPull;
	public SpringJoint2D pullJoint;
	private float pullingForce = 0.1f; 

	private float currentMoveModifier = 1;
	private float currentTurnModifier = 5;

	public Vector3 aimingWorldPos;
	public Vector3 controllerAimingOffset;
	private Vector3 lookingDirection;

	public Vector2 lastVelocity;

	public bool checkingTripping;

	private bool canBeStunned = false;
	public bool isStunned = false;
	public GameObject stunner;

	private int currentHealth = 3;
	public GameObject[] heartContainers;

	private bool canBeDamaged = true;
	private bool isAlive = true;
	private bool killed = false;
	public GameObject deathParticleSystem;

	public LineRenderer pullLine;

	public float totalPullStrength;
	public float currentPullStrength;
	public GameObject pullStrengthBar;

	void Start () 
	{
		instance = this;
	}
	
	void Update () 
	{
		currentPullStrength = Mathf.Clamp(currentPullStrength, 0, 100);
		Vector3 newPullBarScale = new Vector3(currentPullStrength / totalPullStrength, 1, 1);
		pullStrengthBar.transform.localScale = newPullBarScale;

		if (isPulling)
		{
			currentPullStrength -= 0.5f;
		}
		else
		{
			currentPullStrength += 1;
		}


		if (currentPullStrength <= 0)
		{
			KillPull();
			Stun();
		}

		if (!isStunned && isAlive)
		{
			if (Input.GetButton("ControllerShooting"))
			{
				canCapture = true;
				Capturer.instance.Show();
			}
			else
			{
				if (isPulling)
				{
					KillPull();
				}

				canCapture = false;
				Capturer.instance.Hide();
			}

			if (isPulling)
			{
				currentMoveModifier = 0.4f;
				currentTurnModifier = 1.5f;

				if (!checkingTripping)
				{
					CheckTrip();
				}
			}
			else
			{
				currentMoveModifier = 1;
				currentTurnModifier = 1;
			}

			float horizontalChange = Input.GetAxis("Horizontal") * Time.deltaTime * currentMoveModifier * 10;
			float verticalChange = Input.GetAxis("Vertical") * Time.deltaTime * currentMoveModifier * 10;
			Vector3 newPos = new Vector3(this.transform.position.x + horizontalChange, 
			                             this.transform.position.y + verticalChange, 
			                             0);
			this.transform.position = newPos;

			Vector3 capturingDirection = new Vector3(Input.GetAxis("ControllerRightVertical"), 
			                                         Input.GetAxis("ControllerRightHorizontal"),
			                                         0);
			capturingDirection.Normalize();

			if (capturingDirection.x > 0.5f || capturingDirection.x < -0.5f || 
			    capturingDirection.y > 0.5f || capturingDirection.y < -0.5f)
			{
				controllerAimingOffset.y -= Input.GetAxis("ControllerRightVertical") * 1.5f;
				controllerAimingOffset.x += Input.GetAxis("ControllerRightHorizontal") * 1.5f;
				controllerAimingOffset.Normalize();
				controllerAimingOffset *= 8 * currentTurnModifier;

				aimingWorldPos = this.transform.position + controllerAimingOffset;
				PointAt(this.gameObject, aimingWorldPos);
			}
			else
			{
				Vector3 movingDirection = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
				movingDirection.Normalize();
				movingDirection = (movingDirection * 15) + this.transform.position;
				PointAt(this.gameObject, movingDirection);
			}
		}

		if (currentHealth <= 0)
		{
			if (!killed)
			{
				killed = true;	
				Instantiate(deathParticleSystem, this.transform.position, Quaternion.identity);
				this.gameObject.SetActiveRecursively(false);
			}
			isAlive = false;
		}
	}

	public void StartPull()
	{
		isPulling = true;
		killPull = false;
		pullJoint.distance = 1.5f;

		pullLine.enabled = true;

		this.pullJoint.enabled = true;
		this.pullJoint.connectedBody = objCaptured.GetComponent<Rigidbody2D>();

		StartCoroutine(CapturePull_());
	}
	private IEnumerator CapturePull_()
	{
		Rigidbody2D body = objCaptured.GetComponent<Rigidbody2D>() as Rigidbody2D;
		while (isPulling)
		{
			pullLine.SetPosition(0, this.transform.position);
			pullLine.SetPosition(1, objCaptured.transform.position);

			body.AddForce(objCaptured.transform.up * pullingForce * 10);
			pullJoint.distance -= 0.001f * pullingForce;
			PointAt(objCaptured, this.transform.position);
			objCaptured.GetComponent<LittleDudeController>().currentHealth -= 0.5f;

			if (!objCaptured.GetComponent<LittleDudeController>().alive)
			{
				KillPull();
			}

			yield return new WaitForSeconds(0.01f);
		}

		if (!killPull)
		{
			pullLine.enabled = false;
			isPulling = false;
			this.pullJoint.connectedBody = null;
			this.pullJoint.enabled = false;

			objCaptured.GetComponent<RunningBehavior>().isRunning = false;

			this.GetComponent<Rigidbody2D>().isKinematic = true;
			this.GetComponent<Rigidbody2D>().isKinematic = false;
		}
	}

	public void CheckTrip()
	{
		checkingTripping = true;
		canBeStunned = false;
		ResetCanBeStunned();
		StartCoroutine(CheckTrip_());
	}
	public IEnumerator CheckTrip_()
	{
		Vector3 hopePoint = objCaptured.transform.position - (objCaptured.transform.up * pullingForce * 10);		
		float firstDistance = Vector3.Distance(hopePoint, this.transform.position);

		yield return new WaitForSeconds(0.1f);

		float secondDistance = Vector3.Distance(hopePoint, this.transform.position);
		if (((secondDistance + 0.1f) < firstDistance) && canBeStunned)
		{
			KillPull();
			Stun();
		}

		checkingTripping = false;
	}

	public void KillPull()
	{
		objCaptured.GetComponent<RunningBehavior>().KillPull();
		this.pullJoint.enabled = false;
		pullLine.enabled = false;
		isPulling = false;
		killPull = true;
	}

	public void PointAt(GameObject objPointing, Vector3 target)
	{
		Vector3 dir = target - objPointing.transform.position;
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		angle -= 90;
		objPointing.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	public void Stun()
	{
		stunner.SetActive(true);
		isStunned = true;
		isPulling = false;
		Capturer.instance.Hide();
		StartCoroutine(Stun_());
	}
	public IEnumerator Stun_()
	{
		yield return new WaitForSeconds(1f);
		isStunned = false;
		stunner.SetActive(false);
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject.GetComponent<Enemy>() && canBeDamaged && isAlive)
		{
			Instantiate(deathParticleSystem, this.transform.position, Quaternion.identity);
			heartContainers[currentHealth - 1].SetActive(false);
			currentHealth--;
			canBeDamaged = false;
			ResetCanBeDamaged();
		}
	}

	public void ResetCanBeDamaged()
	{
		StartCoroutine(ResetCanBeDamaged_());
	}
	private IEnumerator ResetCanBeDamaged_()
	{
		yield return new WaitForSeconds(0.5f);
		canBeDamaged = true;
	}

	public void ResetCanBeStunned()
	{
		StartCoroutine(ResetCanBeStunned_());
	}
	private IEnumerator ResetCanBeStunned_()
	{
		yield return new WaitForSeconds(0.3f);
		canBeStunned = true;
	}
}