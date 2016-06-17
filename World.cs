using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LampLight {
	class World {

		public int width { get; private set; }
		public int height { get; private set; }

		public double viewX = 1;
		public double viewY = 1;

		private TileData[,] map;

		public World() {



		}

		internal void generate(ref Random rand) {
			width = 1000;
			height = 1000;

			map = new TileData[width, height];

			int h = 20;

			for (int x = 0; x < width; x++) {
				h += rand.Next()%3 - 1;
				for (int y = 0; y < height; y++) {
					map[x, y] = new TileData();
					if (y >= h) {
						map[x, y].set(rand.Next() % (y==h?1:y-h) > 2 ? Tile.tileStone.index : Tile.tileDirt.index, 0, TileData.ORIENT_FULL);
					} else {
						map[x, y].set(Tile.tileAir.index);
					}
				}
			}

		}

		internal void update(LampLightGame game) {
			KeyboardState state = Keyboard.GetState();
			
			int viewW = game.Window.ClientBounds.Width / Tile.TILE_H_SEP;
			int viewH = game.Window.ClientBounds.Height / Tile.TILE_V_SEP;

			if (state.IsKeyDown(Keys.A)) {
				viewX -= .25;
				if (viewX < 1) {
					viewX = 1;
				}
			}
			
			if (state.IsKeyDown(Keys.D)) {
				viewX += .25;
				if (viewX > width-viewW-2) {
					viewX = width-viewW-2;
				}
			}
			
			if (state.IsKeyDown(Keys.W)) {
				viewY -= .25;
				if (viewY < 1) {
					viewY = 1;
				}
			}
			
			if (state.IsKeyDown(Keys.S)) {
				viewY += .25;
				if (viewY > height-viewH-1) {
					viewY = height-viewH-1;
				}
			}
		}

		internal void draw(LampLightGame game) {
			Rectangle src = new Rectangle();
			Rectangle? srcq;
			TileData data;
			Rectangle dest = new Rectangle();
			int viewR = (int)viewX + game.Window.ClientBounds.Width / Tile.TILE_H_SEP;
			int viewB = (int)viewY + game.Window.ClientBounds.Height / Tile.TILE_V_SEP;
			for (int x = (int)viewX-1; x < viewR+2; x++) {
				dest.X = (int)((x-viewX) * Tile.TILE_H_SEP);
				for (int y = (int)viewY-1; y < viewB+1; y++) {
					data = map[x, y];
					srcq = Tile.tiles[data.tileIndex].textureRect;
					if (srcq != null) {
						src = srcq.Value;
						dest.Y = (int)((y-viewY+((x % 2==0)?0.5:0)) * Tile.TILE_V_SEP);

						
						switch (data.orientation) {
							case TileData.ORIENT_HALF_BOTTOM:
								src.Y += Tile.TILE_TEX_V_SEP_HALF;
								dest.Y += Tile.TILE_TEX_V_SEP_HALF;
								goto default;
							default:
								dest.Width = Tile.TILE_TEX_H_SEP;
								break;
							
						}
						switch (data.orientation) {
							case TileData.ORIENT_HALF_RIGHT:
								src.X += Tile.TILE_TEX_H_SEP_HALF;
								dest.X += Tile.TILE_TEX_H_SEP_HALF;
								goto default;
							default: 
								dest.Height = Tile.TILE_TEX_V_SEP;
								break;
							
						}
						game.spriteBatch.Draw(game.textureTileAtlas, dest, src, Color.White);
					}
				}
			}
		}

}
}