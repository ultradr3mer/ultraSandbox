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

                Shader shader = activateMaterial(ref materials[0]);

                //GL.DepthMask(false);

                shader.insertUniform(Shader.Uniform.in_hudvalue, ref elementValue);
                shader.insertUniform(Shader.Uniform.in_hudsize, ref screenSize);
                shader.insertUniform(Shader.Uniform.in_hudpos, ref screenPosition);
                shader.insertUniform(Shader.Uniform.in_hudcolor, ref color);

                //GL.Uniform1(curShader.timeLocation, 1, ref mGameWindow.frameTime);
                //GL.Uniform1(curShader.passLocation, 1, ref mGameWindow.currentPass);

                GL.BindVertexArray(vaoHandle[0]);
                for (int i = 0; i < digits; i++)
                {
                    Vector2 positon = new Vector2(screenSize.X * (digits - 2 * i - 1), 0f) + screenPosition;
                    shader.insertUniform(Shader.Uniform.in_hudpos, ref positon);

                    float value = elementValue / (float)Math.Pow(10, i);
                    shader.insertUniform(Shader.Uniform.in_hudvalue, ref value);

                    GL.DrawElements(BeginMode.Triangles, meshes[0].indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

                    //gameWindow.checkGlError("--Drawing ERROR Hud--");
                }
            }
        }
    }
}
