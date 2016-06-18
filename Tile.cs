using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LampLight {
	class Tile {
		public const int TILE_TEX_V_SEP = 9;
		public const int TILE_TEX_H_SEP = 8;
		public const int TILE_TEX_V_SEP_HALF = TILE_TEX_V_SEP/2;
		public const int TILE_TEX_H_SEP_HALF = TILE_TEX_H_SEP/2;
		public const int TILE_V_SEP = 7;
		public const int TILE_H_SEP = 8;

		internal static Dictionary<byte, Tile> tiles = new Dictionary<byte, Tile>();
	
		internal static Tile tileAir;
		internal static Tile tileStone;
		internal static Tile tileDirt;
		internal static Tile tileLamp;

		internal static void initalize() {
			tileAir =   new Tile(0, "Air"  , false, null         , true, 16);
			tileStone = new Tile(1, "Stone", true , texRect(1, 0));
			tileDirt =  new Tile(2, "Dirt" , true , texRect(2, 0));
			tileLamp =  new Tile(3, "Lamp" , true , texRect(3, 0), false, 16, 255);
		}
		

		static Rectangle texRect(int x, int y) {
			return new Rectangle(x*TILE_TEX_H_SEP, y*TILE_TEX_V_SEP, TILE_TEX_H_SEP, TILE_TEX_V_SEP);
		}

		public byte index { get; private set; }
		public string name { get; private set; }
		public bool solid { get; private set; }
		public byte density { get; private set; }  //NEVER BE 0
		public Rectangle? textureRect { get; private set; }
		public byte lightEmission { get; private set; }
		public bool transparent { get; private set; }
		
		public Tile(byte index, string name, bool solid, Rectangle? rect, bool transparent = false, byte density = 64, byte lightEmission = 0) {
			tiles[index] = this;
			this.index = index;
			this.name = name;
			this.solid = solid;
			this.textureRect = rect;
			this.transparent = transparent;
			this.density = density;
			this.lightEmission = lightEmission;
		}

		
		
		
	}
}