using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LampLight {
	public class EntityPlayer : Entity{

		Rectangle animRectRender;
		Rectangle animRectStand = new Rectangle(7*0, 15*0, 7, 15);
		Rectangle animRectJump = new Rectangle(7*1, 15*0, 7, 15);
		Rectangle animRectFall = new Rectangle(7*2, 15*0, 7, 15);
		Rectangle[] animRectWalk = {
			new Rectangle(7 * 0, 15 * 1, 7, 15),
			new Rectangle(7 * 1, 15 * 1, 7, 15),
			new Rectangle(7 * 2, 15 * 1, 7, 15),
			new Rectangle(7 * 3, 15 * 1, 7, 15)
		};

		public EntityPlayer(LampLightGame game) : base(game){
			hSize = new Vector2(7/2.0f, 15/2.0f);
			animRectRender = new Rectangle(0, 0, 7, 15);
		}

		public override void draw(World world, GameTime gameTime) {
			game.spriteBatch.Draw(game.texturePlayer, position - hSize - world.view, animRectStand, Color.White);
		}

		public override void update(World world, GameTime gameTime) {

		}
	}
}

