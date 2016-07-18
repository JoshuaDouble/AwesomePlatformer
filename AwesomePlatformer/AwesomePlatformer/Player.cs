using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;using Microsoft.Xna.Framework.Input;
namespace AwesomePlatformer
{
    class Player
    {
        Sprite sprite = new Sprite();

        Game1 game = null;
        bool isFalling = true;
        bool isJumping = false;
        SoundEffect jumpSound;
        SoundEffectInstance jumpSoundInstance;        Vector2 velocity = Vector2.Zero;
        Vector2 position = Vector2.Zero;


        public bool canTakeDamage = true;

        bool autoJump = true;
        public Vector2 Velocity
        {
            get { return velocity; }
        }
        public Rectangle Bounds
        {
            get { return sprite.Bounds; }
        }
        public bool IsJumping
        {
            get { return isJumping; }
        }
        public void JumpOnCollision()
        {
            autoJump = true;
        }

        
        public Vector2 Position
        {
            get { return sprite.position; }
        }
        public Player(Game1 game)
        {
            this.game = game;
            isFalling = true;
            isJumping = false;
            velocity = Vector2.Zero;
            position = Vector2.Zero;
            sprite.position = new Vector2(Game1.tile*27, Game1.tile*125);
        }
        public void Load(ContentManager content)
        {
            AnimatedTexture animation = new AnimatedTexture(new Vector2(23,36), 0, 2f, 1);
            animation.Load(content, "walk", 12, 20);

            jumpSound = content.Load<SoundEffect>("Jump");
            jumpSoundInstance = jumpSound.CreateInstance();

            sprite.Add(animation, 0, -5);            sprite.Pause();
            

        }
        public void Update(float deltaTime)
        {
            UpdateInput(deltaTime);
            sprite.Update(deltaTime);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            sprite.Draw(spriteBatch);
        }

        
        
        private void UpdateInput(float deltaTime)
        {
            bool wasMovingLeft = velocity.X < 0;
            bool wasMovingRight = velocity.X > 0;
            bool falling = isFalling;
            Vector2 acceleration = new Vector2(0, Game1.gravity);

            if (Keyboard.GetState().IsKeyDown(Keys.Left) == true)
            {
                acceleration.X -= Game1.acceleration;
                sprite.SetFlipped(true);
                sprite.Play();
            }
            else if (wasMovingLeft == true)
            {
                acceleration.X += Game1.friction;             
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right) == true)
            {
                acceleration.X += Game1.acceleration;
                sprite.SetFlipped(false);
                sprite.Play();
            }
            else if (wasMovingRight == true)
            {
                acceleration.X -= Game1.friction;
            }
            if ((Keyboard.GetState().IsKeyDown(Keys.Up) == true &&
           this.isJumping == false && falling == false) || 
                autoJump == true)
            {
                autoJump = false;
                acceleration.Y -= Game1.jumpImpulse;
                this.isJumping = true;
                jumpSoundInstance.Play();
            }
            // integrate the forces to calculate the new position and velocity
            velocity += acceleration * deltaTime;
            // clamp the velocity so the player doesn't go too fast
            velocity.X = MathHelper.Clamp(velocity.X,
           -Game1.maxVelocity.X, Game1.maxVelocity.X);
            velocity.Y = MathHelper.Clamp(velocity.Y,
           -Game1.maxVelocity.Y, Game1.maxVelocity.Y);
            sprite.position += velocity * deltaTime;
            

            if ((wasMovingLeft && (velocity.X > 0)) ||
               (wasMovingRight && (velocity.X < 0)))
            {
                // clamp at zero to prevent friction from making us jiggle side to side
                velocity.X = 0;
                sprite.Pause();
            }

            int tx = game.PixelToTile(sprite.position.X);
            int ty = game.PixelToTile(sprite.position.Y);
            bool nx = (sprite.position.X) % Game1.tile != 0;
            bool ny = (sprite.position.Y) % Game1.tile != 0;
            bool cell = game.CellAtTileCoord(tx, ty) != 0;
            bool cellright = game.CellAtTileCoord(tx + 1, ty) != 0;
            bool celldown = game.CellAtTileCoord(tx, ty + 1) != 0;
            bool celldiag = game.CellAtTileCoord(tx + 1, ty + 1) != 0;
            
            if (this.velocity.Y > 0)
            {
                if ((celldown && !cell) || (celldiag && !cellright && nx))
                {
                    // clamp the y position to avoid falling into platform below
                    sprite.position.Y = game.TileToPixel(ty);
                    this.velocity.Y = 0; // stop downward velocity
                    this.isFalling = false; // no longer falling
                    this.isJumping = false; // (or jumping)
                    ny = false; // - no longer overlaps the cells below
                }
            }
            else if (this.velocity.Y < 0)
            {
                if ((cell && !celldown) || (cellright && !celldiag && nx))
                {
                    // clamp the y position to avoid jumping into platform above
                    sprite.position.Y = game.TileToPixel(ty + 1);
                    this.velocity.Y = 0; 

                    cell = celldown;
                    cellright = celldiag; // (ditto)
                    ny = false; // player no longer overlaps the cells below
                }
            }

            if (this.velocity.X > 0)
            {
                if ((cellright && !cell) || (celldiag && !celldown && ny))
                {
                    // clamp the x position to avoid moving into the platform
                    // we just hit
                    sprite.position.X = game.TileToPixel(tx);
                    this.velocity.X = 0; // stop horizontal velocity
                    sprite.Pause();
                }
            }
            else if (this.velocity.X < 0)
            {
                if ((cell && !cellright) || (celldown && !celldiag && ny))
                {
                    // clamp the x position to avoid moving into the platform
                    // we just hit
                    sprite.position.X = game.TileToPixel(tx + 1);
                    this.velocity.X = 0; // stop horizontal velocity
                    sprite.Pause();
                }
            }
           
            this.isFalling = !(celldown || (nx && celldiag));

            if (velocity.X == 0)
            {
                sprite.Pause();
            }

        }
       }
   }
    


