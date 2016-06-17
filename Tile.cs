using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LampLight {
	class Tile {
		public const int TILE_TEX_V_SEP = 8;
		public const int TILE_TEX_H_SEP = 9;
		public const int TILE_TEX_V_SEP_HALF = TILE_TEX_V_SEP/2;
		public const int TILE_TEX_H_SEP_HALF = TILE_TEX_H_SEP/2;
		public const int TILE_V_SEP = 8;
		public const int TILE_H_SEP = 7;
		public const int TILE_V_SEP_HALF = TILE_V_SEP/2;

		internal static Dictionary<byte, Tile> tiles = new Dictionary<byte, Tile>();
	
		internal static Tile tileAir;
		internal static Tile tileStone;
		internal static Tile tileDirt;
		
		internal static void initalize() {
			tileAir   = new Tile(0, "Air", false, 0, null);
			tileStone = new Tile(1, "Stone", true, 1, texRect(0, 0));
			tileDirt  = new Tile(2, "Dirt", true, 1, texRect(1, 0));
		}

		static Rectangle texRect(int x, int y) {
			return new Rectangle(x*TILE_TEX_H_SEP, y*TILE_TEX_V_SEP, TILE_TEX_H_SEP, TILE_TEX_V_SEP);
		}

		public byte index { get; private set; }
		public string name { get; private set; }
		public bool solid { get; private set; }
		public float opacity { get; private set; } 
		public Rectangle? textureRect { get; private set; }
		
		public Tile(byte index, string name, bool solid, float opacity, Rectangle? rect) {
			tiles[index] = this;
			this.index = index;
			this.name = name;
			this.solid = solid;
			this.opacity = opacity;
			this.textureRect = rect;
		}

		
		
		
	}
}