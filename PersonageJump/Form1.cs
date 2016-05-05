using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PersonageJump
{
    public partial class Form1 : Form
    {
        Rectangle ground;

        Rectangle personage;

        Graphics g;

        bool isUp, isDown, isLeft, isRigth;

        private Rect rect;

        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            Size = new Size(600, 600);

            rect = new Rect() { Height = 100, Width = 80, Position = new PointF(200, 100) };

            Application.Idle += delegate { Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Update();
            e.Graphics.DrawLine(Pens.Green, 0, World.GroundY, Width, World.GroundY);
            rect.Draw(e.Graphics);
        }

        private DateTime lastUpdate = DateTime.MinValue;

        new void Update()
        {
            var now = DateTime.Now;
            var dt = (float)(now - lastUpdate).TotalMilliseconds / 100f;
            //
            if (lastUpdate != DateTime.MinValue)
            {
                rect.Update(dt);
            }
            //
            lastUpdate = now;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                const int jumpSpeed = 100;

                if (rect.Position.Y + rect.Height / 2 > World.GroundY - 10)//только если стоим на земле
                    rect.Velocity = new PointF(rect.Velocity.X, rect.Velocity.Y - jumpSpeed);//прыгаем вверх
            }
            base.OnKeyDown(e);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            ground = new Rectangle();
            ground.Size = new Size(ClientSize.Width, 50);
            ground.X = 0;
            ground.Y = ClientSize.Height - 50;

            personage = new Rectangle();
            personage.Size = new Size(20, 70);
            personage.Location = new Point(30, ground.Y - personage.Size.Height);

            timer1.Enabled = false;

            g = CreateGraphics();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            g.FillRectangle(Brushes.Black, ground);
            g.FillEllipse(Brushes.OrangeRed, personage);
            timer1.Stop();
            g.Dispose();
        }

       /* private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
 
            if (e.KeyCode == Keys.Up) isUp = true;
            if (e.KeyCode == Keys.Down) isDown = true;

            if (e.KeyCode == Keys.Right) isRigth = true;
            if (e.KeyCode == Keys.Left) isLeft = true;
            
            if (isRigth)
            {
                Clear(personage);
                personage.X += 20;
                DrawNew(personage);
            }

            if (isLeft)
            {
                Clear(personage);
                personage.X -= 20;
                DrawNew(personage);
            }


            if (isUp)
            {
                Clear(personage);
                personage.Y -= 70;
                DrawNew(personage);
                Thread.Sleep(150);
                Clear(personage);
                personage.Y += 70;
                DrawNew(personage);
            }

            if (isUp && isRigth)
            {
                Clear(personage);
                personage.Y -= 70;
                DrawNew(personage);
                Thread.Sleep(250);
                Clear(personage);
                personage.Y += 70;
                personage.X += 20;
                DrawNew(personage);
               // MessageBox.Show("Pressed UP and Rigth");
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up) isUp = false;
            if (e.KeyCode == Keys.Down) isDown = false;

            if (e.KeyCode == Keys.Right) isRigth = false;
            if (e.KeyCode == Keys.Left) isLeft = false;
        }
        */
        private void Clear(Rectangle rect)
        {
            Color c = BackColor;

            using (var g = CreateGraphics())
            using (var b = new SolidBrush(c))
            {
                g.FillEllipse(b, rect);
            }
        }

        private void DrawNew(Rectangle rect)
        {
            using (var g = CreateGraphics())
            {
                g.FillEllipse(Brushes.OrangeRed, rect);
            }
        }
    }

    /// <summary>
    /// Твердое тело
    /// </summary>
    class RigidBody
    {
        //координаты
        public PointF Position { get; set; }
        //скорость
        public PointF Velocity { get; set; }
        //масса
        public float Mass { get; set; }
        //упругость
        public float Spring { get; set; }
        //гравитация
        public bool Gravity { get; set; }
        //приложенная сила
        public PointF Force { get; set; }

        public RigidBody()
        {
            Spring = 0.1f;
            Mass = 1;
            Gravity = true;
        }

        /// <summary>
        /// Обновление
        /// </summary>
        public virtual void Update(float dt)
        {
            //сила
            var force = Force;
            Force = Point.Empty;
            //гравитация
            if (Gravity)
                force = new PointF(force.X, force.Y + 9.8f * Mass);
            //ускорение
            var ax = force.X / Mass;
            var ay = force.Y / Mass;
            //скорость
            Velocity = new PointF(Velocity.X + ax * dt, Velocity.Y + ay * dt);
            //координаты
            Position = new PointF(Position.X + Velocity.X * dt, Position.Y + Velocity.Y * dt);
        }

        /// <summary>
        /// Вызвать при столкновении с землей
        /// </summary>
        public void OnGroundCollision(float groundY)
        {
            if (Velocity.Y < -float.Epsilon) return;
            Position = new PointF(Position.X, groundY - 0.0001f);
            Velocity = new PointF(Velocity.X, -Velocity.Y * Spring);
        }
    }

    static class World
    {
        public const int GroundY = 500;
    }

    /// <summary>
    /// Прямоугольник
    /// </summary>
    class Rect : RigidBody
    {
        public float Width { get; set; }
        public float Height { get; set; }

        public override void Update(float dt)
        {
            //столкновенине с землей?
            if (Position.Y + Height / 2 > World.GroundY)
                OnGroundCollision(World.GroundY - Height / 2);
            //
            base.Update(dt);
        }

        public void Draw(Graphics gr)
        {
            gr.FillRectangle(Brushes.Black, Position.X - Width / 2, Position.Y - Height / 2, Width, Height);
        }
    }
}
