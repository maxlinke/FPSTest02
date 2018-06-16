using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayMenuResolutionButtonScript : MonoBehaviour {

	public Resolution resolution;
	public DisplayMenuScript displayMenu;
	public Text label;

	public void initiateResolutionChange(){
		displayMenu.setResolution(resolution);
	}
}
