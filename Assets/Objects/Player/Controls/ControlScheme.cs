using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerControls {

	public class ControlScheme {

		//camera fov (and gui scale etc) go somewhere else. i haven't decided where but it's not part of the controls

		Dictionary<PropertyKey, IPlayerControlsProperty> map;

		private ControlScheme () {
			map = new Dictionary<PropertyKey, IPlayerControlsProperty>();
		}

		private IPlayerControlsProperty GetProperty (PropertyKey key) {
			IPlayerControlsProperty prop;
			if(map.TryGetValue(key, out prop)){
				return prop;
			}else{
				throw new UnityException("No value in map for key \"" + key.ToString() + "\"");
			}
		}

		public float GetFloat (PropertyKey key) {
			return ((FloatProperty)GetProperty(key)).value;
		}

		public void SetFloat (PropertyKey key, float value) {
			//okay, so maybe the properties SHOULD be classes so they can be reference types...
			//that would also mean i could get rid of the interface and use inheritance
		}

		public static ControlScheme DefaultControls () {
			ControlScheme output = new ControlScheme();
			//		output.map.Add(Property.MOUSE_SENSITIVITY, 3f);	
			//no direct map access, instead wrapper access. 
			//wrapper method can get bool, float, two keycodes, ...
			//creates proper property
			//
			//then there are getfloat, getbool, getkey to get the appropriate thing again
			Category cat = Category.VIEW;
			output.map.Add(PropertyKey.MOUSE_SENSITIVITY, new FloatProperty(cat, 10, 3f));
			output.map.Add(PropertyKey.MOUSE_INVERT_X, new BoolProperty(cat, 20, false));
			output.map.Add(PropertyKey.MOUSE_INVERT_Y, new BoolProperty(cat, 30, false));
			output.map.Add(PropertyKey.KEY_INTERACT, new KeybindProperty(cat, 40, new DoubleKey(KeyCode.E)));
			cat = Category.MOVEMENT;
			output.map.Add(PropertyKey.KEY_MOVE_FORWARD, new KeybindProperty(cat, 10, new DoubleKey(KeyCode.W)));
			output.map.Add(PropertyKey.KEY_MOVE_BACK, new KeybindProperty(cat, 20, new DoubleKey(KeyCode.S)));
			output.map.Add(PropertyKey.KEY_MOVE_LEFT, new KeybindProperty(cat, 30, new DoubleKey(KeyCode.A)));
			output.map.Add(PropertyKey.KEY_MOVE_RIGHT, new KeybindProperty(cat, 40, new DoubleKey(KeyCode.D)));
			output.map.Add(PropertyKey.KEY_JUMP, new KeybindProperty(cat, 50, new DoubleKey(KeyCode.Space)));
			output.map.Add(PropertyKey.KEY_CROUCH_TOGGLE, new KeybindProperty(cat, 60, new DoubleKey(KeyCode.X)));
			output.map.Add(PropertyKey.KEY_CROUCH_HOLD, new KeybindProperty(cat, 70, new DoubleKey(KeyCode.LeftControl)));
			output.map.Add(PropertyKey.KEY_SPRINT_TOGGLE, new KeybindProperty(cat, 80, new DoubleKey(KeyCode.None)));
			output.map.Add(PropertyKey.KEY_SPRINT_HOLD, new KeybindProperty(cat, 90, new DoubleKey(KeyCode.LeftShift)));
			return output;
		}
		
	}

}

