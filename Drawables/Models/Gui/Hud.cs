using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace OpenTkProject.Drawables.Models
{
    public class Hud : Gui
    {
        public GuiElement crossHair;
        public HudNumber fpsCounter;

        public Hud(GameObject parent)
            : base(parent)
        {
            crossHair = new GuiElement(this);
            crossHair.setSizeRel(new Vector2(100, 100));
            crossHair.setMaterial("crosshair.xmf");

            fpsCounter = new HudNumber(this);
            fpsCounter.Position = new Vector2(0, -0.8f);
            fpsCounter.setSizeRel(new Vector2(80, 160));
            fpsCounter.digits = 3;
        }
    }

    public class HudNumber : GuiElement
    {
        public int digits = 6;

        public HudNumber(Gui parent)
            : base(parent)
        {
            setMaterial("number.xmf");
        }

        public override void draw()
        {
            if (isVisible)
            {
                //gameWindow.checkGlError("--uncaught ERROR--");

                Shader mShader = activateMaterial(ref materials[0]);

                //GL.DepthMask(false);

                GL.Uniform2(mShader.hudElementSize, ref screenSize);
                GL.Uniform4(mShader.hudElementColor, ref color);

                GL.Uniform2(mShader.screenSizeLocation, ref gameWindow.virtual_size);
                GL.Uniform2(mShader.renderSizeLocation, ref gameWindow.currentSize);

                //GL.Uniform1(curShader.timeLocation, 1, ref mGameWindow.frameTime);
                //GL.Uniform1(curShader.passLocation, 1, ref mGameWindow.currentPass);

                GL.BindVertexArray(vaoHandle[0]);
                for (int i = 0; i < digits; i++)
                {
                    Vector2 positon = new Vector2(screenSize.X * (digits - 2 * i - 1), 0f) + screenPosition;
                    GL.Uniform2(mShader.hudElementPos, ref positon);

                    float value = elementValue / (float)Math.Pow(10, i);
                    //Console.WriteLine(value);
                    GL.Uniform1(mShader.hudElementValue, 1, ref value);
                    GL.DrawElements(BeginMode.Triangles, meshes[0].indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

                    //gameWindow.checkGlError("--Drawing ERROR Hud--");
                }
            }
        }
    }
}
