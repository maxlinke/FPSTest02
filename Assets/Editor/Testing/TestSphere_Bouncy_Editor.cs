using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestSphere_Bouncy))]
public class TestSphere_Bouncy_Editor : Editor {

	public override void OnInspectorGUI(){
		TestSphere_Bouncy testSphere = (TestSphere_Bouncy)target;

		testSphere.rb = (Rigidbody)EditorGUILayout.ObjectField("Rigidbody rb", testSphere.rb, typeof(Rigidbody), true);
		testSphere.key_bounce = (KeyCode)EditorGUILayout.EnumPopup("Bounce Key", testSphere.key_bounce);
		testSphere.velocity = EditorGUILayout.FloatField("Velocity", testSphere.velocity);
		EditorGUILayout.LabelField("Bounce Height", testSphere.getBounceHeight().ToString());

		if(GUILayout.Button("Bounce")) testSphere.bounce();
	}

}
