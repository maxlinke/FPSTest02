using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IPauseObserver, IPlayerPrefObserver, IPlayerPrefKeybindObserver, IPlayerPrefSettingsObserver {

	[Header("Components")]
	[SerializeField] Rigidbody rb;
	[SerializeField] CapsuleCollider worldCollider;
	[SerializeField] GameObject head;
	[SerializeField] Camera cam;

	[Header("Script Components")]
	[SerializeField] PlayerMovementNEW movement;
	[SerializeField] PlayerViewNEW view;
	[SerializeField] PlayerHealthNEW health;

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

	DoubleKey keyInteract;
	DoubleKey keyPrimaryFire;
	DoubleKey keyToggleGUI;

	float mouseSensitivity;
	float mouseInvert;

	float controllerSensitivity;
	float controllerInvert;

	IGUI gui;

	bool isCrouching;
	bool isSprinting;

	bool paused;

	void Start () {
		AddSelfToPlayerPrefObserverList();
		LoadKeys();
		LoadValues();
		gui = guiObject.GetComponent<IGUI>();
		movement.Initialize(rb, worldCollider, head, health);
		view.Initialize(this, rb, head);
		health.Initialize(rb, head);
		paused = false;	//instead of this maybe GET it from somewhere
	}

	void Reset () {
		movement = GetComponentInChildren<PlayerMovementNEW>();
		view = GetComponentInChildren<PlayerViewNEW>();
		health = GetComponentInChildren<PlayerHealthNEW>();
	}

	void Update () {
		//debug
		if(Input.GetKeyDown(KeyCode.Mouse1)){
			rb.velocity += head.transform.forward * 50f;
		}
		if(Input.GetKeyDown(KeyCode.Q)){
			if(Time.timeScale < 1f) Time.timeScale = 1f;
			else Time.timeScale = 0.05f;
		}
		//not debug
		if(!paused){
			view.ExecuteUpdate(new PlayerViewNEW.ViewInput(GetLookInput(), keyInteract.GetKeyDown(), keyPrimaryFire.GetKeyDown()));
			ManageToggleAndHoldInput(ref isCrouching, keyCrouchHold, keyCrouchToggle);
			ManageToggleAndHoldInput(ref isSprinting, keySprintHold, keySprintToggle);
			movement.ExecuteUpdate();
		}
	}

	void FixedUpdate () {
		PlayerMovementNEW.MoveInput moveInput;
		if(!paused){
			Vector3 dirInput = GetDirectionalInput();
			moveInput = new PlayerMovementNEW.MoveInput(dirInput, keyJump.GetKey(), isCrouching, isSprinting);
		}else{
			moveInput = new PlayerMovementNEW.MoveInput(Vector3.zero, false, isCrouching, isSprinting);
		}
		view.ExecuteFixedUpdate();
		movement.ExecuteFixedUpdate(moveInput);
		gui.SetInteractDisplayMessage(movement.DebugInfo);
		if(view.isHoldingOntoSomething) view.ManageGrabbedObject();
	}

	//TODO on start load what components to have enabled (for example dont be able to move etc)

	public void Pause () {
		paused = true;
	}

	public void Unpause () {
		paused = false;
	}
		
	//IPlayerPrefObserver / loading stuff

	public void AddSelfToPlayerPrefObserverList () {
		PlayerPrefManager.AddObserver(this);
	}

	public void NotifyKeybindsChanged () {
		LoadKeys();
	}

	public void NotifyPlayerSettingsChanged () {
		LoadValues();
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

		keyInteract = DoubleKey.FromPlayerPrefs("key_interact");
		keyPrimaryFire = DoubleKey.FromPlayerPrefs("key_primary_fire");
		keyToggleGUI = DoubleKey.FromPlayerPrefs("key_toggleGUI");
	}

	void LoadValues () {
		mouseSensitivity = PlayerPrefManager.GetFloat("mouse_sensitivity");
		mouseInvert = ((PlayerPrefManager.GetInt("mouse_invert") == 0) ? +1 : -1);
		//TODO controller options!
		//TODO dynamically created bindings menu (enums for different categories etc...) (joystick buttons can be assigned normally but what about the "axes" at the back?)
		Debug.LogWarning("TODO : Controller options!");
		Debug.LogWarning("TODO : Dynamically created bindings menu!");
		controllerSensitivity = mouseSensitivity / 2f;
		controllerInvert = mouseInvert;
		cam.fieldOfView = PlayerPrefManager.GetFloat("camera_fov");
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

	Vector2 GetLookInput () {
		float mouseX = Input.GetAxisRaw("Mouse X");
		float mouseY = Input.GetAxisRaw("Mouse Y") * mouseInvert;
		Vector2 mouseInput = new Vector2(mouseX, mouseY);
		float controllerX = Input.GetAxisRaw("RX");
		float controllerY = Input.GetAxisRaw("RY") * mouseInvert;
		Vector2 controllerInput = new Vector2(controllerX, controllerY);
		Vector2 combined = mouseInput + controllerInput;
		return (combined * mouseSensitivity * Time.timeScale);
	}

}
