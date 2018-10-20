using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls {

	//camera fov (and gui scale etc) go somewhere else. i haven't decided where but it's not part of the controls

	public enum Property {
		MOUSE_SENSITIVITY,
		MOUSE_INVERT_X,
		MOUSE_INVERT_Y,

		KEY_MOVE_FORWARD,
		KEY_MOVE_BACK,		//or backward?
		KEY_MOVE_LEFT,
		KEY_MOVE_RIGHT,

		KEY_JUMP,
		KEY_CROUCH_TOGGLE,
		KEY_CROUCH_HOLD,
		KEY_SPRINT_TOGGLE,
		KEY_SPRINT_HOLD,		//also add alt for walking?

		KEY_INTERACT
	}

	public enum Category {
		VIEW = 0,
		MOVEMENT = 1,
		WEAPONS = 2
	}

	Dictionary<Property, IPlayerControlsProperty> map;

	private PlayerControls () {
		map = new Dictionary<Property, IPlayerControlsProperty>();
	}

	public static PlayerControls DefaultControls () {
		PlayerControls output = new PlayerControls();
//		output.map.Add(Property.MOUSE_SENSITIVITY, 3f);	
		//no direct map access, instead wrapper access. 
		//wrapper method can get bool, float, two keycodes, ...
		//creates proper property
		//
		//then there are getfloat, getbool, getkey to get the appropriate thing again
		return output;
	}

}
