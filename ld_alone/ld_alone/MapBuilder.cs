using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ld_alone
{
    class MapBuilder
    {
        private char[,] tiles;

        /// <summary>
        /// An array of characters representing tiles that are placed on screen.
        /// </summary>
        public char[,] Tiles
        {
            get { return tiles; }
        }

        Dictionary<String, Texture2D> resources;

        private int width;

        /// <summary>
        /// Width of this MapBuilder.
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        private int height;

        /// <summary>
        /// Height of this MapBuilder.
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        private Sign[] signs;

        /// <summary>
        /// Signs associated with this map.
        /// </summary>
        public Sign[] Signs
        {
            get { return signs; }
        }

        private bool hasFirePlace = false;

        /// <summary>
        /// Signifies whether this map will act as a checkpoint
        /// </summary>
        public bool HasFirePlace
        {
            get { return hasFirePlace; }
        }

        private Vector2 firePlaceLoc;

        /// <summary>
        /// Get the location of the fireplace, if there is one
        /// </summary>
        public Vector2 FirePlaceLoc
        {
            get
            {
                if (hasFirePlace) return firePlaceLoc;
                else return new Vector2(-1, -1);
            }
        }

        public String tile_file;

        private Dictionary<String, AnimatedTexture> anim_resources;

        public MapBuilder(String tile_file, Dictionary<String, Texture2D> resources, Dictionary<String, AnimatedTexture> anim_res)
        {
            this.tile_file = tile_file;
            this.resources = resources;
            this.anim_resources = anim_res;

            initWorld();
        }

        private void initWorld()
        {
            StreamReader reader;

            try {
                reader = new StreamReader(tile_file);

                width = int.Parse(reader.ReadLine());
                height = int.Parse(reader.ReadLine());

                tiles = new char[width, height];

                String line;
                int numLines = 0;

                while (numLines < height) {
                    line = reader.ReadLine();
                    for (int i = 0; i < line.Length; i++) {
                        char tile = line[i];
                        if (!char.IsWhiteSpace(tile)) tiles[i, numLines] = line[i];
                        if (tile == 'f' || tile == 'F') {
                            hasFirePlace = true;
                            firePlaceLoc = new Vector2(i, numLines);
                        }
                    }
                    numLines++;
                }

                int numSigns = int.Parse(reader.ReadLine());
                signs = new Sign[numSigns];

                for (int i = 0; i < numSigns; i++) {
                    line = reader.ReadLine();
                    signs[i] = new Sign(line, resources["text_bg"]);
                }

                if (!hasFirePlace) {
                    deadTileDelay = float.Parse(reader.ReadLine());
                    closeness = int.Parse(reader.ReadLine());
                }



            } catch (IOException e) { Console.WriteLine(e.StackTrace); }
        }

        public void Reset()
        {
            initWorld();
        }


        public Texture2D GetTileTexture(int x, int y)
        {
            Texture2D tile = null;

            switch (tiles[x, y]) {

                case '*':
                    tile = resources["rock"];
                    break;
                case '0':
                    tile = resources["sand"];
                    break;
                case 'w':
                    tile = resources["water"];
                    break;
                default:
                    Console.WriteLine("bad tile");
                    break;

            }

            return tile;
        }

        float deadTileTimer = 0;
        float deadTileDelay;
        float closeness = 4;
        Random rand = new Random();

        public void Update(GameTime gameTime)
        {
            deadTileTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (deadTileTimer >= deadTileDelay && !hasFirePlace && Game1.difficulty != Color.SkyBlue) {

                List<Point> points = pointsOnCircle((int)Game1.PlayerPos.X, (int)Game1.PlayerPos.Y, closeness);
                
                int randIndex = rand.Next(points.Count - 1);
                if (randIndex <= 0) randIndex = 0;
                Point randomPoint = points[randIndex];

                tiles[randomPoint.X, randomPoint.Y] = 'b';
                if (Game1.difficulty == Color.HotPink) {

                    // randomly knock out a neighbor
                    try {
                        tiles[randomPoint.X + (rand.Next(1) - 2), randomPoint.Y + (rand.Next(1) - 2)] = 'b';
                    } catch {}
                }

                deadTileTimer = 0;
                Game1.tile.Play();
            }

            if (isfinale && deadTileTimer > 40) {
                tiles[x, y] = 'b';
                if (x >= 15) { x = 0; y++; }
                else x++;
                if (y >= 16) { isfinale = false; y = 0; x = 0; }
                Game1.tile.Play();
                deadTileTimer = 0;
            }

            foreach (KeyValuePair<String, AnimatedTexture> tex in anim_resources) tex.Value.Update(gameTime);
        }

        bool isfinale = false;
        int x = 0, y = 0;

        public void finale()
        {
            isfinale = true;
        }

        private List<Point> pointsOnCircle(int x, int y, double r)
        {

            double dx;
            double dy;

            int xcoord;
            int ycoord;

            List<Point> points = new List<Point>();

            for (float angle = 0; angle < 360; angle += 30) {

                double radians = MathHelper.ToRadians(angle);

                dx = x + r * Math.Cos(radians);
                dy = y + r * Math.Sin(radians);

                xcoord = (int)Math.Ceiling(dx);
                ycoord = (int)Math.Ceiling(dy);

                if(xcoord > 0 && xcoord < 16 && ycoord > 0 && ycoord < 16)
                    points.Add(new Point(xcoord, ycoord));
            }

            return points.ToList<Point>();
        }


        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            spriteBatch.Begin();

            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {

                    switch (tiles[i, j]) {
                        
                        case '*':
                            spriteBatch.Draw(resources["rock"], new Vector2(i * 32, j * 32), color);
                            break;
                        case '0':
                        case 'l':
                            spriteBatch.Draw(resources["sand"], new Vector2(i * 32, j * 32), color);
                            break;
                        case 'w':
                            spriteBatch.End();
                            if (Game1.difficulty != Color.Violet) {
                                anim_resources["water"].Position = new Vector2(i * 32, j * 32);
                                anim_resources["water"].Draw(spriteBatch, color);
                            }
                            else {
                                anim_resources["water"].Position = new Vector2(i * 32, j * 32);
                                anim_resources["water"].Draw(spriteBatch, color);
                            }
                            spriteBatch.Begin();
                            break;
                        case 't':
                            spriteBatch.Draw(resources["sand"], new Vector2(i * 32, j * 32), color);
                            spriteBatch.Draw(resources["tree"], new Vector2(i * 32, j * 32), color);
                            break;
                        case 'T':
                            spriteBatch.Draw(resources["grass"], new Vector2(i * 32, j * 32), color);
                            spriteBatch.Draw(resources["tree"], new Vector2(i * 32, j * 32), color);
                            break;
                        case 's':
                            spriteBatch.Draw(resources["sand"], new Vector2(i * 32, j * 32), color);
                            spriteBatch.Draw(resources["sign"], new Vector2(i * 32, j * 32), color);
                            break;
                        case 'S':
                            spriteBatch.Draw(resources["grass"], new Vector2(i * 32, j * 32), color);
                            spriteBatch.Draw(resources["sign"], new Vector2(i * 32, j * 32), color);
                            break;
                        case 'g':
                        case 'L':
                            spriteBatch.Draw(resources["grass"], new Vector2(i * 32, j * 32), color);
                            break;
                        case 'f':
                            spriteBatch.Draw(resources["sand"], new Vector2(i * 32, j * 32), color);
                            spriteBatch.End();
                            anim_resources["fireplace"].Position = new Vector2(i * 32, j * 32);
                            anim_resources["fireplace"].Draw(spriteBatch, color);
                            spriteBatch.Begin();
                            break;
                        case 'r':
                            if (Game1.difficulty == Color.Violet || Game1.difficulty == Color.HotPink) {
                                spriteBatch.Draw(resources["sand"], new Vector2(i * 32, j * 32), color);
                                spriteBatch.Draw(resources["bear_trap"], new Vector2(i * 32, j * 32), color);
                            }
                            else spriteBatch.Draw(resources["sand"], new Vector2(i * 32, j * 32), color);
                            break;
                        case 'R':
                            if (Game1.difficulty == Color.Violet || Game1.difficulty == Color.HotPink) {
                                spriteBatch.Draw(resources["grass"], new Vector2(i * 32, j * 32), color);
                                spriteBatch.Draw(resources["bear_trap"], new Vector2(i * 32, j * 32), color);
                            }
                            else spriteBatch.Draw(resources["grass"], new Vector2(i * 32, j * 32), color);
                            break;
                        case 'm':
                            if (Game1.difficulty == Color.Violet || Game1.difficulty == Color.HotPink) {
                                spriteBatch.Draw(resources["sand"], new Vector2(i * 32, j * 32), color);
                                spriteBatch.End();
                                anim_resources["smog"].Position = new Vector2(i * 32, j * 32);
                                anim_resources["smog"].Draw(spriteBatch, color);
                                spriteBatch.Begin();
                            }
                            else spriteBatch.Draw(resources["sand"], new Vector2(i * 32, j * 32), color);
                            break;
                        case 'M':
                            if (Game1.difficulty == Color.Violet || Game1.difficulty == Color.HotPink) {
                                spriteBatch.Draw(resources["grass"], new Vector2(i * 32, j * 32), color);
                                spriteBatch.End();
                                anim_resources["smog"].Position = new Vector2(i * 32, j * 32);
                                anim_resources["smog"].Draw(spriteBatch, color);
                                spriteBatch.Begin();
                            }
                            else spriteBatch.Draw(resources["grass"], new Vector2(i * 32, j * 32), color);
                            break;
                        case '1':
                            spriteBatch.Draw(resources["sand"], new Vector2(i * 32, j * 32), color);
                            if (!Game1.collectedItems.Contains("wrench")) {
                                spriteBatch.Draw(resources["wrench"], new Vector2(i * 32, j * 32), color);
                            }
                            break;
                        case '2':
                            spriteBatch.Draw(resources["sand"], new Vector2(i * 32, j * 32), color);
                            if (!Game1.collectedItems.Contains("battery")) {
                                spriteBatch.Draw(resources["battery"], new Vector2(i * 32, j * 32), color);
                            }
                            break;
                        case '3':
                            spriteBatch.Draw(resources["sand"], new Vector2(i * 32, j * 32), color);
                            if (!Game1.collectedItems.Contains("manual")) {
                                spriteBatch.Draw(resources["manual"], new Vector2(i * 32, j * 32), color);
                            }
                            break;
                        case '4':
                            spriteBatch.Draw(resources["sand"], new Vector2(i * 32, j * 32), color);
                            spriteBatch.Draw(resources["spaceship"], new Vector2(i * 32, j * 32), color);
                            break;
                    }
                }
            }

            spriteBatch.End();
            
        }

    }
}
