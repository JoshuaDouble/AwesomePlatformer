using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace AwesomePlatformer
{
    class Sprite
    {
        public Vector2 position = Vector2.Zero;
        List<AnimatedTexture> animations = new List<AnimatedTexture>();
        List<Vector2> animationOffsets = new List<Vector2>();

        int currentAnimation = 0;

        SpriteEffects effects = SpriteEffects.None;

        public Sprite()
        {
        }

        public void Add(AnimatedTexture animation, int xOffset = 0, int yOffset = 0)
        {
            animations.Add(animation);
            animationOffsets.Add(new Vector2(xOffset, yOffset));
        }

       
        public void Update(float deltaTime)
        {
            animations[currentAnimation].UpdateFrame(deltaTime);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            animations[currentAnimation].DrawFrame(spriteBatch,
             position + animationOffsets[currentAnimation], effects);
        }
        public void SetFlipped(bool state)
        {
            if (state == true)
                effects = SpriteEffects.FlipHorizontally;
            else
                effects = SpriteEffects.None;
        }
        public void Pause()
        {
            animations[currentAnimation].Pause();
        }
        public void Play()
        {
            animations[currentAnimation].Play();
        }

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(new Point((int)position.X, (int)position.Y),
                animations[currentAnimation].FrameSize);
            }
        }

    }
}
