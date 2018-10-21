namespace PlayerControls {

	public abstract class PlayerControlsProperty {

		private Category cat;
		private int num;

		public Category category { get { return cat; } }
		public int numberInCategory { get { return num; } }

		public PlayerControlsProperty (Category category, int numberInCategory) {
			this.cat = category;
			this.num = numberInCategory;
		}

	}

}
