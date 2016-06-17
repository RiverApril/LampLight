using System;

namespace LampLight {
	class TileData {
		public const byte ORIENT_FULL = 0;
		public const byte ORIENT_HALF_BOTTOM = 1;
		public const byte ORIENT_HALF_TOP = 2;
		public const byte ORIENT_HALF_LEFT = 3;
		public const byte ORIENT_HALF_RIGHT = 4;

		public byte tileIndex = 0;
		public byte metadata = 0;
		public byte orientation = ORIENT_FULL;

		internal void set(byte index, byte meta = 0, byte orient = ORIENT_FULL) {
			this.tileIndex = index;
			this.metadata = meta;
			this.orientation = orient;
		}
	}
}