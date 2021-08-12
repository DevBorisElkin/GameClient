using UnityEngine;
using System.Collections;
using static JoystickTouchController;
using System;

[RequireComponent(typeof(Rigidbody))]

public class PlayerMovementController : MonoBehaviour
{
	public JoystickTouchController leftController;
	public JoystickTouchController rightController;
	public float speedMovements = 5f;
	public float speedRotation = 5f;

	[Space] public float minRotationAngleToShoot = 5f;

	[Space(5f)]
	public bool online;
	public bool debugRot;

	[HideInInspector]
	public ShootingManager ShootMaster;
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

		ShootMaster = FindObjectOfType<ShootingManager>();
		canShootLocally = true;
		canShootOnline = true;
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

	bool moving;
	bool aiming;
	void LeftController_TouchDetection(bool isTouching)
    {
		moving = isTouching;
    }
	void RightController_TouchDetection(bool isTouching)
    {
		aiming = isTouching;
    }

	void Update()
	{
		if(moving)
			MakeMovement();
		UpdateAim();
		DebugRot();
	}
	void MakeMovement()
    {
		Vector3 translation = new Vector3(leftController.GetTouchPosition.x * Time.deltaTime * speedMovements, 0, leftController.GetTouchPosition.y * Time.deltaTime * speedMovements);
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
				else ShootMaster.MakeActualShot(assignedPlayer.projectileSpawnPoint.position, transform.rotation, gameObject);
			}
		}
	}
	bool canShootLocally;
	bool canShootOnline;
	#region Ease server calculations
	// basically we use that mechanic to not let the player send(spam) shoot requests,
	// until the actual shot from server side has been released
	// Using same reload time as on the server, but as signal takes
	// time to reach the device, effective reload time is about 1.5f
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
	void DebugRot()
    {
		if (!debugRot) return;

		Debug.Log(transform.eulerAngles+"|"+transform.rotation.eulerAngles);

        if (Input.GetKeyDown(KeyCode.Space))
        {
			transform.rotation = Quaternion.Euler(0,0,0);
        }
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

	#endregion
}
