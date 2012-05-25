using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using OpenTK;
using OpenTkProject.Game;
using System.Xml;

namespace OpenTkProject.Drawables.Models
{
    public class PhysModel : Model
    {
        protected RigidBody body;

        private List<string> pBoxList = new List<string> { };

        public bool grabable = true;

        public PhysModel(GameObject parent)
            : base(parent)
        {

        }

        public override RigidBody Body { get { return body; } set { body = value; forceUpdate(); } }

        // saving body to database
        public override void save(ref StringBuilder sb, int level)
        {
            // reading Object Atrributes and Converting them to Strings
            string position = GenericMethods.StringFromVector3(this.Position);
            string rotation = GenericMethods.StringFromJMatrix(Body.Orientation);
            string stringMaterial = GenericMethods.StringFromStringList(Materials);
            string meshes = GenericMethods.StringFromStringList(Meshes);
            string pboxes = GenericMethods.StringFromStringList(PhysBoxes);

            string tab = GenericMethods.tabify(level - 1);
            string tab2 = GenericMethods.tabify(level);

            sb.AppendLine(tab + "<pmodel name='" + name + "'>");
            sb.AppendLine(tab2 + "<position>" + position + "</position>");
            sb.AppendLine(tab2 + "<rotation>" + rotation + "</rotation>");
            sb.AppendLine(tab2 + "<materials>" + stringMaterial + "</materials>");
            sb.AppendLine(tab2 + "<meshes>" + meshes + "</meshes>");
            sb.AppendLine(tab2 + "<pboxes>" + pboxes + "</pboxes>");

            if (IsStatic)
                sb.AppendLine(tab2 + "<isstatic/>");



            /*
            // Creating Sql Command
            sb.Append("INSERT INTO WorldObjects (id, name, position, rotation , material, meshes, pboxes, static )" +
                " VALUES(NULL, '" + name + "', '" + position + "', '" + rotation + "' , '" + stringMaterial + "' , '"
                + meshes + "' , '" + pboxes + "' , " + isstatic + ");");

             */

            Console.WriteLine("Saving model: '" + name + "'");

            saveChilds(ref sb,level);

            sb.AppendLine(tab+"</pmodel>");
        }

        public override Matrix4 Orientation
        {
            get
            {
                return base.Orientation;
            }
            set
            {
                base.Orientation = value;
                if (body != null)
                {
                    body.Orientation = GenericMethods.FromOpenTKMatrix(value);
                }
            }
        }

        public override Vector3 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;

                if (body != null)
                {
                    body.Position = GenericMethods.FromOpenTKVector(value);
                }
            }
        }

        #region physicob management

        public void setPhysMesh(string name)
        {
            pBoxList.Add(name);

            Mesh pboxMesh = gameWindow.meshLoader.getMesh(name);
            Shape objShape = new ConvexHullShape(GenericMethods.FromOpenTKVecArToJVecList(pboxMesh.positionVboData));

            setPhysMesh(objShape);
        }

        public void setPhysMesh(Shape newShape)
        {
            RigidBody newBody = new RigidBody(newShape);

            newBody.Position = GenericMethods.FromOpenTKVector(Position);
            newBody.Mass = 20f;

            newBody.Tag = this;
            newBody.Orientation = GenericMethods.FromOpenTKMatrix(Orientation);

            Body = newBody;
            Scene.world.AddBody(newBody);
        }

        public void setPhysMesh(string pMeshName, JMatrix mOrientation)
        {
            pBoxList.Add(pMeshName);

            Mesh pboxMesh = gameWindow.meshLoader.getMesh(pMeshName);
            ConvexHullShape objShape = new ConvexHullShape(GenericMethods.FromOpenTKVecArToJVecList(pboxMesh.positionVboData));

            setPhysMesh(objShape);
        }



        // needs to be fixed (Compound Shapes)

        public List<string> PhysBoxes
        {
            get { return pBoxList; }
            set
            {

            if (value.Count == 1)
            {
                setPhysMesh(value[0]);
                return;
            }

            CompoundShape.TransformedShape[] transformedShapes
                    = new CompoundShape.TransformedShape[value.Count];

            for (int i = 0; i < value.Count; i++)
            {
                pBoxList.Add(value[i]);

                Mesh pboxMesh = gameWindow.meshLoader.getMesh(value[i]);
                ConvexHullShape curShape = new ConvexHullShape(GenericMethods.FromOpenTKVecArToJVecList(pboxMesh.positionVboData));

                transformedShapes[i] = new CompoundShape.TransformedShape();
                transformedShapes[i].Shape = curShape;
                transformedShapes[i].Orientation = JMatrix.Identity;
                transformedShapes[i].Position = -1.0f * curShape.Shift;
            }


            // Create one compound shape
            CompoundShape cs = new CompoundShape(transformedShapes);


            setPhysMesh(cs);
        } 
        }

        public void updateMatrix()
        {
            Position = GenericMethods.ToOpenTKVector(Body.Position);
            Orientation = GenericMethods.ToOpenTKMatrix(Body.Orientation);
        }

        #endregion physicob management

        #region update

        public override void update()
        {
            updateSelection();

            if (Body != null)
            {
                if (!Body.IsStaticOrInactive || Forceupdate)
                {
                    wasUpdated = true;
                    updateMatrix();
                    updateChilds();
                }
            }
        }

        public override void forceUpdate()
        {
            updateSelection();

            if (Body != null)
            {
                wasUpdated = true;
                updateMatrix();
                updateChilds();
            }
        }

        #endregion upadate

        public void dissolve() {
            createDisModel();

            kill();
        }

        public override void kill()
        {
            Scene.world.RemoveBody(Body);
            base.kill();
        }

        private void createDisModel()
        {
            DissovlingModel disModel = new DissovlingModel(Parent);

            foreach (Material material in materials)
            {
                disModel.addMaterial("dissolve.xmf");
            }

            foreach (Mesh mesh in meshes)
            {
                disModel.addMesh(mesh);
            }

            disModel.Position = Position;
            //disModel.updateModelMatrix();

            disModel.Orientation = Orientation;
            disModel.Scene = Scene;
        }

        public void setName(string newName)
        {
            //parent.childNames.Remove(name);
            //parent.childNames.Add(name, childId);

            Parent.renameChild(this, newName);
            name = newName;
        }

        public bool IsStatic
        {
            get { return body.IsStatic; }
            set
            {
                if (body != null)
                {
                    scene.world.RemoveBody(body);
                    body.IsStatic = value;
                    scene.world.AddBody(body);
                    body.IsActive = true;
                }
            }
        }

        protected override void specialLoad(ref System.Xml.XmlTextReader reader, string type)
        {
            if (reader.Name == "materials" && reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read();
                Materials = GenericMethods.StringListFromString(reader.Value);
            }

            if (reader.Name == "meshes" && reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read();
                Meshes = GenericMethods.StringListFromString(reader.Value);
            }

            if (reader.Name == "pboxes" && reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read();
                PhysBoxes = GenericMethods.StringListFromString(reader.Value);
            }

            if (reader.Name == "isstatic")
                IsStatic = true;
        }
    }
}
