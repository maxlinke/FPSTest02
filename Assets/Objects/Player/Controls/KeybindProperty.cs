namespace PlayerControls {

	public class KeybindProperty : IPlayerControlsProperty{

		private Category cat;
		private int number;
		private DoubleKey val;

		public Category category { get { return this.cat; } }
		public int numberInCategory { get { return number; } }
		public DoubleKey value { get { return value; } }

		public KeybindProperty (Category category, int numberInCategory, DoubleKey value) {
			this.cat = category;
			this.number = numberInCategory;
			this.val = value;
		}

	}

}
