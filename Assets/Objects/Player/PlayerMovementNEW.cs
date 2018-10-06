using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementNEW : MonoBehaviour, IPlayerPrefObserver, IPlayerPrefKeybindObserver {

	enum MovementType {
		GROUNDED,
		AIRBORNE,
		SWIMMING,
		LADDER
	}

	class SurfacePoint {

		public readonly Vector3 point;
		public readonly Vector3 normal;
		public readonly Collider otherCollider;
		public readonly float angle;
		public readonly ContactPoint originalContactPoint;
		public readonly bool isSynthetic;		//is the "originalContactPoint" an actual contact point or just a new ("empty") struct?

		public SurfacePoint (ContactPoint contactPoint) {
			this.point = contactPoint.point;
			this.normal = contactPoint.normal;
			this.otherCollider = contactPoint.otherCollider;
			this.angle = Vector3.Angle(contactPoint.normal, Vector3.up);
			this.originalContactPoint = contactPoint;
			this.isSynthetic = false;
		}

		public SurfacePoint (Vector3 point, Vector3 normal, Collider otherCollider) {
			this.point = point;
			this.normal = normal;
			this.otherCollider = otherCollider;
			this.angle = Vector3.Angle(normal, Vector3.up);
			this.originalContactPoint = new ContactPoint();
			this.isSynthetic = true;
		}

	}

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

	List<ContactPoint> contactPoints;
	bool justJumped;
	bool couldSwim;

	public void Initialize (Rigidbody rb, CapsuleCollider worldCollider, GameObject head, PlayerHealthSystem phs) {
		this.rb = rb;
		this.col = worldCollider;
		this.head = head;
		this.healthSystem = phs;
		AddSelfToPlayerPrefObserverList();
		LoadKeys();
		contactPoints = new List<ContactPoint>();
		pm = col.material;
		col.height = normalHeight;
		col.center = new Vector3(0f, col.height/2f, 0f);
	}
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.Mouse1)) rb.velocity += head.transform.forward * 50f;
	}

	void FixedUpdate () {
		SurfacePoint surfacePoint;
		List<ContactPoint> wallPoints;
		ManageCollisions(contactPoints, out surfacePoint, out wallPoints);
//		if(surfacePoint != null){
//			Debug.DrawRay(surfacePoint.point, surfacePoint.normal, Color.green, 0f, false);
//		}else{
//			Debug.DrawRay(rb.transform.position, Vector3.up, Color.red, 0f, false);
//		}
		Vector3 extraVector;
		MovementType movementType = DetermineMovementType(surfacePoint, wallPoints, out extraVector);
		Debug.Log(movementType.ToString());
		switch(movementType){
		case MovementType.GROUNDED : 
			break;
		case MovementType.AIRBORNE : 
			break;
		case MovementType.SWIMMING : 
			break;
		case MovementType.LADDER : 
			break;
		default : throw new UnityException("Unsupported MovementType \"" + movementType.ToString() + "\"");
		}
		rb.velocity += Physics.gravity * Time.fixedDeltaTime;

		contactPoints.Clear();
		couldSwim = false;	//needs to be reset as it is done in triggerstay
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

	//collisions

	void OnCollisionEnter (Collision collision) {
//		contactPoints.AddRange(collision.contacts);
		FilterAndAddToList(collision.contacts, contactPoints);
	}

	void OnCollisionStay (Collision collision) {
//		contactPoints.AddRange(collision.contacts);
		FilterAndAddToList(collision.contacts, contactPoints);
	}

	//triggers

	void OnTriggerEnter (Collider otherCollider) {
		
	}

	void OnTriggerStay (Collider otherCollider) {
		WaterBody waterBody = otherCollider.gameObject.GetComponent<WaterBody>();
		if(waterBody != null){
			if(head.transform.position.y - waterTriggerOffsetFromEyes < waterBody.waterLevel){
				couldSwim = true;
			}
		}
	}

	void OnTriggerExit (Collider otherCollider) {

	}

	//regular methods

	void ManageCollisions (List<ContactPoint> contactPoints, out SurfacePoint surfacePoint, out List<ContactPoint> wallPoints) {
		RemoveInvalidContactPoints(ref contactPoints);
//		Debug.Log(contactPoints.Count);
		surfacePoint = GetSurfacePoint(contactPoints);
		List<ContactPoint> stepPoints;
		DetermineWallAndStepPoints(contactPoints, surfacePoint, out wallPoints, out stepPoints);
		StepUpSteps(stepPoints, ref surfacePoint);
		//TODO fall damage... now how do i do that now that i dont have wasgrounded etc anymore
	}

	void FilterAndAddToList (ContactPoint[] originalCollisionContacts, List<ContactPoint> contactPoints) {
		for(int i=0; i<originalCollisionContacts.Length; i++){
			ContactPoint point = originalCollisionContacts[i];
			if(point.thisCollider.Equals(col)){		//nullchecking here might not work because the other colliders might only get deleted afterwards
				contactPoints.Add(point);
			}
		}
	}

	void RemoveInvalidContactPoints (ref List<ContactPoint> contactPoints) {
		for(int i=0; i<contactPoints.Count; i++){
			if(contactPoints[i].otherCollider == null){
				contactPoints.RemoveAt(i);
				i--;
			}
		}
	}

	void DetermineWallAndStepPoints (List<ContactPoint> contactPoints, SurfacePoint surfacePoint, out List<ContactPoint> wallPoints, out List<ContactPoint> stepPoints) {
		wallPoints = new List<ContactPoint>();
		stepPoints = new List<ContactPoint>();
		for(int i=0; i<contactPoints.Count; i++){
			ContactPoint point = contactPoints[i];
			float pointAngle = Vector3.Angle(point.normal, Vector3.up);
			float pointOffset = col.radius * (1f - point.normal.y);		//dot(normal, up) == normal.y
			if(pointAngle > moveMaxSlopeAngle){
				wallPoints.Add(point);
			}
			if(pointOffset > 0f && pointOffset <= moveMaxStepOffset){
				if(surfacePoint == null){
					stepPoints.Add(point);
				}else if(!surfacePoint.originalContactPoint.Equals(point)){	//i don't have to worry about the original contact point being null here
					stepPoints.Add(point);
				}
			}
		}
	}

	void StepUpSteps (List<ContactPoint> stepPoints, ref SurfacePoint surfacePoint) {
		//TODO implement this
	}

	MovementType DetermineMovementType (SurfacePoint surfacePoint, List<ContactPoint> wallPoints, out Vector3 extraVector) {
		bool grounded = GetIsGrounded(surfacePoint);
		bool onValidGround = GetIsOnValidGround(surfacePoint);
//		bool onValidGround, onSolidGround;
//		if(grounded){
//			onValidGround = GetIsOnValidGround(surfacePoint);
//			onSolidGround = ColliderIsSolid(surfacePoint.otherCollider);
//		}else{
//			onValidGround = false;
//			onSolidGround = false;
//		}
		Vector3 ladderNormal;
		bool onLadder = (GetIsOnLadder(wallPoints, out ladderNormal) && !onValidGround);
//		bool onLadder = (onValidGround ? false : GetIsOnLadder(wallPoints, out ladderNormal));

		//TODO maybe even the movement could be called here
		//that would save me the "extraVector" and movementType and I could just return the movement vector and gravity...
		//or maybe not because it does seem a bit silly.
		if(onLadder){
			extraVector = ladderNormal;
			return MovementType.LADDER;
		}else if(couldSwim){
			extraVector = Vector3.zero;
			return MovementType.SWIMMING;
		}else if(grounded){
			extraVector = Vector3.zero;
			return MovementType.GROUNDED;
		}else{
			extraVector = Vector3.zero;
			return MovementType.AIRBORNE;
		}

	}

	//utility

	Vector3 GetInputVector () {
		int keyboardZ = (keyMoveForward.GetKey() ? 1 : 0) + (keyMoveBackward.GetKey() ? -1 : 0);
		int keyboardX = (keyMoveLeft.GetKey() ? -1 : 0) + (keyMoveRight.GetKey() ? 1 : 0);
		Vector3 keyboardInput = new Vector3(keyboardX, 0f, keyboardZ).normalized;
		Vector3 controllerInput = Vector3.zero;	//TODO controller input
		Vector3 combined = keyboardInput + controllerInput;
		if(combined.sqrMagnitude > 1f) combined = combined.normalized;
		return combined;
	}

	SurfacePoint GetSurfacePoint (List<ContactPoint> contactPoints) {
		ContactPoint bestPoint = new ContactPoint();	//it's a struct so no null...
		float biggestY = Mathf.NegativeInfinity;	//dot(normal, up) == normal.y and only that matters
		for(int i=0; i<contactPoints.Count; i++){
			ContactPoint point = contactPoints[i];
			float newY = point.normal.y;
			if((newY > biggestY) && (newY > 0f)){
				biggestY = newY;
				bestPoint = point;
			}
		}
		if(bestPoint.otherCollider != null){
			return new SurfacePoint(bestPoint);
		}else{
			return null;
		}
	}

	Vector3 GetSurfaceMoveVector (Vector3 inputVector, Vector3 inputNormal) {
		Vector3 normal = inputNormal.normalized;
		float ix = inputVector.x;
		float iz = inputVector.z;
		float nx = normal.x;
		float ny = normal.y;
		float nz = normal.z;
		float dy = -((ix * nx) + (iz * nz)) / ny;
		Vector3 output = new Vector3(ix, dy, iz).normalized * inputVector.magnitude;
		return output;
	}

	bool ColliderIsSolid (Collider collider) {
		Rigidbody otherRB = collider.attachedRigidbody;
		if(otherRB == null) return true;
		else return (otherRB.isKinematic);
	}

	bool ColliderIsStatic (Collider collider) {
		Rigidbody otherRB = collider.attachedRigidbody;
		if(otherRB == null) return true;
		else return (otherRB.isKinematic && (otherRB.velocity == Vector3.zero));
	}

	bool GetIsGrounded (SurfacePoint surfacePoint) {
		if(surfacePoint == null) return false;
		if(justJumped) return false;
//		if(surfacePoint.angle > 89f) return false;		//TODO leave out or leave in?
		return true;
	}

	bool GetIsOnValidGround (SurfacePoint surfacePoint) {
		if(!GetIsGrounded(surfacePoint)) return false;
		return (surfacePoint.angle <= moveMaxSlopeAngle);
	}

	bool GetIsOnLadder (List<ContactPoint> wallPoints, out Vector3 ladderNormal) {
		for(int i=0; i<wallPoints.Count; i++){
			ContactPoint point = wallPoints[i];
			if(TagManager.CompareTag("Ladder", point.otherCollider.gameObject)){
				float ladderAngle = Vector3.Angle(point.normal, Vector3.up);
				if(ladderAngle < 91f){
					ladderNormal = point.normal;
					return true;
				}
			}
		}
		ladderNormal = Vector3.zero;
		return false;
	}

}
