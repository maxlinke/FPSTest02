using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewNEW : MonoBehaviour, IPlayerPrefObserver, IPlayerPrefKeybindObserver, IPlayerPrefSettingsObserver {

	[SerializeField] float interactRange = 3f;
	[SerializeField] float interactTestsPerSecond = 10f;
	[SerializeField] float maxPickupObjectMass = 20f;
	[SerializeField] float throwForce = 6000f;
//	[SerializeField] [Range(0, 10)] int grabbedObjectSmoothing = 2;

	Player player;
	GameObject head;
	Rigidbody rb;
	Camera cam;
	IGUI gui;

	DoubleKey keyInteract;
	DoubleKey keyPrimaryFire;
	DoubleKey keyToggleGUI;

	float mouseSensitivity;
	float mouseInvert;

	float controllerSensitivity;
	float controllerInvert;

	float tilt;
	float pan;

	int layermaskInteract;
	float interactTestInterval;
	float interactTestTimer;

	Rigidbody grabbedRB;
	CollisionDetectionMode grabbedRBCollisionDetectionMode;
	float grabbedRBDrag;
	float grabbedRBAngularDrag;
	bool grabbedRBUsesGravity;

	public void Initialize (Player player, Rigidbody mainRigidbody, GameObject head, Camera cam, IGUI gui) {
		this.player = player;
		this.rb = mainRigidbody;
		this.head = head;
		this.cam = cam;
		this.gui = gui;
		AddSelfToPlayerPrefObserverList();
		LoadKeys();
		LoadValues();
		Cursor.lockState = CursorLockMode.Locked;
		layermaskInteract = LayerMaskUtils.CreateMask("InteractCast");
		interactTestInterval = 1f / interactTestsPerSecond;
		interactTestTimer = 0f;

	}
	
	void Update () {
		Look();
		if(keyPrimaryFire.GetKeyDown() && Time.timeScale > 0f){	//TODO remove timescale
			Cursor.lockState = CursorLockMode.Locked;
		}
		if(keyInteract.GetKeyDown()){
			if(grabbedRB != null){
				//TODO set flag to drop/throw grabbed object
			}else{
				bool successfullyInteracted;
				TryToInteract(out successfullyInteracted);
				if(!successfullyInteracted){
					//TODO error sound
				}
			}
		}
	}

	void FixedUpdate () {
		MatchRBRotationToHead();
		//TODO check for wish to drop grabbed object and then drop/throw it
	}

	//interfaces

	public void AddSelfToPlayerPrefObserverList () {
		PlayerPrefManager.AddObserver(this);
	}

	public void NotifyKeybindsChanged () {
		LoadKeys();
	}

	public void NotifyPlayerSettingsChanged () {
		LoadValues();
	}

	//regular methods

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

	void LoadKeys () {
		keyInteract = DoubleKey.FromPlayerPrefs("key_interact");
		keyPrimaryFire = DoubleKey.FromPlayerPrefs("key_primary_fire");
		keyToggleGUI = DoubleKey.FromPlayerPrefs("key_toggleGUI");
	}

	void Look () {
		Vector2 look = GetLookInput();
		pan = Mathf.Repeat(pan + look.x, 360);
		tilt = Mathf.Clamp(tilt + look.y, -90, 90);
		float rPan = Mathf.Deg2Rad * pan;
		float rTilt = Mathf.Deg2Rad * tilt;
		float sinPan = Mathf.Sin(rPan);
		float cosPan = Mathf.Cos(rPan);
		float sinTilt = Mathf.Sin(rTilt);
		float cosTilt = Mathf.Cos(rTilt);
		Vector3 fwdHorizontal = new Vector3(sinPan, 0f, cosPan) * cosTilt;
		Vector3 fwdVertical = new Vector3(0f, sinTilt, 0f);
		Vector3 forward = fwdHorizontal + fwdVertical;
		Vector3 upHorizontal = new Vector3(sinPan, 0f, cosPan) * sinTilt * -1f;
		Vector3 upVertical = new Vector3(0f, cosTilt, 0f);
		Vector3 up = upHorizontal + upVertical;
		head.transform.rotation = Quaternion.LookRotation(forward, up);
	}

	void MatchRBRotationToHead () {
		Vector3 headFwd = head.transform.forward;
		Vector3 headUp = head.transform.up;
		float rPan = Mathf.Deg2Rad * pan;
		Vector3 bodyFwd = new Vector3(Mathf.Sin(rPan), 0f, Mathf.Cos(rPan));
		rb.transform.rotation = Quaternion.LookRotation(bodyFwd, Vector3.up);
		head.transform.rotation = Quaternion.LookRotation(headFwd, headUp);
	}

	void TryToInteract (out bool successful) {
		//TODO maybe use an enum to give a reason why interaction didn't work? too heavy, nothing there etc
		IInteractable interactableObject;
		Rigidbody grabbableRigidbody;
		InteractCast(out interactableObject, out grabbableRigidbody);
		if(interactableObject != null){
			interactableObject.Interact(this.player.gameObject);
			successful = true;
		}else if(grabbableRigidbody != null){
			PickupObject(grabbableRigidbody);
			successful = true;
		}else{
			successful = false;
		}
	}

	void PickupObject (Rigidbody grabbableRigidbody) {
		//TODO grab the bloody thing
		Debug.LogWarning("Picking up objects not yet implemented");
	}

	void ResetGrabbedObjectValues () {
		grabbedRB.drag = grabbedRBDrag;
		grabbedRB.angularDrag = grabbedRBAngularDrag;
		grabbedRB.useGravity = grabbedRBUsesGravity;
		grabbedRB.collisionDetectionMode = grabbedRBCollisionDetectionMode;
	}

	void DropGrabbedObject () {
		ResetGrabbedObjectValues();
	}

	void ThrowGrabbedObject () {
		ResetGrabbedObjectValues();
	}

	//utility

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

	void InteractCast (out IInteractable interactableObject, out Rigidbody grabbableRigidbody) {
		interactableObject = null;
		grabbableRigidbody = null;
		RaycastHit hit;
		if(Physics.Raycast(head.transform.position, head.transform.forward, out hit, interactRange, layermaskInteract)){
			interactableObject = hit.collider.GetComponentInParent<IInteractable>();
			if(IsGrabbable(hit.collider.attachedRigidbody)) grabbableRigidbody = hit.collider.attachedRigidbody;
		}
	}

	bool IsGrabbable (Rigidbody otherRB) {
		if(otherRB != null){
			bool kinematic = otherRB.isKinematic;
			bool massOK = (otherRB.mass <= maxPickupObjectMass);
			bool taggedNonGrabbable = TagManager.CompareTag("NonGrabbable", otherRB.gameObject);
			return (!kinematic && massOK && !taggedNonGrabbable);
		}else{
			return false;
		}
	}

}
