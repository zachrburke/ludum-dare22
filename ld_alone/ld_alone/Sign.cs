using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ld_alone
{
    class Sign
    {
        String text;
        Texture2D tex;


        bool shouldDraw = false;

        /// <summary>
        /// Flag whether this sign's dialog should be displayed.
        /// </summary>
        public bool ShouldDraw
        {
            get { return shouldDraw; }
            set { shouldDraw = value; }
        }

        public Sign(String text, Texture2D tex)
        {
            this.text = text;
            this.tex = tex;
        }

        public void Draw(SpriteBatch spriteBatch)
        {

            Rectangle textBox = new Rectangle(112, 112, text.Length * 12 + 30, 40);

            spriteBatch.Begin();
            spriteBatch.Draw(tex, textBox, Color.White);
            spriteBatch.DrawString(Game1.font, text, new Vector2(140, 120), Color.White);
            spriteBatch.End();
        }
            
    }
}
