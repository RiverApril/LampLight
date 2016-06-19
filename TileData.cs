using System;

namespace LampLight {
	class TileData {
		public const byte ORIENT_FULL = 0;
		public const byte ORIENT_HALF_BOTTOM = 1;
		public const byte ORIENT_HALF_TOP = 2;
		public const byte ORIENT_HALF_LEFT = 3;
		public const byte ORIENT_HALF_RIGHT = 4;

		public const byte UPDATE_NONE = 0;
		public const byte UPDATE_TILE = 1 << 0;
		public const byte UPDATE_LIGHT = 1 << 1;
		public const byte UPDATE_INDIRECT = 1 << 2;
		public const byte UPDATE_INDIRECT_NEXT = 1 << 3;

		public byte fIndex = 0;
		public byte fMetadata = 0;
		public byte fOrientation = ORIENT_FULL;
		public byte bIndex = 0;
		public byte light = 0;
		public byte nextLight = 0;
		public byte updateFlags = UPDATE_NONE;

		internal bool fTransparent() {
			return fOrientation != ORIENT_FULL || Tile.tiles[fIndex].transparent;
		}

		internal bool bTransparent() {
			return Tile.tiles[bIndex].transparent;
		}

		internal bool bothTransparent() {
			return fTransparent() && bTransparent();
		}

		internal void setF(byte index, byte meta = 0, byte orient = ORIENT_FULL) {
			this.fIndex = index;
			this.fMetadata = meta;
			this.fOrientation = orient;
			this.updateFlags |= UPDATE_TILE;
			this.updateFlags |= UPDATE_LIGHT;
		}

		internal void setB(byte index) {
			this.bIndex = index;
			this.updateFlags |= UPDATE_TILE;
			this.updateFlags |= UPDATE_LIGHT;
		}
	}
}