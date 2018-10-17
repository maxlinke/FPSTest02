using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HoloPlaneGenerator))]
public class HoloPlaneGeneratorEditor : Editor {

	public override void OnInspectorGUI (){
		DrawDefaultInspector();
		HoloPlaneGenerator hpg = target as HoloPlaneGenerator;
		if(GUILayout.Button("Generate")){
			hpg.Generate();
		}
	}
}
