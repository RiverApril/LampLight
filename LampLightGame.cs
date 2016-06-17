using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;

namespace LampLight {

	public class LampLightGame : Game {
	
		internal GraphicsDeviceManager graphics;
		internal SpriteBatch spriteBatch;

		internal World loadedWorld = null;

		internal Texture2D textureTileAtlas;
		


		public LampLightGame() {
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}
		
		protected override void Initialize() {
			// TODO: Add your initialization logic here

			Tile.initalize();

			loadedWorld = new World();
			Random rand = new Random();
			loadedWorld.generate(ref rand);

			base.Initialize();
		}
		
		protected override void LoadContent() {
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			
			using (var stream = TitleContainer.OpenStream ("tileAtlas.png")) {
		        textureTileAtlas = Texture2D.FromStream(this.GraphicsDevice, stream);
		
		    }

			//TODO: use this.Content to load your game content here 
		}
		
		protected override void Update(GameTime gameTime) {
			// For Mobile devices, this logic will close the Game when the Back button is pressed
			// Exit() is obsolete on iOS
#if !__IOS__ && !__TVOS__
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
#endif

			loadedWorld.update(this);

			base.Update(gameTime);
		}
		
		
		protected override void Draw(GameTime gameTime) {
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

			spriteBatch.Begin(SpriteSortMode.BackToFront);

			loadedWorld.draw(this);
			
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}

