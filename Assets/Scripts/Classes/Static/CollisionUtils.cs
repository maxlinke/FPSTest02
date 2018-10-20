using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollisionUtils {

	/// <summary>
	/// Returns force in Newtons
	/// </summary>
	public static Vector3 GetForce (Collision collision) {
		return collision.impulse / Time.fixedDeltaTime;
	}

	public static Vector3 GetAveragePoint (Collision collision) {
		Vector3 averagePoint = Vector3.zero;
		for(int i=0; i<collision.contacts.Length; i++){
			averagePoint += collision.contacts[i].point;
		}
		return averagePoint / collision.contacts.Length;
	}

	public static Vector3 GetAverageNormal (Collision collision) {
		Vector3 averageNormal = Vector3.zero;
		for(int i=0; i<collision.contacts.Length; i++){
			averageNormal += collision.contacts[i].normal;
		}
		return averageNormal.normalized;
	}

	public static void GetAveragePointAndNormal (Collision collision, out Vector3 averagePoint, out Vector3 averageNormal) {
		averagePoint = Vector3.zero;
		averageNormal = Vector3.zero;
		for(int i=0; i<collision.contacts.Length; i++){
			averagePoint += collision.contacts[i].point;
			averageNormal += collision.contacts[i].normal;
		}
		averagePoint /= collision.contacts.Length;
		averageNormal = averageNormal.normalized;
	}

}
