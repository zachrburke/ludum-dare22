using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ld_alone
{
    delegate void InputHandler(GameTime gameTime);

    /// <summary>
    /// class for adding input functionality to a game state
    /// </summary>
    class InputController
    {
        Dictionary<Keys, InputHandler> inputs;

        /// <summary>
        /// Constructor
        /// </summary>
        public InputController()
        {
            inputs = new Dictionary<Keys, InputHandler>();
        }

        /// <summary>
        /// Check if any keys being held down have a function and run them
        /// if they do
        /// </summary>
        /// <param name="gameTime"></param>
        public int Handle(GameTime gameTime)
        {
            Keys[] keys = Keyboard.GetState().GetPressedKeys();

            foreach (Keys key in keys)
            {
                if (inputs.ContainsKey(key))
                {
                    inputs[key].DynamicInvoke(gameTime);
                }
            }

            return Keyboard.GetState().GetPressedKeys().Length;
        }

        /// <summary>
        /// Add functionality to a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="inputHandler"></param>
        public void Add(Keys key, InputHandler inputHandler)
        {
            inputs.Add(key, inputHandler);
        }
    }
}
