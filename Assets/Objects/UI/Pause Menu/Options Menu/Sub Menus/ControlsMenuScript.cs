using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ControlsMenuScript : MonoBehaviour {

	public KeybinderScript keybinder;
	public GameObject keybindGroupPrefab;
	public GameObject scrollViewContent;
	public Text plac_mouseSens;
	public Toggle tog_mouseInvert;

	private int groupCounter;

	void Start () {
		InitializeMouseStuff();
		InitializeScroller();
	}

	public void SetMouseInvert(bool value){
		PlayerPrefManager.SetInt("mouse_invert", (value ? 1 : 0));
		PlayerPrefManager.MessagePlayerSettingsObservers();
	}

	public void SetMouseSensitivity(string stringValue){
		float value;
		if(float.TryParse(stringValue, out value)){
			PlayerPrefManager.SetFloat("mouse_sensitivity", value);
			PlayerPrefManager.MessagePlayerSettingsObservers();
		}
	}

	public void ResetControls(){
		PlayerPrefManager.ResetKeybind("key_move_forward");
		PlayerPrefManager.ResetKeybind("key_move_backward");
		PlayerPrefManager.ResetKeybind("key_move_left");
		PlayerPrefManager.ResetKeybind("key_move_right");
		PlayerPrefManager.ResetKeybind("key_jump");
		PlayerPrefManager.ResetKeybind("key_crouch_toggle");
		PlayerPrefManager.ResetKeybind("key_crouch_hold");
		PlayerPrefManager.ResetKeybind("key_sprint_toggle");
		PlayerPrefManager.ResetKeybind("key_sprint_hold");
		PlayerPrefManager.ResetKeybind("key_primary_fire");
		PlayerPrefManager.ResetKeybind("key_secondary_fire");
		PlayerPrefManager.ResetKeybind("key_interact");
		PlayerPrefManager.ResetKeybind("key_toggle_flashlight");
		PlayerPrefManager.ResetKeybind("key_toggleGUI");
		PlayerPrefManager.MessageKeybindObservers();
		int childCount = scrollViewContent.transform.childCount;
		for(int i=0; i<childCount; i++){
			GameObject child = scrollViewContent.transform.GetChild(i).gameObject;
			if(!(child.name.Equals("Mouse Sensitivity") || child.name.Equals("Mouse Invert") || child.name.Equals("Reset Keybinds"))){
				Destroy(child);
			}
		}
		InitializeScroller();
	}

	private void InitializeMouseStuff(){
		tog_mouseInvert.isOn = (PlayerPrefManager.GetInt("mouse_invert") == 1);
		plac_mouseSens.text = PlayerPrefManager.GetFloat("mouse_sensitivity").ToString();
	}

	private void InitializeScroller(){
		groupCounter = 3;
		NewScrollerGroup("Forward", "key_move_forward");
		NewScrollerGroup("Backward", "key_move_backward");
		NewScrollerGroup("Left", "key_move_left");
		NewScrollerGroup("Right", "key_move_right");
		groupCounter++;
		NewScrollerGroup("Jump", "key_jump");
		NewScrollerGroup("Crouch (Toggle)", "key_crouch_toggle");
		NewScrollerGroup("Crouch (Hold)", "key_crouch_hold");
		NewScrollerGroup("Sprint (Toggle)", "key_sprint_toggle");
		NewScrollerGroup("Sprint (Hold)", "key_sprint_hold");
		groupCounter++;
		NewScrollerGroup("Primary Fire", "key_primary_fire");
		NewScrollerGroup("Secondary Fire", "key_secondary_fire");
		NewScrollerGroup("Interact", "key_interact");
		NewScrollerGroup("Flashlight", "key_toggle_flashlight");
		groupCounter++;
		NewScrollerGroup("Toggle GUI", "key_toggleGUI");

		float containerHeight = groupCounter * 25f;
		RectTransform contentRectTransform = (RectTransform)scrollViewContent.transform;
		contentRectTransform.offsetMax = Vector2.zero;
		contentRectTransform.offsetMin = new Vector2(0f, -containerHeight);
	}

	private void NewScrollerGroup(string actionName, string prefKey){
		string primaryKey = PlayerPrefManager.GetString(prefKey);
		string secondaryKey = PlayerPrefManager.GetString(prefKey + "_alt");

		GameObject newGroup = Instantiate(keybindGroupPrefab) as GameObject;
		newGroup.transform.SetParent(scrollViewContent.transform, false);
		((RectTransform)newGroup.transform).localPosition = new Vector3(10f, -25f * groupCounter, 0f);

		int childCount = newGroup.transform.childCount;
		if(childCount != 3) throw new ArgumentException("not 3 children in this prefab. the instantiating relies on the exact makeup of the prefab");
		for(int i=0; i<childCount; i++){
			GameObject child = newGroup.transform.GetChild(i).gameObject;
			if(child.name.Equals("Action Name")){
				child.GetComponent<Text>().text = actionName;
			}else if(child.name.Equals("Primary Key")){
				child.name = prefKey;
				child.transform.GetChild(0).GetComponent<Text>().text = primaryKey;
				child.GetComponent<ControlsMenuKeybindButtonScript>().keybinder = keybinder;
			}else if(child.name.Equals("Secondary Key")){
				child.name = prefKey + "_alt";
				child.transform.GetChild(0).GetComponent<Text>().text = secondaryKey;
				child.GetComponent<ControlsMenuKeybindButtonScript>().keybinder = keybinder;
			}else{
				throw new ArgumentException("the child does not have the proper name");
			}
		}
		groupCounter++;
	}

}
