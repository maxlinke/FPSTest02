namespace PlayerControls {

	public class FloatProperty : PlayerControlsProperty {

		public float value;

		public FloatProperty (Category category, int numberInCategory, float value) : base(category, numberInCategory) {
			this.value = value;
		}

	}

}
