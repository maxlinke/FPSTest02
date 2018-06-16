using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDrawHelper{

	public static void DrawCapsuleCollider(CapsuleCollider col, Color drawColor, float drawDuration){
		Vector3 lowerCenter=col.transform.position + Vector3.up*col.radius;
		Vector3 upperCenter=col.transform.position + Vector3.up*col.height + Vector3.down*col.radius;
		DrawHalfCapsuleCollider(lowerCenter, col.transform, col.radius, drawColor, drawDuration, false);
		DrawHalfCapsuleCollider(upperCenter, col.transform, col.radius, drawColor, drawDuration, true);
		for(int i=0; i<=360; i=i+90){
			float rad=((float)i)*Mathf.Deg2Rad;
			Vector3 shift = col.transform.TransformDirection(new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad))) * col.radius;
			Debug.DrawLine(lowerCenter+shift, upperCenter+shift, drawColor, drawDuration);
		}
	}

	public static void DrawSphere(Vector3 midPoint, float radius, Color drawColor, float drawDuration){
		//draw circle in x-z-plane
		Vector3 lastPoint=midPoint + new Vector3(0, 0, 1)*radius;
		for(int i=0; i<=360; i=i+10){
			float rad=((float)i)*Mathf.Deg2Rad;
			Vector3 newPoint=midPoint + new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad))*radius;
			Debug.DrawLine(lastPoint, newPoint, drawColor, drawDuration);
			lastPoint=newPoint;
		}
		//draw circle in x-y-plane
		lastPoint=midPoint + new Vector3(0, 1, 0)*radius;
		for(int i=0; i<=360; i=i+10){
			float rad=((float)i)*Mathf.Deg2Rad;
			Vector3 newPoint=midPoint + new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0f)*radius;
			Debug.DrawLine(lastPoint, newPoint, drawColor, drawDuration);
			lastPoint=newPoint;
		}
		//draw circly in y-z-plane
		lastPoint=midPoint + new Vector3(0, 0, 1)*radius;
		for(int i=0; i<=360; i=i+10){
			float rad=((float)i)*Mathf.Deg2Rad;
			Vector3 newPoint=midPoint + new Vector3(0f, Mathf.Sin(rad), Mathf.Cos(rad))*radius;
			Debug.DrawLine(lastPoint, newPoint, drawColor, drawDuration);
			lastPoint=newPoint;
		}
	}

	private static void DrawHalfCapsuleCollider(Vector3 midPoint, Transform transform, float radius, Color drawColor, float drawDuration, bool top){
		//draw circle in x-z-plane
		Vector3 lastPoint=midPoint + new Vector3(0, 0, 1)*radius;
		for(int i=0; i<=360; i=i+10){
			float rad=((float)i)*Mathf.Deg2Rad;
			Vector3 newPoint=midPoint + new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad))*radius;
			Debug.DrawLine(lastPoint, newPoint, drawColor, drawDuration);
			lastPoint=newPoint;
		}
		//draw circle in x-y-plane
		lastPoint=midPoint + transform.TransformDirection(new Vector3(1, 0, 0))*radius;
		for(int i=0; i<=180; i=i+10){
			float rad=((float)i)*Mathf.Deg2Rad;
			Vector3 newDirection=transform.TransformDirection(new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)*(top ? 1f : -1f), 0f));
			Vector3 newPoint=midPoint + newDirection*radius;
			Debug.DrawLine(lastPoint, newPoint, drawColor, drawDuration);
			lastPoint=newPoint;
		}
		//draw circly in y-z-plane
		lastPoint=midPoint + transform.TransformDirection(new Vector3(0, 0, 1))*radius;
		for(int i=0; i<=180; i=i+10){
			float rad=((float)i)*Mathf.Deg2Rad;
			Vector3 newDirection=transform.TransformDirection(new Vector3(0f, Mathf.Sin(rad)*(top ? 1f : -1f), Mathf.Cos(rad)));
			Vector3 newPoint=midPoint + newDirection*radius;
			Debug.DrawLine(lastPoint, newPoint, drawColor, drawDuration);
			lastPoint=newPoint;
		}
	}

}
