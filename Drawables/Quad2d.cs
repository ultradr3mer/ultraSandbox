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
            draw(curShader, curtexture,Shader.Uniform.in_vector, Vector2.Zero);
        }
        /*
        public void draw(Shader curShader, int[] curtexture, Shader.Uniform uniform, Vector2 vector2)
        {
            draw(curShader, curtexture,new UniformPairList( Shader.Uniform.in_vector, Vector2.Zero));
        }
        */
        public void draw(Shader shader, int[] curtexture, Shader.Uniform uniform, object obj)
        {
            gameWindow.checkGlError("--uncaught ERROR drawing 2d Quad--" + shader.name);

            GL.UseProgram(shader.handle);

            //unis.insert(ref shader);
            shader.insertGenUniform(uniform, obj);
            shader.insertUniform(Shader.Uniform.in_screensize, ref gameWindow.virtual_size);
            shader.insertUniform(Shader.Uniform.in_rendersize, ref gameWindow.currentSize);
            shader.insertUniform(Shader.Uniform.in_time, ref gameWindow.frameTime);

            //shader.insertUniform(Shader.Uniform.in_near, ref );
            //shader.insertUniform(Shader.Uniform.in_far, ref gameWindow.currentSize);

            if (Scene != null)
            {
                shader.insertUniform(Shader.Uniform.in_no_lights, ref Scene.lightCount);
                shader.insertUniform(Shader.Uniform.curLight, ref Scene.currentLight);

                shader.insertUniform(Shader.Uniform.in_eyepos, ref Scene.eyePos);
            }

            initTextures(curtexture, shader.handle, "Texture");
            GL.BindVertexArray(vaoHandle[0]);
            GL.DrawElements(BeginMode.Triangles, meshes[0].indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

            gameWindow.checkGlError("--Drawing ERROR Quad--" + shader.name);
        }

        internal void draw(Scene.ShaderTypes shaderTypes, int[] p, Shader.Uniform uniform, Vector2 vector2)
        {
            Shader shader = Scene.getShader(shaderTypes);
            draw(shader, p, uniform, vector2);
        }

        internal void draw(Scene.ShaderTypes shaderTypes, int[] p)
        {
            Shader shader = Scene.getShader(shaderTypes);
            draw(shader, p);
        }

        internal void draw(Scene.ShaderTypes shaderTypes, int[] p, Shader.Uniform uniform, Matrix4 matrix4)
        {
            Shader shader = Scene.getShader(shaderTypes);
            draw(shader, p, uniform, matrix4);
        }
    }
}
