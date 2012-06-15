using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTkProject.Drawables
{
    public class LightSun : Light
    {
        float maxShadowDist = 120;
        float innerShadowDist  = 30;
        public Vector3 lightAmbient;
        public SunViewInfo innerViewInfo;
        private Matrix4 innerShadowMatrix;

        float nextFarUpdate;

        public LightSun(Vector3 color, Scene scene)
        {
            Parent = scene;
            Color = new Vector4(color,1);

            viewInfo = new SunViewInfo(this, maxShadowDist, maxShadowDist);
            innerViewInfo = new SunViewInfo(this, innerShadowDist, maxShadowDist);
            /*
            viewInfo.zNear = 0.6f;
            viewInfo.zFar = 20f;
            viewInfo.fovy = (float)Math.PI / 2f;
            viewInfo.updateProjectionMatrix();
            */
            

            viewInfo.update();
            innerViewInfo.update();

            nextFarUpdate = gameWindow.frameTime;
        }

        public override void update()
        {
            Position = Parent.Position;
            if (gameWindow.frameTime > nextFarUpdate)
            {
                updateChilds();
                innerShadowMatrix = Matrix4.Mult(innerViewInfo.modelviewProjectionMatrix, bias);

                shadowMatrix = Matrix4.Mult(viewInfo.modelviewProjectionMatrix, bias);
                viewInfo.wasUpdated = true;
                nextFarUpdate = gameWindow.frameTime + 0.5f;
            }
        }

        public void activate(Shader shader, Drawable drawble)
        {
            GL.Uniform3(shader.sunDirection, ref pointingDirection);
            GL.Uniform3(shader.sunColor, ref colorRgb);

            shader.insertUniform(Shader.Uniform.in_lightambient, ref lightAmbient);

            GL.UniformMatrix4(shader.sunMatrix, false, ref shadowMatrix);
            GL.UniformMatrix4(shader.sunInnerMatrix, false, ref innerShadowMatrix);
        }
    }
}
