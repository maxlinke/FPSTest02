using UnityEngine;

public static class KeyCodeUtils {

	public enum KeyCodeType {
		KEYBOARD,
		NUMBERKEY,
		NUMPAD,
		MOUSE,
		CONTROLLER
	}

	public static KeyCode ParseFromString (string s) {
		return (KeyCode)System.Enum.Parse(typeof(KeyCode), s);
	}

	public static string ToString (KeyCode kcode) {
		KeyCodeType type = DetermineType(kcode);
		switch(type){
		case KeyCodeType.KEYBOARD:
			return KeyboardKeyToString(kcode);
		case KeyCodeType.NUMBERKEY:
			return kcode.ToString().Remove(0, "Alpha".Length);
		case KeyCodeType.NUMPAD:
			return "Num" + NumpadKeyToString(kcode);
		case KeyCodeType.MOUSE:
			return MouseButtonToString(kcode);
		case KeyCodeType.CONTROLLER:
			return ControllerButtonToString(kcode);
		default:
			return kcode.ToString();
		}
	}

	static string KeyboardKeyToString (KeyCode kcode) {
		switch(kcode){
		case KeyCode.Ampersand: return "&";
		case KeyCode.Asterisk: return "*";
		case KeyCode.At: return "@";
		case KeyCode.BackQuote: return "`";
		case KeyCode.Backslash: return "\\";	//escaping the escape character here
		case KeyCode.Caret: return "^";
		case KeyCode.Colon: return ":";
		case KeyCode.Comma: return ",";
		case KeyCode.Dollar: return "$";
		case KeyCode.DoubleQuote: return "\"";	//escaping the double quote
		case KeyCode.DownArrow: return "Down";
		case KeyCode.Equals: return "=";
		case KeyCode.Escape: return "Esc";
		case KeyCode.Exclaim: return "!";
		case KeyCode.Greater: return ">";
		case KeyCode.Hash: return "#";
		case KeyCode.LeftAlt: return "LAlt";
		case KeyCode.LeftArrow: return "Left";
		case KeyCode.LeftBracket: return "[";
		case KeyCode.LeftControl: return "LCtrl";
		case KeyCode.LeftParen: return "(";
		case KeyCode.LeftShift: return "LShift";
		case KeyCode.Less: return "<";
		case KeyCode.Minus: return "-";
		case KeyCode.Period: return ".";
		case KeyCode.Plus: return "+";
		case KeyCode.Question: return "?";
		case KeyCode.Quote: return "'";
		case KeyCode.RightAlt: return "RAlt";
		case KeyCode.RightArrow: return "Right";
		case KeyCode.RightBracket: return "]";
		case KeyCode.RightControl: return "RCtrl";
		case KeyCode.RightParen: return ")";
		case KeyCode.RightShift: return "RShift";
		case KeyCode.Semicolon: return ";";
		case KeyCode.Slash: return "/";
		case KeyCode.Underscore: return "_";
		case KeyCode.UpArrow: return "Up";
		default: return kcode.ToString();
		}
	}

	static string NumpadKeyToString (KeyCode kcode) {
		switch(kcode){
		case KeyCode.KeypadDivide: return "/";
		case KeyCode.KeypadEquals: return "=";
		case KeyCode.KeypadMinus: return "-";
		case KeyCode.KeypadMultiply: return "*";
		case KeyCode.KeypadPeriod: return ".";
		case KeyCode.KeypadPlus: return "+";
		default: return kcode.ToString().Remove(0, "Keypad".Length);	//"0"-"9" and "Enter"
		}
	}

	static string MouseButtonToString (KeyCode kcode) {
		switch(kcode){
		case KeyCode.Mouse0: return "LMB";
		case KeyCode.Mouse1: return "RMB";
		case KeyCode.Mouse2: return "MMB";
		default: 
			int number = int.Parse(kcode.ToString().Remove(0, "Mouse".Length));		//no tryparse because i test for startswith when determining the type
			return "Mouse" + (number + 1);
		}
	}

	static string ControllerButtonToString (KeyCode kcode) {
		int number = int.Parse(RemoveControllerNumber(kcode).ToString().Remove(0, "JoystickButton".Length));
		switch(number){
		case 0: return "A Button";
		case 1: return "B Button";
		case 2: return "X Button";
		case 3: return "Y Button";
		case 4: return "Left Bumper";
		case 5: return "Right Bumper";
		case 6: return "Back Button";
		case 7: return "Start Button";
		case 8: return "Left Stick";
		case 9: return "Right Stick";
		default: return "Button " + (number + 1);
		}
	}

	public static KeyCode RemoveControllerNumber (KeyCode kcode) {
		if(DetermineType(kcode) != KeyCodeType.CONTROLLER){
			return kcode;
		}else{
			string s = kcode.ToString();
			s = s.Remove(0, "Joystick".Length);
			if(s.StartsWith("Button")){
				return kcode;
			}else{
				s = s.Remove(0, 1);		//only "0"-"8" can be here, so removing 1 char is enough
				string fullName = "Joystick" + s;	//s will be "ButtonX" with X being any number
				return ParseFromString(fullName);
			}
		}
	}

	public static KeyCodeType DetermineType (KeyCode kcode) {
		string s = kcode.ToString();
		if(s.StartsWith("Keypad")) return KeyCodeType.NUMPAD;
		else if(s.StartsWith("Joystick")) return KeyCodeType.CONTROLLER;
		else if(s.StartsWith("Mouse")) return KeyCodeType.MOUSE;
		else if(s.StartsWith("Alpha")) return KeyCodeType.NUMBERKEY;
		else return KeyCodeType.KEYBOARD;
	}

}
