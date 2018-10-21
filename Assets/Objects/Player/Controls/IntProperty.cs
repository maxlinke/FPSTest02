namespace PlayerControls {

	public class IntProperty : PlayerControlsProperty {

		public int value;

		public IntProperty (Category category, int numberInCategory, int value) : base(category, numberInCategory) {
			this.value = value;
		}

	}

}
