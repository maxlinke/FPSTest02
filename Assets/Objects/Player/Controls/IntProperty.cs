namespace PlayerControls {

	public class IntProperty : IPlayerControlsProperty{

		private Category cat;
		private int number;
		private int val;

		public Category category { get { return this.cat; } }
		public int numberInCategory { get { return number; } }
		public int value { get { return value; } }

		public IntProperty (Category category, int numberInCategory, int value) {
			this.cat = category;
			this.number = numberInCategory;
			this.val = value;
		}

	}

}
