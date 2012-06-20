using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTkProject.Drawables.Models;
using System.Xml;


namespace OpenTkProject.Drawables
{
    public class LightSpot : Light
    {
        public int ProjectionTexture = 0;
        private string texturename;
        private bool useProjectionTexture = false;

        public int lightId;

        new public static string nodename = "lamp";

        public LightSpot(GameObject parent)
        {
            Parent = (Drawable)parent;

            Scene.spotlights.Add(this);

            name = Scene.getUniqueName();
            setupShadow();

            Parent.forceUpdate();

            createRenderObject();
        }

        private void createRenderObject()
        {
            drawable = new LightVolume(this);
            drawable.setMaterial("defShading\\lightSpot.xmf");
            drawable.setMesh("spotlightCone.obj");

            //drawable.Color = new Vector4(0.6f, 0.7f, 1.0f, 1);
            drawable.isVisible = true;
        }

        public void setupShadow()
        {
            viewInfo = new ViewInfo(this);
            viewInfo.zNear = 0.6f;
            viewInfo.zFar = 10f;
            viewInfo.fovy = (float)Math.PI/2f;
            viewInfo.updateProjectionMatrix();
        }

        #region load/save

        public override void save(ref StringBuilder sb, int level)
        {
            // converting object information to strings
            string myposition = GenericMethods.StringFromVector3(Position);
            string direction = GenericMethods.StringFromVector3(PointingDirection);
            string stringColor = GenericMethods.StringFromVector3(colorRgb);
            string mparent = Parent.name;


            string tab = GenericMethods.tabify(level - 1);
            string tab2 = GenericMethods.tabify(level);

            sb.AppendLine(tab + "<lamp name='" + name + "'>");
            sb.AppendLine(tab2 + "<position>" + myposition + "</position>");
            sb.AppendLine(tab2 + "<direction>" + direction + "</direction>");
            sb.AppendLine(tab2 + "<color>" + stringColor + "</color>");
            //sb.AppendLine(tab2 + "<parent>" + mparent + "'/>");

            if(Texture != null)
                sb.AppendLine(tab2 + "<texture>" + Texture + "</texture>");

            // output saving message
            Console.WriteLine("Saving Light: '" + name + "'");

            sb.AppendLine(tab + "</lamp>");

            // save childs
            saveChilds(ref sb, level);
        }

        protected override void specialLoad(ref System.Xml.XmlTextReader reader, string type)
        {
            if (reader.Name == "texture" && reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read();
                Texture = reader.Value;
            }
        }

        #endregion load/save

        public override void update()
        {
            if (Position != Parent.Position)
            {
                Position = Parent.Position;
                wasUpdated = true;
            }

            if (Orientation != Parent.Orientation)
            {
                Orientation = Parent.Orientation;
                wasUpdated = true;
            }

            if (wasUpdated)
            {
                updateChilds();
                shadowMatrix = Matrix4.Mult(viewInfo.modelviewProjectionMatrix, bias);
            }
        }

        public override void kill()
        {
            Scene.spotlights.Remove(this);

            Parent.removeChild(this);

            killChilds();
        }

        public override void activate(Shader shader, Drawable drawble)
        {
            int active = 0;
            if (viewInfo.frustrumCheck(drawble))
            {
                GL.Uniform3(shader.lightLocationsLocation[lightId], ref position);
                GL.Uniform3(shader.lightDirectionsLocation[lightId], ref pointingDirection);
                GL.Uniform3(shader.lightColorsLocation[lightId], ref colorRgb);

                GL.UniformMatrix4(shader.lightViewMatrixLocation[lightId], false, ref shadowMatrix);


                //GL.Uniform1(shader.lightTextureLocation[lightId], 1, ref ProjectionTexture);

                active = 1;
            }
            GL.Uniform1(shader.lightActiveLocation[lightId], 1, ref active);
        }

        public override void activateDeffered(Shader shader)
        {
            shader.insertUniform(Shader.Uniform.defPosition, ref position);
            shader.insertUniform(Shader.Uniform.defDirection, ref pointingDirection);
            shader.insertUniform(Shader.Uniform.defColor, ref colorRgb);

            shader.insertUniform(Shader.Uniform.in_no_lights, ref scene.lightCount);
            shader.insertUniform(Shader.Uniform.curLight, ref lightId);

            shader.insertUniform(Shader.Uniform.defMatrix, ref shadowMatrix);
            shader.insertUniform(Shader.Uniform.defInvPMatrix, ref viewInfo.invModelviewProjectionMatrix);

            //GL.Uniform1(shader.lightTextureLocation[lightId], 1, ref ProjectionTexture);
        }

        public override Vector4 Color
        {
            get
            {
                return base.Color;
            }
            set
            {
                base.Color = value;

                Drawable tmpPar = (Drawable)Parent;
                tmpPar.EmissionColor = colorRgb;
            }
        }

        public string Texture
        {
            get { return texturename; }
            set
            {
                texturename = value;
                ProjectionTexture = gameWindow.textureLoader.getTextureId(value);
                useProjectionTexture = true;
            }
        }
    }
}
