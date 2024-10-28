using System;
using Microsoft.Xna.Framework;

namespace bouncing_ball_simulation.Class
{
    public class Ball
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
                if (position.X - radius < 0) position.X = radius;
                else position.X = w - radius;
            }
            if (position.Y - radius < 0 || position.Y + radius > h)
            {
                velocity.Y = -velocity.Y;
                if (position.Y - radius < 0)
                {
                    float d = position.Y;
                    position.Y = radius;
                    d -= position.Y;
                    velocity.Y = MathF.Sqrt(velocity.Y * velocity.Y - 2 * d * g);
                }
                else
                {
                    float d = position.Y;
                    position.Y = h - radius;
                    d -= position.Y;
                    float correction = velocity.Y * velocity.Y - 2 * d * g;
                    if (correction > 0) velocity.Y = -MathF.Sqrt(correction);
                    else velocity.Y = 0;
                }
            }

            velocity += new Vector2(0, g * dt * 0.5f);
            position += velocity * dt;
            velocity += new Vector2(0, g * dt * 0.5f);
        }

        public void Collision(Ball ball)
        {
            Vector2 d = position - ball.position;
            float dL = d.Length();
            float minD = radius + ball.radius;

            if (minD < dL) return;

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

            float s = minD - dL;
            position += n * s * ball.mass / (mass + ball.mass);
            ball.position -= n * s * mass / (mass + ball.mass);
        }
    }
}