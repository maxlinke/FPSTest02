using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenuScript : MonoBehaviour {

	public GameObject displayMenu;
	public GameObject gameplayMenu;
	public GameObject controlsMenu;

	void Start () {
		showDisplayMenu();
	}

	public void showDisplayMenu(){
		displayMenu.SetActive(true);
		gameplayMenu.SetActive(false);
		controlsMenu.SetActive(false);
	}

	public void showGameplayMenu(){
		displayMenu.SetActive(false);
		gameplayMenu.SetActive(true);
		controlsMenu.SetActive(false);
	}

	public void showControlsMenu(){
		displayMenu.SetActive(false);
		gameplayMenu.SetActive(false);
		controlsMenu.SetActive(true);
	}
}
