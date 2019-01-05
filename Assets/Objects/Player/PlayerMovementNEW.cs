using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//even though there is no regular update/fixedupdate monobehavior is still required because of onCollisionEnter etc
public class PlayerMovementNEW : MonoBehaviour {

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
		public bool velocityComesFromMove;

		public SurfacePoint surfacePoint;
		public SurfacePoint ladderPoint;	//i very much dislike this being here PERMANENTLY but it is directly linked to the onLadder bool...

		public Vector3 incomingVelocity;
		public Vector3 outgoingVelocity;
		public Vector3 incomingOwnVelocity;
		public Vector3 outgoingOwnVelocity;

		public MoveInput moveInput;	

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

	public struct MoveInput {

		public readonly Vector3 directionalInput;
		public readonly bool jumpInput;
		public readonly bool crouchInput;
		public readonly bool sprintInput;

		public MoveInput (Vector3 directionalInput, bool jumpInput, bool crouchInput, bool sprintInput) {
			this.directionalInput = directionalInput;
			this.jumpInput = jumpInput;
			this.crouchInput = crouchInput;
			this.sprintInput = sprintInput;
		}

	}

	[Header("Movement parameters")]
	[SerializeField] float moveSpeedRegular = 8f;
	[SerializeField] float moveSpeedCrouch = 4f;
	[SerializeField] float moveSpeedSprint = 12f;

	[SerializeField] float moveJumpHeight = 1.5f;
	[SerializeField] float moveAccelerationMax = 128f;
	[SerializeField] float moveAccelerationMin = 8f;
	[SerializeField] float moveSlopeAngleLimit = 55f;
	[SerializeField] float moveSlopeSlideStartAngle = 45f;
	[SerializeField] float moveAirControl = 4f;
	[SerializeField] float moveAirAutoDecelThreshold = 10f;
	[SerializeField] float moveAirAutoDeceleration = 5f;
	[SerializeField] float moveLadderJumpSpeed = 4f;
	[SerializeField] float moveLadderHorizontalFactor = 0.33f;

	[Header("Other parameters")]
	[SerializeField] float crouchHeight = 0.9f;
	[SerializeField] float normalHeight = 1.9f;
	[SerializeField] float crouchTime = 0.15f;
	[SerializeField] float uncrouchTime = 0.15f;
	[SerializeField] float eyeOffsetFromTop = 0.1f;
	[SerializeField] float normalGravity = 29.43f;
	[SerializeField] float normalFriction = 0.5f;
	[SerializeField] float waterOffsetFromEyesToSwim = 0.2f;
	[SerializeField] float gravityTurnDegreesPerSecond = 180f;
	[SerializeField] float rigidbodyVelocityProjectMinMass = 40f;
	[SerializeField] float rigidbodyVelocityProjectMaxMass = 200f;

	Player player;
	GameObject head;
	CapsuleCollider col;
	Rigidbody rb;
	PlayerHealthNEW health;

	float jumpSpeed;
	int collisionLayerMaskForRaycasting;

	List<ContactPoint> contactPoints;
	StateData lastState;
	bool canSwim;

	string debugInfo;

	public string DebugInfo {
		get { return debugInfo; }
	}

	public void Initialize (Player player, Rigidbody rb, CapsuleCollider worldCollider, GameObject head, PlayerHealthNEW health) {
		this.player = player;
		this.rb = rb;
		this.col = worldCollider;
		this.head = head;
		this.health = health;
		contactPoints = new List<ContactPoint>();
		SetColliderHeight(normalHeight);	//TODO load from playerprefs later (if i do a half life style campaign)
		jumpSpeed = Mathf.Sqrt(2f * normalGravity * moveJumpHeight);
		collisionLayerMaskForRaycasting = LayerMaskUtils.CreateMask(this.rb.gameObject.layer);
		collisionLayerMaskForRaycasting &= ~LayerMask.GetMask("Water");
		debugInfo = "";
	}
	
	public void ExecuteUpdate () {
		bool uniformScale = (rb.transform.localScale.x == rb.transform.localScale.y);
		uniformScale &= (rb.transform.localScale.x == rb.transform.localScale.z);
		uniformScale &= (rb.transform.localScale.y == rb.transform.localScale.z);
		if(!uniformScale) throw new UnityException("Players may only be scaled uniformly!");
		//scaling is supported on a bare bones level (crouch jumping and swimming basically)
		//everything else is just left as is, which i think fits the very video gamey (small = incredibly fast, still a high jump, big = very sluggish... not very fun...)
	}

	public void ExecuteFixedUpdate (MoveInput moveInput) {
		StateData currentState;
		List<ContactPoint> wallPoints;
		ManageCollisions(contactPoints, moveInput, out currentState, out wallPoints);
		ManageHeight(currentState);
		MovementType movementType = GetMovementType(currentState);
		Vector3 acceleration, gravity;
		float friction;
		switch(movementType){
		case MovementType.GROUNDED : 
			GroundedMovement(ref currentState, lastState, out acceleration, out gravity, out friction);
			break;
		case MovementType.AIRBORNE : 
			AirborneMovement(ref currentState, out acceleration, out gravity, out friction);
			break;
		case MovementType.SWIMMING : 
			WaterMovement(ref currentState, out acceleration, out gravity, out friction);
			break;
		case MovementType.LADDER : 
			LadderMovement(ref currentState, out acceleration, out gravity, out friction);
			break;
		default : 
			throw new UnityException("Unsupported MovementType \"" + movementType.ToString() + "\"");
		}

		if(Input.GetKey(KeyCode.R)) currentState.velocityComesFromMove = true;

		ProjectOnAllSolidAndNearSolidObjects(ref acceleration, wallPoints);
		rb.velocity += (acceleration + gravity) * Time.fixedDeltaTime;
		currentState.outgoingVelocity = rb.velocity;
		currentState.outgoingOwnVelocity = GetRelativeVelocity(rb.velocity, currentState.surfacePoint);		//TODO refactor to take currentState instead of just surfacePoint
		col.material.staticFriction = friction;
		col.material.dynamicFriction = friction;

		//do it after saving the state to not factor into own velocity and stuff
		if(movementType == MovementType.GROUNDED && currentState.onValidGround){
			float slopeLerpFactor = GetSlopeLerpFactor(currentState);	//reminder : 0 = walkable, 1 = not walkable
			float inputLerpFactor = currentState.moveInput.directionalInput.magnitude;	//if input, then no sliding
			float resultLerpFactor = slopeLerpFactor * (1f - inputLerpFactor);
			resultLerpFactor = Mathf.Pow(resultLerpFactor, 0.333f);
			Vector3 slopeAccel = Vector3.ProjectOnPlane(Physics.gravity, currentState.surfacePoint.normal) * resultLerpFactor;
			rb.velocity += slopeAccel * Time.fixedDeltaTime;
		}

		debugInfo = "vFromMove? : " + currentState.velocityComesFromMove.ToString() + "\n";
		debugInfo += movementType.ToString() + "\n";
		if(currentState.onGround){
			debugInfo += currentState.surfacePoint.angle + "°\n";
			debugInfo += "valid? : " + currentState.onValidGround;
		}

		Quaternion gravityRotation = GetGravityRotation(Physics.gravity, rb.transform.up, rb.transform.forward);
		Quaternion newRotation = Quaternion.RotateTowards(rb.transform.rotation, gravityRotation, gravityTurnDegreesPerSecond * Time.fixedDeltaTime);
		rb.MoveRotation(newRotation);

		//save and/or reset fields
		lastState = currentState;
		contactPoints.Clear();
		canSwim = false;	//needs to be reset as it is done in triggerstay
	}

	//collisions / triggers

	void OnCollisionEnter (Collision collision) {
		FilterAndAddToList(collision.contacts, contactPoints);
	}

	void OnCollisionStay (Collision collision) {
		FilterAndAddToList(collision.contacts, contactPoints);
	}

	void OnTriggerStay (Collider otherCollider) {
		WaterBody waterBody = otherCollider.gameObject.GetComponent<WaterBody>();
		if(waterBody != null){
			//water is always in y direction. ALWAYS.
			if((head.transform.position.y - (waterOffsetFromEyesToSwim * rb.transform.localScale.y)) < waterBody.waterLevel){
				canSwim = true;
			}
		}
	}

	//pre movement

	void ManageCollisions (List<ContactPoint> contactPoints, MoveInput moveInput, out StateData currentState, out List<ContactPoint> wallPoints) {
		RemoveInvalidContactPoints(ref contactPoints);
		SurfacePoint surfacePoint = GetSurfacePoint(contactPoints);
		DetermineWallPoints(contactPoints, surfacePoint, out wallPoints);
		currentState = GetStateData(surfacePoint, moveInput, wallPoints, lastState);
		bool overWroteStateData;
		StickToGroundIfNecessary(ref currentState, lastState, wallPoints, out overWroteStateData);
		if(overWroteStateData){
			wallPoints.Clear();
		}
		ManageFallDamage(currentState, lastState);
	}

	void FilterAndAddToList (ContactPoint[] originalCollisionContacts, List<ContactPoint> contactPoints) {
		for(int i=0; i<originalCollisionContacts.Length; i++){
			ContactPoint point = originalCollisionContacts[i];
			if(point.thisCollider.Equals(col)){		//nullchecking here might not work because the other colliders might only get deleted afterwards (as in removing invalid points)
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

	void DetermineWallPoints (List<ContactPoint> contactPoints, SurfacePoint surfacePoint, out List<ContactPoint> wallPoints) {
		wallPoints = new List<ContactPoint>();
		for(int i=0; i<contactPoints.Count; i++){
			ContactPoint point = contactPoints[i];
			float pointAngle = Vector3.Angle(point.normal, rb.transform.up);
			if(pointAngle > moveSlopeAngleLimit){
				wallPoints.Add(point);
			}
		}
	}

	StateData GetStateData (SurfacePoint surfacePoint, MoveInput moveInput, List<ContactPoint> wallPoints, StateData lastState) {
		StateData currentState = new StateData();
		currentState.surfacePoint = surfacePoint;
		currentState.incomingVelocity = rb.velocity;
		currentState.incomingOwnVelocity = GetRelativeVelocity(rb.velocity, surfacePoint);
		float currentOwnSpeed = currentState.incomingOwnVelocity.magnitude;
		float lastOwnSpeed = lastState.outgoingOwnVelocity.magnitude;
		currentState.velocityComesFromMove = (lastState.velocityComesFromMove && (currentOwnSpeed <= lastOwnSpeed));
		currentState.moveInput = moveInput;

		currentState.onGround = GetIsGrounded(surfacePoint, lastState);
		if(currentState.onGround){
			currentState.onValidGround = GetIsOnValidGround(surfacePoint);
			currentState.onSolidGround = GetIsOnSolidGround(surfacePoint);
		}
		currentState.onLadder = GetIsOnLadder(wallPoints, out currentState.ladderPoint);
		currentState.inWater = canSwim;
		return currentState;
	}

	void StickToGroundIfNecessary (ref StateData currentState, StateData lastState, List<ContactPoint> wallPoints, out bool overwroteStateData) {
		overwroteStateData = false;
		if(currentState.onGround) return;
		if(currentState.onLadder) return;
		if(currentState.inWater) return;
		if(lastState.jumped) return;
		if(!lastState.onGround) return;
		if(!lastState.onSolidGround) return;
		if(!currentState.velocityComesFromMove) return;
		if(wallPoints.Count > 0) return;
//		if(currentState.incomingOwnVelocity.magnitude > moveSpeedRegular) return;	//TODO this one's new, might not be necessary
		Vector3 start = player.Bottom + (rb.transform.up * col.radius);
//		Vector3 dir = -lastState.surfacePoint.normal;	//this works more often
//		Vector3 dir = lastState.surfacePoint.normal + rb.transform.up).normalized * -1f;	//this is smoother... less jerky...
		Vector3 dir = (2f * lastState.surfacePoint.normal + rb.transform.up).normalized * -1f;	//best of both worlds
		float dist = col.radius + (2.5f * moveSpeedRegular * Time.fixedDeltaTime);
		RaycastHit hit;
		Debug.DrawRay(start, dir.normalized * dist, Color.magenta, 10f);
		if(Physics.Raycast(start, dir, out hit, dist, collisionLayerMaskForRaycasting)){	
			bool hitAngleOkay = (Vector3.Angle(hit.normal, rb.transform.up) <= moveSlopeAngleLimit);
			bool angleBetweenVelocityAndHitOkay = (Vector3.Angle(currentState.incomingOwnVelocity, hit.normal) < 90f);
			bool colliderSolid = ColliderIsSolid(hit.collider);
			if(hitAngleOkay && angleBetweenVelocityAndHitOkay && colliderSolid){
//				Vector3 properPosition = hit.point + (hit.normal * (col.radius + 0.05f)) - (rb.transform.up * col.radius);	//(old) why the 0.05f? it works fine without... it seems...
				Vector3 properPosition = hit.point + (hit.normal * col.radius) - (rb.transform.up * col.radius);	
				Vector3 projectedVelocity = Vector3.ProjectOnPlane(currentState.incomingVelocity, hit.normal).normalized * currentState.incomingVelocity.magnitude;
				Vector3 additionalGravity = -hit.normal * Physics.gravity.magnitude * Time.fixedDeltaTime;
//				rb.MovePosition(properPosition);
				player.Bottom = properPosition;
				rb.velocity = projectedVelocity + additionalGravity;
				//debug
				Debug.DrawLine(player.Bottom, properPosition, Color.yellow, 10f);
				Debug.LogWarning("HIT! STICKING!");
				//overwrite state data with new info
				SurfacePoint newSurfacePoint = new SurfacePoint(hit.point, hit.normal, hit.collider, rb.transform.up);
				currentState = GetStateData(newSurfacePoint, currentState.moveInput, new List<ContactPoint>(), lastState);
				overwroteStateData = true;
			}else{
				Debug.DrawRay(hit.point, Vector3.up, Color.red, 10f);
				Debug.LogWarning("HIT! No stick tho! angle:" + hitAngleOkay + " angleBetweenVAndHit:" + angleBetweenVelocityAndHitOkay + " solid:" + colliderSolid);
			}
		}else{
			Debug.DrawRay(player.Bottom, Vector3.up, Color.red, 10f);
			Debug.LogWarning("No hit, no stick...");
		}
	}

	void ManageFallDamage (StateData currentState, StateData lastState) {
		if(!lastState.onGround){	//maybe just saying onground is a bad idea but it's videogamey...
			if(currentState.onValidGround && !currentState.onLadder){
				//TODO check for "bounce" tag or whatever i decide to do about bounce pads and the likes...
				Rigidbody otherRB = currentState.surfacePoint.otherCollider.attachedRigidbody;
				health.NotifyOfLanding(otherRB, lastState.incomingVelocity, currentState.incomingVelocity);
//				health.NotifyOfLanding(otherRB, lastState.outgoingVelocity, currentState.incomingVelocity);
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

	//TODO smooth out the head's / camera's movement
	void ManageHeight (StateData currentState) {
		bool crouch = false;
		bool uncrouch = false;
		if(currentState.moveInput.crouchInput){
			if(col.height > crouchHeight) crouch = true;
		}else{
			if(col.height < normalHeight){
				if(CanUncrouch()) uncrouch = true;
				else crouch = true;
			}
		}
		if(crouch){
			float delta = ((crouchHeight - normalHeight) / crouchTime) * Time.fixedDeltaTime;
			ChangeColliderHeight(delta, currentState);
		}else if(uncrouch){
			float delta = ((normalHeight - crouchHeight) / uncrouchTime) * Time.fixedDeltaTime;
			ChangeColliderHeight(delta, currentState);
		}
	}

	void ChangeColliderHeight (float delta, StateData currentState) {
		float newHeight = Mathf.Clamp(col.height + delta, crouchHeight, normalHeight);
		SetColliderHeight(newHeight);
		if(!currentState.onGround){
			rb.transform.position += (rb.transform.up * delta * -1f * rb.transform.localScale.y);
		}
	}

	void SetColliderHeight (float newHeight) {
		Vector3 newCenter = new Vector3(0f, newHeight / 2f, 0f);
		Vector3 newHeadPos = new Vector3(0f, newHeight - eyeOffsetFromTop, 0f);
		col.height = newHeight;
		col.center = newCenter;
		head.transform.localPosition = newHeadPos;
	}

	//actual movement

	void GroundedMovement (ref StateData currentState, StateData lastState, out Vector3 outputAcceleration, out Vector3 outputGravity, out float outputFriction) {
		SurfacePoint surfacePoint = currentState.surfacePoint;
		Vector3 currentVelocity = currentState.incomingOwnVelocity;
		Vector3 inputVector = rb.transform.TransformDirection(currentState.moveInput.directionalInput);
		float currentSpeed = currentVelocity.magnitude;
		float desiredSpeed = GetDesiredSpeed(currentState);
		float frictionLerpFactor = GetSurfaceFrictionLerpFactor(currentState);
		float frictionAppropriateAccel = Mathf.Lerp(moveAccelerationMin, moveAccelerationMax, (frictionLerpFactor * frictionLerpFactor));
		if(currentState.onValidGround){
			Vector3 projectedInput = GetSurfaceMoveVector(inputVector, surfacePoint.normal);
			Vector3 desiredVector = projectedInput * desiredSpeed;
			Vector3 tempAccel = projectedInput * frictionAppropriateAccel;
			float maxAccel = tempAccel.magnitude;
			if(!lastState.onGround){		//TODO why only in this case? why not in general?
				currentState.velocityComesFromMove = (currentSpeed <= desiredSpeed);
			}
			if(currentVelocity.sqrMagnitude > desiredVector.sqrMagnitude){
				desiredVector = projectedInput.normalized * currentSpeed;
				if((currentSpeed <= desiredSpeed) && currentState.velocityComesFromMove){	//case : deceleration
					maxAccel = frictionAppropriateAccel;
				}
				tempAccel = ClampedDeltaVAcceleration(currentVelocity, desiredVector, maxAccel);
			}else{
				Vector3 tempVelocity = currentVelocity + (tempAccel * Time.fixedDeltaTime);
				if(tempVelocity.sqrMagnitude > desiredVector.sqrMagnitude){
					desiredVector = tempVelocity.normalized * desiredVector.magnitude;
					tempAccel = ClampedDeltaVAcceleration(currentVelocity, desiredVector, maxAccel);
				}
				if(tempVelocity.sqrMagnitude >= currentVelocity.sqrMagnitude){
					currentState.velocityComesFromMove = true;
				}
			}

			if(currentState.moveInput.jumpInput){
				tempAccel = Horizontalized(tempAccel) + (rb.transform.up * jumpSpeed / Time.fixedDeltaTime) - Verticalized(currentVelocity / Time.fixedDeltaTime);
				currentState.jumped = true;
			}

			float slopeLerpFactor = GetSlopeLerpFactor(currentState);
			Vector3 tempGravity;
			if(currentState.onSolidGround){
				tempGravity = Vector3.Lerp(Physics.gravity, -surfacePoint.normal * Physics.gravity.magnitude, frictionLerpFactor);;
			}else{
				tempGravity = Physics.gravity;
			}

			if(slopeLerpFactor > 0){
				Vector3 downward = Vector3.ProjectOnPlane(Physics.gravity, surfacePoint.normal).normalized;
				Vector3 downwardAccel = Vector3.Project(tempAccel, downward);
				float accelDot = Vector3.Dot(tempAccel, downward);
				if(accelDot < 0f){
					tempAccel -= downwardAccel * slopeLerpFactor;
				}
				tempGravity = Vector3.Lerp(tempGravity, Physics.gravity, slopeLerpFactor);
			}

			outputAcceleration = tempAccel;
			outputGravity = tempGravity;
			outputFriction = (1f - slopeLerpFactor) * frictionLerpFactor * normalFriction;
			if(!currentState.velocityComesFromMove){
				outputFriction *= GetCrouchLerpFactor();
			}
		}
		else{
			Vector3 downward = Vector3.ProjectOnPlane(Physics.gravity, surfacePoint.normal).normalized;
			Vector3 downwardVelocity = Vector3.Project(currentVelocity, downward);
			Vector3 lateralVelocity = currentVelocity - downwardVelocity;
			Vector3 tempAccel = inputVector * frictionAppropriateAccel;
			if(Vector3.Dot(tempAccel, surfacePoint.normal) < 0f){
				tempAccel = Vector3.ProjectOnPlane(tempAccel, surfacePoint.normal);
				Vector3 tempVelocity = currentVelocity + (tempAccel * Time.fixedDeltaTime);
				Vector3 lateralTempVelocity = tempVelocity - downwardVelocity;
				if(lateralTempVelocity.sqrMagnitude > (desiredSpeed * desiredSpeed)){
					Vector3 desiredVector = tempVelocity.normalized * lateralVelocity.magnitude;
					tempAccel = ClampedDeltaVAcceleration(currentVelocity, desiredVector, moveAccelerationMax);
				}
				tempAccel -= Vector3.Project(tempAccel, downward);
			}

			currentState.velocityComesFromMove = false;
			outputAcceleration = tempAccel;
			outputGravity = Physics.gravity;
			outputFriction = 0f;
		}

	}

	void AirborneMovement (ref StateData currentState, out Vector3 outputAcceleration, out Vector3 outputGravity, out float outputFriction) {
		Vector3 currentVelocity = currentState.incomingOwnVelocity;
		Vector3 inputVector = rb.transform.TransformDirection(currentState.moveInput.directionalInput);
		float currentGroundSpeed = Horizontalized(currentVelocity).magnitude;
		float desiredGroundSpeed = GetDesiredSpeed(currentState);
		Vector3 desiredVector = inputVector * desiredGroundSpeed;
		if(inputVector.magnitude > 0.5f){		//TODO this is a weird condition but should work for now
			float speedModifier = Mathf.Clamp(Mathf.Sqrt(currentGroundSpeed / desiredGroundSpeed), 1f, Mathf.Infinity);
			Vector3 tempAccel = desiredVector * speedModifier * moveAirControl;		//TODO and this is not acceleration...
			Vector3 tempVelocity = Horizontalized(currentVelocity + (tempAccel * Time.fixedDeltaTime));
			float resultGroundSpeed = tempVelocity.magnitude;
			if((resultGroundSpeed > currentGroundSpeed) && (currentGroundSpeed > desiredGroundSpeed)){
				Vector3 deltaV = (tempVelocity.normalized * currentGroundSpeed) - Horizontalized(currentVelocity);
				tempAccel = deltaV / Time.fixedDeltaTime;
			}
			outputAcceleration = tempAccel;
		}else{
			if(currentGroundSpeed < moveAirAutoDecelThreshold){
//				currentState.velocityComesFromMove = true;		//velocity from move doesn't really matter in the air..
				outputAcceleration = ClampedDeltaVAcceleration(Horizontalized(currentVelocity), desiredVector, moveAirAutoDeceleration);
			}else{
				outputAcceleration = Vector3.zero;
			}
		}
		outputGravity = Physics.gravity;
		outputFriction = 0f;
	}

	//TODO i have too much control at high speeds.. at high speeds it should be more redirect than brake...
	void WaterMovement (ref StateData currentState, out Vector3 outputAcceleration, out Vector3 outputGravity, out float outputFriction) {
		Vector3 currentVelocity = currentState.incomingOwnVelocity;
		Vector3 inputVector = head.transform.TransformDirection(currentState.moveInput.directionalInput);
		if(currentState.moveInput.jumpInput){
			inputVector += rb.transform.up;
			if(inputVector.sqrMagnitude > 1f){
				inputVector = inputVector.normalized;
			}
		}
		float desiredSpeed = GetDesiredSpeed(currentState);
		Vector3 desiredVector = inputVector * desiredSpeed;
		Vector3 tempAccel = inputVector * moveAccelerationMax;
		float maxAccel = tempAccel.magnitude;	//no more acceleration than this!
		if(currentVelocity.sqrMagnitude > desiredVector.sqrMagnitude){
			desiredVector = inputVector.normalized * currentVelocity.magnitude;
			tempAccel = ClampedDeltaVAcceleration(currentVelocity, desiredVector, maxAccel);
		}else{
			Vector3 tempVelocity = currentVelocity + (tempAccel * Time.fixedDeltaTime);
			if(tempVelocity.sqrMagnitude > desiredVector.sqrMagnitude){
				desiredVector = tempVelocity.normalized * desiredVector.magnitude;
				tempAccel = ClampedDeltaVAcceleration(currentVelocity, desiredVector, maxAccel);
			}
		}
		if(currentState.onSolidGround){
			Vector3 surfaceNormal = currentState.surfacePoint.normal;
			if(Vector3.Dot(surfaceNormal, tempAccel) < 0f){
				tempAccel = Vector3.ProjectOnPlane(tempAccel, currentState.surfacePoint.normal);
			}
		}
		outputAcceleration = tempAccel;
		outputGravity = Physics.gravity;
		outputFriction = normalFriction;
	}

	void LadderMovement (ref StateData currentState, out Vector3 outputAcceleration, out Vector3 outputGravity, out float outputFriction) {
		Vector3 currentVelocity = currentState.incomingOwnVelocity;
		Vector3 inputVector = head.transform.TransformDirection(Vector3.Scale(currentState.moveInput.directionalInput, new Vector3(moveLadderHorizontalFactor, 1f, 1f)));
		Vector3 ladderNormal = currentState.ladderPoint.normal;
		Vector3 projectedInput = Vector3.ProjectOnPlane(inputVector, ladderNormal);
		float desiredSpeed = GetDesiredSpeed(currentState);
		Vector3 desiredVector = projectedInput * desiredSpeed;
//		float maxAccel = inputVector.magnitude * moveAcceleration;
		float maxAccel = moveAccelerationMax;
		Vector3 tempAccel = ClampedDeltaVAcceleration(currentVelocity, desiredVector, maxAccel);
		if(currentState.moveInput.jumpInput && (Vector3.Dot(head.transform.forward, ladderNormal) > 0f)){
			Vector3 jumpDirection = (ladderNormal + head.transform.forward).normalized;
			tempAccel += jumpDirection * moveLadderJumpSpeed / Time.fixedDeltaTime;
			outputGravity = Physics.gravity;
			outputFriction = 0f;
		}else{
			Vector3 tempVelocity = currentVelocity + (tempAccel * Time.fixedDeltaTime);
			float tempSpeed = tempVelocity.magnitude;
			float frictionFactor = ((tempSpeed > desiredSpeed) ? Mathf.Sqrt(tempSpeed/desiredSpeed) : 1f);
			outputFriction = normalFriction * frictionFactor;
			outputGravity = -ladderNormal * Physics.gravity.magnitude;
		}
		outputAcceleration = tempAccel;
	}

	//utility

	void ProjectOnAllSolidAndNearSolidObjects (ref Vector3 vector, List<ContactPoint> contacts) {
		for(int i=0; i<contacts.Count; i++){
			Vector3 normal = contacts[i].normal;
			Vector3 projected = Vector3.ProjectOnPlane(vector, normal);
			if((Vector3.Dot(vector, normal) < 0f)){
				if(ColliderIsSolid(contacts[i].otherCollider)){
					vector = projected;
				}else{
					Rigidbody otherRB = contacts[i].otherCollider.attachedRigidbody;
					float massDelta = otherRB.mass - rigidbodyVelocityProjectMinMass;
					float maxDelta = rigidbodyVelocityProjectMaxMass - rigidbodyVelocityProjectMinMass;
					float lerpValue = Mathf.Clamp01(massDelta / maxDelta);
					vector = Vector3.Lerp(vector, projected, lerpValue);
				}
			}
		}
	}

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

	Vector3 ClampedDeltaVAcceleration (Vector3 currentVelocity, Vector3 targetVelocity, float maxAccel) {
		Vector3 deltaV = targetVelocity - currentVelocity;
		Vector3 deltaVAccel = deltaV / Time.fixedDeltaTime;
		if(deltaVAccel.sqrMagnitude > (maxAccel * maxAccel)){
			return deltaV.normalized * maxAccel;
		}else{
			return deltaVAccel;
		}
	}

	float GetDesiredSpeed (StateData currentState) {
		float baseSpeed = moveSpeedRegular;
		if(currentState.moveInput.sprintInput){
			if(!currentState.inWater && !currentState.onLadder || currentState.onGround){
				baseSpeed = moveSpeedSprint;
			}
		}
		return Mathf.Lerp(moveSpeedCrouch, baseSpeed, GetCrouchLerpFactor());
	}

	SurfacePoint GetSurfacePoint (List<ContactPoint> contactPoints) {
		ContactPoint bestPoint = new ContactPoint();	//it's a struct so no null...
		float biggestDot = Mathf.NegativeInfinity;	
		for(int i=0; i<contactPoints.Count; i++){
			ContactPoint point = contactPoints[i];
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
		Vector3 projected = ProjectOnPlaneAlongVector(inputVector, inputNormal, rb.transform.up);
		return projected.normalized * inputVector.magnitude;
	}

	//TODO maybe you are jumping in a train car or something... ? (big trigger "relative air" or something)
	//TODO if triggerstay just check for an attached rigidbody? add their velocities (assuming it's multiple) together..
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

	Quaternion GetGravityRotation (Vector3 gravity, Vector3 currentUp, Vector3 currentForward) {
		if(gravity.Equals(Vector3.zero)){
			return Quaternion.LookRotation(currentForward, currentUp);
		}else{
			gravity = gravity.normalized;
			currentUp = currentUp.normalized;
			currentForward = currentForward.normalized;
			Vector3 newUp = -gravity;
			float dot = Vector3.Dot(currentForward, gravity);
			Vector3 tempForward;
			if(Mathf.Abs(dot) < 0.9f){		//arbitrary, but works :P
				tempForward = currentForward;
			}else{
				tempForward = currentForward + currentUp;
			}
			Vector3 newForward = Vector3.ProjectOnPlane(tempForward, newUp);
			return Quaternion.LookRotation(newForward, newUp);
		}
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
		return (surfacePoint.angle <= moveSlopeAngleLimit);
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
		Vector3 rayStart = player.Bottom + (rb.transform.up * (col.height - col.radius));
		Vector3 rayDir = rb.transform.up;
		float rayLength = col.radius + (normalHeight - col.height);
//		return !Physics.Raycast(rayStart, rayDir, rayLength, collisionLayerMaskForRaycasting);
		RaycastHit hit;
		if(Physics.Raycast(rayStart, rayDir, out hit, rayLength, collisionLayerMaskForRaycasting)){
			Debug.Log("can't uncrouch, hit \"" + hit.collider + "\"");
			return false;
		}else{
			return true;
		}
	}

	/// <summary>
	/// 0 = no friction, 1 = normal friction
	/// </summary>
	float GetSurfaceFrictionLerpFactor (StateData currentState) {
		PhysicMaterial otherPM = currentState.surfacePoint.otherCollider.material;
		float averageSurfaceFriction = (otherPM.staticFriction + otherPM.dynamicFriction) / 2f;
		return Mathf.Clamp01(averageSurfaceFriction / 0.6f);
	}

	/// <summary>
	/// 0 = crouched, 1 = uncrouched
	/// </summary>
	float GetCrouchLerpFactor () {
		return Mathf.Clamp01((col.height - crouchHeight) / (normalHeight - crouchHeight));
	}

	/// <summary>
	/// 0 = walkable, 1 = not walkable
	/// </summary>
	float GetSlopeLerpFactor (StateData currentState) {
		return Mathf.Clamp01((currentState.surfacePoint.angle - moveSlopeSlideStartAngle) / (moveSlopeAngleLimit - moveSlopeSlideStartAngle));
	}

}
