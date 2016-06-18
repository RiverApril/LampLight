using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LampLight {
	class World {

		public int width { get; private set; }
		public int height { get; private set; }

		public double viewX = 1;
		public double viewY = 1;

		private TileData[,] map;

		Thread lightingThread;
		private List<Point> lightSources = new List<Point>();
		private List<Point> lightUpdates = new List<Point>();

		public bool running = false;

		

		public World() {

			running = true;

			lightingThread = new Thread(new ThreadStart(lightingFunction));
			lightingThread.Start();

		}

		public void lightingFunction() {
			while (running) {
				while (lightUpdates.Count > 0 && running) {
					//Console.WriteLine("Light");
					Point p = lightUpdates[0];
					lightUpdates.Remove(p);

					int ym = p.Y + 255;
					int xw;
					int xm;

					for (int y = Math.Max(p.Y-255, 0); y <= Math.Min(ym, height-1); y++) {
						xw = Math.Abs(y-p.Y) + 128;
						xm = p.X + xw;
						for (int x = Math.Max(p.X-xw, 0); x <= Math.Min(xm, width-1); x++) {
							map[x, y].nextLight = 0;
						}
					}

					for (int i = 0; i < lightSources.Count && running; i++) {
						Point lp = lightSources[i];
						int ld = hexDistance(lp.X, lp.Y, p.X, p.Y);
						if (ld <= 511) {
							TileData data = map[lp.X, lp.Y];
							Tile tileF = Tile.tiles[data.fIndex];
							Tile tileB = Tile.tiles[data.bIndex];
							byte le = 0;
							if (tileF.lightEmission > 0) {
								le = tileF.lightEmission;
							}
							if (data.fTransparent()) {
								if (tileB.lightEmission > 0) {
									le = Math.Max(le, tileB.lightEmission);
								}
							}
							spreadLight(lp.X, lp.Y, le);
						}
					}
					
					for (int y = Math.Max(p.Y-255, 0); y <= Math.Min(ym, height-1); y++) {
						xw = Math.Abs(y-p.Y) + 128;
						xm = p.X + xw;
						for (int x = Math.Max(p.X-xw, 0); x <= Math.Min(xm, width-1); x++) {
							map[x, y].light = map[x, y].nextLight;
						}
					}
					
					Console.WriteLine("Updated Light at {0}, {1}", p.X, p.Y);
				}
			}
		}

		public void spreadLight(int x, int y, byte light) {
			if (light > map[x, y].nextLight && running) {
				//Console.WriteLine("spread");
				map[x, y].nextLight = Math.Max(light, map[x, y].nextLight);
				byte d = Tile.tiles[map[x, y].fIndex].density;
				if (d == 0) {
					d = 1;
				}
				if (d > light) {
					light = 0;
					return;
				} else {
					light -= d;
				}
				if (x > 0) {
					spreadLight(x-1, y, light);
				}
				if (x < width-1) {
					spreadLight(x+1, y, light);
				}
				if (y > 0) {
					spreadLight(x, y-1, light);
					spreadLight(x+(y % 2 == 0 ? 1 : -1), y-1, light);
				}
				if (y < height-1) {
					spreadLight(x, y+1, light);
					spreadLight(x+(y % 2 == 0 ? 1 : -1), y+1, light);
				}
			}
		}

		public int hexDistance(int x1, int y1, int x2, int y2) {
			int xx1 = x1 + y1;
			int xx2 = x2 + y2;
			int yy1 = y1 / 2;
			int yy2 = y2 / 2;
			int zz1 = -(xx1 + yy1);
			int zz2 = -(xx2 + yy2);
			return Math.Max(xx2 - xx1, Math.Max(yy2 - yy1, zz2 - zz1));
		}

		private void lightUpdate(int x, int y) {
			Point p = new Point(x, y);
			TileData data = map[x, y];
			Tile tileF = Tile.tiles[data.fIndex];
			Tile tileB = Tile.tiles[data.bIndex];
			if (tileF.lightEmission > 0 || (data.fTransparent() && tileB.lightEmission > 0)) {		
				if (!lightSources.Contains(p)) {
					lightSources.Add(p);
					lightUpdates.Add(p);
				}
			} else {
				if (lightSources.Contains(p)) {
					lightSources.Remove(p);
					lightUpdates.Add(p);
				}
			}
		}

		internal void generate(ref Random rand) {
			width = 1000;
			height = 1000;

			map = new TileData[width, height];

			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					map[x, y] = new TileData();
					map[x, y].setF(rand.Next() % 2 == 0 ? Tile.tileStone.index : Tile.tileDirt.index, 0, TileData.ORIENT_FULL);
					map[x, y].setB(Tile.tileStone.index);
				}
			}

		}

		internal void update(LampLightGame game) {
			KeyboardState kState = Keyboard.GetState();
			MouseState mState = Mouse.GetState();
			
			int viewW = game.Window.ClientBounds.Width / Tile.TILE_H_SEP;
			int viewH = game.Window.ClientBounds.Height / Tile.TILE_V_SEP;

			if (kState.IsKeyDown(Keys.A)) {
				viewX -= .25;
				if (viewX < 1) {
					viewX = 1;
				}
			}
			
			if (kState.IsKeyDown(Keys.D)) {
				viewX += .25;
				if (viewX > width-viewW-2) {
					viewX = width-viewW-2;
				}
			}
			
			if (kState.IsKeyDown(Keys.W)) {
				viewY -= .25;
				if (viewY < 1) {
					viewY = 1;
				}
			}
			
			if (kState.IsKeyDown(Keys.S)) {
				viewY += .25;
				if (viewY > height-viewH-2) {
					viewY = height-viewH-2;
				}
			}
			int mouseTileY = (int)(((mState.Y - Tile.TILE_TEX_V_SEP_HALF) / Tile.TILE_V_SEP) + viewY);
			int mouseTileX = (int)(((mState.X) / Tile.TILE_H_SEP) + ((mouseTileY % 2==0)?-0.5:0.5) + viewX);

			//Console.WriteLine("Mouse tile: {0}, {1}", mouseTileX, mouseTileY);

			if (mState.LeftButton == ButtonState.Pressed) {

				if (mouseTileX >= 0 && mouseTileX < width && mouseTileY >= 0 && mouseTileY < height) {
					if (kState.IsKeyDown(Keys.L)) {
						map[mouseTileX, mouseTileY].setF(Tile.tileLamp.index);
					} else {
						map[mouseTileX, mouseTileY].setF(Tile.tileAir.index);
					}
				}
				
			}
			
			
			TileData data;

			int viewR = (int)viewX + game.Window.ClientBounds.Width / Tile.TILE_H_SEP;
			int viewB = (int)viewY + game.Window.ClientBounds.Height / Tile.TILE_V_SEP;
			
			for (int y = (int)viewY - 1; y < viewB + 2; y++) {
				for (int x = (int)viewX - 1; x < viewR + 2; x++) {
					data = map[x, y];
					if (data.updateFlags != TileData.UPDATE_NONE) {
						if ((data.updateFlags & TileData.UPDATE_TILE) != 0) {
							data.updateFlags -= TileData.UPDATE_TILE;
						}
						
						if ((data.updateFlags & TileData.UPDATE_LIGHT) != 0) {
							data.updateFlags -= TileData.UPDATE_LIGHT;
							lightUpdate(x, y);
							//Console.WriteLine("fIndex: {0}", Tile.tiles[data.fIndex].name);
						}
					}

				}
			}
			
			
			
		}

		internal void draw(LampLightGame game) {
			Rectangle src = new Rectangle();
			Rectangle? srcq;
			TileData data;
			Tile tileF;
			Tile tileB;
			Rectangle dest = new Rectangle();
			Color color = new Color(255, 255, 255, 255);
			int viewR = (int)viewX + game.Window.ClientBounds.Width / Tile.TILE_H_SEP;
			int viewB = (int)viewY + game.Window.ClientBounds.Height / Tile.TILE_V_SEP;

			for (int y = (int)viewY - 1; y < viewB + 2; y++) {
				dest.Y = (int)((y - viewY) * Tile.TILE_V_SEP);
				for (int x = (int)viewX - 1; x < viewR + 2; x++) {
					data = map[x, y];
					tileF = Tile.tiles[data.fIndex];

					color.R = data.light;
					color.G = data.light;
					color.B = data.light;

					if (data.fTransparent()) {
						tileB = Tile.tiles[data.bIndex];
						if (tileB.textureRect != null) {
							game.spriteBatch.Draw(game.textureTileAtlas, dest, tileB.textureRect, color);
						}
					}
					srcq = tileF.textureRect;
					
					if (srcq != null) {
						src = srcq.Value;
						dest.X = (int)((x-viewX+((y % 2==0)?0.5:0)) * Tile.TILE_H_SEP);

						
						switch (data.fOrientation) {
							case TileData.ORIENT_HALF_LEFT:
							case TileData.ORIENT_HALF_RIGHT:
								dest.Width = Tile.TILE_TEX_H_SEP_HALF;
								break;
							case TileData.ORIENT_HALF_BOTTOM:
								src.Y += Tile.TILE_TEX_V_SEP_HALF;
								dest.Y += Tile.TILE_TEX_V_SEP_HALF;
								goto default;
							default:
								dest.Width = Tile.TILE_TEX_H_SEP;
								break;
							
						}
						switch (data.fOrientation) {
							case TileData.ORIENT_HALF_TOP:
							case TileData.ORIENT_HALF_BOTTOM:
								dest.Height = Tile.TILE_TEX_V_SEP_HALF;
								break;
							case TileData.ORIENT_HALF_RIGHT:
								src.X += Tile.TILE_TEX_H_SEP_HALF;
								dest.X += Tile.TILE_TEX_H_SEP_HALF;
								goto default;
							default:
								dest.Height = Tile.TILE_TEX_V_SEP;
								break;
							
						}
						game.spriteBatch.Draw(game.textureTileAtlas, dest, src, color);
					}
				}
			}
		}

}
}