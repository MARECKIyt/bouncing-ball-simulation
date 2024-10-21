using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// bez optymalizacji jedynie 1025 piłek na 60fps a z optymalizacją (pomijając drastyczny spadek prawie do 0 fps w momencie ściśnięcia kulek) aż 6201 piłek na 120fps (chociaż piłki z wyjątkiem jednej miały promień 2 a nie 4 żeby zmieściły się na ekranie co powoduje że każda piłka ma mniej potencjalnych kolizji do sprawdzenia przez zaptymalizowany algorytm ale ciiiii) a jeżeli nie chcemy mieć dużego spadku poniżej 120fps to na 4193 piłkach działa świetnie
namespace bouncing_ball_simulation
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        SpriteFont font;
        int w = 1920;
        int h = 1080;
        int wS = 608;
        int hS = 1080;

        double dt = 1d / 120d;
        List<Ball> balls = new List<Ball>{};
        List<int> sortedBalls = new List<int>();
        
        int l;
        Texture2D circTxt;
        float g = 0;

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

        public Game1()
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
            TargetElapsedTime = TimeSpan.FromSeconds(dt);
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            circTxt = Content.Load<Texture2D>("ball");
            font = Content.Load<SpriteFont>("font");

            balls.Add(new Ball(50, 1, new Vector2(50, 300), new Vector2(200, 0), Color.Blue));
            balls.Add(new Ball(50, 4, new Vector2(550, 300), new Vector2(50, 0), Color.Orange));
            balls.Add(new Ball(50, 5, new Vector2(550, 500), new Vector2(50, 100), Color.Green));
            //Gas(1000, 3, 3, 1, Color.Blue, 100, 100);

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
                    float mom = balls[i].mass * Math.Abs(balls[i].velocity.X) * 2;
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
            else
                return 1;
        }
        // functions whos generate start state
        void Gas(int particles, int minR, int maxR, float mass, Color color, float randomVx, float randomVy)
        {
            for (int i = 0; i < particles; i++)
            {
                int r = random.Next(minR, maxR);
                float m = (float)Math.PI * r * r * mass;
                if (color == Color.Black)
                {
                    int randColor = random.Next(0, 10);
                    balls.Add(new Ball(r, m, new Vector2((float)random.NextDouble() * wS, (float)random.NextDouble() * hS), new Vector2((float)random.NextDouble() * (randomVx * 2) - randomVx, (float)random.NextDouble() * (randomVy * 2) - randomVy), colors[randColor]));
                }
                balls.Add(new Ball(r, m, new Vector2((float)random.NextDouble() * wS, (float)random.NextDouble() * hS), new Vector2((float)random.NextDouble() * (randomVx * 2) - randomVx, (float)random.NextDouble() * (randomVy * 2) - randomVy), color));
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
                    position.Y = radius;
                else
                    position.Y = h - radius;
            }

            velocity += new Vector2(0, g * dt);
            position += velocity * dt;
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
            Debug.WriteLine(color.B.ToString() + " " + vnLP + "=" + vnL + "*(" + mass + "-" + ball.mass + ")+" + "2*" + ball.mass + "*" + bvnL + ") / (" + mass + "+" + ball.mass + ")");
            Debug.WriteLine(ball.color.B.ToString() + " " + bvnLP + "=" + bvnL + "*(" + ball.mass + "-" + mass + ")+" + "2*" + mass + "*" + vnL + ") / (" + ball.mass + "+" + mass + ")");

            /*Vector2 v1 = velocity;
            Vector2 v2 = ball.velocity;
            float m1 = mass;
            float m2 = ball.mass;
            Vector2 x1 = position;
            Vector2 x2 = ball.position;
            velocity = v1 - (2 * m2 / (m1 + m2)) * Vector2.Dot(v1 - v2, x1 - x2) / d.LengthSquared() * (x1 - x2);
            ball.velocity = v2 - (2 * m1 / (m2 + m1)) * Vector2.Dot(v2 - v1, x2 - x1) / d.LengthSquared() * (x2 - x1);*/

            //float s = radius + ball.radius - d.Length();
            //position += n * s * 0.5f;
            //ball.position -= n * s * 0.5f;
        }
    }
}