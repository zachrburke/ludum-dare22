using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ld_alone
{
    class Player : AnimatedTexture
    {

        public Texture2D idle;

        /// <summary>
        /// Get the players Idle Texture.
        /// </summary>
        public Texture2D Idle { get{return idle;} }

        private bool isMoving = false;

        /// <summary>
        /// Toggle whether the hero is moving or not.
        /// </summary>
        public bool IsMoving {
            get{return isMoving;}
            set{isMoving = value;}
        }

        public Player(Texture2D idle, Texture2D moving)
            : base(moving, 2, 50, false)
        {

            this.idle = idle;
        }

        /// <summary>
        /// Handle the players logic.
        /// </summary>
        /// <param name="gameTime">Snapshot of current gametime</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Draw the hero sprite.  If the player is moving, draw the animated
        /// sprite.  Otherwise, draw the idle sprite.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="color"></param>
        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (isMoving) {
                base.Draw(spriteBatch, color);
            }
            else {
                spriteBatch.Begin();
                spriteBatch.Draw(idle, Position, color);
                spriteBatch.End();
            }
        }


          
    }
}
