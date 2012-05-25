using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTkProject.Drawables
{
    public class Quad2d:Drawable
    {
        public Quad2d(OpenTkProjectWindow mGameWindow)
        {
            this.gameWindow = mGameWindow;
            setMaterial("composite.xmf");
            setMesh("sprite_plane.obj");
        }

        public Quad2d(GameObject parent) 
        {
            Parent = parent;
            setMaterial("composite.xmf");
            setMesh("sprite_plane.obj");
        }

        public Quad2d() { }

        public void draw(Shader curShader, int[] curtexture)
        {
            draw(curShader, curtexture, Vector2.Zero);
        }

        public void draw(Shader curShader, int[] curtexture, Vector2 shadingVec)
        {
            gameWindow.checkGlError("--uncaught ERROR drawing 2d Quad--" + curShader.name);

            GL.UseProgram(curShader.handle);

            GL.Uniform2(curShader.vectorLocation, ref shadingVec);
            GL.Uniform2(curShader.screenSizeLocation, ref gameWindow.virtual_size);
            GL.Uniform2(curShader.renderSizeLocation, ref gameWindow.currentSize);
            GL.Uniform1(curShader.timeLocation,1, ref gameWindow.frameTime);

            if (Scene != null)
            {
                GL.Uniform1(curShader.LightCountLocation, 1, ref Scene.lightCount);
                GL.Uniform1(curShader.curLightLocation, 1, ref Scene.currentLight);
            }

            initTextures(curtexture, curShader.handle, "Texture");
            GL.BindVertexArray(vaoHandle[0]);
            GL.DrawElements(BeginMode.Triangles, meshes[0].indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

            gameWindow.checkGlError("--Drawing ERROR Quad--" + curShader.name);
        }
    }
}
