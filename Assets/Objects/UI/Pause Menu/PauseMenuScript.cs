using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuScript : MonoBehaviour {

	public GameObject menuBackground;
	public GameObject menuButtons;
	public GameObject optionsMenu;
	public GameObject keybinder;

	private bool optionsMenuStateMemory;

	private bool paused;
	private float timeScaleMemory;

	public bool blockPauseToggle;


	void Start () {
		keybinder.SetActive(false);
		ClosePauseMenu();
		paused = false;
		optionsMenuStateMemory = true;	//TODO change before build
	}
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape)){
			if(!blockPauseToggle){
				if(!paused) Pause();
				else Unpause();
			}
		}
	}

	public void Pause(){
		if(!paused){
			timeScaleMemory = Time.timeScale;
			Time.timeScale = 0f;
			Cursor.lockState = CursorLockMode.None;
			OpenPauseMenu();
			paused = true;
		}
	}

	public void Unpause(){
		if(paused){
			Time.timeScale = timeScaleMemory;
			timeScaleMemory = 1f;
			Cursor.lockState = CursorLockMode.Locked;
			ClosePauseMenu();
			paused = false;
		}
	}

	public void Quit(){
		Application.Quit();
	}

	public void ToggleOptionsMenu(){
		optionsMenu.SetActive(!optionsMenu.activeSelf);
	}

	private void OpenPauseMenu(){
		menuBackground.SetActive(true);
		menuButtons.SetActive(true);
		optionsMenu.SetActive(optionsMenuStateMemory);
	}

	private void ClosePauseMenu(){
		menuBackground.SetActive(false);
		menuButtons.SetActive(false);
		optionsMenuStateMemory = optionsMenu.activeSelf;
		optionsMenu.SetActive(false);
	}


}
