using UnityEngine;
using System.Collections;
using static JoystickTouchController;
using static NetworkingMessageAttributes;
using System;
using System.Collections.Generic;
using static EnumsAndData;

[RequireComponent(typeof(Rigidbody))]

public class PlayerMovementController : MonoBehaviour
{
	#region Init
	public JoystickTouchController leftController;
	public JoystickTouchController rightController;
	
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
		isPushingBackBySalmonRune = false;

		SetLocalAmountOfJumps(OnlineGameManager.maxJumpsAmount);

		EventManager.isAlive = true;
		Invoke(nameof(InvokableSetAvailableForRaycast), 0.5f);
		EventManager.sendCoordinatesToServer = true;
	}

	void InvokableSetAvailableForRaycast() => StartCoroutine(EventManager.instance.SetIsAvailableForRaycaster());

	// temporarily deprecated
	//IEnumerator SendPlayerMovement()
    //{
    //    while (true)
    //    {
	//		yield return new WaitForSeconds(0.02f); // 50 times a sec
	//		//Debug.Log("OnPlayerMoved|"+transform.position+"|"+transform.rotation);
	//		OnlineGameManager.instance.OnPlayerMoved(transform.position, transform.rotation.eulerAngles);
	//	}
	//}
	void InitOnline()
    {
		StopAllCoroutines();
		//StartCoroutine(SendPlayerMovement());
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
		GetMovementAndRotationSpeed(out float _movementSpeed, out float _rotationSpeed);
		MakeMovement(_movementSpeed);
		UpdateAim(_rotationSpeed);

		OnlineGameManager.instance.OnPlayerMoved(transform.position, transform.rotation.eulerAngles);

		MakePushing();
	}
	[HideInInspector] Vector3 lastMovement;
	void MakeMovement(float _movementSpeed)
    {
		if (!moving || pushingByProjectile || isPushingBackBySalmonRune) { lastMovement = Vector3.zero; return; }
		//Vector3 translation = new Vector3(leftController.GetTouchPosition.x * Time.deltaTime * speedMovements, 0, leftController.GetTouchPosition.y * Time.deltaTime * speedMovements);
		Vector3 translation = new Vector3(leftController.GetTouchPosition.x, 0, leftController.GetTouchPosition.y).normalized * _movementSpeed * Time.deltaTime;
		lastMovement = translation;
		transform.Translate(translation, Space.World);
    }
	void UpdateAim(float _rotationSpeed)
	{
		Vector2 value;

		if(aiming || moving)
        {
			if (aiming) value = rightController.GetTouchPosition;
			else value = leftController.GetTouchPosition;


			float yAxis = Mathf.Atan2(value.x, value.y) * Mathf.Rad2Deg;
			Quaternion targetRot = Quaternion.Euler(transform.rotation.x, yAxis, transform.rotation.z);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);

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
					//StartCoroutine(ReloadLocallyCoroutine());
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
			Vector3 force = pushingVector * GetCorrectPushbackForce() * Time.deltaTime;
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
		yield return new WaitForSeconds(0.3f);
		canShootLocally = true;
    }
	public void ForbidToShootFromServer()
	{
		canShootOnline = false;
		StartCoroutine(ReloadFromServerCoroutine());
	}
	IEnumerator ReloadFromServerCoroutine()
	{
		yield return new WaitForSeconds(0.25f);
		canShootOnline = true;
	}
	#endregion

	#region Jump related
	public void TryToJump_Request()
	{
		if (!EventManager.isAlive) return;
		if (!pushingByProjectile_cantJump && !isPushingBackBySalmonRune && canJumpLocally && localJumpsAmount > 0)
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
		rb.AddForce(Vector3.up * GetCorrectJumpForce(), forceModeOnJump);
	}
	public int localJumpsAmount;
	public bool canJumpLocally; // local reload 2 seconds
	float localJumpReload = 2f;

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
		if (!EventManager.isAlive) return;

		GravityProjectile gp = collision.gameObject.GetComponent<GravityProjectile>();

		if (gp != null)
        {
			if(gp.playerToIgnore != gameObject)
            {
				if (pushingByProjectile) return;

				// set possible killer
				if (hitAssignedToPlayer != null) StopCoroutine(hitAssignedToPlayer);
				hitAssignedToPlayer = HitReferencedToPlayerTimeoutCoroutine(gp.dbIdOfPleyerWhoMadeLaunch);
				StartCoroutine(hitAssignedToPlayer);

				if (!gp.activeRuneEffects.Contains(Rune.Black))
                {
					SetLastShotRuneEffects(gp.activeRuneEffects);
					StartCoroutine(PushbackCoroutine(collision.gameObject.transform.forward));
				}
                else
                {
					// tmp black rune death equals to spike death
					EventManager.isAlive = false;
					StartCoroutine(EventManager.instance.KillPlayer(DeathDetails.BlackRuneKilled, 0));
				}
            }
        }
        if (collision.gameObject.tag.Equals("Game_Wall"))
        {
			//Debug.Log("Player was pushed to the wall");
			pushingByProjectile = false;
		}
    }

	bool isPushingBackBySalmonRune;
    public void OnTriggerEnter(Collider other)
    {
		if (!EventManager.isAlive) return;
		
		if(other.gameObject.layer == LayerMask.NameToLayer("SalmonRuneBarrier"))
        {
			isPushingBackBySalmonRune = true;
			StartCoroutine(SalmonRunePushbackCoroutine());

			int dbIdOfHitter = -1;
			Player hitter = other.gameObject.GetComponentInParent<Player>();
			if (hitter != null)
				dbIdOfHitter = hitter.playerData.db_id;

			if(hitAssignedToPlayer != null) StopCoroutine(hitAssignedToPlayer);
			hitAssignedToPlayer = HitReferencedToPlayerTimeoutCoroutine(dbIdOfHitter);
			StartCoroutine(hitAssignedToPlayer);

			Vector3 direction = (transform.position - other.transform.position).normalized;

			direction = new Vector3(direction.x * salmonRunePushbackForce.x, 1f * salmonRunePushbackForce.y, direction.z * salmonRunePushbackForce.z);
			rb.velocity = Vector3.zero;
			pushingByProjectile = false;
			rb.AddForce(direction, salmonRuneForceMode);
		}
	}
	IEnumerator SalmonRunePushbackCoroutine()
    {
		yield return new WaitForSeconds(salmonRunePushbackCooldown);
		isPushingBackBySalmonRune = false;
    }

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
		if(dbIdOfHitter != -1)
        {
			dbIdOflastHitPlayer = dbIdOfHitter;
			yield return new WaitForSeconds(hitIsReferencedToPlayer);
			dbIdOflastHitPlayer = -1;
		}
		yield break;
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
		EventManager.isAvailableForRaycaster = false;

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

		SetLastShotRuneEffects(null);

		StartCoroutine(EventManager.instance.SetIsAvailableForRaycaster());
	}

	#endregion

	#region Adaptive projectile hit settings
	public float speedMovements = 5.6f;
	public float speedRotation = 8f;

	public ForceMode forceModeOnJump = ForceMode.VelocityChange;
	public float forceToApplyOnJump = 10.5f;

	public float forceToApplyOnGravityShot = 22f;
	public float pushbackDuration = 0.95f;

	List<Rune> lastShotRuneEffects;

	DateTime lightBlueRuneEffectStartTime;
	DateTime redRuneEffectStartTime;

	// LightBlue Rune
	float lightBlueRuneEffectDuration = 13f;
	float lightBlueRunePushbackDecrease = -0.45f;
	float lightBlueRuneMovementSpeedDecrease = -0.37f;
	float lightBlueRuneRotationSpeedDecrease = -0.5f;
	float lightBlueRuneJumpForceDecrease = -0.20f;

	// SpringGreen Rune
	float springGreenRuneSpeedIncrease = 0.4f;
	float springGreenRuneRotationSpeedIncrease = 0.5f;
	// DarkGreen Rune
	float darkGreenRuneJumpForceIncrease = 0.25f;

	// RedRune
	float redRuneEffectDuration = 13f;

	// SalmonRune
	float salmonRuneMovementSpeedDecrease = -0.8f;
	float salmonRuneJumpForceDecrease = -0.8f;
	public Vector3 salmonRunePushbackForce = new Vector3(13f, 5f, 13f);
	public ForceMode salmonRuneForceMode = ForceMode.VelocityChange;
	public float salmonRunePushbackCooldown = 2f;

	float checkNegativeEffectsExpirationEach = 0.2f;

	public IEnumerator negativeEffectsController;
	IEnumerator NegativeEffectsController()
    {
		while(EventManager.isAlive && lastShotRuneEffects!= null && lastShotRuneEffects.Count > 0)
        {
			yield return new WaitForSeconds(checkNegativeEffectsExpirationEach);

            if (lastShotRuneEffects.Contains(Rune.LightBlue))
            {
				if ((DateTime.Now - lightBlueRuneEffectStartTime).TotalMilliseconds > TimeSpan.FromSeconds(lightBlueRuneEffectDuration).TotalMilliseconds)
                {
					lastShotRuneEffects.Remove(Rune.LightBlue);
					SendMessageDebuffEnded(Rune.LightBlue);
                }
            }
			if (lastShotRuneEffects.Contains(Rune.Red))
			{
				if ((DateTime.Now - redRuneEffectStartTime).TotalMilliseconds > TimeSpan.FromSeconds(redRuneEffectDuration).TotalMilliseconds)
                {
					lastShotRuneEffects.Remove(Rune.Red);
					CameraRenderingManager.instance.SetRedRuneDebuffState(false);
					SendMessageDebuffEnded(Rune.Red);
                }
			}
		}
    }
	void SendMessageDebuffStarted(Rune debuff)
	{
		string message = $"{PLAYER_RECEIVED_DEBUFF}|{ConnectionManager.instance.currentUserData.db_id}|{debuff}";
		ConnectionManager.instance.SendMessageToServer(message, BorisUnityDev.Networking.MessageProtocol.TCP);
	}
	void SendMessageDebuffEnded(Rune debuff)
    {
		string message = $"{PLAYER_DEBUFF_ENDED}|{ConnectionManager.instance.currentUserData.db_id}|{debuff}";
		ConnectionManager.instance.SendMessageToServer(message, BorisUnityDev.Networking.MessageProtocol.TCP);
	}
	public void SetLastShotRuneEffects(List<Rune> runeEffects)
    {
		lastShotRuneEffects = runeEffects;
		if(runeEffects != null && runeEffects.Count > 0)
        {
			if (runeEffects.Contains(Rune.LightBlue)) lightBlueRuneEffectStartTime = DateTime.Now;
			if (runeEffects.Contains(Rune.Red)) 
			{ 
				redRuneEffectStartTime = DateTime.Now;
				CameraRenderingManager.instance.SetRedRuneDebuffState(true);
			}

			if (negativeEffectsController != null) StopCoroutine(negativeEffectsController);
			negativeEffectsController = NegativeEffectsController();
			StartCoroutine(negativeEffectsController);

			foreach (var a in runeEffects)
			{
				if (a == Rune.Red || a == Rune.LightBlue)
					SendMessageDebuffStarted(a);
			}
		}
	}

	public float GetCorrectPushbackForce()
    {
		float defaultMultiplier = 1f;
		if (lastShotRuneEffects != null && lastShotRuneEffects.Count > 0)
        {
			if (lastShotRuneEffects.Contains(Rune.LightBlue)) defaultMultiplier += lightBlueRunePushbackDecrease;
		}
		return forceToApplyOnGravityShot * defaultMultiplier;
    }

	void GetMovementAndRotationSpeed(out float movementSpeed, out float rotationSpeed)
    {
		float defaultMovementMultiplier = 1f;
		float defaultRotationMultiplier = 1f;

		if (lastShotRuneEffects != null && lastShotRuneEffects.Count > 0)
        {
			if (lastShotRuneEffects.Contains(Rune.LightBlue)) defaultMovementMultiplier += lightBlueRuneMovementSpeedDecrease;
			if (lastShotRuneEffects.Contains(Rune.LightBlue)) defaultRotationMultiplier += lightBlueRuneRotationSpeedDecrease;
		}

		if (assignedPlayer.runeEffects.Contains(Rune.SpringGreen)) defaultMovementMultiplier += springGreenRuneSpeedIncrease;
		if (assignedPlayer.runeEffects.Contains(Rune.SpringGreen)) defaultRotationMultiplier += springGreenRuneRotationSpeedIncrease;

		movementSpeed = speedMovements * defaultMovementMultiplier;
		rotationSpeed = speedMovements * defaultRotationMultiplier;
	}

	float GetCorrectJumpForce()
	{
		float defaultMultiplier = 1f;
		if (lastShotRuneEffects != null && lastShotRuneEffects.Count > 0)
        {
			if (lastShotRuneEffects.Contains(Rune.LightBlue)) defaultMultiplier += lightBlueRuneJumpForceDecrease;
		}
		if (assignedPlayer.runeEffects.Contains(Rune.DarkGreen)) defaultMultiplier += darkGreenRuneJumpForceIncrease;

		return forceToApplyOnJump * defaultMultiplier;
	}

	#endregion

	#region LightBlueRune debuff
	// COOL BUT CANT ACCOMPLISH IN CURRENT ENVIROMNENT
	/*
	int groundLayer;
	int xRayObjLayer;

	bool DoesLayerBelongsToGround(int layer)
    {
		//Debug.Log($"{layer} || {groundLayer} || {xRayObjLayer}");
		if (layer == groundLayer || layer == xRayObjLayer) return true;
		return false;
    }

	float checkGroundLeaveEach = 0.05f;
	float considerLeftGroundAfter = 0.11f;

	DateTime lastSeenOnGround;
	DateTime leftTheGround;

	public bool onGround;

	public delegate void TouchedTheGround();
	public event TouchedTheGround TouchedTheGroundEvent;

	public delegate void LeftTheGround();
	public event LeftTheGround LeftTheGroundEvent;
	void InitLightBlueRuneDebuff()
    {
		groundLayer = 10;
		xRayObjLayer = 8;

		StartCoroutine(GroundLeaveCheck());
	}

    private void OnCollisionStay(Collision collision)
    {
		Debug.Log("OnCollisionStay - player movement controller");
		if (DoesLayerBelongsToGround(collision.collider.gameObject.layer))
		{
            if (!onGround)
            {
				Debug.Log("Got On Ground");

			}
			//Debug.Log("Touched Ground");
			lastSeenOnGround = DateTime.Now;
			onGround = true;

			
		}
	}

    private void OnCollisionExit(Collision collision)
    {
		if (DoesLayerBelongsToGround(collision.collider.gameObject.layer))
		{
			//Debug.Log("Left the Ground");
			leftTheGround = DateTime.Now;

			//Debug.Log("Left the ground");
		}
	}

    IEnumerator GroundLeaveCheck()
    {
        while (true)
        {
			yield return new WaitForSeconds(checkGroundLeaveEach);

			//Debug.Log("GroundLeaveCheck");

			leftTheGround += (DateTime.Now - leftTheGround);

			if (onGround && leftTheGround > lastSeenOnGround)
            {
				//Debug.Log("Moved out of ground");

				if((leftTheGround - lastSeenOnGround).TotalMilliseconds > TimeSpan.FromSeconds(considerLeftGroundAfter).TotalMilliseconds)
                {
					Debug.Log("Left the ground");
					onGround = false;
                }
            }

        }
    }

    void OnCollisionEnter_Ground(Collision collision)
    {
		Debug.Log("OnCollisionEnter");
		Debug.Log(LayerMask.LayerToName(collision.collider.gameObject.layer));
		if(DoesLayerBelongsToGround(collision.collider.gameObject.layer))
        {
			Debug.Log("Touched Ground");
        }
    }

	void OnCollisionExit_Ground(Collision collision)
    {
		Debug.Log("OnCollisionExit");
		if (DoesLayerBelongsToGround(collision.collider.gameObject.layer))
		{
			Debug.Log("Left Ground");
		}
	}
	*/
    #endregion
}
