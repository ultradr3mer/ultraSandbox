using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTkProject.Loader;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTkProject.Drawables.Models
{
    class AnimatedModel : PhysModel
    {
        new public static string nodename = "animodel";

        public AnimatedModel(GameObject parent)
            : base(parent)
        {
        }

        protected override void setupMatrices(ref ViewInfo curView,ref  Shader curShader,ref Mesh curMesh)
        {
            base.setupMatrices(ref curView, ref curShader, ref curMesh);
            AnimationData animationData = curMesh.curAnimationData;
            int curframe = (int)(animationData.animationPos / animationData.stepSize);
            Console.WriteLine(curframe);

            int framecount = animationData.Matrices.Length;
            if (curframe > framecount - 1)
                curframe = framecount - 1;

            Matrix4[] matrices = animationData.Matrices[curframe];
            int bonecount = matrices.Length;

            for (int i = 0; i < bonecount; i++)
            {
                GL.UniformMatrix4(curShader.BoneMatixLocations[i], false, ref matrices[i]);

                //Console.WriteLine(curframe);
            }
        }

        public override void update()
        {
            base.update();
            for (int i = 0; i < vaoHandle.Length; i++)
			{
                AnimationData animationData = meshes[i].curAnimationData;
                animationData.animationPos += gameWindow.lastFrameDuration;
                if (animationData.animationPos > animationData.lastFrame)
                    animationData.animationPos -= animationData.lastFrame;

                meshes[i].curAnimationData = animationData;
            }
        }

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

            sb.AppendLine(tab + "<" + nodename + " name='" + name + "'>");
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

            saveChilds(ref sb, level);

            sb.AppendLine(tab + "</" + nodename + ">");
        }
    }
}
