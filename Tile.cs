namespace LampLight {
	class Tile {
	
		internal static Tile tileAir;
		internal static Tile tileStone;
		internal static Tile tileDirt;
		
		internal static void initalize() {
			int index = 0;
			tileAir = new Tile(index++, "Air", false, 0);
			tileStone = new Tile(index++, "Stone", true, 1);
			tileDirt = new Tile(index++, "Dirt", true, 1);
		}
		
		public int index { get; private set; }
		public string name { get; private set; }
		public bool solid { get; private set; }
		public float opacity { get; private set; } // 0.0 - 1.0
		
		public Tile(int index, string name, bool solid, float opacity) {
			this.index = index;
			this.name = name;
			this.solid = solid;
			this.opacity = opacity;
		}

		
		
		
	}
}