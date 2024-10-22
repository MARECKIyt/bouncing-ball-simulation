using System;
using System.Collections.Generic;
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

        double dt;
        List<Ball> balls = new List<Ball>{};
        List<int> sortedBalls = new List<int>();
        
        int l;
        Texture2D circTxt;
        float g;

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
            IsFixedTimeStep = true;
            dt = 1d / 120d;
            TargetElapsedTime = TimeSpan.FromSeconds(dt);
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            circTxt = Content.Load<Texture2D>("ball");
            font = Content.Load<SpriteFont>("font");

            wS = 1080;
            hS = 1080;
            g = 50;

            // templates of physical phenomena - szablony zjawisk fizycznych

            // just a few balls - po prostu kilka kulek
            balls.Add(new Ball(111, 5, new Vector2(300, 112), new Vector2(-100, 230), Color.Blue));
            balls.Add(new Ball(69, 1, new Vector2(250, 333), new Vector2(100, 200) , Color.Green));
            balls.Add(new Ball(50, 0.5f, new Vector2(500, 777), new Vector2(42.0f, -125), Color.Orange));

            /* // gas diffusion - dyfuzja gazów
            g = 0;
            Gas(500, 7, 1, Color.Blue, 100, new float[4] {0, wS * 0.5f, 0, hS});
            Gas(350, 9, 1.5f, Color.Green, 100, new float[4] { 0.5f * wS, wS, 0, hS });
            */

            /* // buoyancy force - siła wyporu
            Gas(3500, 3, 1, Color.DarkBlue, 100);
            balls.Add(new Ball(69, 15, new Vector2(wS * 0.25f, hS * 0.5f), Vector2.Zero, Color.Green));
            balls.Add(new Ball(69, 85, new Vector2(wS * 0.75f, hS * 0.5f), Vector2.Zero, Color.Orange));
            */

            /* // lighter gas goes up and heavier gas goes down - lżejszy gaz idzie do góry a cięższy do dołu
            Gas(1666, 5, 3, Color.Blue, 50, new float[4] { 0, wS, 0, hS * 0.5f});
            Gas(1666, 5, 1, Color.Green, 50, new float[4] { 0, wS, hS * 0.5f, hS });
            */

            for (int i=0; i<balls.Count; i++)
            {
                sortedBalls.Add(i);
            }
            sortedBalls.Sort(Sort);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            sortedBalls.Sort(Sort);
            l = balls.Count;
            e = 0;

            for (int i = 0; i < l; i++)
            {
                Ball b1 = balls[sortedBalls[i]];
                b1.Move((float)dt, g, wS, hS);
                e += b1.velocity.Length() * b1.velocity.Length() * b1.mass / 2;
                e += b1.mass * (hS - b1.position.Y - b1.radius) * g;
                for (int j = i + 1; j<l; j++)
                {
                    Ball b2 = balls[sortedBalls[j]];
                    if (b1.position.Y + b1.radius - (b2.position.Y - b2.radius) > 0)
                    {
                        b1.Collision(b2);
                    }
                    else break;
                }
            }

            coolDown -= (float)dt;
            for (int i = 0; i < l; i++)
            {
                if (balls[i].position.X - balls[i].radius < 0 || balls[i].position.X + balls[i].radius > wS)
                {
                    float mom = balls[i].mass * MathF.Abs(balls[i].velocity.X) * 2;
                    if (balls[i].position.Y < h / 2)
                        mom1 += mom;
                    else
                        mom2 += mom;
                }
                else if (balls[i].position.Y - balls[i].radius < 0 || balls[i].position.Y + balls[i].radius > hS)
                {
                    float mom = balls[i].mass * Math.Abs(balls[i].velocity.Y) * 2;
                    if (balls[i].position.Y < h / 2)
                        mom4 += mom;
                    else
                        mom3 += mom;
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

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            for (int i = 0; i < l; i++)
            {
                Ball ball = balls[i];
                _spriteBatch.Draw(circTxt, new Rectangle((int)(ball.position.X + 0.5f) - ball.radius, (int)(ball.position.Y + 0.5f - ball.radius), ball.radius * 2, ball.radius * 2), ball.color);
            }

            _spriteBatch.DrawString(font, ((int)(1 / (float)gameTime.ElapsedGameTime.TotalSeconds)).ToString(), new Vector2(wS + 1, 1), Color.White);
            _spriteBatch.DrawString(font, ((int)(p4 + 0.5)).ToString(), new Vector2(wS + 1, 71), Color.White);
            _spriteBatch.DrawString(font, ((int)(p1 + 0.5)).ToString(), new Vector2(wS + 1, 101), Color.White);
            _spriteBatch.DrawString(font, ((int)(p2 + 0.5)).ToString(), new Vector2(wS + 1, 131), Color.White);
            _spriteBatch.DrawString(font, ((int)(p3 + 0.5)).ToString(), new Vector2(wS + 1, 161), Color.White);
            _spriteBatch.DrawString(font, ((int)(e + 0.5)).ToString(), new Vector2(wS + 1, 251), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
        int Sort(int b1, int b2)
        {
            if (balls[b1].position.Y - balls[b1].radius - (balls[b2].position.Y - balls[b2].radius) < 0)
                return -1;
            else if (balls[b1].position.Y - balls[b1].radius - (balls[b2].position.Y - balls[b2].radius) == 0)
                return 0;
            else
                return 1;
        }

        void Gas(int particles, int r, float m, Color color, float randomV, float[] range = null)
        {
            if (range == null) range = new float[4] { 0, wS, 0, hS};

            for (int i = 0; i < particles; i++)
            {
                if (color == Color.Black) color = colors[random.Next(0, 10)];
                balls.Add(new Ball(r, m, new Vector2(random.Next((int)range[0] + r, (int)range[1] - r), random.Next((int)range[2] + r, (int)range[3] - r)), new Vector2((float)random.NextDouble() * (randomV * 2) - randomV, (float)random.NextDouble() * (randomV * 2) - randomV), color));
            }
        }
    }

    class Ball
    {
        public int radius;
        public float mass;
        public Vector2 position;
        public Vector2 velocity;
        public Color color;

        public Ball(int r, float m, Vector2 p, Vector2 v, Color c)
        {
            radius = r;
            mass = m;
            position = p;
            velocity = v;
            color = c;
        }

        public void Move(float dt, float g, int w, int h)
        {
            if (position.X - radius < 0 || position.X + radius > w)
            {
                velocity.X = -velocity.X;
                if (position.X - radius < 0)
                    position.X = radius;
                else
                    position.X = w - radius;
            }
            if (position.Y - radius < 0 || position.Y + radius > h)
            {
                velocity.Y = -velocity.Y;
                if (position.Y - radius < 0)
                {
                    float d = position.Y;
                    position.Y = radius;
                    d -= position.Y;
                    float correction = velocity.Y * velocity.Y - 2 * d * g;
                    if (correction > 0) velocity.Y = MathF.Sqrt(correction);
                }
                else
                {
                    float d = position.Y;
                    position.Y = h - radius;
                    d -= position.Y;
                    float correction = velocity.Y * velocity.Y - 2 * d * g;
                    if (correction > 0) velocity.Y = -MathF.Sqrt(correction);
                }
            }

            velocity += new Vector2(0, g * dt * 0.5f);
            position += velocity * dt;
            velocity += new Vector2(0, g * dt * 0.5f);
        }

        public void Collision(Ball ball)
        {
            float minDistance = radius + ball.radius;
            if (minDistance < (position - ball.position).Length())
                return;

            Vector2 d = position - ball.position; ;
            Vector2 n = Vector2.Normalize(d);
            Vector2 t = Vector2.Normalize(new Vector2(-n.Y, n.X));
            
            float vnL = Vector2.Dot(velocity, n);
            float vtL = Vector2.Dot(velocity, t);
            float bvnL = Vector2.Dot(ball.velocity, n);
            float bvtL = Vector2.Dot(ball.velocity, t);

            float vnLP = (vnL * (mass - ball.mass) + 2 * ball.mass * bvnL) / (mass + ball.mass);
            float bvnLP = (bvnL * (ball.mass - mass) + 2 * mass * vnL) / (mass + ball.mass);
            
            velocity = vnLP * n + vtL * t;
            ball.velocity = bvnLP * n + bvtL * t;

            float s = radius + ball.radius - d.Length();
            position += 1.01f * n * s * ball.mass / (mass + ball.mass);
            ball.position -= 1.01f * n * s * mass / (mass + ball.mass);
        }
    }
}