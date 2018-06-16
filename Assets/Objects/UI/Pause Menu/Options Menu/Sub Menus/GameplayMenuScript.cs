using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayMenuScript : MonoBehaviour {

	public GameObject graphicalUserInterface;
	private IGUI gui;

	public Text plac_guiScale;
	public Text plac_crosshairScale;
	public Text plac_fov;

	void Start(){
		gui = graphicalUserInterface.GetComponent<IGUI>();
		SetPlaceholderTexts();
	}

	public void SetGUIScale(string stringValue){
		float value;
		if(float.TryParse(stringValue, out value)){
			gui.setGUIScale(value);
			PlayerPrefManager.SetFloat("gui_gui_scale", value);
		}
	}

	public void SetCrosshairScale(string stringValue){
		float value;
		if(float.TryParse(stringValue, out value)){
			gui.setCrosshairScale(value);
			PlayerPrefManager.SetFloat("gui_crosshair_scale", value);
		}
	}

	public void SetFOV(string stringValue){
		float value;
		if(float.TryParse(stringValue, out value)){
			PlayerPrefManager.SetFloat("camera_fov", value);
			PlayerPrefManager.MessagePlayerSettingsObservers();
		}
	}

	private void SetPlaceholderTexts(){
		plac_guiScale.text = PlayerPrefManager.GetFloat("gui_gui_scale").ToString();
		plac_crosshairScale.text = PlayerPrefManager.GetFloat("gui_crosshair_scale").ToString();
		plac_fov.text = PlayerPrefManager.GetFloat("camera_fov").ToString();
	}

}
