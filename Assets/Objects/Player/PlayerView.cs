using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour, IPlayerPrefObserver, IPlayerPrefKeybindObserver, IPlayerPrefSettingsObserver {

	public GameObject head;
	public GameObject graphicalUserInterface;
	public Camera cam;
	public PlayerWeaponSystem weaponSystem;

	IGUI gui;

	[SerializeField] float interactRange = 3f;
	[SerializeField] float interactTestsPerSecond = 10f;

	float interactTestInterval;
	float interactTestTimer;

	GameObject lastInteractObject;
	IInteractable currentInteractObject;

	[SerializeField] float maxPickupObjectMass = 20f;
	[SerializeField] float throwForce = 6000f;
	[SerializeField] [Range(0, 10)] int grabbedObjectSmoothing = 2;

	GameObject grabbedObject;
	Rigidbody grabbedObjectRB;
	float grabbedObjectDrag;
	float grabbedObjectAngularDrag;
	bool grabbedObjectUsesGravity;
	Vector3[] grabbedObjectPositions;
	Vector3[] grabbedObjectRotations;
	int grabbedObjectSmoothingIndex;

	float mouseSensitivity;
	float mouseInvert;

	DoubleKey keyInteract;
	DoubleKey keyPrimaryFire;
	DoubleKey keyToggleGUI;

	int layermaskInteract;
	int layermaskProp;

	int layerWater;

	void Start () {
		AddSelfToPlayerPrefObserverList();
		LoadValues();
		LoadKeys();
		layermaskInteract = GetLayerMask(LayerMask.NameToLayer("InteractCast"));
		layermaskProp = GetLayerMask(LayerMask.NameToLayer("Prop"));
		layerWater = LayerMask.NameToLayer("Water");
		interactTestInterval = 1f / interactTestsPerSecond;
		interactTestTimer = 0f;
		gui = graphicalUserInterface.GetComponent<IGUI>();
		InitializeGrabbedObjectSmoothingArrays();
		Cursor.lockState = CursorLockMode.Locked;
	}
	
	void Update () {
		if(Time.timeScale > 0f){
			MouseLook();
			if(Input.GetKeyDown(KeyCode.Mouse0)) Cursor.lockState = CursorLockMode.Locked;
			if(keyInteract.GetKeyDown()){
				if(grabbedObject == null) TryToInteract();
				else DropGrabbedObject(false);
			}
			if(keyPrimaryFire.GetKeyDown()){
				if(grabbedObject != null){
					DropGrabbedObject(true);
				}
			}
			if(keyToggleGUI.GetKeyDown()){
				gui.toggleGUI();
			}
		}
	}

	void FixedUpdate(){
		GrabbedObjectManager();
		InteractInfoManager();
	}

	public void AddSelfToPlayerPrefObserverList(){
		PlayerPrefManager.AddObserver(this);
	}

	public void NotifyKeybindsChanged(){
		LoadKeys();
	}

	public void NotifyPlayerSettingsChanged(){
		LoadValues();
	}

	private void LoadValues(){
		mouseSensitivity = PlayerPrefManager.GetFloat("mouse_sensitivity");
		mouseInvert = ((PlayerPrefManager.GetInt("mouse_invert") == 0) ? -1 : 1);
		cam.fieldOfView = PlayerPrefManager.GetFloat("camera_fov");
	}

	private void LoadKeys(){
		keyInteract = DoubleKey.FromPlayerPrefs("key_interact");
		keyPrimaryFire = DoubleKey.FromPlayerPrefs("key_primary_fire");
		keyToggleGUI = DoubleKey.FromPlayerPrefs("key_toggleGUI");
	}

	private void MouseLook(){
		Vector2 mouseMovement = GetMouseMovement();
		transform.Rotate(new Vector3(0f, mouseMovement.x, 0f));
		head.transform.Rotate(new Vector3(mouseMovement.y, 0f, 0f));
		KeepCameraRightSideUp();
	}

	private void KeepCameraRightSideUp(){
		if(head.transform.localEulerAngles.y != 0){
			if(head.transform.localEulerAngles.x < 180f) head.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
			else head.transform.localEulerAngles = new Vector3(270f, 0f, 0f);
		}
	}

	private Vector2 GetMouseMovement(){
		float movementX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
		float movementY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * mouseInvert;
		return new Vector2(movementX, movementY);
	}

	private int GetLayerMask(int layer){
		string[] names = new string[32];
		for(int i=0; i<32; i++){
			if(!Physics.GetIgnoreLayerCollision(layer, i)) names[i] = LayerMask.LayerToName(i);
		}
		return LayerMask.GetMask(names);
	}

	private void TryToInteract(){
		RaycastHit hit;
		if(Physics.Raycast(head.transform.position, head.transform.forward, out hit, interactRange, layermaskInteract)){
			if(TagManager.CompareTag("Interactable", hit.collider.gameObject)){
				currentInteractObject = hit.collider.gameObject.GetComponent<IInteractable>();
				if(currentInteractObject != null) currentInteractObject.Interact(this.gameObject);
				else Debug.LogError(hit.collider.gameObject + " has no <IInteractable> but is supposed to. Message from PlayerView > Update");
			}else if(grabbedObject == null && hit.collider.attachedRigidbody != null){
				Rigidbody otherRB = hit.collider.attachedRigidbody;
				if((!otherRB.isKinematic) && otherRB.mass <= maxPickupObjectMass){
					if(!TagManager.CompareTag("NonGrabbable", hit.collider.gameObject)){
						GrabObject(hit.collider.gameObject);
					}
				}
			}
		}
	}

	private void InteractInfoManager(){
		interactTestTimer += Time.fixedDeltaTime;
		if(interactTestTimer >= interactTestInterval){
			if(grabbedObject == null){
				bool enableDisplay = false;
				RaycastHit hit;
				if(Physics.Raycast(head.transform.position, head.transform.forward, out hit, interactRange, layermaskInteract)){
					if(TagManager.CompareTag("Interactable", hit.collider.gameObject)){
						if(hit.collider.gameObject != lastInteractObject){
							currentInteractObject = hit.collider.gameObject.GetComponent<IInteractable>();
						}
						if(currentInteractObject != null){
							gui.SetInteractDisplayMessage(currentInteractObject.GetDescription());
						}
						else{
							gui.SetInteractDisplayMessage(null);
						}
						lastInteractObject = hit.collider.gameObject;
						enableDisplay = true;
					}
					else{
						if(hit.collider.attachedRigidbody != null){
							Rigidbody otherRB = hit.collider.attachedRigidbody;
							if((!otherRB.isKinematic) && otherRB.mass <= maxPickupObjectMass){
								if(!TagManager.CompareTag("NonGrabbable", hit.collider.gameObject)){
									gui.SetInteractDisplayMessage("Pick up");
									enableDisplay = true;
								}
							}
						}
					}
				}
				if(enableDisplay) gui.EnableInteractDisplay();
				else gui.DisableInteractDisplay();
			}else{
				gui.DisableInteractDisplay();
			}
			interactTestTimer -= interactTestInterval;
		}
	}

	void OnCollisionEnter(Collision collision){
		if(grabbedObject != null){
			foreach(ContactPoint point in collision.contacts){
				if(point.otherCollider.gameObject == grabbedObject){
					DropGrabbedObject(false);
					break;
				}
			}
		}
	}

	private void InitializeGrabbedObjectSmoothingArrays(){
		grabbedObjectPositions = new Vector3[grabbedObjectSmoothing + 1];
		grabbedObjectRotations = new Vector3[grabbedObjectSmoothing + 1];
		grabbedObjectSmoothingIndex = 0;
	}

	private void FillGrabbedObjectSmoothingArrays(){
		Vector3 grObjPos = grabbedObject.transform.position;
		Vector3 grObjRot = grabbedObject.transform.eulerAngles;
		for(int i=0; i< grabbedObjectPositions.Length; i++){
			grabbedObjectPositions[i] = grObjPos;
			grabbedObjectRotations[i] = grObjRot;
		}
	}

	private void GrabObject(GameObject obj){
		weaponSystem.HolsterWeapon();
		weaponSystem.holsterLock = true;
		grabbedObject = obj;
		//grabbedObject.transform.localRotation = Quaternion.identity;
		grabbedObject.transform.localEulerAngles = new Vector3(0f, transform.localEulerAngles.y, 0f);
		grabbedObjectRB = obj.GetComponent<Rigidbody>();
		grabbedObjectUsesGravity = grabbedObjectRB.useGravity;
		grabbedObjectRB.useGravity = false;
		grabbedObjectRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		grabbedObjectDrag = grabbedObjectRB.drag;
		grabbedObjectAngularDrag = grabbedObjectRB.angularDrag;
		grabbedObjectRB.drag = 0f;
		grabbedObjectRB.angularDrag = 10f;
		FillGrabbedObjectSmoothingArrays();
	}

	private void DropGrabbedObject(bool shouldThrow){
		weaponSystem.holsterLock = false;
		grabbedObjectRB.useGravity = grabbedObjectUsesGravity;
		grabbedObjectRB.collisionDetectionMode = CollisionDetectionMode.Discrete;
		grabbedObjectRB.velocity = GetSmoothedGrabbedObjectVelocity();
		grabbedObjectRB.angularVelocity = GetSmoothedGrabbedObjectAngularVelocity();
		grabbedObjectRB.drag = grabbedObjectDrag;
		grabbedObjectRB.angularDrag = grabbedObjectAngularDrag;
		if(shouldThrow){
			grabbedObjectRB.AddForce(head.transform.forward * GetThrowAccel(grabbedObjectRB.mass), ForceMode.Acceleration);
			//grabbedObjectRB.AddForce(head.transform.forward * GetForceMultiplier(grabbedObjectRB.mass), ForceMode.Force);
			grabbedObjectRB.AddTorque(Random.insideUnitSphere * Random.Range(50, 100));
			weaponSystem.UnholsterWeapon();
		}
		grabbedObject = null;
		grabbedObjectRB = null;
	}

	private void GrabbedObjectManager(){
		if(grabbedObject != null){
			Vector3 properPosition = head.transform.position + head.transform.forward * 2f;
			Vector3 localProperPosition = transform.InverseTransformPoint(properPosition) - head.transform.localPosition;
			float grabAngle = Vector3.Angle(localProperPosition, Vector3.down);
			if(grabAngle < 45f){
				properPosition = head.transform.position + (transform.forward + Vector3.down).normalized * 2f;
			}
			if(Vector3.Distance(properPosition, grabbedObject.transform.position) > 2f){
				DropGrabbedObject(false);
				return;
			}
			Vector3 deltaPos = properPosition - grabbedObject.transform.position;

			/*
			bool correctProperPosition = false;
			Vector3 rayStart = grabbedObject.transform.position;
			RaycastHit hit = new RaycastHit();
			int i=0;
			for(; i<5; i++){
				if(Physics.Raycast(rayStart, deltaPos, out hit, deltaPos.magnitude, layermaskProp)){
					if(hit.collider.gameObject == grabbedObject){
						rayStart = hit.point - (hit.normal * 0.01f);
					}else{
						correctProperPosition = true;
						break;
					}
				}else{
					correctProperPosition = false;
					break;
				}
			}
			if(correctProperPosition){
				properPosition = hit.point + hit.normal * 0.01f;
			}else{
				if(i == 5) Debug.LogError("bad collision model right here");
			}
			*/

			RaycastHit hit;
			if(Physics.Raycast(grabbedObject.transform.position, deltaPos, out hit, deltaPos.magnitude, layermaskProp)){
				if(hit.collider.gameObject == grabbedObject){
					Debug.LogError("props should have only one convex collider enclosing their center. otherwise i'll have to refactor the grabbedobjectmanager");
				}else{
					//if(!TagManager.CompareTag("Water", hit.collider.gameObject)){
					if(hit.collider.gameObject.layer != layerWater){
						properPosition = hit.point + hit.normal * 0.01f;
					}
				}
			}

			grabbedObjectRB.velocity=Vector3.zero;
			grabbedObjectRB.MovePosition(properPosition);
			grabbedObjectPositions[grabbedObjectSmoothingIndex] = grabbedObject.transform.position;
			grabbedObjectRotations[grabbedObjectSmoothingIndex] = grabbedObject.transform.eulerAngles;
			grabbedObjectSmoothingIndex = (grabbedObjectSmoothingIndex + 1) % grabbedObjectPositions.Length;
		}
	}

	private Vector3 GetSmoothedGrabbedObjectVelocity(){
		//not an average of all, just the difference between the oldest known and the current
		Vector3 diff = grabbedObject.transform.position - grabbedObjectPositions[grabbedObjectSmoothingIndex];
		float timespan = Time.fixedDeltaTime * grabbedObjectPositions.Length;
		return diff/timespan;
	}

	private Vector3 GetSmoothedGrabbedObjectAngularVelocity(){
		//not an average of all, just the difference between the oldest known and the current
		Vector3 diff = Mathf.Deg2Rad * (grabbedObject.transform.eulerAngles - grabbedObjectRotations[grabbedObjectSmoothingIndex]);
		float timespan = Time.fixedDeltaTime * grabbedObjectPositions.Length;
		return diff/timespan;
	}

	//original throwforce was 8000
	private float GetForceMultiplier(float mass){
		if(mass > 5) return throwForce;
		else return Mathf.Lerp(throwForce/2f, throwForce, mass/5f);
	}

	private float GetThrowAccel(float mass){
		if(mass <= 1f){
			return throwForce;
		}else{
			float lerpFactorLinear = Mathf.Clamp01((mass - 1f) / (maxPickupObjectMass - 1f));
			return Mathf.Lerp(throwForce, throwForce/10f, Mathf.Pow(lerpFactorLinear, 0.125f));
		}
	}

}
