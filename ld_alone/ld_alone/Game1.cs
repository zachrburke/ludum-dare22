using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ld_alone
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player;

        public static Vector2 PlayerPos;

        InputController inputController;
        MapBuilder current;
        MapBuilder[,] world = new MapBuilder[16, 16];

        Texture2D abyss;

        SoundEffect hurt;
        SoundEffect pickup;
        public static SoundEffect tile;

        int world_x = 2;
        int world_y = 0;

        int[] checkPoint;

        public static SpriteFont font;

        public static Color difficulty = Color.White;
        public static int numItems = 0;
        public static List<String> collectedItems = new List<String>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            inputController = new InputController();

            inputController.Add(Keys.A, moveLeft);
            inputController.Add(Keys.S, moveDown);
            inputController.Add(Keys.D, moveRight);
            inputController.Add(Keys.W, moveUp);

            graphics.PreferredBackBufferWidth = 512;
            graphics.PreferredBackBufferHeight = 512;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            checkPoint = new int[] { world_x, world_y, 12, 12 };

            base.Initialize();
        }

        Dictionary<String, Texture2D> resources = new Dictionary<String, Texture2D>();
        Texture2D spaceship;

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = this.Content.Load<SpriteFont>("font");

            Texture2D player_idle = this.Content.Load<Texture2D>("sprites/hero");
            Texture2D player_moving = this.Content.Load<Texture2D>("sprites/hero_moving");
             
            abyss = this.Content.Load<Texture2D>("bgs/abyss");
            spaceship = this.Content.Load<Texture2D>("sprites/spaceship_flying");
            hurt = this.Content.Load<SoundEffect>("sounds/hurt");
            pickup = this.Content.Load<SoundEffect>("sounds/pickup");
            tile = this.Content.Load<SoundEffect>("sounds/lose_tile");

            
            resources.Add("rock", this.Content.Load<Texture2D>("bg_tiles/stone_block"));
            resources.Add("water", this.Content.Load<Texture2D>("bg_tiles/sea"));
            resources.Add("sand", this.Content.Load<Texture2D>("bg_tiles/sand_tile"));
            resources.Add("grass", this.Content.Load<Texture2D>("bg_tiles/grass_tile"));
            resources.Add("tree", this.Content.Load<Texture2D>("bg_tiles/tree"));
            resources.Add("sign", this.Content.Load<Texture2D>("sprites/sign"));
            resources.Add("wrench", this.Content.Load<Texture2D>("sprites/wrench"));
            resources.Add("battery", this.Content.Load<Texture2D>("sprites/battery"));
            resources.Add("manual", this.Content.Load<Texture2D>("sprites/manual"));
            resources.Add("spaceship", this.Content.Load<Texture2D>("sprites/spaceship"));
            resources.Add("text_bg", this.Content.Load<Texture2D>("bgs/sign_dialog"));
            resources.Add("bear_trap", this.Content.Load<Texture2D>("bg_tiles/beartrap"));

            Dictionary<String, AnimatedTexture> anim_res = new Dictionary<String, AnimatedTexture>();
            anim_res.Add("fireplace", new AnimatedTexture(this.Content.Load<Texture2D>("anim_tiles/fireplace"), 2, 60, false));
            anim_res.Add("water", new AnimatedTexture(this.Content.Load<Texture2D>("anim_tiles/sea"), 2, 200, false));
            anim_res.Add("red_water", new AnimatedTexture(this.Content.Load<Texture2D>("anim_tiles/red_water"), 2, 200, false));
            anim_res.Add("smog", new AnimatedTexture(this.Content.Load<Texture2D>("anim_tiles/smog"), 2, 60, false));

            String[] files = Directory.GetFiles("levels2");
            
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    world[j, i] = new MapBuilder(files[(i * 8) + j], resources, anim_res);
                }
            }

            current = world[world_x, world_y];

            player = new Player(player_idle, player_moving);

        }

        bool dialogOpen = false;
        float inputDelay = 0;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            //Console.Clear();

            inputDelay += gameTime.ElapsedGameTime.Milliseconds;

            if (inputDelay > 100) {
                if (Keyboard.GetState().IsKeyDown(Keys.Space) && !dialogOpen)
                    checkSigns();
                else if(Keyboard.GetState().IsKeyDown(Keys.Space) && dialogOpen) {
                    foreach (Sign sign in current.Signs) sign.ShouldDraw = false;
                    dialogOpen = false;
                }
                inputDelay = 0;
            }

            int numKeys = 0;

            if(!dialogOpen && !gameover)
                numKeys = inputController.Handle(gameTime);

            if (numKeys > 0) {
                player.IsMoving = true;
            }
            else {
                player.IsMoving = false;
            }

            current.Update(gameTime);

            if (gameover) player.Position.Y -= .4f;

            player.Update(gameTime);

            Rectangle bounds = player.Frame;

            int x = (int)bounds.Center.X % 32;
            x = (int)(player.Position.X + x) / 32;

            int y = (int)bounds.Center.Y % 32;
            y = (int)(player.Position.Y + y) / 32;

            PlayerPos = new Vector2(x, y);

            base.Update(gameTime);
        }

        /// <summary>
        /// Shoddiest form of collision detection known to man
        /// </summary>
        /// <returns>true if there is a collision</returns>
        private bool checkCollisions(int xoff, int yoff)
        {
            int x = (int) PlayerPos.X;
            int y = (int) PlayerPos.Y;

            Rectangle lean_bounds = player.Frame;
            lean_bounds.X = (int) (player.Position.X + 4);
            lean_bounds.Y = (int) (player.Position.Y + 4);
            lean_bounds.Width -= 8;
            lean_bounds.Height -= 8;

            Rectangle strict_bounds = player.Frame;
            strict_bounds.X = (int)(player.Position.X - 1);
            strict_bounds.Width++;
            strict_bounds.Y = (int)(player.Position.Y);

            Rectangle tile_bounds = new Rectangle((x + xoff) * 32, (y + yoff) * 32, 32, 32);

            //Console.WriteLine("x: " + x + " y: " + y);

            bool stop = false;

            if (x + xoff < 0 || x + xoff > 15 || y + yoff < 0 || y + yoff > 15)
                stop = true;
            else {
                switch (current.Tiles[x + xoff, y + yoff]) {
                    case '*':
                    case 'w':
                    case 't':
                    case 'T':
                    case 's':
                    case 'S':
                    case 'f':
                        if(strict_bounds.Intersects(tile_bounds))
                            stop = true;
                        break;
                    case 'l':
                    case 'L':
                        
                        if (x == 0 || x == 15 || y == 0 || y == 15) {
                            if (x == 0 && world_x > 0) { // Go the the left map
                                world_x--;
                                player.Position.X = 450;
                            }
                            if (x == 15 && world_x < 15) { // Go to the right map
                                world_x++;
                                player.Position.X = 56;
                            }
                            if (y == 0 && world_y < 15) { // Go to the top map
                                world_y++;
                                player.Position.Y = 450;
                            }
                            if (y == 15 && world_y > 0) {
                                world_y--;  // go to the bottom map
                                player.Position.Y = 56;
                            }

                            current = world[world_x, world_y];
                            current.Reset();
                            
                            if (current.HasFirePlace) 
                                checkPoint = new int[]{
                                    world_x,
                                    world_y,
                                    (int) current.FirePlaceLoc.X,
                                    (int) current.FirePlaceLoc.Y
                                };
                        }
                        break;
                    case 'b':
                        if (lean_bounds.Intersects(tile_bounds)) {
                            current = world[checkPoint[0], checkPoint[1]];
                            world_x = checkPoint[0];
                            world_y = checkPoint[1];
                            current.Reset();
                            player.Position = current.FirePlaceLoc * 32;
                            hurt.Play();
                        }
                        break;
                    case 'r':
                    case 'R':
                    case 'm':
                    case 'M':
                        if (Game1.difficulty == Color.Violet || Game1.difficulty == Color.HotPink) {
                            if (strict_bounds.Intersects(tile_bounds)) {
                                current = world[checkPoint[0], checkPoint[1]];
                                world_x = checkPoint[0];
                                world_y = checkPoint[1];
                                current.Reset();
                                player.Position = current.FirePlaceLoc * 32;
                                hurt.Play();
                            }
                        }
                        break;
                    case '1':
                        if (strict_bounds.Intersects(tile_bounds)) {
                            collectedItems.Add("wrench");
                            numItems++;
                            current.Tiles[x + xoff, y + yoff] = '0';
                            pickup.Play();
                        }
                        break;
                    case '2':
                        if (strict_bounds.Intersects(tile_bounds)) {
                            collectedItems.Add("battery");
                            numItems++;
                            current.Tiles[x + xoff, y + yoff] = '0';
                            pickup.Play();
                        }
                        break;
                    case '3':
                        if (strict_bounds.Intersects(tile_bounds)) {
                            collectedItems.Add("manual");
                            numItems++;
                            current.Tiles[x + xoff, y + yoff] = '0';
                            pickup.Play();
                        }
                        break;
                    case '4':
                        if (strict_bounds.Intersects(tile_bounds) && numItems >= 3) {
                            current.finale();
                            gameover = true;
                            player.idle = spaceship;
                            current.Tiles[x + xoff, y + yoff] = '0';
                        }
                        break;

                }
            }

            return stop;
        }

        bool gameover = false;

        public void checkSigns()
        {
            int x = (int)PlayerPos.X;
            int y = (int)PlayerPos.Y;

            if (y - 1 > 0) {
                if (current.Tiles[x, y - 1] == 's' || current.Tiles[x, y - 1] == 'S') {

                    int signId = 0;

                    for (int i = y - 1; i >= 0; i--) {
                        for (int j = x - 1; j >= 0; j--) {
                            if (current.Tiles[j, i] == 's' || current.Tiles[j, i] == 'S') signId++;
                        }
                    }

                    current.Signs[signId].ShouldDraw = true;
                    Console.WriteLine(signId);
                    dialogOpen = true;
                }
            }
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (numItems) {
                case 0:
                    difficulty = Color.White;
                    break;
                case 1:
                    difficulty = Color.Violet;
                    break;
                case 2:
                    difficulty = Color.HotPink;
                    break;
                case 3:
                    difficulty = Color.SkyBlue;
                    break;
            }

            spriteBatch.Begin();
            spriteBatch.Draw(abyss, new Vector2(0, 0), Color.White);

            if (gameover) {

                String text = "Congratulations... You win!\n Hit escape to exit the game";

                Rectangle textBox = new Rectangle(100, 400, text.Length * 12 + 30, 60);

                //spriteBatch.Draw(resources["text_bg"], textBox, Color.White);
                spriteBatch.DrawString(Game1.font, text, new Vector2(140, 400), Color.White);
            }

            spriteBatch.End();

            current.Draw(spriteBatch, difficulty);
            player.Draw(spriteBatch, Color.White);

            foreach (Sign sign in current.Signs) {
                if (sign.ShouldDraw) sign.Draw(spriteBatch);
            }

            base.Draw(gameTime);
        }

        float movementSpeed = 2f;

        #region inputs

        private void moveUp(GameTime gameTime)
        {
            if(!checkCollisions(0, -1))
                player.Position.Y -= movementSpeed;

            if(checkCollisions(0, 0))
                player.Position.Y += 12;
        }

        private void moveDown(GameTime gameTime)
        {
            if(!checkCollisions(0, 1))
                player.Position.Y += movementSpeed;

            if (checkCollisions(0, 0))
                player.Position.Y -= 12;
        }

        private void moveLeft(GameTime gameTime)
        {
            if(!checkCollisions(-1, 0))
                player.Position.X -= movementSpeed;

            if (checkCollisions(0, 0))
                player.Position.X += 12;
        }

        private void moveRight(GameTime gameTime)
        {
            if(!checkCollisions(1, 0))
                player.Position.X += movementSpeed;

            if (checkCollisions(0, 0))
                player.Position.X -= 12;
        }

        #endregion
    }
}
