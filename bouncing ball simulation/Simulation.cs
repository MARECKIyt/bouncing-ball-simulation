using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using bouncing_ball_simulation.Class;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace bouncing_ball_simulation
{
    public class Simulation : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        SpriteFont font;
        int w = 1920;
        int h = 1080;
        int wS;
        int hS;
        float timeScale = 1f;

        float dt;
        List<Ball> balls = new List<Ball>{};
        
        int l;
        float g;

        Texture2D smallCircTxt;
        Texture2D bigCircTxt;

        float coolDown;
        float saveCoolDown = 10;

        float mom1;
        float mom2;
        float mom3;
        float mom4;
        float p1;
        float p2;
        float p3;
        float p4;
        float e;
        bool showFPS = true;
        bool calculatePressureAndEnergy = true;

        Random random = new Random();

        Color[] colors = new Color[] { Color.Cyan, Color.Blue, Color.Orange, Color.Green, Color.Yellow, Color.Red, Color.Purple, Color.DeepPink, Color.White, Color.LightGreen };

        public Simulation()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = w;
            _graphics.PreferredBackBufferHeight = h;
            Window.AllowUserResizing = true;
            _graphics.IsFullScreen = true;
            IsFixedTimeStep = false;
            dt = 1f / 120f; // to jest docelowy delta time jeżeli symulacja nie będzie w stanie działać tak szybko to zachowa swoją dokładność ale symulacja zwolni (przynajmniej tak powinno być)
            TargetElapsedTime = TimeSpan.FromSeconds(dt);
            wS = 1080;
            hS = 1080;
            g = 50;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            smallCircTxt = Content.Load<Texture2D>("smallBall");
            bigCircTxt = Content.Load<Texture2D>("bigBall");
            font = Content.Load<SpriteFont>("galleryFont");

            // templates of physical phenomena - szablony zjawisk fizycznych
            // just a few balls - po prostu kilka kulek
            g = 100;
            balls.Add(new Ball(111, 5, new Vector2(300, 112), new Vector2(-100, 230), Color.Blue));
            balls.Add(new Ball(69, 1, new Vector2(250, 333), new Vector2(100, 200) , Color.Green));
            balls.Add(new Ball(50, 0.5f, new Vector2(500, 777), new Vector2(42.0f, -125), Color.Orange));
            balls.Add(new Ball(55, 0.6f, new Vector2(575, 961), new Vector2(60, -125), Color.Yellow));
            balls.Add(new Ball(55, 0.6f, new Vector2(900, 100), new Vector2(0, 0), Color.Purple));
            

            /*// gas diffusion - dyfuzja gazów
            g = 0;
            Gas(500, 7, 10, Color.Blue, 100, new float[4] {0, wS * 0.5f, 0, hS});
            Gas(350, 9, 150, Color.Green, 100, new float[4] { 0.5f * wS, wS, 0, hS });
            */

            /*// buoyancy force - siła wyporu
            Gas(3500, 3, 1, Color.DarkBlue, 100);
            balls.Add(new Ball(69, 15, new Vector2(wS * 0.25f, hS * 0.5f), Vector2.Zero, Color.Green));
            balls.Add(new Ball(69, 85, new Vector2(wS * 0.5f, hS * 0.5f), Vector2.Zero, Color.Orange));
            balls.Add(new Ball(69, 420, new Vector2(wS * 0.75f, hS * 0.5f), Vector2.Zero, Color.Yellow));
            */

            /*// buoyancy force with more balls - siła wyporu z wiekszą ilością kulek
            timeScale = 1f;
            Gas(25000, 1, 0.1f, Color.Blue, 1);
            balls.Add(new Ball(69, 10, new Vector2(wS * 0.25f, hS * 0.5f), Vector2.Zero, Color.Green));
            balls.Add(new Ball(69, 35, new Vector2(wS * 0.5f, hS * 0.5f), Vector2.Zero, Color.Orange));
            balls.Add(new Ball(69, 420, new Vector2(wS * 0.75f, hS * 0.5f), Vector2.Zero, Color.Yellow));
            */

            /*// lighter gas goes up and heavier gas goes down - lżejszy gaz idzie do góry a cięższy do dołu
            Gas(1666, 5, 3, Color.Blue, 50, new float[4] { 0, wS, 0, hS * 0.5f});
            Gas(1666, 5, 1, Color.Green, 50, new float[4] { 0, wS, hS * 0.5f, hS });
            */
            
            /*// lighter gas goes up and heavier gas goes down with more balls - lżejszy gaz idzie do góry a cięższy do dołu z większą ilością kulek
            timeScale = 0.125f;
            Gas(6000, 3, 0.5f, Color.Blue, 1, new float[4] { 0, wS, 0, hS * 0.5f});
            Gas(9000, 3, 0.1f, Color.Green, 1, new float[4] { 0, wS, hS * 0.5f, hS });
            */

            balls.Sort(Sort);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            l = balls.Count;
            Parallel.ForEach(balls, b => { b.Move(dt, g, wS, hS, timeScale); });
            balls.Sort(Sort);
            Parallel.For(0, l, i =>
            {
                Ball b1 = balls[i];
                for (int j = i + 1; j < l; j++)
                {
                    Ball b2 = balls[j];
                    if (b1.position.X + b1.radius - (b2.position.X - b2.radius) > 0) b1.Collision(b2);
                    else break;
                }
            });

            if (calculatePressureAndEnergy)
            {
                coolDown -= dt;
                e = 0;
                for (int i = 0; i < l; i++)
                {
                    Ball b = balls[i];
                    e += b.velocity.LengthSquared() * b.mass * 0.5f;
                    e += b.mass * (hS - b.position.Y - b.radius) * g;

                    if (b.position.X - b.radius < 0 || b.position.X + b.radius > wS)
                    {
                        float mom = b.mass * MathF.Abs(b.velocity.X) * 2;
                        if (balls[i].position.Y < h / 2) mom1 += mom;
                        else mom2 += mom;
                    }
                    else if (b.position.Y - b.radius < 0 || b.position.Y + b.radius > hS)
                    {
                        float mom = b.mass * Math.Abs(b.velocity.Y) * 2;
                        if (b.position.Y < h / 2) mom4 += mom;
                        else mom3 += mom;
                    }
                }

                if (coolDown < 0)
                {
                    coolDown = saveCoolDown;
                    p1 = mom1 / (hS * saveCoolDown);
                    p2 = mom2 / (hS * saveCoolDown);
                    p3 = mom3 / (wS * saveCoolDown);
                    p4 = mom4 / (wS * saveCoolDown);
                    mom1 = 0;
                    mom2 = 0;
                    mom3 = 0;
                    mom4 = 0;
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            foreach (Ball ball in balls)
            {
                Texture2D txt;
                if (ball.radius > 12) txt = bigCircTxt;
                else txt = smallCircTxt;
                _spriteBatch.Draw(txt, new Rectangle((int)(ball.position.X + 0.5f) - ball.radius, (int)(ball.position.Y + 0.5f - ball.radius), ball.radius * 2, ball.radius * 2), ball.color);
            }

            if (showFPS) _spriteBatch.DrawString(font, ((int)(1 / (float)gameTime.ElapsedGameTime.TotalSeconds)).ToString(), new Vector2(wS + 1, 1), Color.White);
            if (calculatePressureAndEnergy)
            {
                _spriteBatch.DrawString(font, ((int)(p4 + 0.5)).ToString(), new Vector2(wS + 1, 71), Color.White);
                _spriteBatch.DrawString(font, ((int)(p1 + 0.5)).ToString(), new Vector2(wS + 1, 101), Color.White);
                _spriteBatch.DrawString(font, ((int)(p2 + 0.5)).ToString(), new Vector2(wS + 1, 131), Color.White);
                _spriteBatch.DrawString(font, ((int)(p3 + 0.5)).ToString(), new Vector2(wS + 1, 161), Color.White);
                _spriteBatch.DrawString(font, ((int)(e + 0.5)).ToString(), new Vector2(wS + 1, 251), Color.White);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
        static int Sort(Ball b1, Ball b2)
        {
            if (b1.position.X - b1.radius < b2.position.X - b2.radius)
                return -1;
            else
                return 1;
        }

        void Gas(int particles, int r, float m, Color color, float randomV, float[] range = null)
        {
            if (range == null) range = new float[4] { 0, wS, 0, hS};

            for (int i = 0; i < particles; i++)
            {
                if (color == Color.Black) color = colors[random.Next(0, 10)];
                float vx = 0;
                float vy = 0;
                if (randomV > 0)
                {
                    vx = (float)random.NextDouble() * (randomV * 2) - randomV;
                    vy = (float)random.NextDouble() * (randomV * 2) - randomV;
                }
                balls.Add(new Ball(r, m, new Vector2(random.Next((int)range[0] + r, (int)range[1] - r), random.Next((int)range[2] + r, (int)range[3] - r)), new Vector2(vx, vy), color));
            }
        }
    }
}