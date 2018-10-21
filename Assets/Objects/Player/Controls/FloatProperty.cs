namespace PlayerControls {

	public struct FloatProperty : IPlayerControlsProperty {

		private Category cat;
		private int number;
		private float val;

		public Category category { get { return this.cat; } }
		public int numberInCategory { get { return number; } }
		public float value { get { return value; } }

		public FloatProperty (Category category, int numberInCategory, float value) {
			this.cat = category;
			this.number = numberInCategory;
			this.val = value;
		}

	}

}
