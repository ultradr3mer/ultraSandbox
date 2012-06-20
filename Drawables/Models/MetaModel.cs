using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jitter.Dynamics;
using OpenTkProject.Game.Voxel;
using OpenTK;
using System.Xml;

namespace OpenTkProject.Drawables.Models
{
    class MetaModel : PhysModel
    {
        public VoxelVolume volume;

        new public static string nodename = "metamodel";

        public MetaModel(GameObject parent)
            : base(parent)
        {
            volume = new VoxelVolumeSphere(scene.voxelManager, 3f);

            IsStatic = true;

            grabable = false;
        }

        public override void save(ref StringBuilder sb, int level)
        {
            // reading Object Atrributes and Converting them to Strings
            string position = GenericMethods.StringFromVector3(this.Position);
            string rotation = GenericMethods.StringFromJMatrix(Body.Orientation);
            string stringMaterial = GenericMethods.StringFromStringList(Materials);
            string meshes = GenericMethods.StringFromStringList(Meshes);
            string pboxes = GenericMethods.StringFromStringList(PhysBoxes);
            string radius = GenericMethods.StringFromFloat(volume.AffectionRadius);

            string tab = GenericMethods.tabify(level - 1);
            string tab2 = GenericMethods.tabify(level);

            sb.AppendLine(tab + "<"+nodename+" name='" + name + "'>");
            sb.AppendLine(tab2 + "<position>" + position + "</position>");
            //sb.AppendLine(tab2 + "<rotation>" + rotation + "</rotation>");
            sb.AppendLine(tab2 + "<materials>" + stringMaterial + "</materials>");
            sb.AppendLine(tab2 + "<meshes>" + meshes + "</meshes>");
            sb.AppendLine(tab2 + "<pboxes>" + pboxes + "</pboxes>");
            sb.AppendLine(tab2 + "<vradius>" + radius + "</vradius>");


            Console.WriteLine("Saving metamodel: '" + name + "'");

            saveChilds(ref sb, level);

            sb.AppendLine(tab + "</"+nodename+">");
        }

        protected override void specialLoad(ref System.Xml.XmlTextReader reader, string type)
        {
            base.specialLoad(ref reader, type);

            if (reader.Name == "vradius" && reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read();
                volume.AffectionRadius = GenericMethods.FloatFromString(reader.Value);
            }
        }

        public override RigidBody Body { get { return body; } set { body = value; forceUpdate(); body.IsStatic = true; } }

        public override Vector3 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                volume.Position = value;
            }
        }

        public override void kill()
        {
            volume.kill();
            base.kill();
        }

        public override void draw(ViewInfo curView, bool targetLayer)
        {
        }

        public override void drawNormal(ViewInfo curView)
        {
        }

        public override void drawShadow(ViewInfo curView)
        {
        }

        public override void drawDefInfo(ViewInfo curView)
        {
        }
    }
}
