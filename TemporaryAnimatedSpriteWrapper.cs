using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Diagnostics;

namespace LoveFestival
{
    public class TemporaryAnimatedSpriteWrapper : TemporaryAnimatedSprite
    {
        public TemporaryAnimatedSpriteWrapper(Texture2D texture, Rectangle sourceRect, Vector2 position, bool flipped, float alphaFade, Color color)
            : base("", sourceRect, position, flipped, alphaFade, color)
        {
            this.texture = texture;
            Debug.WriteLine("construct");
        }
    }
}
