using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.Maps.Tiled;
using MonoGame.Extended.ViewportAdapters;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace AwesomePlatformer
{
  
    public class Game1 : Game
    {
        public static int tile = 64;
        public static float meter = tile;
        public static float gravity = meter * 9.8f * 4f;
        public static Vector2 maxVelocity = new Vector2(meter * 7, meter * 13);
        public static float acceleration = maxVelocity.X * 2;
        public static float friction = maxVelocity.X * 6;
        public static float jumpImpulse = meter * 2000;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont arialFont;
        int score = 0;
        int lives = 3;

        const int STATE_SPLASH = 0;
        const int STATE_GAME = 1;
        const int STATE_GAMEOVER = 2;
        const int STATE_WIN = 3;
        const int STATE_GAMEOVERSPIKE = 4;
        int gameState = STATE_SPLASH;

        Texture2D medal = null;
        Texture2D heart = null;
        Player player = null;

        Camera2D camera = null;
        TiledMap map = null;
        TiledTileLayer collisionLayer;
        private Song gameMusic;

        List<Enemy> enemies = new List<Enemy>();
        Sprite goal = null;
        TiledObject SpikeBox = null;
      


        public int ScreenWidth
        {
            get
            {
                return graphics.GraphicsDevice.Viewport.Width;
            }
        }
        public int ScreenHeight
        {
            get
            {
                return graphics.GraphicsDevice.Viewport.Height;
            }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        
        protected override void Initialize()
        {
            
           

            base.Initialize();
        }

        private void DrawSplashState(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            spriteBatch.DrawString(arialFont, "Kill The Zombie Peas",
                            new Vector2(335, 200), Color.White);

            spriteBatch.DrawString(arialFont, "Press Enter to Play",
                            new Vector2(340, 250), Color.White);
            spriteBatch.End();
        }

        private void DrawGameOverState(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(arialFont, "Come On Their Just Peas",
                            new Vector2(315, 250), Color.White);
            spriteBatch.DrawString(arialFont, "Game Over", new Vector2(360, 200),
                                    Color.White);
            spriteBatch.End();
        }

        private void DrawGameOverSpikeState(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(arialFont, "Death By Spikes",
                            new Vector2(345, 250), Color.White);
            spriteBatch.DrawString(arialFont, "Ouch!",
                            new Vector2(380, 270), Color.White);
            spriteBatch.DrawString(arialFont, "Game Over", new Vector2(360, 200),
                                    Color.White);
            spriteBatch.End();
        }
        private void DrawYouWinState(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(arialFont, "You Win", new Vector2(360, 200),
                                    Color.White);
            spriteBatch.End();
        }

        private void UpdateYouWinState(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                
                gameState = STATE_GAME;
                ResetGame();
            }
        }

        private void UpdateGameOverSpikeState(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {

                gameState = STATE_GAME;
                ResetGame();
            }
        }

        private void UpdateGameOverState(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                gameState = STATE_SPLASH;
                ResetGame();
            }
        }

        private void UpdateSplashState(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                gameState = STATE_GAME;
                
            }
        }
        private void DrawGameState(SpriteBatch spriteBatch)
        {
            // TODO: Add your drawing code here

            var transformMatrix = camera.GetViewMatrix();

            spriteBatch.Begin(transformMatrix: transformMatrix);

            map.Draw(spriteBatch);
            foreach (Enemy e in enemies)
            {
                e.Draw(spriteBatch);
            }
            goal.Draw(spriteBatch);

            player.Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();

            for (int i = 0; i < score; i++)
            {
                spriteBatch.Draw(medal, new Vector2(10 + i * 10,
               20), Color.White);
            }

            for (int i = 0; i < lives; i++)
            {
                spriteBatch.Draw(heart, new Vector2(ScreenWidth - 80 - i * 20,
               20), Color.White);
            }


            spriteBatch.End();
        }

      
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
           

            arialFont = Content.Load<SpriteFont>("Arial");
            heart = Content.Load<Texture2D>("Heart");
            medal = Content.Load<Texture2D>("Medal");

            
            ResetGame();
        }

        private void ResetGame()
        {
            player = new Player(this);
            player.Load(Content);

            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice,
               (int)(2.2 * graphics.GraphicsDevice.Viewport.Width),
               (int)(2.2 * graphics.GraphicsDevice.Viewport.Height));
            camera = new Camera2D(viewportAdapter);
            camera.Position = new Vector2(0, graphics.GraphicsDevice.Viewport.Height);

            map = Content.Load<TiledMap>("Level1");
            foreach (TiledTileLayer layer in map.TileLayers)
            {
                if (layer.Name == "Collisions")
                    collisionLayer = layer;
            }

            enemies.Clear();
            
            foreach (TiledObjectGroup group in map.ObjectGroups)
            {
                if (group.Name == "Enemies")
                {
                    foreach (TiledObject obj in group.Objects)
                    {
                        Enemy enemy = new Enemy(this);
                        enemy.Load(Content);
                        enemy.Position = new Vector2(obj.X, obj.Y);
                        enemies.Add(enemy);
                    }
                }
                if (group.Name == "Spike Hit")
                {
                    foreach (TiledObject obj in group.Objects)
                    {
                        SpikeBox = obj;

                    }
                }
                if (group.Name == "Goal")
                {

                    TiledObject obj = group.Objects[0];
                    if (obj != null)
                    {
                        AnimatedTexture anim = new AnimatedTexture(
                        Vector2.Zero, 0, 2, 1);
                        anim.Load(Content, "chest", 1, 1);
                        goal = new Sprite();
                        goal.Add(anim, 0, 5);
                        goal.position = new Vector2(obj.X, obj.Y);
                    }
                }
            }
            gameMusic = Content.Load<Song>("SuperHero_original_no_Intro");
            MediaPlayer.Play(gameMusic);

            lives = 3;
            score = 0;

        }


        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            

            if (lives == 0)
            {
                
                gameState = STATE_GAMEOVER;
            }            
            if (enemies.Count == 0)
            {
                gameState = STATE_WIN;
            }
           
            switch (gameState)
            {
                case STATE_SPLASH:
                    UpdateSplashState(deltaTime);
                    break;
                case STATE_GAME:
                    UpdateGameState(deltaTime);
                    break;
                case STATE_GAMEOVER:
                    UpdateGameOverState(deltaTime);
                    break;
                case STATE_WIN:
                    UpdateYouWinState(deltaTime);
                    break;
                case STATE_GAMEOVERSPIKE:
                    UpdateGameOverSpikeState(deltaTime);
                    break;
            }
            base.Update(gameTime);
        }

        protected void UpdateGameState(float deltaTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
                     
            player.Update(deltaTime);

            foreach (Enemy e in enemies)
            {
                e.Update(deltaTime);
            }
            camera.Position = player.Position - new Vector2(ScreenWidth, ScreenHeight);

            CheckCollisions();           
        }

       

        private void CheckCollisions()
        {
            bool hasCollidedEnemies = false;
            foreach (Enemy e in enemies)
            {
                if (IsColliding(player.Bounds, e.Bounds) == true)
                {
                    hasCollidedEnemies = true;
                    if (player.IsJumping && player.Velocity.Y > 0)
                    {
                        player.JumpOnCollision();
                        enemies.Remove(e);
                        score = score + 1;
                        break;
                    }
                    else
                    {
                        if (player.canTakeDamage)
                        {
                            lives = lives - 1;
                            player.canTakeDamage = false;

                        }
                    }
                }
            }

            Rectangle rec = new Rectangle((int)SpikeBox.X, (int)SpikeBox.Y, (int)SpikeBox.Width, (int)SpikeBox.Height);
            if (IsColliding(player.Bounds, rec) == true)
            {
                gameState = STATE_GAMEOVERSPIKE;
            }

            if (hasCollidedEnemies == false)
            {
                player.canTakeDamage = true;
            }

            
            
        }

        

        

        private bool IsColliding(Rectangle rect1, Rectangle rect2)
        {
            if (rect1.X + rect1.Width < rect2.X ||
            rect1.X > rect2.X + rect2.Width ||
            rect1.Y + rect1.Height < rect2.Y ||
            rect1.Y > rect2.Y + rect2.Height)
            {
                // these two rectangles are not colliding
                return false;
            }
            
            return true;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (gameState)
            {
                case STATE_SPLASH:
                    DrawSplashState(spriteBatch);
                    break;
                case STATE_GAME:
                    DrawGameState(spriteBatch);
                    break;
                case STATE_GAMEOVER:
                    DrawGameOverState(spriteBatch);
                    break;
                case STATE_WIN:
                    DrawYouWinState(spriteBatch);
                    break;
                case STATE_GAMEOVERSPIKE:
                    DrawGameOverSpikeState(spriteBatch);
                    break;
            }

            


            base.Draw(gameTime);
        }

        public int PixelToTile(float pixelCoord)
        {
            return (int)Math.Floor(pixelCoord / tile);
        }
        public int TileToPixel(int tileCoord)
        {
            return tile * tileCoord;
        }
        public int CellAtPixelCoord(Vector2 pixelCoords)
        {
            if (pixelCoords.X < 0 ||
           pixelCoords.X > map.WidthInPixels || pixelCoords.Y < 0)
                return 1;
            // let the player drop of the bottom of the screen (this means death)
            if (pixelCoords.Y > map.HeightInPixels)
                return 0;
            return CellAtTileCoord(
           PixelToTile(pixelCoords.X), PixelToTile(pixelCoords.Y));
        }
        public int CellAtTileCoord(int tx, int ty)
        {
            if (tx < 0 || tx >= map.Width || ty < 0)
                return 1;
            // let the player drop of the bottom of the screen (this means death)
            if (ty >= map.Height)
                return 0;
            TiledTile tile = collisionLayer.GetTile(tx, ty);
            return tile.Id;
        }

    }
}
