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

        public void draw(Shader shader, int[] curtexture, Vector2 shadingVec)
        {
            gameWindow.checkGlError("--uncaught ERROR drawing 2d Quad--" + shader.name);

            GL.UseProgram(shader.handle);

            shader.insertUniform(Shader.Uniform.in_vector, ref shadingVec);
            shader.insertUniform(Shader.Uniform.in_screensize, ref gameWindow.virtual_size);
            shader.insertUniform(Shader.Uniform.in_rendersize, ref gameWindow.currentSize);
            shader.insertUniform(Shader.Uniform.in_time, ref gameWindow.frameTime);

            if (Scene != null)
            {
                shader.insertUniform(Shader.Uniform.in_no_lights, ref Scene.lightCount);
                shader.insertUniform(Shader.Uniform.curLight, ref Scene.currentLight);
            }

            initTextures(curtexture, shader.handle, "Texture");
            GL.BindVertexArray(vaoHandle[0]);
            GL.DrawElements(BeginMode.Triangles, meshes[0].indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

            gameWindow.checkGlError("--Drawing ERROR Quad--" + shader.name);
        }
    }
}
