using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace bouncing_ball_simulation.Class
{
    public class TextBox
    {
        public bool active = false;
        public string text = "";

        public void Update()
        {
            if (active)
            {
                KeyboardState keyboardState = Keyboard.GetState();
                Keys[] pressedKeys = keyboardState.GetPressedKeys();
                Keys key = pressedKeys[0];

                if (key == Keys.Back && text.Length > 0)
                {
                    text = text.Remove(text.Length);
                }
                else if (char.IsLetterOrDigit((char)key))
                {
                    text += ((char)key).ToString();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, Vector2 size, Texture2D txt)
        {
            spriteBatch.DrawString(font, text, position, Color.White);
        }
    }
}