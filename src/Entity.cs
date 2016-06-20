using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LampLight {
	public abstract class Entity {

		public int uid { get; private set;}
		public bool active;
		protected LampLightGame game;

		public Vector2 position { get; protected set;}
		public Vector2 hSize { get; protected set;}

		public Entity(LampLightGame game){
			this.game = game;
			this.uid = 0;
			this.active = false;
		}

		public void initalUIDAssignment(int newUid){
			if(uid == 0) {
				uid = newUid;
				active = true;
			} else {
				Console.WriteLine("Hey now, You can't change an Entity's uid once set!");
			}
		}

		public void _update(World world, GameTime gameTime){
			if(active && uid > 0){
				update(world, gameTime);
			}
		}

		public void _draw(World world, GameTime gameTime){
			if(active && uid > 0){
				draw(world, gameTime);
			}
		}

		public void removeFromWorld(World world){
			world.removeFromWorld(this);
		}

		public abstract void update(World world, GameTime gameTime);
		public abstract void draw(World world, GameTime gameTime);

	}

}