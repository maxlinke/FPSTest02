using UnityEngine;

namespace PlayerControls {

	public class KeybindProperty : PlayerControlsProperty {

		public DoubleKey value;

		public KeybindProperty (Category category, int numberInCategory, DoubleKey value) : base(category, numberInCategory) {
			this.value = value;
		}

		public KeybindProperty (Category category, int numberInCategory, KeyCode kcode) : base(category, numberInCategory) {
			this.value = new DoubleKey(kcode);
		}

	}

}
