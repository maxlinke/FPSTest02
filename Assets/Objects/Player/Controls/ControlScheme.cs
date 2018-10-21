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

		public static ControlScheme LoadFromString (string stringifiedControlScheme) {
			ControlScheme output = new ControlScheme();
			string[] lines = stringifiedControlScheme.Split("\n".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
			for(int i=0; i<lines.Length; i++){
				string[] elements = lines[i].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
				PropertyKey key = (PropertyKey)System.Enum.Parse(typeof(PropertyKey), elements[0]);
				Category cat = (Category)System.Enum.Parse(typeof(Category), elements[1]);
				int num = int.Parse(elements[2]);
				switch(elements[3]){
				case "k":
					KeyCode kcode1 = KeyCodeUtils.ParseFromString(elements[4]);
					KeyCode kcode2 = KeyCodeUtils.ParseFromString(elements[5]);
					DoubleKey dk = new DoubleKey(kcode1, kcode2);
					output.map.Add(key, new KeybindProperty(cat, num, dk));
					break;
				case "i":
					output.map.Add(key, new IntProperty(cat, num, int.Parse(elements[4])));
					break;
				case "b":
					output.map.Add(key, new BoolProperty(cat, num, bool.Parse(elements[4])));
					break;
				case "f":
					output.map.Add(key, new FloatProperty(cat, num, float.Parse(elements[4])));
					break;
				default:
					throw new UnityException("Unsupported property type denoter \"" + elements[3] + "\"");
				}
			}
			return output;
		}

		/// <summary>
		/// Makes a string that can be loaded into an identical ControlScheme again.
		/// Each line is a key-value pair of the internal dictionary with all the necessary details separated by spaces.
		/// The first element is the PropertyKey, the second the Category, the third the number in that category.
		/// Following is a character denoting what type the property is. "k" stands for a keybind, "i" for an integer, "b" for a bool and "f" for a float.
		/// </summary>
		public string ToSaveableString () {
			string output = "";
			foreach(PropertyKey key in map.Keys){
				output += key.ToString();
				output += " ";
				PlayerControlsProperty prop = GetProperty(key);
				output += prop.category.ToString();
				output += " ";
				output += prop.numberInCategory.ToString();
				output += " ";
				if(prop is KeybindProperty){
					DoubleKey dk = ((KeybindProperty)prop).value;
					output += "k";
					output += " ";
					output += dk.primaryKeyCode.ToString();
					output += " ";
					output += dk.secondaryKeyCode.ToString();
				}else if(prop is IntProperty){
					output += "i " + ((IntProperty)prop).value.ToString();
				}else if(prop is BoolProperty){
					output += "b " + ((BoolProperty)prop).value.ToString();
				}else if(prop is FloatProperty){
					output += "f " + ((FloatProperty)prop).value.ToString();
				}else{
					throw new UnityException("Couldn't save because Property Type \"" + prop.GetType().ToString() + "\" is unsupported");
				}
				output += "\n";
			}
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

