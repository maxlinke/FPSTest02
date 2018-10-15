using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewNEW : MonoBehaviour {

	public struct ViewInput {

		public readonly Vector2 lookInput;
		public readonly bool interactInput;
		public readonly bool fireInput;

		public ViewInput (Vector2 lookInput, bool interactInput, bool fireInput) {
			this.lookInput = lookInput;
			this.interactInput = interactInput;
			this.fireInput = fireInput;
		}

	}

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

	float tilt;
	float pan;
	float bodyPan;

	int layermaskInteract;
	float interactTestInterval;
	float interactTestTimer;

//	int collisionLayerMaskForProps;

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
		Cursor.lockState = CursorLockMode.Locked;
		layermaskInteract = LayerMaskUtils.CreateMask("InteractCast");
		interactTestInterval = 1f / interactTestsPerSecond;
		interactTestTimer = 0f;

	}
	
	public void ExecuteUpdate (ViewInput viewInput) {
		Look(viewInput.lookInput);
		if(viewInput.fireInput){
			Cursor.lockState = CursorLockMode.Locked;
		}
		if(viewInput.interactInput){
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

	public void ExecuteFixedUpdate () {
		MatchRBRotationToHead();
		//TODO check for wish to drop grabbed object and then drop/throw it
	}

	void Look (Vector2 lookInput) {
		pan = Mathf.Repeat(pan + lookInput.x, 360);
		tilt = Mathf.Clamp(tilt + lookInput.y, -90, 90);
		float deltaPan = Mathf.Repeat(pan - bodyPan, 360);
		head.transform.localRotation = Quaternion.identity;
		head.transform.Rotate(Vector3.up, deltaPan);
		head.transform.Rotate(Vector3.left, tilt);
	}

	void MatchRBRotationToHead () {
		Vector3 headFwd = head.transform.forward;
		Vector3 headUp = head.transform.up;
		float lerpFactor = tilt / 90f;
		float absLerpFactor = Mathf.Abs(lerpFactor);
		Vector3 bodyFwd = ((1f - absLerpFactor) * headFwd) - (lerpFactor * headUp);		//it cycles around in a weird way. maybe i can find a way to calculate the vector directly
		bodyFwd = Vector3.ProjectOnPlane(bodyFwd, rb.transform.up);
		rb.transform.rotation = Quaternion.LookRotation(bodyFwd, rb.transform.up);
		head.transform.rotation = Quaternion.LookRotation(headFwd, headUp);
		bodyPan = pan;
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

	//deprecated but i keep it around because it's fun to compare it to the new one :)
//	void Look (Vector2 lookInput) {
//		pan = Mathf.Repeat(pan + lookInput.x, 360);
//		tilt = Mathf.Clamp(tilt + lookInput.y, -90, 90);
//		float rPan = Mathf.Deg2Rad * pan;
//		float rTilt = Mathf.Deg2Rad * tilt;
//		float sinPan = Mathf.Sin(rPan);
//		float cosPan = Mathf.Cos(rPan);
//		float sinTilt = Mathf.Sin(rTilt);
//		float cosTilt = Mathf.Cos(rTilt);
//		Vector3 fwdHorizontal = new Vector3(sinPan, 0f, cosPan) * cosTilt;
//		Vector3 fwdVertical = new Vector3(0f, sinTilt, 0f);
//		Vector3 forward = fwdHorizontal + fwdVertical;
//		Vector3 upHorizontal = new Vector3(sinPan, 0f, cosPan) * sinTilt * -1f;
//		Vector3 upVertical = new Vector3(0f, cosTilt, 0f);
//		Vector3 up = upHorizontal + upVertical;
//		head.transform.rotation = Quaternion.LookRotation(forward, up);	
//	}

}
