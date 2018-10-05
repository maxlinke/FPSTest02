using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewNEW : MonoBehaviour, IPlayerPrefObserver, IPlayerPrefKeybindObserver, IPlayerPrefSettingsObserver {

	[SerializeField] float interactRange = 3f;
	[SerializeField] float interactTestsPerSecond = 10f;
	[SerializeField] float maxPickupObjectMass = 20f;
	[SerializeField] float throwForce = 6000f;
	[SerializeField] [Range(0, 10)] int grabbedObjectSmoothing = 2;

	public GameObject head;
	Rigidbody rb;
	Camera cam;
	IGUI gui;

	DoubleKey keyInteract;
	DoubleKey keyPrimaryFire;
	DoubleKey keyToggleGUI;

	float mouseSensitivity;
	float mouseInvert;

	float tilt;
	float pan;

	public void Initialize (Rigidbody mainRigidbody, GameObject head, Camera cam, IGUI gui) {
		this.rb = mainRigidbody;
		this.head = head;
		this.cam = cam;
		this.gui = gui;
		AddSelfToPlayerPrefObserverList();
		LoadKeys();
		LoadValues();
		Cursor.lockState = CursorLockMode.Locked;
	}
	
	void Update () {
		MouseLook();
		if(Input.GetKeyDown(KeyCode.Mouse0)) Cursor.lockState = CursorLockMode.Locked;
		if(Input.GetKeyDown(KeyCode.N)) Time.timeScale = 0.1f;
		if(Input.GetKeyDown(KeyCode.M)) Time.timeScale = 1f;
	}

	void FixedUpdate () {
		MatchRBRotationToHead();
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
		cam.fieldOfView = PlayerPrefManager.GetFloat("camera_fov");
	}

	void LoadKeys () {
		keyInteract = DoubleKey.FromPlayerPrefs("key_interact");
		keyPrimaryFire = DoubleKey.FromPlayerPrefs("key_primary_fire");
		keyToggleGUI = DoubleKey.FromPlayerPrefs("key_toggleGUI");
	}

	void MouseLook () {
		Vector2 mouseMove = GetMouseMovement();
		pan = Mathf.Repeat(pan + mouseMove.x, 360);
		tilt = Mathf.Clamp(tilt + mouseMove.y, -90, 90);
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

	//utility

	Vector2 GetMouseMovement () {
		float movementX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.timeScale;
		float movementY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.timeScale * mouseInvert;
		return new Vector2(movementX, movementY);
	}

}
