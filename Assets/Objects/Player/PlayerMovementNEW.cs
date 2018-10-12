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

		public SurfacePoint (ContactPoint contactPoint, Vector3 up) {
			this.point = contactPoint.point;
			this.normal = contactPoint.normal;
			this.otherCollider = contactPoint.otherCollider;
			this.angle = Vector3.Angle(contactPoint.normal, up);
			this.originalContactPoint = contactPoint;
			this.isSynthetic = false;
		}

		public SurfacePoint (Vector3 point, Vector3 normal, Collider otherCollider, Vector3 up) {
			this.point = point;
			this.normal = normal;
			this.otherCollider = otherCollider;
			this.angle = Vector3.Angle(normal, up);
			this.originalContactPoint = new ContactPoint();
			this.isSynthetic = true;
		}

		public override string ToString () {
			string output = "";
			output += "point : " + point.ToString() + "\n";
			output += "normal : " + normal.ToString() + "\n";
			output += "otherCollider : " + otherCollider.name + "\n";
			output += "isSynthetic : " + isSynthetic;
			return output;
		}

	}

	struct StateData {	//TODO also include surfacepoint and wascrouching/sprinting? i mean why not?

		public bool onGround;
		public bool onValidGround;
		public bool onSolidGround;
		public bool onLadder;
		public bool inWater;		//as in can swim, not just the feet in water
		public bool jumped;

		public SurfacePoint surfacePoint;
		public SurfacePoint ladderPoint;	//i very much dislike this being here PERMANENTLY but it is directly linked to the onLadder bool...

		public Vector3 incomingVelocity;
		public Vector3 outgoingVelocity;

		public Vector3 incomingOwnVelocity;
		public Vector2 outgoingOwnVelocity;

		public Vector3 rawInput;

		public bool velocityComesFromMove;

		public override string ToString () {
			string output = "";
			output += "onGround : " + onGround + "\n";
			output += "onValidGround : " + onValidGround + "\n";
			output += "onSolidGround : " + onSolidGround + "\n";
			output += "onLadder : " + onLadder + "\n";
			output += "inWater : " + inWater + "\n";
			output += "velocityFromMove : " + velocityComesFromMove;
			return output;
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
	[SerializeField] float moveSlopeSlideStartAngle = 45f;
	[SerializeField] float moveSlideControl = 12f;
	[SerializeField] float moveAirControl = 4f;
	[SerializeField] float moveAirAutoDecelSpeed = 10f;
	[SerializeField] float moveAirAutoDeceleration = 5f;

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

	float jumpSpeed;

	DoubleKey keyMoveForward;
	DoubleKey keyMoveBackward;
	DoubleKey keyMoveLeft;
	DoubleKey keyMoveRight;

	DoubleKey keyJump;
	DoubleKey keyCrouchToggle;
	DoubleKey keySprintToggle;
	DoubleKey keyCrouchHold;
	DoubleKey keySprintHold;

	bool isSprinting;
	bool wasSprinting;

	List<ContactPoint> contactPoints;
	StateData lastState;
	bool canSwim;

	Vector3 DEBUG_lastPos;

	bool isCrouching{
		get{
			return col.height < ((normalHeight + crouchHeight) / 2f);
		}
	}

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
		jumpSpeed = Mathf.Sqrt(2f * normalGravity * moveJumpHeight);
	}
	
	void Update () {
//		CrouchManager();
		if(Input.GetKeyDown(KeyCode.Mouse1)) rb.velocity += head.transform.forward * 50f;
	}

	void FixedUpdate () {
		StateData currentState;
		List<ContactPoint> wallPoints;
		ManageCollisions(contactPoints, out currentState, out wallPoints);
		MovementType movementType = GetMovementType(currentState);
//		bool gotAcceleratedInbetween = (currentState.incomingVelocity.sqrMagnitude > lastState.outgoingVelocity.sqrMagnitude);
		//debug stuff
//		Debug.Log(movementType.ToString());
		//TODO i guess set justJumped to false (here) before movement begins 
		//and do the sticking in the collision managing or after that
		//and if sticking is done, ref the surfacepoint and overwrite it with a "synthetic" one
		Vector3 ownVelocity, acceleration, gravity;
		switch(movementType){
		case MovementType.GROUNDED : 
			GroundedMovement(ref currentState, out acceleration, out gravity);
			break;
		case MovementType.AIRBORNE : 
			AirborneMovement(ref currentState, out acceleration, out gravity);
			break;
		case MovementType.SWIMMING : 
			acceleration = Vector3.zero;
			gravity = Physics.gravity;
			break;
		case MovementType.LADDER : 
			acceleration = Vector3.zero;
			gravity = Physics.gravity;
			break;
		default : 
			throw new UnityException("Unsupported MovementType \"" + movementType.ToString() + "\"");
		}

		rb.velocity += (acceleration + gravity) * Time.fixedDeltaTime;
		currentState.outgoingVelocity = rb.velocity;

		Debug.DrawLine(DEBUG_lastPos, rb.transform.position, (currentState.onGround ? Color.green : Color.red), 10f);
		Debug.DrawRay(rb.transform.position, Vector3.up * 0.1f, Color.white, 10f);
		DEBUG_lastPos = rb.transform.position;
		//save and/or reset fields
		lastState = currentState;
		contactPoints.Clear();
		canSwim = false;	//needs to be reset as it is done in triggerstay
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
				canSwim = true;
			}
		}
	}

	void OnTriggerExit (Collider otherCollider) {

	}

	//pre movement

	void ManageCollisions (List<ContactPoint> contactPoints, out StateData currentState, out List<ContactPoint> wallPoints) {
		RemoveInvalidContactPoints(ref contactPoints);
		SurfacePoint surfacePoint = GetSurfacePoint(contactPoints);
		List<ContactPoint> stepPoints;
		DetermineWallAndStepPoints(contactPoints, surfacePoint, out wallPoints, out stepPoints);
		currentState = GetStateData(surfacePoint, wallPoints, lastState);
//		StepUpSteps(ref currentState, lastState, stepPoints);	//TODO it's fucky, so it's commented out.
		StickToGroundIfNecessary(ref currentState, lastState);
		ManageFallDamage(currentState, lastState);
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
			Vector3 delta = point.point - rb.transform.position;
			float pointOffset = (delta - Horizontalized(delta)).magnitude;
			float pointAngle = Vector3.Angle(point.normal, rb.transform.up);
//			float pointOffset = col.radius * (1f - point.normal.y);		//dot(normal, up) == normal.y
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

	StateData GetStateData (SurfacePoint surfacePoint, List<ContactPoint> wallPoints, StateData lastState) {
		StateData currentState = new StateData();
		currentState.surfacePoint = surfacePoint;
		currentState.incomingVelocity = rb.velocity;
		currentState.incomingOwnVelocity = GetRelativeVelocity(rb.velocity, surfacePoint);
		float currentOwnSpeed = currentState.incomingVelocity.magnitude;
		float lastOwnSpeed = lastState.incomingVelocity.magnitude;
		currentState.velocityComesFromMove = (lastState.velocityComesFromMove && (currentOwnSpeed < lastOwnSpeed));
		currentState.rawInput = GetInputVector();

		currentState.onGround = GetIsGrounded(surfacePoint, lastState);
		if(currentState.onGround){
			currentState.onValidGround = GetIsOnValidGround(surfacePoint);
			currentState.onSolidGround = GetIsOnSolidGround(surfacePoint);
		}
		currentState.onLadder = GetIsOnLadder(wallPoints, out currentState.ladderPoint);
		currentState.inWater = canSwim;
		return currentState;
	}

	void StepUpSteps (ref StateData currentState, StateData lastState, List<ContactPoint> stepPoints) {
		if(!currentState.onGround) return;
		if(!currentState.velocityComesFromMove) return;
		if(currentState.rawInput.magnitude > 0.5f){	//TODO again with the weird condition for "gotInput" but it works..
			SurfacePoint surfacePoint = currentState.surfacePoint;
			for(int i=0; i<stepPoints.Count; i++){
				ContactPoint point = stepPoints[i];
				Vector3 delta = point.point - surfacePoint.point;
				float deltaHeight = (delta - Horizontalized(delta)).magnitude;
				bool pointIsAbove = deltaHeight > 0.01f;	//TODO hardcoded values, yum...
				bool colliderStatic = ColliderIsStatic(point.otherCollider);
				Vector3 flattenedNormal = Horizontalized(point.normal).normalized;
				Vector3 flattenedVelocity = Horizontalized(currentState.incomingOwnVelocity).normalized;
				float directionDot = Vector3.Dot(flattenedNormal, flattenedVelocity);
				if(pointIsAbove && colliderStatic && (directionDot < -0.5f)){
					Vector3 surfPointToStepPoint = point.point - surfacePoint.point;
					Vector3 projectedVector = Vector3.ProjectOnPlane(surfPointToStepPoint, surfacePoint.normal);
					Vector3 projectedPoint = surfacePoint.point + projectedVector;
					float distance = Vector3.Distance(projectedPoint, point.point);
					if(distance > 0.05f){	//if the point is at least 5cm out of the current move plane (make sure it's not the top of a ramp or something)
						Vector3 start = point.point + (point.normal * col.radius) + (rb.transform.up * 0.01f);
						Vector3 dir = -point.normal;
						float dist = col.radius + 0.1f;
						int mask = LayerMaskUtils.CreateMask(rb.gameObject.layer);	//TODO proper mask
						RaycastHit hit;
						Debug.DrawRay(start, dir.normalized * dist, Color.cyan, 10f);
						if(Physics.Raycast(start, dir, out hit, dist, mask)){
							float angleDifference = Vector3.Angle(point.normal, hit.normal);
							if(angleDifference > 5f){
								//TODO the following is nearly identical with ground sticking. maybe put it in an extra method
								Vector3 properPosition = hit.point + (hit.normal * (col.radius + 0.05f)) - (rb.transform.up * col.radius);
								//debug
								Debug.DrawLine(rb.transform.position, properPosition, Color.yellow, 10f);
								Debug.LogError("STEPPING");
//								rb.MovePosition(properPosition);
								rb.transform.position = properPosition;
								rb.velocity = lastState.incomingVelocity - (hit.normal * Physics.gravity.magnitude * Time.fixedDeltaTime);
								//overwrite state data with new info
								SurfacePoint newSurfacePoint = new SurfacePoint(hit.point, hit.normal, hit.collider, rb.transform.up);
								currentState = GetStateData(newSurfacePoint, new List<ContactPoint>(), lastState);
							}
						}
					}
				}
			}
		}
	}

	void StickToGroundIfNecessary (ref StateData currentState, StateData lastState) {
		if(currentState.onGround) return;
		if(lastState.jumped) return;
		if(!lastState.onGround) return;
		if(!lastState.onSolidGround) return;
		if(!currentState.velocityComesFromMove) return;
		Vector3 start = rb.transform.position + (rb.transform.up * col.radius);
//		Vector3 dir = -lastState.surfacePoint.normal;
		Vector3 dir = (lastState.surfacePoint.normal + rb.transform.up).normalized * -1f;	//this is smoother... less jerky...
		float dist = col.radius + (2.5f * moveSpeedRegular * Time.fixedDeltaTime);
		int mask = LayerMaskUtils.CreateMask(rb.gameObject.layer);	//TODO proper mask
		RaycastHit hit;
		Debug.DrawRay(start, dir.normalized * dist, Color.magenta, 10f);
		if(Physics.Raycast(start, dir, out hit, dist, mask)){	//TODO proper layermask that doesn't COLLIDE with water
			bool hitAngleOkay = (Vector3.Angle(hit.normal, rb.transform.up) <= moveMaxSlopeAngle);
			bool angleBetweenVelocityAndHitOkay = (Vector3.Angle(currentState.incomingOwnVelocity, hit.normal) < 90f);
			bool colliderSolid = ColliderIsSolid(hit.collider);
			if(hitAngleOkay && angleBetweenVelocityAndHitOkay && colliderSolid){
				Vector3 properPosition = hit.point + (hit.normal * (col.radius + 0.05f)) - (rb.transform.up * col.radius);	//TODO why the 0.05f?
				rb.MovePosition(properPosition);
				rb.velocity = Vector3.ProjectOnPlane(currentState.incomingVelocity, hit.normal) - (hit.normal * Physics.gravity.magnitude * Time.fixedDeltaTime);
				//debug
				Debug.DrawLine(rb.transform.position, properPosition, Color.yellow, 10f);
				Debug.LogError("HIT! STICKING!");
				//overwrite state data with new info
				SurfacePoint newSurfacePoint = new SurfacePoint(hit.point, hit.normal, hit.collider, rb.transform.up);
				currentState = GetStateData(newSurfacePoint, new List<ContactPoint>(), lastState);
			}else{
				string message = "HIT, NO TELEPORT!\n";
				message += "hitAngleOkay : " + hitAngleOkay + "\n";
				message += "angleBetweenVelocityAndHitOkay : " + angleBetweenVelocityAndHitOkay + "\n";
				message += "colliderSolid : " + colliderSolid;
				if(!colliderSolid) message += " (" + hit.collider.name + ")";
				Debug.LogError(message);
			}
		}else{
			Debug.LogError("MISS!");
		}
	}

	void ManageFallDamage (StateData currentState, StateData lastState) {
		if(!lastState.onGround){	//maybe just saying onground is a bad idea but it's videogamey...
			if(currentState.onValidGround && !currentState.onLadder){
//				healthSystem.NotifyOfLanding(lastVelocity, rb.velocity);
				Debug.LogWarning("TODO, healthsystem and stuff");
			}
		}
	}

	MovementType GetMovementType (StateData currentState) {
		if(currentState.onLadder && !currentState.onValidGround){
			return MovementType.LADDER;
		}else if(currentState.inWater){
			return MovementType.SWIMMING;
		}else if(currentState.onGround){
			return MovementType.GROUNDED;
		}else{
			return MovementType.AIRBORNE;
		}

	}

	//actual movement

	//TODO bring back velocitycomesfrommove. if it doesn't set the friction to zero, otherwise, idk...
	void GroundedMovement (ref StateData currentState, out Vector3 outputAcceleration, out Vector3 outputGravity) {
		outputAcceleration = Vector3.zero;
		SurfacePoint surfacePoint = currentState.surfacePoint;
		Vector3 currentVelocity = currentState.incomingOwnVelocity;
		float currentSpeed = currentVelocity.magnitude;
		Vector3 inputVector = rb.transform.TransformDirection(currentState.rawInput);
		//slope ok
		if(currentState.onValidGround){
			inputVector = GetSurfaceMoveVector(inputVector, surfacePoint.normal);
			//regular accel/decel
			if(currentSpeed < moveSpeedSprint){
//				Vector3 desiredVelocity = inputVector * GetHeightAppropriateSpeed();	//TODO and crouch and sprint..
				Vector3 desiredVelocity = inputVector * GetDesiredSpeed(currentState);
				outputAcceleration += ClampedAcceleration(currentVelocity, desiredVelocity, moveAcceleration);
			}//redirecting
			else{
				Vector3 desiredVelocity = Vector3.Lerp(rb.transform.forward, inputVector, inputVector.magnitude) * currentSpeed;
				outputAcceleration += ClampedAcceleration(currentVelocity, desiredVelocity, moveAcceleration);
			}
			if(keyJump.GetKey()){
				//TODO make it possible for gravity to go sideways
				//so horizontalized should stay in local space
				//and jumpvelocity should go along rb.transform.up (or just rb.transform.TransformDirection(...))
				//and all the surface data should work with rb.transform.up...
				outputAcceleration = Horizontalized(outputAcceleration) + (rb.transform.up * jumpSpeed / Time.fixedDeltaTime) - Verticalized(currentVelocity / Time.fixedDeltaTime);
				currentState.jumped = true;
			}
		}
		//slope to steep
		else{

		}
		if((currentVelocity + (outputAcceleration * Time.fixedDeltaTime)).magnitude < moveSpeedSprint) currentState.velocityComesFromMove = true;
		float lerpFactor = Mathf.Clamp01((surfacePoint.angle - moveSlopeSlideStartAngle) / (moveMaxSlopeAngle - moveSlopeSlideStartAngle));
		outputGravity = Vector3.Lerp(-surfacePoint.normal, Vector3.down, lerpFactor) * Physics.gravity.magnitude;
	}

	void AirborneMovement (ref StateData currentState, out Vector3 outputAcceleration, out Vector3 outputGravity) {
		Vector3 currentVelocity = currentState.incomingOwnVelocity;
		Vector3 inputVector = rb.transform.TransformDirection(currentState.rawInput);
		float currentGroundSpeed = Horizontalized(currentVelocity).magnitude;
		float desiredGroundSpeed = GetDesiredSpeed(currentState);
		Vector3 desiredVector = inputVector * desiredGroundSpeed;
		if(inputVector.magnitude > 0.5f){		//TODO this is a weird condition but should work for now
			float speedModifier = Mathf.Clamp(Mathf.Sqrt(currentGroundSpeed / desiredGroundSpeed), 1f, Mathf.Infinity);
			Vector3 tempAccel = desiredVector * speedModifier * moveAirControl ;
			Vector3 tempVelocity = Horizontalized(currentVelocity + (tempAccel * Time.fixedDeltaTime));
			float resultGroundSpeed = tempVelocity.magnitude;
			if((resultGroundSpeed > currentGroundSpeed) && (currentGroundSpeed > desiredGroundSpeed)){
				Vector3 deltaV = (tempVelocity.normalized * currentGroundSpeed) - Horizontalized(currentVelocity);
				tempAccel = deltaV / Time.fixedDeltaTime;
			}
			outputAcceleration = tempAccel;
		}else{
			if(currentGroundSpeed < moveAirAutoDecelSpeed){
				outputAcceleration = ClampedAcceleration(Horizontalized(currentVelocity), desiredVector, moveAirAutoDeceleration);
			}else{
				outputAcceleration = Vector3.zero;
			}
		}
		outputGravity = Physics.gravity;
	}

	//managers

	void CrouchManager (ref bool isCrouching) {
		bool crouchWish = isCrouching;
		if(keyCrouchHold.GetKey()) crouchWish = true;
		if(keyCrouchHold.GetKeyUp()) crouchWish = false;
		if(keyCrouchToggle.GetKeyDown()){
			if(!isCrouching) crouchWish = true;
			else crouchWish = false;
		}
		isCrouching = crouchWish;
		//TODO can't do with the isCrouching bool that is only get (based on collider height)
		//i somehow need to pass out the result and modify the height with time
		//crouch/uncrouch TIME synced with ANIMATIONS or animator.setfloat to be between standing and crouching
	}

	//TODO this is THUS FAR the same as crouchmanager
	//i can obviously do this in one method, but what would i name it?
	void SprintManager (ref bool isSprinting) {
		bool sprintWish = isSprinting;
		if(keySprintHold.GetKey()) sprintWish = true;
		if(keySprintHold.GetKeyUp()) sprintWish = false;
		if(keySprintToggle.GetKeyDown()){
			if(!isSprinting) sprintWish = true;
			else sprintWish = false;
		}
		isSprinting = sprintWish;
	}

	//utility

	Vector3 ProjectOnPlaneAlongVector (Vector3 input, Vector3 normal, Vector3 projectVector) {
		float x = Vector3.Dot(normal, input) / Vector3.Dot(normal, projectVector);
		return (input - (x * projectVector));
	}

	Vector3 Horizontalized (Vector3 vector) {
		return Vector3.ProjectOnPlane(vector, rb.transform.up);
	}

	Vector3 Verticalized (Vector3 vector) {
		return Vector3.Project(vector, rb.transform.up);
	}

	Vector3 ClampedAcceleration (Vector3 currentVelocity, Vector3 targetVelocity, float maxAccel) {
		Vector3 deltaV = targetVelocity - currentVelocity;
		Vector3 deltaVAccel = deltaV / Time.fixedDeltaTime;
		if(deltaVAccel.magnitude > maxAccel){
			return deltaV.normalized * maxAccel;
		}else{
			return deltaVAccel;
		}
	}

	Vector3 GetInputVector () {
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

	float GetDesiredSpeed (StateData currentState) {
		//TODO never return 0. always between the slowest move speed and the fastest... 
		//with respect to crouching and sprinting.
//		return GetHeightAppropriateSpeed();
		return moveSpeedRegular;
	}

	float GetHeightAppropriateSpeed () {
		float lerpFactor = (col.height - crouchHeight) / (normalHeight - crouchHeight);
		return Mathf.Lerp(moveSpeedCrouch, moveSpeedRegular, lerpFactor);
	}

	SurfacePoint GetSurfacePoint (List<ContactPoint> contactPoints) {
		ContactPoint bestPoint = new ContactPoint();	//it's a struct so no null...
		float biggestDot = Mathf.NegativeInfinity;	
		for(int i=0; i<contactPoints.Count; i++){
			ContactPoint point = contactPoints[i];
//			float newY = point.normal.y;
//			if((newY > biggestY) && (newY > 0f)){
//				biggestY = newY;
//				bestPoint = point;
//			}
			float newDot = Vector3.Dot(point.normal, rb.transform.up);
			if((newDot > biggestDot) && (newDot > 0f)){
				biggestDot = newDot;
				bestPoint = point;
			}
		}
		if(bestPoint.otherCollider != null){
			return new SurfacePoint(bestPoint, rb.transform.up);
		}else{
			return null;
		}
	}

	Vector3 GetSurfaceMoveVector (Vector3 inputVector, Vector3 inputNormal) {
//		Vector3 normal = inputNormal.normalized;
//		float ix = inputVector.x;
//		float iz = inputVector.z;
//		float nx = normal.x;
//		float ny = normal.y;
//		float nz = normal.z;
//		float dy = -((ix * nx) + (iz * nz)) / ny;
//		Vector3 output = new Vector3(ix, dy, iz).normalized * inputVector.magnitude;
//		return output;
		return ProjectOnPlaneAlongVector(inputVector, inputNormal, rb.transform.up).normalized * inputVector.magnitude;
	}

	//TODO maybe you are jumping in a train car or something... ? (big trigger "relative air" or something)
	Vector3 GetRelativeVelocity (Vector3 totalVelocity, SurfacePoint surfacePoint) {
		Vector3 otherVelocity;
		if(surfacePoint == null){
			otherVelocity = Vector3.zero;
		}else if(surfacePoint.otherCollider == null){
			otherVelocity = Vector3.zero;
		}else if(surfacePoint.otherCollider.attachedRigidbody == null){
			otherVelocity = Vector3.zero;
		}else{
			otherVelocity = surfacePoint.otherCollider.attachedRigidbody.velocity;
		}
		return totalVelocity - otherVelocity;
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

	bool GetIsGrounded (SurfacePoint surfacePoint, StateData lastState) {
		if(surfacePoint == null) return false;
		if(lastState.jumped) return false;
		if(surfacePoint.angle > 89f) return false;		//if this is not in, some walls (straight up) count as ground...
		return true;
	}

	bool GetIsOnValidGround (SurfacePoint surfacePoint) {
		return (surfacePoint.angle <= moveMaxSlopeAngle);
	}

	bool GetIsOnSolidGround (SurfacePoint surfacePoint) {
		if(surfacePoint.otherCollider.attachedRigidbody == null) return true;
		if(surfacePoint.otherCollider.attachedRigidbody.isKinematic) return true;
		return false;
	}

	bool GetIsOnLadder (List<ContactPoint> wallPoints, out SurfacePoint ladderPoint) {
		for(int i=0; i<wallPoints.Count; i++){
			ContactPoint point = wallPoints[i];
			if(TagManager.CompareTag("Ladder", point.otherCollider.gameObject)){
				float ladderAngle = Vector3.Angle(point.normal, rb.transform.up);
				if(ladderAngle < 91f){
					ladderPoint = new SurfacePoint(point, rb.transform.up);
					return true;
				}
			}
		}
		ladderPoint = null;
		return false;
	}

	bool CanUncrouch () {
		if(!isCrouching){
			return false;
		}else{
			Vector3 rayStart = transform.position + (rb.transform.up * col.height / 2f);
			Vector3 rayDir = rb.transform.up;
			float rayLength = normalHeight - (col.height / 2f);
			int completePlayerCollisionMask = LayerMaskUtils.CreateMask("Player");
			int onlyWaterMask = LayerMask.GetMask("Water");
			int crouchcastLayermask = completePlayerCollisionMask & (~onlyWaterMask);
			string logString = "";
			logString += LayerMaskUtils.MaskToBinaryString(completePlayerCollisionMask) + "\n";
			logString += LayerMaskUtils.MaskToBinaryString(onlyWaterMask) + "\n";
			logString += LayerMaskUtils.MaskToBinaryString(crouchcastLayermask);
			Debug.LogWarning(logString);
			RaycastHit hit;
			if(Physics.Raycast(rayStart, rayDir, out hit, rayLength, crouchcastLayermask)){	
				//TODO the raycast currently also hits water
				//hitting water is no reason to not uncrouch
				//hitting water is also no way to say it's okay to uncrouch
				//i could either make a new layer to csat
				//or i could recast the ray
				//OOOOOOOOORRRRRRRRRR
				//i just AND (&) out the water layer from the layermask
				//TODO layermaskutils for the cast-masks. no need to waste precious collision layers :P
				return false;
			}else{
				return true;
			}
		}
	}

}
