using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LampLight {
	public class World {

		public int width { get; private set; }
		public int height { get; private set; }

		public double viewX = 1;
		public double viewY = 1;

		private TileData[,] map;
		
		private const int LIGHT_CHUNK_WIDTH = (256 / Tile.DENSITY_MIN);
		private const int LIGHT_CHUNK_HEIGHT = (256 / Tile.DENSITY_MIN);

		private int LIGHT_CHUNK_HOR_QTY;
		private int LIGHT_CHUNK_VER_QTY;

		private int[] nineChunkGrid;

		Thread lightingThread;
		private List<List<Point>> lightSources = new List<List<Point>>();
		private List<Point> lightUpdates = new List<Point>();

		private Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
		private List<Entity> entitiesToAdd = new List<Entity>();
		private List<int> entitiesToRemove = new List<int>();

		public bool running = false;

		public bool debugButtonDown = false;

		Random rand = new Random();

		LampLightGame game;

		KeyboardState kState;
		MouseState mState;

		int nextUid = 1;
		

		public World(LampLightGame game) {

			this.game = game;

			running = true;
			
			width = 1024;
			height = 1024;

			LIGHT_CHUNK_HOR_QTY = height / LIGHT_CHUNK_WIDTH;
			LIGHT_CHUNK_VER_QTY = width / LIGHT_CHUNK_HEIGHT;

			nineChunkGrid = new int[9] {
			 -LIGHT_CHUNK_HOR_QTY-1, -LIGHT_CHUNK_HOR_QTY, -LIGHT_CHUNK_HOR_QTY+1
			, -1				   , 0					 , 1
			, LIGHT_CHUNK_HOR_QTY-1, LIGHT_CHUNK_HOR_QTY, LIGHT_CHUNK_HOR_QTY+1};
			
			for (int y = 0; y < height; y+=LIGHT_CHUNK_WIDTH) {
				for (int x = 0; x < width; x+=LIGHT_CHUNK_HEIGHT) {
					lightSources.Add(new List<Point>());
				}
			}

			lightingThread = new Thread(new ThreadStart(lightingFunction));
			lightingThread.Start();
		}

		public void lightingFunction() {
			while (running) {
				if(lightUpdates.Count > 0) {
					Point p = lightUpdates[0];
					//Console.WriteLine("Light");
					lightUpdates.Remove(p);

					const int range = (256 / Tile.DENSITY_MIN);
					const int range2 = 2 * range;

					int ym, xw, xm;

					int lsi = ((p.Y / LIGHT_CHUNK_VER_QTY) * (height / LIGHT_CHUNK_HEIGHT)) + (p.X / LIGHT_CHUNK_HOR_QTY);

					ym = p.Y + range;
					for (int y = Math.Max(p.Y - range, 0); y <= Math.Min(ym, height - 1); y++) {
						xw = (Math.Abs(y - p.Y) / 2) + range;
						xm = p.X + xw;
						for (int x = Math.Max(p.X - xw, 0); x <= Math.Min(xm, width - 1); x++) {
							map[x, y].nextLight = 0;
						}
					}

					for(int j = 0; j < 9; j++) {
						int lsii = nineChunkGrid[j] + lsi;
						if(lsii < 0 || lsii > lightSources.Count) {
							continue;
						}
						for(int i = 0; i < lightSources[lsii].Count && running; i++) {
							Point lp = lightSources[lsii][i];
							int ld = hexDistance(lp.X, lp.Y, p.X, p.Y);
							if(ld <= range) {
								ym = lp.Y + range;
								for(int y = Math.Max(lp.Y - range, 0); y <= Math.Min(ym, height - 1); y++) {
									xw = (Math.Abs(y - lp.Y) / 2) + range;
									xm = lp.X + xw;
									for(int x = Math.Max(p.X - xw, 0); x <= Math.Min(xm, width - 1); x++) {
										map[x, y].nextLight = 0;
									}
								}
							}
						}
					}
					for (int j = 0; j < 9; j++) {
						int lsii = nineChunkGrid[j] + lsi;
						if (lsii < 0 || lsii > lightSources.Count) {
							continue;
						}
						for(int i = 0; i < lightSources[lsii].Count && running; i++) {
							Point lp = lightSources[lsii][i];
							int ld = hexDistance(lp.X, lp.Y, p.X, p.Y);
							if(ld <= range) {
								prespreadLight(lp.X, lp.Y);
							}
						}

					}

					ym = p.Y + range2;
					for (int y = Math.Max(p.Y - range2, 0); y <= Math.Min(ym, height - 1); y++) {
						xw = (Math.Abs(y - p.Y) / 2) + range2;
						xm = p.X + xw;
						for (int x = Math.Max(p.X - xw, 0); x <= Math.Min(xm, width - 1); x++) {
							map[x, y].light = map[x, y].nextLight;
						}
					}

					//Console.WriteLine("Updated Light at {0}, {1}", p.X, p.Y);
				
				}
			}
		}

		/*public void worldToTile(int wx, int wy, out int tx, out int ty) {

		}*/

		public void screenToWorld(int sx, int sy, out int wx, out int wy) {
			wx = sx + (int)(viewX * Tile.TILE_H_SEP);
			wy = sy + (int)(viewY * Tile.TILE_V_SEP);
		}

		public void screenToTile(int sx, int sy, out int tx, out int ty) {
			ty = (int)(((sy - Tile.TILE_TEX_V_SEP_HALF) / Tile.TILE_V_SEP) + viewY);
			tx = (int)(((sx) / Tile.TILE_H_SEP) + ((ty % 2==0)?-0.5:0.5) + viewX);
		}

		public void tileToScreen(int tx, int ty, out int sx, out int sy) {
			sx = (int)((tx-viewX+((ty % 2==0)?0.5:0)) * Tile.TILE_H_SEP);
			sy = (int)((ty - viewY) * Tile.TILE_V_SEP);
		}

		void prespreadLight(int x, int y) {
			TileData data = map[x, y];
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
			spreadLight(x, y, le);
		}

		private void precalcLight() {
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					updateLightSource(x, y);
				}
			}
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					prespreadLight(x, y);
					map[x, y].updateFlags -= TileData.UPDATE_LIGHT;
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
				if (x > 0 && x < width-1 && y > 0 && y < height-1) {
					spreadLight(x-1, y, light);
					spreadLight(x+1, y, light);
					spreadLight(x, y-1, light);
					spreadLight(x+(y % 2 == 0 ? 1 : -1), y-1, light);
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

		private void updateLightSource(int x, int y) {
			Point p = new Point(x, y);
			TileData data = map[x, y];
			Tile tileF = Tile.tiles[data.fIndex];
			Tile tileB = Tile.tiles[data.bIndex];
			int lsi = ((p.Y/LIGHT_CHUNK_VER_QTY) * (height / LIGHT_CHUNK_HEIGHT)) + (p.X/LIGHT_CHUNK_HOR_QTY);
			if (tileF.lightEmission > 0 || (data.fTransparent() && tileB.lightEmission > 0)) {		
				if (!lightSources[lsi].Contains(p)) {
					lightSources[lsi].Add(p);
				}
			} else {
				if (lightSources[lsi].Contains(p)) {
					lightSources[lsi].Remove(p);
				}
			}
		}

		private void lightUpdate(int x, int y) {
			updateLightSource(x, y);
			Point p = new Point(x, y);
			if (!lightUpdates.Contains(p)) {
				lightUpdates.Add(p);
			}
		}

		void tileSpread(int x, int y, int i, Func<int, int, int, int> p) {
			i = p(x, y, i);
			if (i > 0) {
				if (x > 0 && x < width-1 && y > 0 && y < height-1) {
					tileSpread(x-1, y, i, p);
					tileSpread(x+1, y, i, p);
					tileSpread(x, y-1, i, p);
					tileSpread(x+(y % 2 == 0 ? 1 : -1), y-1, i, p);
					tileSpread(x, y+1, i, p);
					tileSpread(x+(y % 2 == 0 ? 1 : -1), y+1, i, p);
				}
			}
		}

		internal void generate() {

			map = new TileData[width, height];

			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					map[x, y] = new TileData();
					map[x, y].setF(Tile.tileStone.index);
					map[x, y].setB(Tile.tileStone.index);
				}
			}

			int dirtPatches = 1000;

			for (int c = 0; c < dirtPatches; c++) {
				int xc = rand.Next() % width;
				int yc = rand.Next() % height;

				int caveSize = 5 + rand.Next(100);

				tileSpread(xc, yc, caveSize, delegate (int x, int y, int i) {
					if (map[x, y].fIndex == Tile.tileDirt.index) {
						return 0;
					} else {
						map[x, y].setF(Tile.tileDirt.index);
						return i-((rand.Next()%3)+1);
					}
				});
				
			}

			int gravelPatches = 1000;

			for (int c = 0; c < gravelPatches; c++) {
				int xc = rand.Next() % width;
				int yc = rand.Next() % height;

				int caveSize = 5 + rand.Next(40);

				tileSpread(xc, yc, caveSize, delegate (int x, int y, int i) {
					if (map[x, y].fIndex == Tile.tileGravel.index) {
						return 0;
					} else {
						map[x, y].setF(Tile.tileGravel.index);
						return i-((rand.Next()%3)+1);
					}
				});
				
			}

			int caveCount = 1000;

			for (int c = 0; c < caveCount; c++) {
				int xc = rand.Next() % width;
				int yc = rand.Next() % height;

				int caveSize = 5 + rand.Next(200);

				tileSpread(xc, yc, caveSize, delegate (int x, int y, int i) {
					if (map[x, y].fIndex == Tile.tileAir.index) {
						return 0;
					} else {
						map[x, y].setF(Tile.tileAir.index);
						return i-((rand.Next()%3)+1);
					}
				});
				
			}

			precalcLight();

		}

		internal bool tileUpdate(int x, int y, bool direct, bool setup = false) {
			if (direct && !setup && x > 0 && x < width-1 && y > 0 && y < height-1) {
				map[x-1, y].updateFlags |= TileData.UPDATE_INDIRECT_NEXT;
				map[x+1, y].updateFlags |= TileData.UPDATE_INDIRECT_NEXT;
				map[x, y-1].updateFlags |= TileData.UPDATE_INDIRECT_NEXT;
				map[x+(y % 2 == 0 ? 1 : -1), y-1].updateFlags |= TileData.UPDATE_INDIRECT_NEXT;
				map[x, y+1].updateFlags |= TileData.UPDATE_INDIRECT_NEXT;
				map[x+(y % 2 == 0 ? 1 : -1), y+1].updateFlags |= TileData.UPDATE_INDIRECT_NEXT;
			}
			TileData data = map[x, y];
			Tile tileF = Tile.tiles[data.fIndex];
			Tile tileB = Tile.tiles[data.bIndex];
			bool l = rand.Next() % 2==0;
			bool r = false;
			if ((tileF.tileFlags & Tile.TILE_FLAG_GRAVITY) != 0) {
				if (x > 0 && x < width - 1 && y < height - 1) {
					if (l && (Tile.tiles[map[x, y+1].fIndex].tileFlags & Tile.TILE_FLAG_REPLACEABLE) != 0) {
						map[x, y + 1].setF(data.fIndex, data.fMetadata, data.fOrientation);
						map[x, y].setF(Tile.tileAir.index);
						r = true;
					}
					if (!l && (Tile.tiles[map[x + (y % 2 == 0 ? 1 : -1), y+1].fIndex].tileFlags & Tile.TILE_FLAG_REPLACEABLE) != 0) {
						map[x + (y % 2 == 0 ? 1 : -1), y + 1].setF(data.fIndex, data.fMetadata, data.fOrientation);
						map[x, y].setF(Tile.tileAir.index);
						r = true;
					}
				}
			}
			if ((tileB.tileFlags & Tile.TILE_FLAG_GRAVITY) != 0) {
				if (x > 0 && x < width - 1 && y < height - 1) {
					if (l && (Tile.tiles[map[x, y+1].bIndex].tileFlags & Tile.TILE_FLAG_REPLACEABLE) != 0) {
						map[x, y + 1].setB(data.bIndex);
						map[x, y].setB(Tile.tileAir.index);
						r = true;
					}
					if (!l && (Tile.tiles[map[x + (y % 2 == 0 ? 1 : -1), y+1].bIndex].tileFlags & Tile.TILE_FLAG_REPLACEABLE) != 0) {
						map[x + (y % 2 == 0 ? 1 : -1), y + 1].setB(data.bIndex);
						map[x, y].setB(Tile.tileAir.index);
						r = true;
					}
				}
			}
			return r;
		}

		internal void update(GameTime gameTime) {
			kState = Keyboard.GetState();
			mState = Mouse.GetState();
			
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
			int mouseTileX, mouseTileY;

			screenToTile(mState.X, mState.Y, out mouseTileX, out mouseTileY);
			
			//Console.WriteLine("Mouse tile: {0}, {1}", mouseTileX, mouseTileY);

			if (mState.LeftButton == ButtonState.Pressed) {

				if (mouseTileX >= 0 && mouseTileX < width && mouseTileY >= 0 && mouseTileY < height) {
					if (kState.IsKeyDown(Keys.L)) {
						map[mouseTileX, mouseTileY].setF(Tile.tileLamp.index);
					} else if (kState.IsKeyDown(Keys.G)) {
						map[mouseTileX, mouseTileY].setF(Tile.tileGlass.index);
					} else {
						map[mouseTileX, mouseTileY].setF(Tile.tileAir.index);
					}
				}
				
			}

			debugButtonDown = kState.IsKeyDown(Keys.OemTilde);
			
			
			TileData data;

			int viewR = (int)viewX + game.Window.ClientBounds.Width / Tile.TILE_H_SEP;
			int viewB = (int)viewY + game.Window.ClientBounds.Height / Tile.TILE_V_SEP;
			
			for (int y = (int)viewY - 1; y < viewB + 2; y++) {
				for (int x = (int)viewX - 1; x < viewR + 2; x++) {
					data = map[x, y];
					if (data.updateFlags != TileData.UPDATE_NONE) {
						if ((data.updateFlags & TileData.UPDATE_TILE) != 0) {
							data.updateFlags -= TileData.UPDATE_TILE;
							tileUpdate(x, y, true);
						}
						
						if ((data.updateFlags & TileData.UPDATE_INDIRECT) != 0) {
							data.updateFlags -= TileData.UPDATE_INDIRECT;
							tileUpdate(x, y, false);
						}
						
						if ((data.updateFlags & TileData.UPDATE_INDIRECT_NEXT) != 0) {
							data.updateFlags -= TileData.UPDATE_INDIRECT_NEXT;
							data.updateFlags |= TileData.UPDATE_INDIRECT;
						}
						
						if ((data.updateFlags & TileData.UPDATE_LIGHT) != 0) {
							data.updateFlags -= TileData.UPDATE_LIGHT;
							lightUpdate(x, y);
						}
					}

				}
			}


			while(entitiesToAdd.Count > 0){
				entitiesToAdd[0].initalUIDAssignment(nextUid++);
				entities.Add(entitiesToAdd[0].uid, entitiesToAdd[0]);
			}

			while(entitiesToRemove.Count > 0){
				int eid = entitiesToRemove[0];
				entities.Remove(eid);
				entitiesToRemove.Remove(eid);
			}

			foreach(Entity e in entities.Values){
				e._update(this, gameTime);
			}
			
			
			
		}

		internal void draw(GameTime gameTime) {
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
					
					dest.X = (int)((x-viewX+((y % 2==0)?0.5:0)) * Tile.TILE_H_SEP);

					if (data.fTransparent()) {
						tileB = Tile.tiles[data.bIndex];
						if (tileB.textureRect != null) {
							color.R = (byte)(data.light/2);
							color.G = (byte)(data.light/2);
							color.B = (byte)(data.light/2);
							game.spriteBatch.Draw(game.textureTileAtlas, dest, tileB.textureRect, color);
						}
					}
					srcq = tileF.textureRect;

					color.R = data.light;
					color.G = data.light;
					color.B = data.light;
					
					if (srcq != null) {
						src = srcq.Value;

						
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

			foreach(Entity e in entities.Values){
				e._draw(this, gameTime);
			}



			Point wp = new Point();
			screenToWorld(mState.X, mState.Y, out wp.X, out wp.Y);
			Point tp = new Point();
			screenToTile(mState.X, mState.Y, out tp.X, out tp.Y);
			Point tsp = new Point();
			tileToScreen(tp.X, tp.Y, out tsp.X, out tsp.Y);

			game.spriteBatch.Draw(game.textureTileAtlas, tsp.ToVector2(), Tile.selectTileRect, Color.White);

			Vector2 v = mState.Position.ToVector2();

			game.spriteBatch.DrawString(game.gameFont, string.Format("World: {0}, {1}", wp.X, wp.Y), v, Color.White);
			v.Y += 12;
			game.spriteBatch.DrawString(game.gameFont, string.Format("Tile: {0}, {1}", tp.X, tp.Y), v, Color.White);
			v.Y += 12;
			game.spriteBatch.DrawString(game.gameFont, string.Format("LightUpdates: {0}", lightUpdates.Count), v, Color.White);
		}

		public void removeFromWorld(Entity e){
			e.active = false;
			entitiesToRemove.Add(e.uid);
		}

	}
}