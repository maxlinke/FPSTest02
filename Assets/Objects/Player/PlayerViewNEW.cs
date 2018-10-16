using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//monobehavior remains because of collisions
public class PlayerViewNEW : MonoBehaviour {

	struct RigidbodySettings {

		public readonly float mass;
		public readonly bool useGravity;
		public readonly CollisionDetectionMode collisionDetectionMode;
		public readonly RigidbodyInterpolation interpolation;
		public readonly float drag;
		public readonly float angularDrag;

		public RigidbodySettings (Rigidbody rb) {
			this.mass = rb.mass;
			this.useGravity = rb.useGravity;
			this.collisionDetectionMode = rb.collisionDetectionMode;
			this.interpolation = rb.interpolation;
			this.drag = rb.drag;
			this.angularDrag = rb.angularDrag;
		}

	}

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

	[Header("Interacting")]
	[SerializeField] float interactRange = 3f;
	[SerializeField] float interactTestsPerSecond = 10f;

	[Header("Grabbing")]
	[SerializeField] float grabbedObjectMaxMass = 20f;
	[SerializeField] float grabbedObjectMaxDistance = 2f;		//as in the distance it's held at
	[SerializeField] float grabbedObjectMaxThrowForce = 6000f;
	[SerializeField] float grabbedObjectMaxThrowTorque = 100f;
	[SerializeField] float grabbedObjectDistanceFromIntendedPositionToDrop = 1f;
	[SerializeField] float grabbedObjectDefaultAngularDrag = 5f;

	Player player;
	GameObject head;
	Rigidbody rb;
	Camera cam;
	IGUI gui;

	float tilt;

	float interactTestInterval;
	float interactTestTimer;

	int layermaskInteract;
	int collisionLayerMaskForProps;

	Rigidbody grabbedRB;
	RigidbodySettings grabbedRBSettings;
	int smoothingIndex;

	public bool isHoldingOntoSomething {
		get { return (grabbedRB != null); }
	}

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
		collisionLayerMaskForProps = LayerMaskUtils.CreateMask("Prop");
		collisionLayerMaskForProps &= ~LayerMask.GetMask("Water");
	}
	
	public void ExecuteUpdate (ViewInput viewInput) {
		Look(viewInput.lookInput);

		interactTestTimer += Time.deltaTime;
		if(interactTestTimer > interactTestInterval){
			UpdateInteractInfo();
		}
		interactTestTimer = interactTestTimer % interactTestInterval;

		if(viewInput.fireInput){
			Cursor.lockState = CursorLockMode.Locked;
			if(grabbedRB != null){
				DropObject(ref grabbedRB, grabbedRBSettings, true);
				//TODO unholster
			}
		}
		if(viewInput.interactInput){
			if(grabbedRB != null){
				DropObject(ref grabbedRB, grabbedRBSettings, false);
				//TODO unholster
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
	}

	//collisions

	void OnCollisionEnter (Collision collision) {
		if(grabbedRB != null){
			for(int i=0; i<collision.contacts.Length; i++){
				if(collision.contacts[i].otherCollider.attachedRigidbody == grabbedRB){
					DropObject(ref grabbedRB, grabbedRBSettings, false);
					break;
				}
			}
		}
	}

	//regular methods

	void Look (Vector2 lookInput) {
		float oldTilt = tilt;
		tilt = Mathf.Clamp(tilt + lookInput.y, -90, 90);
		float deltaTilt = tilt - oldTilt;
		float deltaPan = lookInput.x;
		head.transform.RotateAround(head.transform.position, rb.transform.up, deltaPan);
		head.transform.Rotate(Vector3.left, deltaTilt);
	}

	void MatchRBRotationToHead () {
		Vector3 headFwd = head.transform.forward;
		Vector3 headUp = head.transform.up;
		float lerpFactor = tilt / 90f;
		float absLerpFactor = Mathf.Abs(lerpFactor);
		Vector3 bodyFwd = ((1f - absLerpFactor) * headFwd) - (lerpFactor * headUp);
		bodyFwd = Vector3.ProjectOnPlane(bodyFwd, rb.transform.up);
		rb.transform.rotation = Quaternion.LookRotation(bodyFwd, rb.transform.up);
		head.transform.localEulerAngles = new Vector3(-tilt, 0f, 0f);
	}

	public void ManageGrabbedObject () {
		Vector3 intendedPosition = head.transform.position + (head.transform.forward * grabbedObjectMaxDistance);
		//TODO the whole angle shenanigans
		if((intendedPosition - grabbedRB.transform.position).magnitude > grabbedObjectDistanceFromIntendedPositionToDrop){
			DropObject(ref grabbedRB, grabbedRBSettings, false);
			//TODO unholster
			return;
		}
		Vector3 start = grabbedRB.transform.position;
		Vector3 deltaPos = intendedPosition - grabbedRB.transform.position;
		grabbedRB.gameObject.SetActive(false);
		RaycastHit hit;
		Vector3 properPosition;
		if(Physics.Raycast(start, deltaPos, out hit, deltaPos.magnitude, collisionLayerMaskForProps)){
			properPosition = hit.point;
		}else{
			properPosition = intendedPosition;
		}
		grabbedRB.gameObject.SetActive(true);
		grabbedRB.velocity = rb.velocity + ((properPosition - grabbedRB.transform.position) / Time.fixedDeltaTime);
	}

	void UpdateInteractInfo () {
		IInteractable interactableObject;
		Rigidbody grabbableRigidbody;
		InteractCast(out interactableObject, out grabbableRigidbody);
		if(interactableObject != null){
			string description = interactableObject.GetDescription();
			Debug.Log(description);
			//enable display + show description
		}else if(grabbableRigidbody != null){
			Debug.Log("pick up");
			//enable display + show "Pick up"
		}else{
			//disable the display / display nothing. whatever floats your boat, man
		}
	}

	//TODO maybe use an enum to give a reason why interaction didn't work? too heavy, nothing there etc
	void TryToInteract (out bool successful) {
		IInteractable interactableObject;
		Rigidbody grabbableRigidbody;
		InteractCast(out interactableObject, out grabbableRigidbody);
		if(interactableObject != null){
			interactableObject.Interact(this.player.gameObject);
			successful = true;
		}else if(grabbableRigidbody != null){
			//TODO "holster" weapons. and if fire is pressed, make sure the weapon isn't fired
			//... just because it gets "unholstered" here and then the weapon script gets the fire input
			PickupObject(grabbableRigidbody, out grabbedRB, out grabbedRBSettings);
			successful = true;
		}else{
			successful = false;
		}
	}

	void PickupObject (Rigidbody grabbableRigidbody, out Rigidbody grabbedRB, out RigidbodySettings grabbedRBSettings) {
		grabbedRBSettings = new RigidbodySettings(grabbableRigidbody);
		grabbableRigidbody.mass = 1f;	//HACK find something proper to limit the velocity...
		grabbableRigidbody.useGravity = false;
		grabbableRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		grabbableRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		grabbableRigidbody.drag = 0f;
		grabbableRigidbody.angularDrag = grabbedObjectDefaultAngularDrag;
		grabbedRB = grabbableRigidbody;
		grabbedRB.MovePosition(head.transform.position + (head.transform.forward * grabbedObjectMaxDistance));
	}

	void FillArrays (ref Vector3[] positions, ref Vector3[] rotations, Rigidbody otherRB) {
		for(int i=0; i<positions.Length; i++){
			positions[i] = otherRB.transform.position;
			rotations[i] = otherRB.transform.eulerAngles;
		}
	}

	void DropObject (ref Rigidbody otherRB, RigidbodySettings settings, bool shouldThrow) {
		SetRigidbodySettings(otherRB, settings);
		if(shouldThrow){
			float accel = GetThrowAcceleration(otherRB.mass);
			float torque = Random.value * grabbedObjectMaxThrowTorque;
			otherRB.AddForce(head.transform.forward * accel, ForceMode.Acceleration);
			otherRB.AddTorque(Random.insideUnitSphere * torque);
		}
		otherRB = null;
	}

	void SetRigidbodySettings (Rigidbody otherRB, RigidbodySettings settings) {
		otherRB.mass = settings.mass;
		otherRB.useGravity = settings.useGravity;
		otherRB.collisionDetectionMode = settings.collisionDetectionMode;
		otherRB.interpolation = settings.interpolation;
		otherRB.drag = settings.drag;
		otherRB.angularDrag = settings.angularDrag;
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
			bool massOK = (otherRB.mass <= grabbedObjectMaxMass);
			bool taggedNonGrabbable = TagManager.CompareTag("NonGrabbable", otherRB.gameObject);
			return (!kinematic && massOK && !taggedNonGrabbable);
		}else{
			return false;
		}
	}

	float GetThrowAcceleration (float otherMass) {
		if(otherMass <= 1f){
			return grabbedObjectMaxThrowForce;
		}else{
			float lerpFactorLinear = Mathf.Clamp01((otherMass - 1f) / (grabbedObjectMaxMass - 1f));
			return Mathf.Lerp(grabbedObjectMaxThrowForce, grabbedObjectMaxThrowForce/10f, Mathf.Pow(lerpFactorLinear, 0.125f));
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
