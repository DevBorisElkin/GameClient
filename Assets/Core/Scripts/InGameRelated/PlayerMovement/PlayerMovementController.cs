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

	public bool collidedWithSpikeTrap;

	Rigidbody rb;

	[Space] public float minRotationAngleToShoot = 5f;

	[HideInInspector]
	public EventManager _EventManager;
	public Player assignedPlayer;

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
		pushingByProjectile = false;
		pushingByProjectile_cantJump = false;

		SetLocalAmountOfJumps(OnlineGameManager.maxJumpsAmount);

		EventManager.isAlive = true;
		EventManager.sendCoordinatesToServer = true;
	}

	IEnumerator SendPlayerMovement()
    {
        while (true)
        {
			yield return new WaitForSeconds(0.02f); // 50 times a sec
			//Debug.Log("OnPlayerMoved|"+transform.position+"|"+transform.rotation);
			OnlineGameManager.instance.OnPlayerMoved(transform.position, transform.rotation.eulerAngles);
		}
	}
	void InitOnline()
    {
		StopAllCoroutines();
		StartCoroutine(SendPlayerMovement());
    }
    #endregion
    void LeftController_TouchDetection(bool isTouching) { moving = isTouching; }
	void RightController_TouchDetection(bool isTouching) { aiming = isTouching; }

	bool pushingByProjectile;
	bool pushingByProjectile_cantJump;
	bool moving;
	bool aiming;
	void FixedUpdate()
	{
		if (!EventManager.isAlive) return;
		MakeMovement();
		UpdateAim();
		MakePushing();
	}
	[HideInInspector] Vector3 lastMovement;
	void MakeMovement()
    {
		if (!moving || pushingByProjectile) { lastMovement = Vector3.zero; return; }
		//Vector3 translation = new Vector3(leftController.GetTouchPosition.x * Time.deltaTime * speedMovements, 0, leftController.GetTouchPosition.y * Time.deltaTime * speedMovements);
		Vector3 translation = new Vector3(leftController.GetTouchPosition.x, 0, leftController.GetTouchPosition.y).normalized * speedMovements * Time.deltaTime;
		lastMovement = translation;
		transform.Translate(translation, Space.World);
    }
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
				if (canShootLocally && canShootOnline)
				{
					StartCoroutine(ReloadLocallyCoroutine());
					OnlineGameManager.instance.TryToShootOnline(assignedPlayer.projectileSpawnPoint.position, transform.eulerAngles);
				}
			}
		}
	}
	bool IsAgnleToTheTargetIsNormal(Quaternion currentRot, Quaternion targetRot)
	{
		float angle = Quaternion.Angle(currentRot, targetRot);
		if (angle > minRotationAngleToShoot) return false;
		else return true;
	}
	void MakePushing()
    {
		if (!pushingByProjectile) return;

		RaycastHit hit;
		if(Physics.Raycast(transform.position, pushingVector, out hit, 1f))
        {
            if (hit.collider.gameObject.tag.Equals("Game_Wall"))
            {
				pushingByProjectile = false;
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
		if (!EventManager.isAlive) return;
		if (!pushingByProjectile_cantJump && canJumpLocally && localJumpsAmount > 0)
		{
			StartCoroutine(CooldownJumpLocallyCoroutine());
			ConnectionManager.instance.SendMessageToServer($"{JUMP_REQUEST}");
		}
	}
	public void SetLocalAmountOfJumps(int newAmount)
	{
		localJumpsAmount = newAmount;
		_EventManager.txt_jumpsLeft.text = localJumpsAmount.ToString();
	}
	public void SetAmountOfJumps(int newAmount)
	{
		localJumpsAmount = newAmount;
	}
	public void MakeJumpOnline()
	{
		rb.AddForce(Vector3.up * forceToApplyOnJump, forceModeOnJump);
	}
	public int localJumpsAmount;
	public bool canJumpLocally; // local reload 2 seconds
	float localJumpReload = 2f;

	public ForceMode forceModeOnJump;
	public float forceToApplyOnJump = 5f;
	IEnumerator CooldownJumpLocallyCoroutine()
	{
		canJumpLocally = false;
		yield return new WaitForSeconds(localJumpReload);
		canJumpLocally = true;
	}
	#endregion
	#region Got Hit By Projectile
    public void OnCollisionEnter(Collision collision)
    {
		GravityProjectile gp = collision.gameObject.GetComponent<GravityProjectile>();

		if (gp != null)
        {
			if(gp.playerToIgnore != gameObject)
            {
				StartCoroutine(PushbackCoroutine(collision.gameObject.transform.forward));

				if (hitAssignedToPlayer != null) StopCoroutine(hitAssignedToPlayer);
				hitAssignedToPlayer = HitReferencedToPlayerTimeoutCoroutine(gp.dbIdOfPleyerWhoMadeLaunch);
				StartCoroutine(hitAssignedToPlayer);
            }
        }
        if (collision.gameObject.tag.Equals("Game_Wall"))
        {
			//Debug.Log("Player was pushed to the wall");
			pushingByProjectile = false;
		}
    }
	public float forceToApplyOnGravityShot = 1f;
	public float pushbackDuration = 0.75f;

	float hitIsReferencedToPlayer = 5f;
	public int dbIdOflastHitPlayer = -1;
	Vector3 pushingVector;
	IEnumerator PushbackCoroutine(Vector3 projectileDir)
    {
		rb.isKinematic = true;
		pushingByProjectile = true;
		pushingByProjectile_cantJump = true;
		pushingVector = projectileDir;
		yield return new WaitForSeconds(pushbackDuration);
		pushingByProjectile = false;
		rb.isKinematic = false;
		yield return new WaitForSeconds(0.45f);
		pushingByProjectile_cantJump = false;
	}
	public IEnumerator hitAssignedToPlayer;
	IEnumerator HitReferencedToPlayerTimeoutCoroutine(int dbIdOfHitter)
	{
		dbIdOflastHitPlayer = dbIdOfHitter;
		yield return new WaitForSeconds(hitIsReferencedToPlayer);
		dbIdOflastHitPlayer = -1;
	}

	#endregion
	void OnDestroy()
	{
		leftController.TouchStateEvent  -= LeftController_TouchDetection;
		rightController.TouchStateEvent -= RightController_TouchDetection;

		StopAllCoroutines();
	}

    #region DeathRelated

	[EditorButton]
	public void KillPlayer()
    {
		rb.constraints = RigidbodyConstraints.None;

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
    }

	public void RevivePlayer(Vector3 spawnPosition, int newJumpsAmount)
    {
		if (EventManager.instance == null) return;
		EventManager.instance.camSimpleFollow.SetFalling(false);
		SetLocalAmountOfJumps(newJumpsAmount);
		collidedWithSpikeTrap = false;
		pushingByProjectile = false;
		pushingByProjectile_cantJump = false;

		rb.isKinematic = false;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		transform.position = spawnPosition;
		transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(-180, 180), 0);
		EventManager.sendCoordinatesToServer = true;
		EventManager.isAlive = true;
	}

    #endregion
}
