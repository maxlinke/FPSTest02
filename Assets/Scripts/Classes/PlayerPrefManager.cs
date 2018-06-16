using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefManager{

	/*
	 * README:
	 *
	 * Convention : 
	 * > Use underscores to separate words
	 * > Always write lowercase
	 * > Secondary values, if existent, have the name of the primary value plus "_alt"
	 * > Boolean values are to be saved as 1 (true) or 0 (false)
	 * 
	 */

	private static List<StringKeyValuePair> strings;
	private static List<FloatKeyValuePair> floats;
	private static List<IntKeyValuePair> ints;

	private static List<IPlayerPrefObserver> observers;

	static PlayerPrefManager(){
		strings = new List<StringKeyValuePair>();
		floats = new List<FloatKeyValuePair>();
		ints = new List<IntKeyValuePair>();
		observers = new List<IPlayerPrefObserver>();

		InitializeFromPlayerMovement();
		InitializeFromPlayerView();
		InitializeFromPlayerWeaponry();
	}

	public static void AddObserver(IPlayerPrefObserver observer){
		if(!observers.Contains(observer)) observers.Add(observer);
	}

	public static void MessageKeybindObservers(){
		foreach(IPlayerPrefObserver observer in observers){
			if(observer is IPlayerPrefKeybindObserver){
				((IPlayerPrefKeybindObserver)observer).NotifyKeybindsChanged();
			}
		}
	}

	public static void MessagePlayerSettingsObservers(){
		foreach(IPlayerPrefObserver observer in observers){
			if(observer is IPlayerPrefSettingsObserver){
				((IPlayerPrefSettingsObserver)observer).NotifyPlayerSettingsChanged();
			}
		}
	}

	private static void InitializeFromPlayerMovement(){
		strings.Add(new StringKeyValuePair("key_move_forward", "W"));
		strings.Add(new StringKeyValuePair("key_move_forward_alt", "UpArrow"));
		strings.Add(new StringKeyValuePair("key_move_backward", "S"));
		strings.Add(new StringKeyValuePair("key_move_backward_alt", "DownArrow"));
		strings.Add(new StringKeyValuePair("key_move_left", "A"));
		strings.Add(new StringKeyValuePair("key_move_left_alt", "LeftArrow"));
		strings.Add(new StringKeyValuePair("key_move_right", "D"));
		strings.Add(new StringKeyValuePair("key_move_right_alt", "RightArrow"));

		strings.Add(new StringKeyValuePair("key_jump", "Space"));
		strings.Add(new StringKeyValuePair("key_jump_alt", "None"));
		strings.Add(new StringKeyValuePair("key_crouch_toggle", "X"));
		strings.Add(new StringKeyValuePair("key_crouch_toggle_alt", "None"));
		strings.Add(new StringKeyValuePair("key_crouch_hold", "LeftControl"));
		strings.Add(new StringKeyValuePair("key_crouch_hold_alt", "None"));
		strings.Add(new StringKeyValuePair("key_sprint_toggle", "None"));
		strings.Add(new StringKeyValuePair("key_sprint_toggle_alt", "None"));
		strings.Add(new StringKeyValuePair("key_sprint_hold", "LeftShift"));
		strings.Add(new StringKeyValuePair("key_sprint_hold_alt", "None"));
	}

	private static void InitializeFromPlayerView(){
		ints.Add(new IntKeyValuePair("mouse_invert", 0));
		floats.Add(new FloatKeyValuePair("mouse_sensitivity", 3f));
		floats.Add(new FloatKeyValuePair("camera_fov", 60f));

		strings.Add(new StringKeyValuePair("key_interact", "E"));
		strings.Add(new StringKeyValuePair("key_interact_alt", "None"));

		strings.Add(new StringKeyValuePair("key_toggleGUI", "F1"));
		strings.Add(new StringKeyValuePair("key_toggleGUI_alt", "None"));

		floats.Add(new FloatKeyValuePair("gui_gui_scale", 1f));
		floats.Add(new FloatKeyValuePair("gui_crosshair_scale", 1f));
	}

	private static void InitializeFromPlayerWeaponry(){
		strings.Add(new StringKeyValuePair("key_primary_fire", "Mouse0"));
		strings.Add(new StringKeyValuePair("key_primary_fire_alt", "None"));
		strings.Add(new StringKeyValuePair("key_secondary_fire", "Mouse1"));
		strings.Add(new StringKeyValuePair("key_secondary_fire_alt", "None"));

		strings.Add(new StringKeyValuePair("key_toggle_flashlight", "T"));
		strings.Add(new StringKeyValuePair("key_toggle_flashlight_alt", "None"));
	}

	public static void ResetKey(string key){
		if(HasKey(key)){
			PlayerPrefs.DeleteKey(key);
		}
		else throw new Exception ("No such key is known to the PlayerPrefManager");
	}

	public static void ResetKeybind(string key){
		if(HasKey(key)){
			PlayerPrefs.DeleteKey(key);
			PlayerPrefs.DeleteKey(key + "_alt");
		}
		else throw new Exception ("No such key is known to the PlayerPrefManager");
	}

	public static bool HasKey(string key){
		foreach(StringKeyValuePair p in strings){
			if(p.key.Equals(key)) return true;
		}
		foreach(FloatKeyValuePair p in floats){
			if(p.key.Equals(key)) return true;
		}
		foreach(IntKeyValuePair p in ints){
			if(p.key.Equals(key)) return true;
		}
		return false;
	}

	public static string GetString(string key){
		StringKeyValuePair pair = new StringKeyValuePair(null, null);
		foreach(StringKeyValuePair p in strings){
			if(p.key.Equals(key)){
				pair = p;
				break;
			}
		}
		if(pair.key == null) throw new Exception("The key \"" + key + "\" is not registered as a STRING-key in the PlayerPrefManager");
		return PlayerPrefs.GetString(pair.key, pair.defaultValue);
	}

	public static float GetFloat(string key){
		FloatKeyValuePair pair = new FloatKeyValuePair(null, 0f);
		foreach(FloatKeyValuePair p in floats){
			if(p.key.Equals(key)){
				pair = p;
				break;
			}
		}
		if(pair.key == null) throw new Exception("The key \"" + key + "\" is not registered as a FLOAT-key in the PlayerPrefManager");
		return PlayerPrefs.GetFloat(pair.key, pair.defaultValue);
	}

	public static int GetInt(string key){
		IntKeyValuePair pair = new IntKeyValuePair(null, 0);
		foreach(IntKeyValuePair p in ints){
			if(p.key.Equals(key)){
				pair = p;
				break;
			}
		}
		if(pair.key == null) throw new Exception("The key \"" + key + "\" is not registered as a INTEGER-key in the PlayerPrefManager");
		return PlayerPrefs.GetInt(pair.key, pair.defaultValue);
	}

	public static void SetString(string key, string value){
		foreach(StringKeyValuePair p in strings){
			if(p.key.Equals(key)){
				PlayerPrefs.SetString(key, value);
				//Debug.LogWarning("attempt to set \"" + key + "\" to \"" + value + "\" in playerprefs");
				return;
			}
		}
		throw new Exception("The key \"" + key + "\" is not registered as a STRING-key in the PlayerPrefManager");
	}

	public static void SetFloat(string key, float value){
		foreach(FloatKeyValuePair p in floats){
			if(p.key.Equals(key)){
				PlayerPrefs.SetFloat(key, value);
				//Debug.LogWarning("attempt to set \"" + key + "\" to \"" + value + "\" in playerprefs");
				return;
			}
		}
		throw new Exception("The key \"" + key + "\" is not registered as a FLOAT-key in the PlayerPrefManager");
	}

	public static void SetInt(string key, int value){
		foreach(IntKeyValuePair p in ints){
			if(p.key.Equals(key)){
				PlayerPrefs.SetInt(key, value);
				//Debug.LogWarning("attempt to set \"" + key + "\" to \"" + value + "\" in playerprefs");
				return;
			}
		}
		throw new Exception("The key \"" + key + "\" is not registered as a INTEGER-key in the PlayerPrefManager");
	}

}

struct StringKeyValuePair{
	public string key;
	public string defaultValue;
	public StringKeyValuePair(string key, string defaultValue){
		this.key = key;
		this.defaultValue = defaultValue;
	}
}

struct FloatKeyValuePair{
	public string key;
	public float defaultValue;
	public FloatKeyValuePair(string key, float defaultValue){
		this.key = key;
		this.defaultValue = defaultValue;
	}
}

struct IntKeyValuePair{
	public string key;
	public int defaultValue;
	public IntKeyValuePair(string key, int defaultValue){
		this.key = key;
		this.defaultValue = defaultValue;
	}
}
