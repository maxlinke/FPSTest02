using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HoloPlaneAnimator))]
public class HoloPlaneAnimatorEditor : Editor {

	public override void OnInspectorGUI (){
		DrawDefaultInspector();
		HoloPlaneAnimator hpa = target as HoloPlaneAnimator;
		if(GUILayout.Button("Apply Texture")){
			hpa.ApplyTextureToMaterial();
		}
	}
}
