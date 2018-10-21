using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerControls {

	public class ControlScheme {

		//camera fov (and gui scale etc) go somewhere else. i haven't decided where but it's not part of the controls

		Dictionary<PropertyKey, PlayerControlsProperty> map;

		private ControlScheme () {
			map = new Dictionary<PropertyKey, PlayerControlsProperty>();
		}

		//external

		public float GetFloat (PropertyKey key) {
			return ((FloatProperty)GetProperty(key)).value;
		}

		public void SetFloat (PropertyKey key, float value) {
			((FloatProperty)GetProperty(key)).value = value;
		}

		public int GetInt (PropertyKey key) {
			return ((IntProperty)GetProperty(key)).value;
		}

		public void SetInt (PropertyKey key, int value) {
			((IntProperty)GetProperty(key)).value = value;
		}

		public bool GetBool (PropertyKey key) {
			return ((BoolProperty)GetProperty(key)).value;
		}

		public void SetBool (PropertyKey key, bool value) {
			((BoolProperty)GetProperty(key)).value = value;
		}

		public DoubleKey GetKeybind (PropertyKey key) {
			return ((KeybindProperty)GetProperty(key)).value;
		}

		public void SetKeybind (PropertyKey key, DoubleKey value) {
			((KeybindProperty)GetProperty(key)).value = value;
		}

		//TODO remove the number (if possible) and just use the ordering of the propertykeys
		//TODO make use of the new walking key 

		public static ControlScheme DefaultControls () {
			ControlScheme output = new ControlScheme();
			Category cat = Category.VIEW;
			output.map.Add(PropertyKey.MOUSE_SENSITIVITY, new FloatProperty(cat, 1, 3f));
			output.map.Add(PropertyKey.MOUSE_INVERT_X, new BoolProperty(cat, 2, false));
			output.map.Add(PropertyKey.MOUSE_INVERT_Y, new BoolProperty(cat, 3, false));
			cat = Category.MOVEMENT;
			output.map.Add(PropertyKey.KEY_MOVE_FORWARD, new KeybindProperty(cat, 1, KeyCode.W));
			output.map.Add(PropertyKey.KEY_MOVE_BACK, new KeybindProperty(cat, 2, KeyCode.S));
			output.map.Add(PropertyKey.KEY_MOVE_LEFT, new KeybindProperty(cat, 3, KeyCode.A));
			output.map.Add(PropertyKey.KEY_MOVE_RIGHT, new KeybindProperty(cat, 4, KeyCode.D));
			output.map.Add(PropertyKey.KEY_JUMP, new KeybindProperty(cat, 5, KeyCode.Space));
			output.map.Add(PropertyKey.KEY_CROUCH_HOLD, new KeybindProperty(cat, 6, KeyCode.LeftControl));
			output.map.Add(PropertyKey.KEY_CROUCH_TOGGLE, new KeybindProperty(cat, 7, KeyCode.X));
			output.map.Add(PropertyKey.KEY_WALK_HOLD, new KeybindProperty(cat, 8, KeyCode.LeftAlt));
			output.map.Add(PropertyKey.KEY_WALK_TOGGLE, new KeybindProperty(cat, 9, KeyCode.None));
			output.map.Add(PropertyKey.KEY_SPRINT_HOLD, new KeybindProperty(cat, 10, KeyCode.LeftShift));
			output.map.Add(PropertyKey.KEY_SPRINT_TOGGLE, new KeybindProperty(cat, 11, KeyCode.None));
			cat = Category.WEAPONS;
			//TODO 
			cat = Category.MISCELLANEOUS;
			output.map.Add(PropertyKey.KEY_INTERACT, new KeybindProperty(cat, 1, KeyCode.E));
			return output;
		}

		//internal

		PlayerControlsProperty GetProperty (PropertyKey key) {
			PlayerControlsProperty prop;
			if(map.TryGetValue(key, out prop)){
				return prop;
			}else{
				throw new UnityException("No value in map for key \"" + key.ToString() + "\"");
			}
		}
		
	}

}

