using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ld_alone
{
    /// <summary>
    /// Stores an animated Texture from one evenly spaced image file
    /// </summary>
    class AnimatedTexture
    {
        public int nFrames;
        public int msDelay;
        int currFrame;

        int width;
        int height;

        float timer;
        bool vert;

        Rectangle frame;

        public Rectangle Frame
        {
            get { return frame; }
        }

        public int NumFrames
        {
            get { return nFrames; }
        }

        private Texture2D texture;

        public Texture2D Texture
        {
            get { return texture; }
        }

        public Vector2 Position;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tex">Texture to be animated</param>
        /// <param name="numFrames">number of frames to be animated</param>
        /// <param name="FPS">time between switching frames</param>
        /// <param name="Vert">true if vertically spaced, false for horizontal</param>
        public AnimatedTexture(Texture2D tex, int numFrames, int FPS, bool Vert)
        {
            texture = tex;
            nFrames = numFrames;
            msDelay = FPS;
            vert = Vert;

            //Set the width of each frame by dividing the Texture by the number of frames
            if (!Vert)
            {
                width = texture.Width / nFrames;
                height = texture.Height;
            }
            else
            {
                width = texture.Width;
                height = texture.Height / nFrames;
            }

            //Create the actual frame
            frame = new Rectangle(0, 0, width, height);

            //Initial Position
            Position = new Vector2(400, 300);

        }

        public virtual void Update(GameTime gametime)
        { 
            //Increment timer
            timer += gametime.ElapsedGameTime.Milliseconds;
  
            //Check if it's time to switch frames
            if (timer >= msDelay)
            {
                //reset timer
                timer = 0;
    
                //return to the first frame if there are no more frames
                if (currFrame++ == nFrames - 1)
                    currFrame = 0;
  
                //shift Position of the frame
                frame.X = currFrame * width;
                frame.Y = 0;
            }
        }

        //Draw the part of the Texture specified by the frame
        public virtual void Draw(SpriteBatch spriteBatch, Color color)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, Position, frame, color);
            spriteBatch.End();
        }

    }
}
