using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LampLight {
	public class Tile {
		public const int TILE_TEX_V_SEP = 14;
		public const int TILE_TEX_H_SEP = 12;
		public const int TILE_TEX_V_SEP_HALF = TILE_TEX_V_SEP/2;
		public const int TILE_TEX_H_SEP_HALF = TILE_TEX_H_SEP/2;
		public const int TILE_V_SEP = 11;
		public const int TILE_H_SEP = 12;

		internal static Dictionary<byte, Tile> tiles = new Dictionary<byte, Tile>();
	
		internal static Tile tileAir;
		internal static Tile tileStone;
		internal static Tile tileDirt;
		internal static Tile tileLamp;
		internal static Tile tileGlass;
		internal static Tile tileGravel;

		public const byte DENSITY_MIN = 8;
		public const byte DENSITY_AIRLIKE = 8;
		public const byte DENSITY_GLASSLIKE = 12;
		public const byte DENSITY_SOLID = 32;

		public const byte TILE_FLAG_NONE = 0;
		public const byte TILE_FLAG_GRAVITY = 1 << 0;
		public const byte TILE_FLAG_REPLACEABLE = 1 << 1;

		public static Rectangle selectTileRect = texRect(0, 0);

		internal static void initalize() {
			byte a = 0;
			tileAir    = new Tile(a++, "Air"    , false, null         , true , DENSITY_AIRLIKE  , 0  , TILE_FLAG_REPLACEABLE);
			tileStone  = new Tile(a++, "Stone"  , true , texRect(1, 0));
			tileDirt   = new Tile(a++, "Dirt"   , true , texRect(2, 0));
			tileLamp   = new Tile(a++, "Lamp"   , true , texRect(3, 0), true, DENSITY_GLASSLIKE, 255);
			tileGlass  = new Tile(a++, "Glass"  , true , texRect(4, 0), true , DENSITY_GLASSLIKE);
			tileGravel = new Tile(a++, "Gravel" , true , texRect(5, 0), false, DENSITY_SOLID    , 0  , TILE_FLAG_GRAVITY);
		}
		

		public static Rectangle texRect(int x, int y) {
			return new Rectangle(x*TILE_TEX_H_SEP, y*TILE_TEX_V_SEP, TILE_TEX_H_SEP, TILE_TEX_V_SEP);
		}

		public byte index { get; private set; }
		public string name { get; private set; }
		public bool solid { get; private set; }
		public byte density { get; private set; } // NEVER BE 0
		public Rectangle? textureRect { get; private set; }
		public byte lightEmission { get; private set; }
		public bool transparent { get; private set; }
		public byte tileFlags { get; private set; }
		
		public Tile(byte index, string name, bool solid, Rectangle? rect, bool transparent = false, byte density = DENSITY_SOLID, byte lightEmission = 0, byte tileFlags = TILE_FLAG_NONE) {
			tiles[index] = this;
			this.index = index;
			this.name = name;
			this.solid = solid;
			this.textureRect = rect;
			this.transparent = transparent;
			this.density = Math.Max(density, DENSITY_MIN);
			this.lightEmission = lightEmission;
			this.tileFlags = tileFlags;
		}

		
		
		
	}
}