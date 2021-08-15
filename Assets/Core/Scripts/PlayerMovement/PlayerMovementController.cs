using UnityEngine;
using System.Collections;
using static JoystickTouchController;
using static NetworkingMessageAttributes;
using System;

[RequireComponent(typeof(Rigidbody))]

public class PlayerMovementController : MonoBehaviour
{
	#region Init
	public JoystickTouchController leftController;
	public JoystickTouchController rightController;
	public float speedMovements = 5f;
	public float speedRotation = 5f;

	Rigidbody rb;

	[Space] public float minRotationAngleToShoot = 5f;

	[Space(5f)]
	public bool online;
	public bool debugRot;

	[HideInInspector]
	public EventManager _EventManager;
	public Player assignedPlayer;

	[HideInInspector] public bool isAlive;

	void Awake()
	{
		InitSystems();
		InitOnline();
	}
	void InitSystems()
    {
		if (leftController == null || rightController == null)
		{
			JoystickTouchController[] controllers = FindObjectsOfType<JoystickTouchController>();
			foreach (JoystickTouchController a in controllers)
			{
				if (a.joystickType.Equals(JoystickType.Left))
				{
					if (leftController == null)
                    {
						leftController = a;
						continue;
					}
				}
				if (a.joystickType.Equals(JoystickType.Right))
				{
					if (rightController == null)
						rightController = a;
				}
			}
		}
		leftController.TouchStateEvent += LeftController_TouchDetection;
		rightController.TouchStateEvent += RightController_TouchDetection;

		rb = GetComponent<Rigidbody>();
		_EventManager = FindObjectOfType<EventManager>();
		canShootLocally = true;
		canShootOnline = true;
		canJumpLocally = true;
		canJumpOnline = true;
		isAlive = true;
	}
	IEnumerator SendPlayerMovement()
    {
        while (true)
        {
			yield return new WaitForSeconds(0.05f);
			//Debug.Log("OnPlayerMoved|"+transform.position+"|"+transform.rotation);
			OnlineGameManager.instance.OnPlayerMoved(transform.position, transform.rotation.eulerAngles);
		}
	}
	void InitOnline()
    {
		if (!online) return;

		StopAllCoroutines();
		StartCoroutine(SendPlayerMovement());
    }
    #endregion
    void LeftController_TouchDetection(bool isTouching) { moving = isTouching; }
	void RightController_TouchDetection(bool isTouching) { aiming = isTouching; }

	bool pushingByProjectile;
	bool moving;
	bool aiming;
	void Update()
	{
		if (!isAlive) return;
		MakeMovement();
		UpdateAim();
		DebugRot();
		MakePushing();
	}
	[HideInInspector] Vector3 lastMovement;
	void MakeMovement()
    {
		if (!moving || pushingByProjectile) { lastMovement = Vector3.zero; return; }
		Vector3 translation = new Vector3(leftController.GetTouchPosition.x * Time.deltaTime * speedMovements, 0, leftController.GetTouchPosition.y * Time.deltaTime * speedMovements);
		lastMovement = translation;
		transform.Translate(translation, Space.World);
    }
	Quaternion lastRotation;
	void UpdateAim()
	{
		Vector2 value;

		if(aiming || moving)
        {
			if (aiming) value = rightController.GetTouchPosition;
			else value = leftController.GetTouchPosition;


			float yAxis = Mathf.Atan2(value.x, value.y) * Mathf.Rad2Deg;
			Quaternion targetRot = Quaternion.Euler(transform.rotation.x, yAxis, transform.rotation.z);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, speedRotation * Time.deltaTime);
			lastRotation = transform.rotation;

			TryToShoot(value, targetRot);
		}
	}

	public void TryToShoot(Vector2 joystickInput, Quaternion targetRot)
    {
		if (!aiming) return;
		if (joystickInput.x < -0.5f || joystickInput.x > 0.5f || joystickInput.y < -0.5f || joystickInput.y > 0.5f)
		{
			if (IsAgnleToTheTargetIsNormal(transform.rotation, targetRot))
			{
                if (online)
                {
					if(canShootLocally && canShootOnline)
                    {
						StartCoroutine(ReloadLocallyCoroutine());
						OnlineGameManager.instance.TryToShootOnline(assignedPlayer.projectileSpawnPoint.position, transform.eulerAngles);
                    }
				} 
				else _EventManager.MakeActualShot(assignedPlayer.projectileSpawnPoint.position, transform.rotation, gameObject, "");
			}
		}
	}
	void MakePushing()
    {
		if (!pushingByProjectile) return;

		//float debugRange = 1f;
		//Debug.DrawRay(transform.position, pushingVector * debugRange, Color.red);

		RaycastHit hit;
		if(Physics.Raycast(transform.position, pushingVector, out hit, 1f))
        {
            if (hit.collider.gameObject.tag.Equals("Game_Wall"))
            {
				pushingByProjectile = false;
				//Debug.Log("Pushing by projectile was prevented because of a Game_Wall");
				return;
            }
        }
        else
        {
			Vector3 force = pushingVector * forceToApplyOnGravityShot * Time.deltaTime;
			transform.Translate(force, Space.World);
		}
	}

	#region Reload Addons
	// basically we use that mechanic to not let the player send(spam) shoot requests,
	// until the actual shot from server side has been released
	// Using same reload time as on the server, but as signal takes
	// time to reach the device, effective reload time is about 1.5f
	bool canShootLocally;
	bool canShootOnline;
	IEnumerator ReloadLocallyCoroutine()
    {
		canShootLocally = false;
		yield return new WaitForSeconds(0.5f);
		canShootLocally = true;
    }
	public void ForbidToShootFromServer()
	{
		canShootOnline = false;
		StartCoroutine(ReloadFromServerCoroutine());
	}
	IEnumerator ReloadFromServerCoroutine()
	{
		yield return new WaitForSeconds(1.4f);
		canShootOnline = true;
	}
	#endregion

	#region Jump related
	public void TryToJump_Request()
	{
		if (!isAlive) return;
		if (canJumpLocally && canJumpOnline)
		{
			StartCoroutine(CooldownJumpLocallyCoroutine());
			ConnectionManager.instance.SendMessageToServer($"{JUMP_REQUEST}");
		}
	}
	public void MakeJumpOnline()
	{
		StartCoroutine(JumpCooldownFromServerCoroutine()); // olnine reload
		rb.AddForce(Vector3.up * forceToApplyOnJump, forceModeOnJump);

	}
	bool canJumpLocally;
	bool canJumpOnline;
	public ForceMode forceModeOnJump;
	public float forceToApplyOnJump = 5f;
	IEnumerator CooldownJumpLocallyCoroutine()
	{
		canJumpLocally = false;
		yield return new WaitForSeconds(0.5f);
		canJumpLocally = true;
	}
	IEnumerator JumpCooldownFromServerCoroutine()
	{
		canJumpOnline = false;
		yield return new WaitForSeconds(2f);
		canJumpOnline = true;
	}
	#endregion

	public void OnCollisionEnter(Collision collision)
    {
		GravityProjectile gp = collision.gameObject.GetComponent<GravityProjectile>();

		if (gp != null)
        {
			if(gp.playerToIgnore != gameObject)
				StartCoroutine(PushbackCoroutine(collision.gameObject.transform.forward));
        }
        if (collision.gameObject.tag.Equals("Game_Wall"))
        {
			//Debug.Log("Player was pushed to the wall");
			pushingByProjectile = false;
		}
			
    }
	public float forceToApplyOnGravityShot = 1f;
	public float pushbackDuration = 0.75f;
	Vector3 pushingVector;
	IEnumerator PushbackCoroutine(Vector3 projectileDir)
    {
		pushingByProjectile = true;
		pushingVector = projectileDir;
		yield return new WaitForSeconds(pushbackDuration);
		pushingByProjectile = false;
    }
    void OnDestroy()
	{
		leftController.TouchStateEvent  -= LeftController_TouchDetection;
		rightController.TouchStateEvent -= RightController_TouchDetection;
	}

	#region Addons
	bool IsAgnleToTheTargetIsNormal(Vector3 targetPos)
	{
		Vector3 targetDir = targetPos - transform.position;
		targetDir = targetDir.normalized;

		float dot = Vector3.Dot(targetDir, transform.forward);
		float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

		float adjustedAngle = Mathf.Abs(angle);

		if (adjustedAngle > minRotationAngleToShoot) return false;
		else return true;
	}
	bool IsAgnleToTheTargetIsNormal(Quaternion currentRot, Quaternion targetRot)
	{
		float angle = Quaternion.Angle(currentRot, targetRot);
		if (angle > minRotationAngleToShoot) return false;
		else return true;
	}
	void DebugRot()
	{
		if (!debugRot) return;

		Debug.Log(transform.eulerAngles + "|" + transform.rotation.eulerAngles);

		if (Input.GetKeyDown(KeyCode.Space))
		{
			transform.rotation = Quaternion.Euler(0, 0, 0);
		}
	}
    #endregion

    #region DeathRelated

	[EditorButton]
	public void KillPlayer()
    {
		isAlive = false;
		rb.constraints = RigidbodyConstraints.None;
        //rb.AddForce(new Vector3(UnityEngine.Random.Range(0,10), 0, UnityEngine.Random.Range(0, 10)), ForceMode.Impulse);

        if (!lastMovement.Equals(Vector3.zero))
        {
			rb.AddForce(new Vector3(lastMovement.x * 50, 0, lastMovement.z * 50), ForceMode.Impulse);
			
		}
        else
        {
			bool X_Positive = UnityEngine.Random.Range(0, 2) == 1;
			bool Z_Positive = UnityEngine.Random.Range(0, 2) == 1;
			float xRandomForce;
			float zRandomForce;

			if (X_Positive) xRandomForce = UnityEngine.Random.Range(25, 50) * speedMovements * Time.deltaTime;
			else xRandomForce = UnityEngine.Random.Range(-25, -50) * speedMovements * Time.deltaTime;

			if (Z_Positive) zRandomForce = UnityEngine.Random.Range(25, 50) * speedMovements * Time.deltaTime;
			else zRandomForce = UnityEngine.Random.Range(-25, -50) * speedMovements * Time.deltaTime;

			rb.AddForce(new Vector3(xRandomForce, 0, zRandomForce), ForceMode.Impulse);
		}
		Invoke(nameof(RevivePlayer), 3f);
    }

	public void RevivePlayer()
    {
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
		transform.position = _EventManager.spawnPositions[EventManager.randowSpawnPosIndex].spawnPos.transform.position;
		transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(-180, 180), 0);

		isAlive = true;
	}

    #endregion
}
