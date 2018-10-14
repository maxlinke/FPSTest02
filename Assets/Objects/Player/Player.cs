using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IPauseObserver, IPlayerPrefObserver, IPlayerPrefKeybindObserver {

	[Header("Components")]
	[SerializeField] Rigidbody rb;
	[SerializeField] CapsuleCollider worldCollider;
	[SerializeField] GameObject head;
	[SerializeField] Camera cam;

	[Header("Script Components")]
	[SerializeField] PlayerMovementNEW playerMovement;
	[SerializeField] PlayerViewNEW playerView;
	[SerializeField] PlayerHealthSystem playerHealthSystem;
	[SerializeField] PlayerWeaponSystem playerWeaponSystem;

	[Header("Scene Objects")]
	[SerializeField] GameObject guiObject;

	DoubleKey keyMoveForward;
	DoubleKey keyMoveBackward;
	DoubleKey keyMoveLeft;
	DoubleKey keyMoveRight;

	DoubleKey keyJump;
	DoubleKey keyCrouchToggle;
	DoubleKey keySprintToggle;
	DoubleKey keyCrouchHold;
	DoubleKey keySprintHold;

	IGUI gui;

	bool isCrouching;
	bool isSprinting;

	void Start () {
		AddSelfToPlayerPrefObserverList();
		LoadKeys();
		gui = guiObject.GetComponent<IGUI>();
		playerMovement.Initialize(rb, worldCollider, head, playerHealthSystem, gui);
		playerView.Initialize(this, rb, head, cam, gui);
	}

	void Reset () {
		playerMovement = GetComponentInChildren<PlayerMovementNEW>();
		playerView = GetComponentInChildren<PlayerViewNEW>();
		playerHealthSystem = GetComponentInChildren<PlayerHealthSystem>();
		playerWeaponSystem = GetComponentInChildren<PlayerWeaponSystem>();
	}

	void Update () {
		if(Input.GetKeyDown(KeyCode.Mouse1)){
			rb.velocity += head.transform.forward * 50f;
		}
		if(Input.GetKeyDown(KeyCode.Q)){	//TODO debug slowmo. remove if done with that stuff
			if(Time.timeScale < 1f) Time.timeScale = 1f;
			else Time.timeScale = 0.05f;
		}
		ManageToggleAndHoldInput(ref isCrouching, keyCrouchHold, keyCrouchToggle);
		ManageToggleAndHoldInput(ref isSprinting, keySprintHold, keySprintToggle);
		playerMovement.ExecuteUpdate();
	}

	void FixedUpdate () {
		Vector3 dirInput = GetDirectionalInput();
		PlayerMovementNEW.MoveInput moveInput = new PlayerMovementNEW.MoveInput(dirInput, keyJump.GetKey(), isCrouching, isSprinting);
		playerMovement.ExecuteFixedUpdate(moveInput);
	}

	//TODO on start load what components to have enabled (for example dont be able to move etc)

	public void Pause () {
//		movementWasEnabled = playerMovement.enabled;
//		viewWasEnabled = playerView.enabled;
//		healthSystemWasEnabled = playerHealthSystem.enabled;
//		weaponSystemWasEnabled = playerWeaponSystem.enabled;
		SetAllFunctionsEnabled(false);
	}

	public void Unpause () {
//		playerMovement.enabled = movementWasEnabled;
//		playerView.enabled = viewWasEnabled;
//		playerHealthSystem.enabled = healthSystemWasEnabled;
//		playerWeaponSystem.enabled = weaponSystemWasEnabled;
	}

	public void SetAllFunctionsEnabled (bool value) {
//		playerMovement.enabled = value;
//		playerView.enabled = value;
//		playerHealthSystem.enabled = value;
//		playerWeaponSystem.enabled = value;
	}
		
	//IPlayerPrefObserver / IPlayerPrefKeybindObserver / loading stuff

	public void AddSelfToPlayerPrefObserverList () {
		PlayerPrefManager.AddObserver(this);
	}

	public void NotifyKeybindsChanged () {
		LoadKeys();
	}

	void LoadKeys () {
		keyMoveForward = DoubleKey.FromPlayerPrefs("key_move_forward");
		keyMoveBackward = DoubleKey.FromPlayerPrefs("key_move_backward");
		keyMoveLeft = DoubleKey.FromPlayerPrefs("key_move_left");
		keyMoveRight = DoubleKey.FromPlayerPrefs("key_move_right");

		keyJump = DoubleKey.FromPlayerPrefs("key_jump");
		keyCrouchToggle = DoubleKey.FromPlayerPrefs("key_crouch_toggle");
		keySprintToggle = DoubleKey.FromPlayerPrefs("key_sprint_toggle");
		keyCrouchHold = DoubleKey.FromPlayerPrefs("key_crouch_hold");
		keySprintHold = DoubleKey.FromPlayerPrefs("key_sprint_hold");
	}

	//utility

	void ManageToggleAndHoldInput (ref bool value, DoubleKey keyHold, DoubleKey keyToggle) {
		if(keyHold.GetKey()) value = true;
		if(keyHold.GetKeyUp()) value = false;
		if(keyToggle.GetKeyDown()) value = !value;
	}

	Vector3 GetDirectionalInput () {
		int keyboardZ = (keyMoveForward.GetKey() ? 1 : 0) + (keyMoveBackward.GetKey() ? -1 : 0);
		int keyboardX = (keyMoveLeft.GetKey() ? -1 : 0) + (keyMoveRight.GetKey() ? 1 : 0);
		Vector3 keyboardInput = new Vector3(keyboardX, 0f, keyboardZ);
		float controllerX = Input.GetAxisRaw("LX");
		float controllerY = Input.GetAxisRaw("LY");
		Vector3 controllerInput = new Vector3(controllerX, 0f, controllerY);
		Vector3 combined = keyboardInput + controllerInput;
		if(combined.sqrMagnitude > 1f) combined = combined.normalized;
		return combined;
	}

}
