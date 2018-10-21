namespace PlayerControls {

	public class BoolProperty : PlayerControlsProperty {

		public bool value;

		public BoolProperty (Category category, int numberInCategory, bool value) : base(category, numberInCategory) {
			this.value = value;
		}

	}

}
