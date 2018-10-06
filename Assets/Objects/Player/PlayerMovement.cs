using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour, IPlayerPrefObserver, IPlayerPrefKeybindObserver {

	//TODO replace every foreach with a nice little int count = <list>.count ... for(int i=0; i<count; i++) ... <list>[i]...
	//TODO look for if(x.mag > y.mag) NO OTHER THINGS and replace them with sqmag.

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

	[Header("Components")]
	[SerializeField] GameObject head;
	[SerializeField] CapsuleCollider col;
	[SerializeField] Rigidbody rb;
	[SerializeField] PlayerHealthSystem healthSystem;
	[SerializeField] PlayerWaterTriggerScript waterTrigger;

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
//	[SerializeField] float sprintDuration;
//	[SerializeField] float sprintCooldown;
//	[SerializeField] float sprintRegeneration;
	[SerializeField] float crouchHeight = 0.9f;
	[SerializeField] float normalHeight = 1.9f;
	[SerializeField] float crouchEyeLevel = 0.8f;
	[SerializeField] float normalEyeLevel = 1.8f;
	[SerializeField] float normalGravity = 29.43f;
	[SerializeField] float normalStaticFriction = 0.5f;
	[SerializeField] float normalDynamicFriction = 0.5f;
	[SerializeField] float normalWaterTriggerPos = 1.6f;
	[SerializeField] float crouchWaterTriggerPos = 0.7f;

	List<ContactPoint> contactPoints;
	List<ContactPoint> wallPoints;
	List<ContactPoint> stepPoints;
	ContactPoint surfacePoint;
	Vector3 surfaceNormal;
	float surfaceAngle;

//	Vector3 inputVector;
	Vector3 gravityVector;

	bool gotMoveInput;

//	Vector3 desiredDirection;	//normalized, only direction
	Vector3 desiredVector;		//magnitude is important
	float desiredSpeed;

	bool isGrounded;
	bool isOnValidGround;
	bool isOnSolidGround;
	bool isOnLadder;
	bool isInWater;
	bool isSwimming;

	bool wasOnSolidGround;
	bool wasGrounded;

	bool isCrouching;
	bool wasCrouching;

	bool isSprinting;
	bool wasSprinting;
//	float sprintTimer;
//	float sprintCooldownTimer;

	Vector3 footObjectVelocity;
	Vector3 ownVelocity;

	Vector3 jumpVelocity;
	bool justJumped;
	bool velocityComesFromMove;

	Vector3 lastVelocity;
	Vector3 lastOwnVelocity;
	Vector3 lastValidGroundSpeed;
	Vector3 lastSurfaceNormal;
	Vector3 lastSurfacePoint;

	Vector3 ladderNormal;

	int layermaskPlayer;
	int layerWater;

	void Reset () {
		moveSpeedRegular = 8f;
		moveSpeedCrouch = 4f;
		moveSpeedSprint = 12f;
		moveAcceleration = 256f;
		moveJumpHeight = 1.5f;
		moveMaxSlopeAngle = 55f;
		moveMaxStepOffset = 0.3f;
		moveSlideControl = 12f;	//TODO even i don't understand these values fully. but they do work wonderfully...
		moveAirControl = 4f;

		crouchHeight = 0.9f;
		normalHeight = 1.9f;
		crouchEyeLevel = 0.8f;	//TODO maybe work with an eyeoffset variable?
		normalEyeLevel = 1.8f;
		normalGravity = 29.43f;
		normalStaticFriction = 0.5f;
		normalDynamicFriction = 0.5f;
		normalWaterTriggerPos = 1.6f;	//TODO maybe make the watertrigger be related to the head?
		crouchWaterTriggerPos = 0.7f;
	}

	void Start(){
		AddSelfToPlayerPrefObserverList();
		LoadKeys();
		contactPoints = new List<ContactPoint>();
		wallPoints = new List<ContactPoint>();
		stepPoints = new List<ContactPoint>();
		pm = col.material;
		layermaskPlayer = GetLayerMask(LayerMask.NameToLayer("Player"));
		layerWater = LayerMask.NameToLayer("Water");
		jumpVelocity = new Vector3(0f, Mathf.Sqrt(2 * normalGravity * moveJumpHeight), 0f);	//eeehhh... this might need a special kind of treatment in case moveJumpHeight gets changed...
		col.center = new Vector3(0f, col.height/2f, 0f);	//just making sure. before i had the height at 1.9 and the center at 0.9 (should be 0.95) which caused some problems that could only be solved by crouching and uncrouching
		//float stepOffsetAngle = Mathf.Rad2Deg * Mathf.Acos(1f - (moveMaxStepOffset / col.radius));
		//float slopeAngleOffset = col.radius * (1f - Mathf.Cos(Mathf.Deg2Rad * moveMaxSlopeAngle));
	}
	
	void Update(){
		if(Time.timeScale > 0f){
			CrouchManager();
			SprintManager();
			if(Input.GetKeyDown(KeyCode.Mouse1)) rb.velocity += head.transform.forward * 50f;
		}
	}

	void FixedUpdate(){
		InitializeFixedUpdate();
		FixedUpdateMovement();
		PrepareNextFixedUpdate();
	}

	public void AddSelfToPlayerPrefObserverList(){
		PlayerPrefManager.AddObserver(this);
	}

	public void NotifyKeybindsChanged(){
		LoadKeys();
	}

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

	void InitializeFixedUpdate(){
		ManageCollisions();
		DetermineIsStates();
		ManageFallDamage();
	}

	void ManageFallDamage(){	//this is not good practice but otherwise the health system would have to implement collision detection and managing too (as in knowing that i was airborne and now i'm not etc)
		if(!wasGrounded && isOnValidGround && !isOnLadder){
			healthSystem.NotifyOfLanding(lastVelocity, rb.velocity);
		}
	}

	void ManageCollisions(){
		RemoveInvalidContactPoints(ref contactPoints);
		surfacePoint = GetSurfacePoint(contactPoints);
		surfaceNormal = surfacePoint.normal;
		surfaceAngle = Vector3.Angle(surfaceNormal, Vector3.up);
//		SurfacePoint surfacePoint = GetSurfacePointNEW(contactPoints);
		DetermineWallAndStepPoints();
		StepUpSteps();
	}

	void RemoveInvalidContactPoints (ref List<ContactPoint> contactPoints) {
//		List<ContactPoint> removePoints = new List<ContactPoint>();
//		foreach(ContactPoint point in contactPoints){
//			if(point.otherCollider == null) removePoints.Add(point);
//		}
//		foreach(ContactPoint point in removePoints){
//			contactPoints.Remove(point);
//		}
		for(int i=0; i<contactPoints.Count; i++){
			if(contactPoints[i].otherCollider == null){
				contactPoints.RemoveAt(i);
				i--;
			}
		}
	}

	void DetermineWallAndStepPoints(){
		foreach(ContactPoint point in contactPoints){
			float pointAngle = Vector3.Angle(point.normal, Vector3.up);
			//float pointOffset = col.radius * (1f - Vector3.Dot(point.normal, Vector3.up));
			float pointOffset = col.radius * (1f - point.normal.y);
			if(pointAngle > moveMaxSlopeAngle){
				wallPoints.Add(point);
			}
			if(pointOffset > 0f && pointOffset <= moveMaxStepOffset && !point.Equals(surfacePoint)){
				stepPoints.Add(point);
			}
		}
	}
		
	/*
	private void StepUpSteps(){
		foreach(ContactPoint point in stepPoints){
			float pointOffset = col.radius * (1f - Vector3.Dot(point.normal, Vector3.up));
			if(velocityComesFromMove && gotMoveInput && ColliderIsStatic(point.otherCollider)){
				float directionDot = Vector3.Dot(Horizontalize(point.normal).normalized, Horizontalize(lastValidGroundSpeed).normalized);
				if((directionDot < - 0.64f) && pointOffset > 0f){
					Vector3 rayOrigin = point.point + point.normal * col.radius + Vector3.up * 0.01f;
					RaycastHit hit;
					if(Physics.Raycast(rayOrigin, -point.normal, out hit, col.radius + 0.1f, layermaskPlayer)){
						float slopeAngle = Vector3.Angle(point.normal, Vector3.up);
						float slopeDifference = Mathf.Abs(slopeAngle - surfaceAngle);
						if(slopeDifference > 20f){
							bool itsJustASlope = Vector3.Angle(hit.normal, point.normal) < 1f;
							bool goForTheTeleport = false;
							if(itsJustASlope){
								if(slopeAngle <= moveMaxSlopeAngle){
									if(slopeAngle > 55f){
										goForTheTeleport = true;
									}
								}
							}else{
								goForTheTeleport = true;
							}
							if(goForTheTeleport && ColliderIsStatic(hit.collider)){
								Vector3 properPosition = hit.point + hit.normal * col.radius + Vector3.down * col.radius;
								rb.MovePosition(properPosition);
								surfacePoint = point;
								surfaceNormal = Vector3.up;
								rb.velocity = lastVelocity;
							}
						}
					}
				}
			}
		}
	}
	*/

	private void StepUpSteps(){
		if(gotMoveInput && velocityComesFromMove){
			
			int stepPointCount = stepPoints.Count;
			for(int i=0; i<stepPointCount; i++){
				ContactPoint point = stepPoints[i];

				float normalDot = Vector3.Dot(surfacePoint.normal, point.normal);
				float directionDot = Vector3.Dot(Horizontalize(point.normal).normalized, Horizontalize(lastVelocity).normalized);
				bool pointIsAboveSurfacePoint = point.point.y > (surfacePoint.point.y + 0.01f);
				if(normalDot < 0.9998476f && directionDot < 0f && pointIsAboveSurfacePoint && ColliderIsStatic(point.otherCollider)){	//if angle between surfacepoint and hitnormal > 1°

					Vector3 surfPointToStepPoint = point.point - surfacePoint.point;
					Vector3 projectedVector = Vector3.ProjectOnPlane(surfPointToStepPoint, surfacePoint.normal);
					Vector3 projectedPoint = surfacePoint.point + projectedVector;
					float distance = Vector3.Distance(projectedPoint, point.point);
					if(distance > 0.05f){	//if the point is at least 5cm out of the current move plane (make sure it's not the top of a ramp or something)

						Vector3 rayOrigin = point.point + (point.normal * col.radius) + Vector3.up * 0.01f;
						Vector3 rayDirection = -point.normal;
						RaycastHit hit;
						if(Physics.Raycast(rayOrigin, rayDirection, out hit, col.radius + 0.1f, layermaskPlayer)){

							float normalDot2 = Vector3.Dot(point.normal, hit.normal);
							if(normalDot2 < 0.996195f){		//if difference in normals > 5°

								Vector3 properPosition = hit.point + hit.normal * col.radius + Vector3.down * col.radius;
								rb.MovePosition(properPosition);
								surfacePoint = point;
								surfaceNormal = hit.normal;
								rb.velocity = lastVelocity;
								Debug.LogWarning("stepping " + Time.time);

							}
						}
					}
				}
			}
		}
	}

	private void DetermineIsStates(){
		isGrounded = GetIsGrounded();
		isOnSolidGround = GetIsOnSolidGround();
		isOnValidGround = GetIsOnValidGround();
		isOnLadder = GetIsOnLadder();
		isInWater = waterTrigger.isInWater;
		isSwimming = GetIsSwimming();
	}

	private void FixedUpdateMovement () {
		Vector3 rawInput = GetInputVector();
		gotMoveInput = rawInput.magnitude > 0f;
		gravityVector = Vector3.down;
		footObjectVelocity = GetFootObjectVelocity();
		ownVelocity = GetOwnVelocity();
		if(ownVelocity.magnitude > lastOwnVelocity.magnitude) velocityComesFromMove = false;
		//crouchheightmanager is here in the original
		if(isOnLadder) LadderMovement(rawInput);
		else{
			if(isSwimming) WaterMovement(rawInput);
			else{
//				desiredDirection = GetDesiredDirection();
				desiredSpeed = GetDesiredSpeed();
				if(isGrounded) GroundedMovement(rawInput);
				else AirborneMovement(rawInput);
				if(moveMaxSlopeAngle > 45f){
					float lerpFactor = Mathf.Clamp01((surfaceAngle - 45f) / (moveMaxSlopeAngle - 45f));
					gravityVector = Vector3.Lerp(gravityVector, Vector3.down, lerpFactor).normalized;
				}
				ownVelocity += gravityVector * Physics.gravity.magnitude * Time.fixedDeltaTime;
			}
		}
		PreventGoingIntoWalls();
		FinalizeVelocity();

		if(isGrounded) Debug.DrawRay(transform.position, Vector3.up * 0.05f, Color.green, 10f);
		else Debug.DrawRay(transform.position, Vector3.up * 0.05f, Color.red, 10f);
	}

	private void LadderMovement(Vector3 rawInput){
		if(gotMoveInput){
//			Vector3 inputDirection = Vector3.ProjectOnPlane(head.transform.TransformDirection(new Vector3(0f, 0f, rawInput.z)), ladderNormal);
//			Vector3 inputDirection = Vector3.ProjectOnPlane(head.transform.TransformDirection(rawInput), ladderNormal);
			Vector3 inputDirection = Vector3.ProjectOnPlane(head.transform.TransformDirection(new Vector3(rawInput.x * 0.33f, 0f, rawInput.z)), ladderNormal);	//TODO don't do the *0.33f thing here. do something else...
			if(ownVelocity.magnitude <= moveSpeedRegular){
				ownVelocity += inputDirection * moveAcceleration * Time.fixedDeltaTime;
				if(ownVelocity.magnitude > moveSpeedRegular) ownVelocity = ownVelocity.normalized * moveSpeedRegular;
			}else{
				Vector3 idealVector = inputDirection * moveSpeedRegular;
				Vector3 deltaV = idealVector - ownVelocity;
				Vector3 velocityChange = deltaV.normalized * moveAcceleration * Time.deltaTime;
				if(velocityChange.magnitude > deltaV.magnitude) velocityChange = deltaV;
				ownVelocity += velocityChange;
			}
		}else{
			Vector3 decelVector = ownVelocity.normalized * (-moveAcceleration) * Time.fixedDeltaTime;
			if(decelVector.magnitude > ownVelocity.magnitude) decelVector = -ownVelocity;
			ownVelocity += decelVector;
		}
		if(keyJump.GetKey()){
			if(Vector3.Dot(ladderNormal, head.transform.forward) > 0f) ownVelocity += ladderNormal * 5;
		}else{
			gravityVector = -ladderNormal * 0.1f;
		}
	}

	private void WaterMovement(Vector3 rawInput){
		Debug.LogWarning("watermovement");
		if(gotMoveInput || keyJump.GetKey()){
			Vector3 desiredDirection = head.transform.TransformDirection(rawInput);
			if(keyJump.GetKey()){
				desiredDirection = (desiredDirection + Vector3.up).normalized;
			}
			desiredSpeed = GetDesiredSpeed();
			if(ownVelocity.magnitude <= desiredSpeed){
				ownVelocity += desiredDirection * moveAcceleration * Time.fixedDeltaTime;
				if(ownVelocity.magnitude > desiredSpeed) ownVelocity = ownVelocity.normalized * desiredSpeed;
			}else{
				Vector3 idealVector = desiredDirection * moveSpeedRegular;
				Vector3 deltaV = idealVector - ownVelocity;
				Vector3 velocityChange = deltaV.normalized * moveAcceleration * Time.fixedDeltaTime;
				if(velocityChange.magnitude > deltaV.magnitude) velocityChange = deltaV;
				ownVelocity += velocityChange;
			}
		}
		if(ownVelocity.y > 0){
			if((waterTrigger.transform.position + (ownVelocity * Time.fixedDeltaTime)).y > waterTrigger.waterLevel){
				if(ownVelocity.y <= moveSpeedCrouch){
					float distanceToTop = waterTrigger.waterLevel - waterTrigger.transform.position.y;
					ownVelocity = Horizontalize(ownVelocity) + (Vector3.up * Mathf.Clamp((distanceToTop / Time.fixedDeltaTime), 0, Mathf.Infinity));
				}
				if(keyJump.GetKey()){
					bool touchingWall = false;
					foreach(ContactPoint point in wallPoints){
						touchingWall = true;
						break;
					}
					if(touchingWall){
						ownVelocity.y = 1.5f * jumpVelocity.y;	//this only works because i can jump plenty high
					}
				}
			}
		}

	}

	private void GroundedMovement(Vector3 rawInput){
		Vector3 inputDirection = transform.TransformDirection(rawInput);
		justJumped = false;
		if(isOnSolidGround) wasOnSolidGround = true;		//TODO wtf?
		else wasOnSolidGround = false;
		//slope ok
		if(isOnValidGround){
			if(!wasGrounded && ownVelocity.magnitude < moveSpeedRegular) velocityComesFromMove = true;
			ManageVelocityFriction();
			desiredVector = GetSurfaceMoveVector(inputDirection, surfaceNormal) * desiredSpeed;
			//got input
			if(gotMoveInput){
				//full control
				if(ownVelocity.magnitude < desiredSpeed){
					ownVelocity += desiredVector.normalized * moveAcceleration * Time.fixedDeltaTime;
					if(ownVelocity.magnitude > desiredSpeed) ownVelocity = ownVelocity.normalized * desiredSpeed;
					velocityComesFromMove = true;
					lastValidGroundSpeed = ownVelocity;
					SetGravityVectorBasedOnSurfaceNormal();
				}//velocity too great
				else{
					//decelerating
					if(ownVelocity.magnitude < moveSpeedSprint && velocityComesFromMove){
						DecelerateWithMoveAcceleration();
						SetGravityVectorBasedOnSurfaceNormal();
						velocityComesFromMove = true;
					}//redirecting
					else{
						Vector3 whereIdLikeToGo = inputDirection.normalized * ownVelocity.magnitude;
						Vector3 deltaV = whereIdLikeToGo - ownVelocity;
						if(deltaV.magnitude > desiredSpeed * moveSlideControl) deltaV = deltaV.normalized * desiredSpeed * moveSlideControl;
						ownVelocity += deltaV * Time.fixedDeltaTime;
						if(ownVelocity.magnitude <= desiredSpeed) velocityComesFromMove = true;
						else velocityComesFromMove = false;
					}
				}
			}//no input
			else{
				if(ownVelocity.magnitude > lastValidGroundSpeed.magnitude && ownVelocity.magnitude > 0.01f) velocityComesFromMove = false;
				//decelerate
				if(ownVelocity.magnitude < moveSpeedSprint && velocityComesFromMove){
					DecelerateWithMoveAcceleration();
					velocityComesFromMove = true;
					lastValidGroundSpeed = ownVelocity;
					SetGravityVectorBasedOnSurfaceNormal();
				}//slide along
				else{
					velocityComesFromMove = false;
					lastValidGroundSpeed = Vector3.zero;
				}
			}
			//want to jump
			if(keyJump.GetKey()){
				ownVelocity = Horizontalize(ownVelocity) + jumpVelocity;
				if(ownVelocity.magnitude < moveSpeedSprint){
					velocityComesFromMove = true;
					lastValidGroundSpeed = ownVelocity;
				}else{
					velocityComesFromMove = false;
					lastValidGroundSpeed = Vector3.zero;
				}
				SetFrictionToZero();
				justJumped = true;
			}

		}
		//slope too steep
		else{
			SetFrictionToZero();
			if(gotMoveInput){
				desiredVector = inputDirection * desiredSpeed;
				float currentGroundSpeed = Horizontalize(ownVelocity).magnitude;
				float speedModifier = (currentGroundSpeed > desiredSpeed ? Mathf.Sqrt(currentGroundSpeed / desiredSpeed) : 1f);
				Vector3 temp = ownVelocity + desiredVector * moveAirControl * Time.fixedDeltaTime * speedModifier;
				if(Horizontalize(temp).magnitude > currentGroundSpeed && currentGroundSpeed > desiredSpeed){
					Vector3 temp2 = Horizontalize(temp).normalized * currentGroundSpeed;
					temp = new Vector3(temp2.x, temp.y, temp2.z);
				}
				ownVelocity = temp;
			}
		}
		wasGrounded = true;
		lastSurfaceNormal = surfaceNormal;
		lastSurfacePoint = surfacePoint.point;
	}

	private void AirborneMovement(Vector3 rawInput){
		bool notReallyAirborne = StickToGroundIfNecessary();
		Vector3 desiredDirection = transform.TransformDirection(rawInput);
		if(!notReallyAirborne){
			float airSpeed = Horizontalize(ownVelocity).magnitude;
			if(gotMoveInput){
				desiredVector = desiredDirection * desiredSpeed;
				float airspeedModifier = (airSpeed > desiredSpeed ? Mathf.Sqrt(airSpeed/desiredSpeed) : 1f);
				Vector3 temp = ownVelocity + desiredVector * moveAirControl * Time.fixedDeltaTime * airspeedModifier;
				if(Horizontalize(temp).magnitude > airSpeed && airSpeed > desiredSpeed){
					Vector3 temp2 = Horizontalize(temp).normalized * airSpeed;
					temp = new Vector3(temp2.x, temp.y, temp2.z);
				}
				ownVelocity = temp;
			}//no input
			else{
				//decelerate when slow
				if(airSpeed < (moveSpeedSprint - 0.1f)){
					Vector3 temp = -Horizontalize(ownVelocity);
					Vector3 deltaV = temp.normalized * 5f * Time.fixedDeltaTime;
					if(deltaV.magnitude > temp.magnitude) deltaV = temp;
					ownVelocity += deltaV;
				}
			}
			if(ownVelocity.magnitude > lastValidGroundSpeed.magnitude * 2f) velocityComesFromMove = false;
			justJumped = false;
			wasGrounded = false;
		}
	}

	private void DecelerateWithMoveAcceleration(){
		Vector3 deltaV = -Vector3.ProjectOnPlane(ownVelocity, surfaceNormal);
		Vector3 temp = deltaV.normalized * moveAcceleration * Time.fixedDeltaTime;
		if(temp.magnitude < deltaV.magnitude) deltaV = temp;
		ownVelocity += deltaV;
	}

	/*
	private void StickToGroundIfNecessary(){
		RaycastHit hit;
		Vector3 negativeY = new Vector3(ownVelocity.x, -Mathf.Abs(ownVelocity.y), ownVelocity.z);
		Vector3 castPoint = transform.position + Vector3.up * col.radius - lastSurfaceNormal * col.radius;
		if(Physics.Raycast(castPoint, negativeY, out hit, negativeY.magnitude * Time.fixedDeltaTime * 4, layermaskPlayer)){
			//Debug.Log("hit");
			DebugDrawHelper.DrawSphere(transform.position, 0.1f, Color.green, 5f);
			if(hit.collider.attachedRigidbody == null ? true : hit.collider.attachedRigidbody.isKinematic){
				ownVelocity = negativeY;
			}
		}else{
			//Debug.Log("miss");
			DebugDrawHelper.DrawSphere(transform.position, 0.1f, Color.red, 5f);
			wasGrounded = false;
		}
	}
	*/

	private bool StickToGroundIfNecessary(){
		if(!wasGrounded) return false;
		if(justJumped) return false;
		if(!velocityComesFromMove) return false;
		if(!wasOnSolidGround) return false;
		Vector3 rayOrigin = transform.position + (Vector3.up * col.radius);
		//Vector3 rayDirection = (-lastSurfaceNormal + Vector3.down).normalized;
		Vector3 rayDirection = -lastSurfaceNormal;
		float rayLength = col.radius + (2.5f * moveSpeedRegular * Time.fixedDeltaTime);
		RaycastHit hit;
		//Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.yellow, 10f);
		if(Physics.Raycast(rayOrigin, rayDirection, out hit, rayLength, layermaskPlayer)){
			float hitAngle = Vector3.Angle(hit.normal, Vector3.up);		//TODO if performance becomes very important i can change this to just the dot product and compare it to being < 0
			float angleBetweenVelocityAndHit = Vector3.Angle(ownVelocity, hit.normal);
			if(hitAngle <= moveMaxSlopeAngle && angleBetweenVelocityAndHit < 90f){
				//Debug.Log("sticking. angle : " + angleBetweenVelocityAndHit);
				Vector3 properPosition = hit.point + (hit.normal * (col.radius + 0.05f)) + (Vector3.down * col.radius);
				//Debug.DrawRay(properPosition, Vector3.up * 0.5f, Color.cyan, 10f);
				rb.MovePosition(properPosition);
				ownVelocity = Vector3.ProjectOnPlane(ownVelocity, hit.normal) - (hit.normal * Physics.gravity.magnitude * Time.fixedDeltaTime);
				wasGrounded = false;
				return true;
			}
		}
		return false;
	}

	private void PreventGoingIntoWalls(){
		foreach(ContactPoint point in wallPoints){
			if(Vector3.Dot(Horizontalize(ownVelocity).normalized, Horizontalize(point.normal).normalized) < 0f){
				Rigidbody otherRB = point.otherCollider.attachedRigidbody;
				Vector3 projectedVelocity = Vector3.ProjectOnPlane(ownVelocity, point.normal);
				if((otherRB == null ? true : otherRB.isKinematic)){
					ownVelocity = projectedVelocity;
				}else{
					float lerpValue = Mathf.Clamp01((otherRB.mass - 40f) / (160f)); //start of lerp at 40, end at 200
					ownVelocity = Vector3.Lerp(ownVelocity, projectedVelocity, lerpValue);
				}
			}
		}
	}

	private void SetFrictionToZero(){
		pm.staticFriction = 0f;
		pm.dynamicFriction = 0f;
	}

	private void ResetFriction(){
		pm.staticFriction = normalStaticFriction;
		pm.dynamicFriction = normalDynamicFriction;
	}

	private void ManageVelocityFriction(){
		float frictionFactor = ((isCrouching && ownVelocity.magnitude > moveSpeedCrouch) ? 0f : 1f);
		pm.staticFriction = normalStaticFriction * frictionFactor;
		pm.dynamicFriction = normalDynamicFriction * frictionFactor;
	}

	private void SetGravityVectorBasedOnSurfaceNormal(){
		if(surfacePoint.otherCollider.attachedRigidbody == null) gravityVector = -surfaceNormal;
		else if(surfacePoint.otherCollider.attachedRigidbody.isKinematic) gravityVector = -surfaceNormal;
		else gravityVector = Vector3.down;
	}

	private void PrepareNextFixedUpdate(){
		contactPoints.Clear();
		wallPoints.Clear();
		stepPoints.Clear();
		lastVelocity = rb.velocity;
		lastOwnVelocity = ownVelocity;
		isInWater = false;
	}

	//at least in Unity 5.61f1 OnCollisionStay is only called AFTER OnCollisionEnter. as in a whole physics tick after.
	//so that's why there's enter and stay both here...

	void OnCollisionEnter(Collision collision){
		contactPoints.AddRange(collision.contacts);
	}

	void OnCollisionStay(Collision collision){
		contactPoints.AddRange(collision.contacts);
	}

	private Vector3 Horizontalize(Vector3 input){
		return new Vector3(input.x, 0f, input.z);
	}

	private bool GetIsGrounded(){
		if(surfacePoint.otherCollider == null) return false;
		if(justJumped) return false;
		if(surfaceAngle > 89f) return false;
		return true;
	}

	private bool GetIsOnValidGround(){
		if(!isGrounded) return false;
		if(surfaceAngle > moveMaxSlopeAngle) return false;
		return true;
	}

	private bool GetIsOnSolidGround(){
		if(!isGrounded) return false;
		if(surfacePoint.otherCollider.attachedRigidbody == null) return true;
		if(surfacePoint.otherCollider.attachedRigidbody.isKinematic) return true;
		return false;
	}

	private bool GetIsOnLadder(){
		if(surfacePoint.otherCollider != null){
			if(isOnValidGround){
				return false;
			}
		}
		foreach(ContactPoint point in wallPoints){
			if(TagManager.CompareTag("Ladder", point.otherCollider.gameObject)){
				if(point.normal.y > 0){
					ladderNormal = point.normal;
					return true;
				}
				else if(Vector3.Angle(point.normal, Vector3.up) < 91f){
					ladderNormal = point.normal;
					return true;
				}else{
					ladderNormal = Vector3.zero;
				}
			}
		}
		return false;
	}

	private bool GetIsSwimming(){
		if(!isInWater) return false;
		float waterLevel = waterTrigger.waterLevel;
		if(waterTrigger.transform.position.y < waterLevel && !isOnLadder) return true;
		if(isOnValidGround) return false;
		return true;
	}

	private void CrouchManager(){
		bool isGoingToCrouch = wasCrouching;
		if(keyCrouchHold.GetKey()) isGoingToCrouch = true;
		if(keyCrouchHold.GetKeyUp()) isGoingToCrouch = false;
		if(keyCrouchToggle.GetKeyDown()) isGoingToCrouch = !wasCrouching;
		if(!isGoingToCrouch && wasCrouching){
			Vector3 rayOrigin = transform.position + (Vector3.up * col.height / 2f);
			float rayLength = normalHeight - (col.height / 2f);
			RaycastHit hit;
			if(Physics.Raycast(rayOrigin, Vector3.up, out hit, rayLength, layermaskPlayer)){
				if(hit.collider.gameObject.layer != layerWater){
					isGoingToCrouch = true;
					Debug.LogWarning("crouchcast hit " + hit.collider.name + ", layer " + LayerMask.LayerToName(hit.collider.gameObject.layer));
				}
			}
		}
		isCrouching = isGoingToCrouch;
		if(isCrouching && col.height > crouchHeight){
			col.height = crouchHeight;
			col.center = new Vector3(0f, crouchHeight/2f, 0f);
			head.transform.localPosition = new Vector3(0f, crouchEyeLevel, 0f);
			waterTrigger.transform.localPosition = new Vector3(0f, crouchWaterTriggerPos, 0f);
			if(!isGrounded) transform.position = transform.position + (Vector3.up * (normalHeight - crouchHeight) / 2f);
		}
		if(!isCrouching && col.height < normalHeight){
			col.height = normalHeight;
			col.center = new Vector3(0f, normalHeight/2f, 0f);
			head.transform.localPosition = new Vector3(0f, normalEyeLevel, 0f);
			waterTrigger.transform.localPosition = new Vector3(0f, normalWaterTriggerPos, 0f);
			if(!isGrounded) transform.position = transform.position + (Vector3.down * (normalHeight - crouchHeight) / 2f);
		}
		wasCrouching = isCrouching;
	}

	private void SprintManager(){
		bool wantToSprint = false;
		if(keySprintHold.GetKey()) wantToSprint = true;
		if(keySprintHold.GetKeyUp()) wantToSprint = false;
		if(keySprintToggle.GetKeyDown()) wantToSprint = !wasSprinting;
		if(wantToSprint && gotMoveInput && !isCrouching){
			isSprinting = true;
		}else{
			isSprinting = false;
		}
		wasSprinting = isSprinting;
	}

	private bool ColliderIsStatic(Collider collider){
		if(collider == null) throw new NullReferenceException("The referenced collider \"" + collider.name + "\" is NULL");
		Rigidbody colliderRB = collider.attachedRigidbody;
		if(colliderRB == null) return true;
		if(colliderRB.isKinematic && colliderRB.velocity == Vector3.zero) return true;
		return false;
	}

	private bool ColliderIsSolid(Collider collider){
		if(collider == null) throw new NullReferenceException("The referenced collider \"" + collider.name + "\" is NULL");
		Rigidbody colliderRB = collider.attachedRigidbody;
		if(colliderRB == null) return true;
		if(colliderRB.isKinematic) return true;
		return false;
	}

	private Vector3 GetInputVector(){
		int inputZ = (keyMoveForward.GetKey() ? 1 : 0) + (keyMoveBackward.GetKey() ? -1 : 0);
		int inputX = (keyMoveLeft.GetKey() ? -1 : 0) + (keyMoveRight.GetKey() ? 1 : 0);
		return new Vector3(inputX, 0f, inputZ);
	}

//	private Vector3 GetDesiredDirection(){
//		return transform.TransformDirection(inputVector).normalized * inputVector.magnitude;
//	}

	private float GetDesiredSpeed(){
		if(!gotMoveInput) return 0f;
		if(isCrouching) return moveSpeedCrouch;
		if(isSprinting) return moveSpeedSprint;
		return moveSpeedRegular;
	}

	ContactPoint GetSurfacePoint(List<ContactPoint> contacts){		//TODO remove if unused
		ContactPoint temp = new ContactPoint();
		float biggestDot = -1f;
		foreach(ContactPoint point in contacts){
			if(temp.otherCollider == null){
				temp = point;
				biggestDot = Vector3.Dot(point.normal, Vector3.up);
			}else{
				float newDot = Vector3.Dot(point.normal, Vector3.up);
				if(newDot > biggestDot){
					biggestDot=newDot;
					temp=point;
				}
			}
		}
		if(temp.normal.y > 0f) return temp;
		else return new ContactPoint();
	}

	SurfacePoint GetSurfacePointNEW (List<ContactPoint> contactPoints) {
		SurfacePoint output = null;
		float biggestY = Mathf.NegativeInfinity;		//dot(normal, up) is just the normal's y, so i'm just using 
		for(int i=0; i<contactPoints.Count; i++){
			ContactPoint point = contactPoints[i];
			float newY = point.normal.y;
			if((newY > biggestY) && (newY > 0f)){
				biggestY = newY;
				output = new SurfacePoint(point);
			}
		}
		return output;
	}

	private Vector3 GetSurfaceMoveVector(Vector3 inputVector, Vector3 inputNormal){
		inputVector = inputVector.normalized;
		inputNormal = inputNormal.normalized;
		float ix = inputVector.x;
		float iz = inputVector.z;
		float nx = inputNormal.x;
		float ny = inputNormal.y;
		float nz = inputNormal.z;
		float deltaY = -((ix * nx) + (iz * nz)) / ny;
		return new Vector3(ix, deltaY, iz).normalized;
	}

	private Vector3 GetFootObjectVelocity(){
		if(surfacePoint.otherCollider != null){
			Rigidbody surfaceRB = surfacePoint.otherCollider.attachedRigidbody;
			if(surfaceRB != null){
				if(surfaceRB.isKinematic) return surfaceRB.velocity;
			}
		}
		return Vector3.zero;
	}

	private Vector3 GetOwnVelocity(){
		return rb.velocity - footObjectVelocity;
	}

	private void FinalizeVelocity(){
		rb.velocity = footObjectVelocity + ownVelocity;
	}

	private int GetLayerMask(int layer){
		string[] names = new string[32];
		for(int i=0; i<32; i++){
			if(!Physics.GetIgnoreLayerCollision(layer, i)) names[i] = LayerMask.LayerToName(i);
		}
		return LayerMask.GetMask(names);
	}

}
