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

        new public static string nodename = "lamp";

        public LightSpot(GameObject parent)
        {
            Parent = (Drawable)parent;

            Scene.spotlights.Add(this);

            name = Scene.getUniqueName();
            setupShadow();

            Parent.forceUpdate();
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

        internal void activate(Shader shader, int i, Drawable drawble)
        {
            int active = 0;
            if (viewInfo.frustrumCheck(drawble))
            {
                GL.Uniform3(shader.lightLocationsLocation[i], ref position);
                GL.Uniform3(shader.lightDirectionsLocation[i], ref pointingDirection);
                GL.Uniform3(shader.lightColorsLocation[i], ref colorRgb);

                GL.UniformMatrix4(shader.lightViewMatrixLocation[i], false, ref shadowMatrix);


                GL.Uniform1(shader.lightTextureLocation[i], 1, ref ProjectionTexture);

                active = 1;
            }
            GL.Uniform1(shader.lightActiveLocation[i], 1, ref active);
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
                ProjectionTexture = gameWindow.textureLoader.getTexture(value);
                useProjectionTexture = true;
            }
        }
    }
}
