using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementNEW : MonoBehaviour, IPlayerPrefObserver, IPlayerPrefKeybindObserver {

	[Header("Movement parameters")]
	[SerializeField] float moveSpeedRegular = 8f;
	[SerializeField] float moveSpeedCrouch = 4f;
	[SerializeField] float moveSpeedSprint = 12f;
	[SerializeField] float moveAcceleration = 256f;
	[SerializeField] float moveJumpHeight = 1.5f;
	[SerializeField] float moveMaxSlopeAngle = 55f;
	[SerializeField] float moveMaxStepOffset = 0.3f;
	[SerializeField] float moveSlideControl = 12f;
	[SerializeField] float moveAirControl = 4f;

	[Header("Other parameters")]
	[SerializeField] float crouchHeight = 0.9f;
	[SerializeField] float normalHeight = 1.9f;
	[SerializeField] float eyeOffsetFromTop = 0.1f;
	[SerializeField] float normalGravity = 29.43f;
	[SerializeField] float normalStaticFriction = 0.5f;
	[SerializeField] float normalDynamicFriction = 0.5f;
	[SerializeField] float waterTriggerOffsetFromEyes = 0.2f;

	GameObject head;
	CapsuleCollider col;
	Rigidbody rb;
	PlayerHealthSystem healthSystem;
	PhysicMaterial pm;

	DoubleKey keyMoveForward;
	DoubleKey keyMoveBackward;
	DoubleKey keyMoveLeft;
	DoubleKey keyMoveRight;

	DoubleKey keyJump;
	DoubleKey keyCrouchToggle;
	DoubleKey keySprintToggle;
	DoubleKey keyCrouchHold;
	DoubleKey keySprintHold;

	public void Initialize (Rigidbody rb, CapsuleCollider worldCollider, GameObject head, PlayerHealthSystem phs) {
		this.rb = rb;
		this.col = worldCollider;
		this.head = head;
		this.healthSystem = phs;
		pm = worldCollider.material;
	}
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.Mouse1)) rb.velocity += head.transform.forward * 50f;
	}

	void FixedUpdate () {
		rb.velocity += Physics.gravity * Time.fixedDeltaTime;
	}

	//IPlayerPrefObserver / IPlayerPrefKeybindObserver

	public void AddSelfToPlayerPrefObserverList () {
		PlayerPrefManager.AddObserver(this);
	}

	public void NotifyKeybindsChanged () {
		LoadKeys();
	}

	//collisions

	void OnCollisionEnter (Collision collision) {

	}

	void OnCollisionStay (Collision collision) {

	}

	//triggers

	void OnTriggerEnter (Collider otherCollider) {
		
	}

	void OnTriggerStay (Collider otherCollider) {
		WaterBody waterBody = otherCollider.gameObject.GetComponent<WaterBody>();
		if(waterBody != null){
			if(head.transform.position.y - waterTriggerOffsetFromEyes < waterBody.waterLevel){

			}
		}
	}

	void OnTriggerExit (Collider otherCollider) {

	}

	//keybinds

	void LoadKeys(){
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
}
