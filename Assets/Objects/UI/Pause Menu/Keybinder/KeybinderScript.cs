using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeybinderScript : MonoBehaviour {

	[Header("Internal stuff")]
	public PauseMenuScript pauseMenu;
	public Text actionNameTextField;
	float time_removeBind;
	public float holdTimeToRemove;

	[Header("Set externally")]
	public string propertyName;
	public Text callingButtonTextField;


	// Use this for initialization
	void Start () {
		
	}
	
	void Update(){
		if(Input.GetKeyDown(KeyCode.Escape)){
			time_removeBind = Time.unscaledTime + holdTimeToRemove;
		}else if(Input.GetKeyUp(KeyCode.Escape)){
			gameObject.SetActive(false);
		}else if(Input.GetKey(KeyCode.Escape)){
			if(Time.unscaledTime > time_removeBind){
				setKeyBind(propertyName, KeyCode.None);
			}
		}else if(Input.anyKeyDown){
			foreach(KeyCode kcode in System.Enum.GetValues(typeof(KeyCode))){
				if (Input.GetKeyDown(kcode)){
					setKeyBind(propertyName, kcode);
				}
			}
		}
	}

	void OnEnable(){
		pauseMenu.blockPauseToggle = true;
	}

	void OnDisable(){
		pauseMenu.blockPauseToggle = false;
	}

	void setKeyBind(string name, KeyCode kcode){
		PlayerPrefManager.SetString(name, kcode.ToString());
		PlayerPrefManager.MessageKeybindObservers();
		callingButtonTextField.text = kcode.ToString();
		gameObject.SetActive(false);
	}

}
