using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTkProject.Drawables.Models
{
    public class Gui : Quad2d
    {
        public Vector2 sizePx;

        public static Vector4 colorA = new Vector4(0.8f, 0.3f, 0.8f, 1.0f);
        public static Vector4 colorB = new Vector4(0.6f, 0.7f, 1.0f, 1.0f);

        public Gui(GameObject parent)
        {
            Parent = parent;
            sizePx = gameWindow.virtual_size;
        }

        public virtual void performClick(Vector2 pos)
        {
            clickChilds(pos);
        }

        public override void update()
        {
            updateChilds();
        }

        public virtual void clickChilds(Vector2 pos)
        {
            foreach (var child in childs)
            {
                Gui gChild = (Gui)child;
                gChild.performClick(pos);
            }
        }

        public virtual void performHover(Vector2 pos)
        {
            hoverChilds(pos);
        }

        public virtual void hoverChilds(Vector2 pos)
        {
            foreach (var child in childs)
            {
                Gui gChild = (Gui)child;
                gChild.performHover(pos);
            }
        }

        public Gui() { }

        public override void draw()
        {
            if (isVisible)
            {
                GL.Enable(EnableCap.Blend);

                drawChilds();
            }

        }

        public void drawChilds()
        {
            foreach (Drawable child in childs)
            {
                child.draw();
            }
        }
    }


    public class GuiElement : Gui
    {
        protected Vector2 screenSize = Vector2.One;
        protected Vector2 screenPosition = Vector2.Zero;

        protected float elementValue = 0f;

        public GuiElement(Gui parent)
        {
            Parent = parent;

            this.sizePx = parent.sizePx;

            setMaterial("crosshair.xmf");
            setMesh("sprite_plane.obj");

            color = Gui.colorA;
        }

        public override void draw()
        {
            if (isVisible)
            {
                Shader shader = activateMaterial(ref materials[0]);

                //GL.DepthMask(false);
                shader.insertUniform(Shader.Uniform.in_hudvalue, ref elementValue);
                shader.insertUniform(Shader.Uniform.in_hudsize, ref screenSize);
                shader.insertUniform(Shader.Uniform.in_hudpos, ref screenPosition);
                shader.insertUniform(Shader.Uniform.in_hudcolor, ref color);

                shader.insertUniform(Shader.Uniform.in_screensize, ref gameWindow.virtual_size);
                shader.insertUniform(Shader.Uniform.in_rendersize, ref gameWindow.currentSize);

                //GL.Uniform1(curShader.timeLocation, 1, ref mGameWindow.frameTime);
                //GL.Uniform1(curShader.passLocation, 1, ref mGameWindow.currentPass);

                GL.BindVertexArray(vaoHandle[0]);
                GL.DrawElements(BeginMode.Triangles, meshes[0].indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

                drawChilds();
            }
        }
        
        public void setSizePix(Vector2 newSize)
        {
            Size = Vector2.Divide(newSize, gameWindow.virtual_size);
        }

        static Vector2 virtualScreenSize = new Vector2(1920, 1080);
        public void setSizeRel(Vector2 newSize)
        {
            Size = Vector2.Divide(newSize, virtualScreenSize);
        }

        public new virtual Vector2 Size { get { return screenSize; } set { screenSize = value; } }

        public new virtual Vector2 Position { get { return screenPosition; } set { screenPosition = value; } }

        public void setPositionPix(Vector2 newPos)
        {
            Position = Vector2.Divide(newPos, gameWindow.virtual_size);
        }

        public void setValue(float newValue)
        {
            this.elementValue = newValue;
        }
    }


    public delegate void OnClick(Button caller);

    public class ButtonList : GuiElement
    {
        public ButtonList(Gui parent)
            : base(parent)
        {
        }

        public override void draw()
        {
            drawChilds();
        }

        public override int AddChild(GameObject newChild)
        {
            childs.Add(newChild);
            calculateChildPos();

            return childs.Count - 1;
        }

        private void calculateChildPos()
        {
            Vector2 direction = new Vector2(0, Size.Y);

            Vector2 from = Position - direction;
            Vector2 to = Position + direction;

            for (int i = 0; i < childs.Count; i++)
            {
                Button gChild = (Button)childs[i];

                float iRel = (float)i / (childs.Count - 1);

                gChild.Position = from * iRel + to * (1 - iRel);

                //childs[i] = gChild;
            }
        }

        public override void removeChild(GameObject reChild)
        {
            childs.Remove(reChild);
        }
    }

    public class Button : GuiElement
    {
        Vector2 Min;
        Vector2 Max;

        OnClick handlerClick;

        float clicked;

        public Button(Gui parent)
            : base(parent)
        {
            setMaterial("hud\\blank_icon.xmf");
        }

        public OnClick HandlerClick { get { return handlerClick; } set { handlerClick = value; } }

        public override Vector2 Size
        {
            get { return screenSize; }
            set { screenSize = value; updateBB(); }
        }

        public override Vector2 Position
        {
            get { return screenPosition; }
            set { screenPosition = value; updateBB(); }
        }

        private void updateBB()
        {
            Min = Position - Size;
            Max = Position + Size;
        }

        public override void performHover(Vector2 pos)
        {
            hoverChilds(pos);

            if (pos.X > Min.X && pos.X < Max.X
                && pos.Y > Min.Y && pos.Y < Max.Y)
            {
                color = Gui.colorA;
            }
            else
            {
                color = Gui.colorB * 0.2f;
            }

            color = color + Vector4.One * clicked * 0.2f;
        }


        public override void performClick(Vector2 pos)
        {
            clickChilds(pos);

            if (pos.X > Min.X && pos.X < Max.X
                && pos.Y > Min.Y && pos.Y < Max.Y)
            {
                clicked = 1;
                if (handlerClick != null)
                    handlerClick(this);
            }
        }

        public override void update()
        {
            clicked *= 0.8f;
        }
    }
}
