using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace OpenTkProject.Drawables.Models
{
    public class LightVolume : Model
    {
        Light light;

        public LightVolume(GameObject parent)
        {
            Parent = parent;
            light = (Light)parent;
        }

        internal void draw(int[] textures, ref ViewInfo curView)
        {
            Shader shader = materials[0].shader;
            Mesh curMesh = meshes[0];

            gameWindow.checkGlError("--uncaught ERROR drawing lightVolume--" + shader.name);

            GL.UseProgram(shader.handle);

            //shader.insertUniform(Shader.Uniform.in_vector, ref shadingVec);
            shader.insertUniform(Shader.Uniform.in_screensize, ref gameWindow.virtual_size);
            shader.insertUniform(Shader.Uniform.in_rendersize, ref gameWindow.currentSize);
            shader.insertUniform(Shader.Uniform.in_time, ref gameWindow.frameTime);
            shader.insertUniform(Shader.Uniform.invMVPMatrix, ref curView.invModelviewProjectionMatrix);

            shader.insertUniform(Shader.Uniform.viewUp , ref curView.pointingDirectionUp);
            shader.insertUniform(Shader.Uniform.viewRight, ref curView.pointingDirectionRight);
            shader.insertUniform(Shader.Uniform.viewDirection, ref curView.pointingDirection);
            shader.insertUniform(Shader.Uniform.viewPosition, ref curView.position);

            if (Scene != null)
            {
                shader.insertUniform(Shader.Uniform.in_no_lights, ref Scene.lightCount);
                shader.insertUniform(Shader.Uniform.curLight, ref Scene.currentLight);

                shader.insertUniform(Shader.Uniform.in_eyepos, ref Scene.eyePos);
            }

            setupMatrices(ref curView,ref shader,ref  curMesh);

            light.activateDeffered(shader);

            initTextures(textures, shader.handle, "Texture");
            GL.BindVertexArray(vaoHandle[0]);
            GL.DrawElements(BeginMode.Triangles, meshes[0].indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

            gameWindow.checkGlError("--Drawing ERROR Volume--" + shader.name);
        }
    }
}
