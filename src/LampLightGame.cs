using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LampLight {

	public class LampLightGame : Game {
	
		internal GraphicsDeviceManager graphics;
		internal SpriteBatch spriteBatch;

		internal World loadedWorld = null;
		
		internal SpriteFont gameFont;
		internal Texture2D textureTileAtlas;
		internal Texture2D texturePlayer;


		float fps = 0f;
		const int SAMPLE_QTY = 50;
		int[] samples = new int[SAMPLE_QTY];
		int currentSample = 0;
		int ticksAggregate = 0;
		int secondSinceStart = 0;
		


		public LampLightGame() {
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}
		
		protected override void Initialize() {
			base.Initialize();

			//this.IsMouseVisible = true;
			graphics.SynchronizeWithVerticalRetrace = true;
			TargetElapsedTime = new TimeSpan(TimeSpan.TicksPerSecond / 60);

			Mouse.WindowHandle = Window.Handle;
			
			Tile.initalize();

			loadedWorld = new World(this);
			loadedWorld.generate();
		}
		
		protected override void LoadContent() {
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			textureTileAtlas = this.Content.Load<Texture2D>("tileAtlas");
			texturePlayer = this.Content.Load<Texture2D>("player");
			gameFont = this.Content.Load<SpriteFont>("gameFont");

			//TODO: use this.Content to load your game content here 
		}
		
		protected override void Dispose(bool disposing) {
			Console.WriteLine("exiting...");
			loadedWorld.running = false;
			base.Dispose(disposing);
		}
		
		protected override void Update(GameTime gameTime) {
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			loadedWorld.update(gameTime);

			base.Update(gameTime);
		}

		private int sumArray(int[] array) {
			int val = 0;
			for (int i = 0; i < array.Length;i++) {
				val += array[i];
			}
			return val;
		}
		
		
		protected override void Draw(GameTime gameTime) {
			//FPS calc:
			samples[currentSample++] = (int)gameTime.ElapsedGameTime.Ticks;
			ticksAggregate += (int)gameTime.ElapsedGameTime.Ticks;
			if (ticksAggregate > TimeSpan.TicksPerSecond) {
				ticksAggregate -= (int)TimeSpan.TicksPerSecond;
				secondSinceStart++;
			}
			if (currentSample == SAMPLE_QTY) {
				float averageFrameTime = (float)sumArray(samples) / SAMPLE_QTY;
				fps = TimeSpan.TicksPerSecond / averageFrameTime;
				currentSample = 0;
			}
		
			//
			graphics.GraphicsDevice.Clear(Color.Black);

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			loadedWorld.draw(gameTime);

			spriteBatch.DrawString(gameFont, string.Format("FPS: {0}", fps.ToString("00")), new Vector2(0, 0), Color.White);
			
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}

