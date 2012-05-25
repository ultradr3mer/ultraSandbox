using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Input;
using System.Windows.Forms;

namespace OpenTkProject
{
    public class GameInput : GameObject
    {
        public Point move;
        public KeyboardDevice keyboard;
        public MouseDevice mouse;

        public GameInput(Scene scene, KeyboardDevice keyboard, MouseDevice mouse)
        {
            Parent = scene;

            this.keyboard = keyboard;
            this.mouse = mouse;
        }

        public override void update()
        {
            // Calculates how far the mouse has moved since the last call to this method
            Point center = new Point(
                (gameWindow.Bounds.Left + gameWindow.Bounds.Right) / 2,
                (gameWindow.Bounds.Top + gameWindow.Bounds.Bottom) / 2);

            Point mouse_current = gameWindow.PointToScreen(new Point(gameWindow.Mouse.X, gameWindow.Mouse.Y));
            Cursor.Position = center;

            Point mouse_delta = new Point(
                mouse_current.X - center.X,
                -mouse_current.Y + center.Y);

            move = mouse_delta;
            //return mouse_delta;
        }
    }
}
