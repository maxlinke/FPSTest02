namespace PlayerControls {

	public struct BoolProperty : IPlayerControlsProperty{

		private Category cat;
		private int number;
		private bool val;

		public Category category { get { return this.cat; } }
		public int numberInCategory { get { return number; } }
		public bool value { get { return value; } }

		public BoolProperty (Category category, int numberInCategory, bool value) {
			this.cat = category;
			this.number = numberInCategory;
			this.val = value;
		}

	}

}