using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Globalization;
using System.Xml;

namespace OpenTkProject.Loader
{
    public class ColladaScene
    {


        public List<Vector3> positionVboDataList;
        public List<Vector3> normalVboDataList;
        public List<Vector2> textureVboDataList;
        public List<Face> FaceList;
        public List<Vertice> FpIndiceList;

        public float[][] boneWeights;
        public int[][] boneIds;

        float lastFrame = 0;
        int frameCount = 0;

        NumberFormatInfo nfi = GenericMethods.getNfi();
        static char[] seperator = " ".ToCharArray();

        private List<ColladaObject> colladaObjects = new List<ColladaObject> { };
        private List<ColladaNode> nodes = new List<ColladaNode> { };
        private List<ColladaAnimation> animations = new List<ColladaAnimation> { };

        private Collada collada;
        public string armatureName;
        public float stepSize;
        private AnimationDataGenerator colladaAnimationData;
        public AnimationData animationData;

        class ColladaObject
        {
            public string id;
            public List<ColladaInput> inputs = new List<ColladaInput> { };
            public ColladaObject source;
            protected ColladaScene scene;

            protected List<ColladaObject> childs = new List<ColladaObject> { };

            protected string nodename;
            protected ColladaObject parent;
            private string name;
            public string sid;

            public virtual List<Vector3> Vector3Data { get { return source.Vector3Data; } set { } }
            public virtual List<Vector2> Vector2Data { get { return source.Vector2Data; } set { } }

            public virtual string[] NameAry { get { return source.NameAry; } set { } }
            public virtual float[] FloatAry { get { return source.FloatAry; } set { } }

            public ColladaObject (ref XmlTextReader reader,  ColladaObject parent, ColladaScene scene, string nodename)
            {
                this.parent = parent;
                if(parent != null)
                    parent.childs.Add(this);

                this.scene = scene;
                scene.colladaObjects.Add(this);

                this.nodename = nodename;

                while (reader.MoveToNextAttribute())
                {
                    specialHeaderAttributes(ref reader);
                    genericHeaderAttributes(ref reader);
                }
                while (reader.Read() && !(reader.Name == nodename && reader.NodeType == XmlNodeType.EndElement) )
                {
                    specialAttributes(ref reader);
                    genericAttributes(ref reader);
                }
            }

            public virtual void generate()
            {
            }

            protected ColladaInput getInput(string p)
            {
                foreach (var input in inputs)
                {
                    if (input.semantic == p)
                        return input;
                }
                return null;
            }

            protected int CalculateAttributeCount()
            {
                int attributeCount = 0;
                foreach (var input in inputs)
                {
                    if (input.offset > attributeCount)
                        attributeCount = input.offset;
                }
                attributeCount++;
                return attributeCount;
            }

            public ColladaObject(ref XmlTextReader reader, ColladaObject parent, ColladaScene scene)
            {
                this.parent = parent;
                parent.childs.Add(this);

                this.scene = scene;
                scene.colladaObjects.Add(this);

                while (reader.MoveToNextAttribute())
                {
                    specialHeaderAttributes(ref reader);
                    genericHeaderAttributes(ref reader);
                }
            }

            protected virtual void specialHeaderAttributes(ref XmlTextReader reader)
            {
            }

            private void genericHeaderAttributes(ref XmlTextReader reader)
            {
                if (reader.Name == "id")
                {
                    id = reader.Value;
                }
                if (reader.Name == "name")
                {
                    name = reader.Value;
                }
                if (reader.Name == "sid")
                    sid = reader.Value;
            }

            protected virtual void specialAttributes(ref XmlTextReader reader)
            {
            }

            private void genericAttributes(ref XmlTextReader reader)
            {
                if (reader.Name == "source" && reader.NodeType != XmlNodeType.EndElement)
                {
                    ColladaObject newObj = new ColladaSource(ref reader, this, scene);
                }

                if (reader.Name == "vertices" && reader.NodeType != XmlNodeType.EndElement)
                {
                    ColladaObject newObj = new ColladaVerts(ref reader, this, scene);
                }

                if (reader.Name == "animation" && reader.NodeType != XmlNodeType.EndElement)
                {
                    ColladaObject newObj = new ColladaAnimation(ref reader, this, scene);
                }

                if (reader.Name == "controller" && reader.NodeType != XmlNodeType.EndElement)
                {
                    ColladaObject newObj = new ColladaControler(ref reader, this, scene);
                }

                if (reader.Name == "polylist" && reader.NodeType != XmlNodeType.EndElement)
                {
                    ColladaObject newObj = new ColladaPolys(ref reader, this, scene);
                }
                /*
                if (reader.Name == "visual_scene")
                {
                    ColladaObject newObj = new ColladaVScene(ref reader, this, scene);
                    scene.colladaObjects.Add(newObj);
                }
                */
                if (reader.Name == "vertex_weights" && reader.NodeType != XmlNodeType.EndElement)
                {
                    ColladaObject newObj = new ColladaVertWeights(ref reader, this, scene);
                }

                if (reader.Name == "node" && reader.NodeType != XmlNodeType.EndElement)
                {
                    ColladaObject newObj = new ColladaNode(ref reader, this, scene);
                }

                if (reader.Name == "input")
                    inputs.Add(new ColladaInput(ref reader, this, scene));
            }
        }
        /*
        class ColladaVScene : ColladaObject
        {
            public ColladaVScene(ref XmlTextReader reader, ColladaObject parent, ColladaScene scene)
                : base(ref reader, parent, scene, "visual_scene")
            {
            }
        }
        */
        class ColladaNode : ColladaObject
        {
            public TranslationData translationData = new TranslationData();
            public RotationData rotationData = new RotationData();
            public string referenceName;
            private string treeName;
            public bool rootNode;
            public List<Matrix4> boneAnimationMatrices;
            public Matrix4 boneBaseMatrix = Matrix4.Identity;
            public Matrix4 invBaseMatrix;

            public struct TranslationData
            {
                public Vector3 baseTranslation;
                public Matrix4 baseMatrix;
                
                public List<Vector3> animationTranslation;

                public List<Matrix4> animationMatrices;

                public List<Matrix4> generateMatrices(int frameCount)
                {
                    baseMatrix = Matrix4.CreateTranslation(baseTranslation);

                    animationMatrices = new List<Matrix4> { };

                    if (animationTranslation == null)
                        for (int i = 0; i < frameCount; i++)
                        {
                            animationMatrices.Add(baseMatrix);
                        }
                    else
                    {
                        int dataFrameCount = animationTranslation.Count;
                        for (int i = 0; i < frameCount; i++)
                        {
                            Vector3 translation;

                            if (dataFrameCount > i)
                                translation = animationTranslation[i];
                            else
                                translation = animationTranslation[dataFrameCount - 1];

                            animationMatrices.Add(Matrix4.CreateTranslation(translation));
                        }
                    }

                    return animationMatrices;
                }
            }

            public struct RotationData
            {
                public float baseRotationX;
                public float baseRotationY;
                public float baseRotationZ;
                public List<float> animationRotationX;
                public List<float> animationRotationY;
                public List<float> animationRotationZ;

                public List<Matrix4> animationMatrices;

                public Matrix4 baseMatrix;

                public List<Matrix4> generateMatrices(int frameCount)
                {
                    float rotX = MathHelper.DegreesToRadians(baseRotationX);
                    float rotY = MathHelper.DegreesToRadians(baseRotationY);
                    float rotZ = MathHelper.DegreesToRadians(baseRotationZ);

                    baseMatrix = Matrix4.Identity;

                    baseMatrix *= Matrix4.CreateRotationX(rotX);
                    baseMatrix *= Matrix4.CreateRotationY(rotY);
                    baseMatrix *= Matrix4.CreateRotationZ(rotZ);

                    animationMatrices = new List<Matrix4> { };

                    List<Matrix4> animationMatricesX = new List<Matrix4> { };
                    if (animationRotationX == null)
                        animationMatricesX.Add(Matrix4.CreateRotationX(rotX));
                    else
                    {
                        foreach (var frameRotation in animationRotationX)
                        {
                            float rot = MathHelper.DegreesToRadians(frameRotation);
                            animationMatricesX.Add(Matrix4.CreateRotationX(rot));
                        }
                    }

                    List<Matrix4> animationMatricesY = new List<Matrix4> { };
                    if (animationRotationY == null)
                        animationMatricesY.Add(Matrix4.CreateRotationY(rotY));
                    else
                    {
                        foreach (var frameRotation in animationRotationY)
                        {
                            float rot = MathHelper.DegreesToRadians(frameRotation);
                            animationMatricesY.Add(Matrix4.CreateRotationY(rot));
                        }
                    }

                    List<Matrix4> animationMatricesZ = new List<Matrix4> { };
                    if (animationRotationZ == null)
                        animationMatricesZ.Add(Matrix4.CreateRotationZ(rotZ));
                    else
                    {
                        foreach (var frameRotation in animationRotationZ)
                        {
                            float rot = MathHelper.DegreesToRadians(frameRotation);
                            animationMatricesZ.Add(Matrix4.CreateRotationZ(rot));
                        }
                    }

                    int frameCountX = animationMatricesX.Count;
                    int frameCountY = animationMatricesY.Count;
                    int frameCountZ = animationMatricesZ.Count;

                    for (int i = 0; i < frameCount; i++)
                    {
                        Matrix4 tmpMat = Matrix4.Identity;

                        Matrix4 rotationX;
                        if (frameCountX > i)
                            rotationX = animationMatricesX[i];
                        else
                            rotationX = animationMatricesX[frameCountX - 1];

                        Matrix4 rotationY;
                        if (frameCountY > i)
                            rotationY = animationMatricesY[i];
                        else
                            rotationY = animationMatricesY[frameCountY - 1];

                        Matrix4 rotationZ;
                        if (frameCountZ > i)
                            rotationZ = animationMatricesZ[i];
                        else
                            rotationZ = animationMatricesZ[frameCountZ - 1];

                        tmpMat *= rotationX;
                        tmpMat *= rotationY;
                        tmpMat *= rotationZ;

                        animationMatrices.Add(tmpMat);
                    }

                    return animationMatrices;
                }
            }

            public ColladaNode(ref XmlTextReader reader, ColladaObject parent, ColladaScene scene)
                : base(ref reader, parent, scene, "node")
            {
                scene.nodes.Add(this);

                TreeName = treeName;
            }

            protected override void specialHeaderAttributes(ref XmlTextReader reader)
            {
                if (reader.Name == "id")
                {
                    treeName = reader.Value;
                    rootNode = true;
                }

                if (reader.Name == "sid")
                    sid = reader.Value;
            }

            protected override void specialAttributes(ref XmlTextReader reader)
            {
                if (reader.Name == "node" && reader.NodeType != XmlNodeType.EndElement)
                {
                    ColladaObject newObj = new ColladaJoint(ref reader, this, scene);
                }

                if (reader.Name == "translate" && reader.NodeType != XmlNodeType.EndElement)
                    setTranslation(ref reader);

                if (reader.Name == "rotate" && reader.NodeType != XmlNodeType.EndElement)
                    setRotation(ref reader);
            }

            private void setRotation(ref XmlTextReader reader)
            {
                reader.Read();
                string[] tmpAry = reader.Value.Split(' ');
                Vector3 pointer = new Vector3();
                pointer.X = GenericMethods.FloatFromString(tmpAry[0]);
                pointer.Y = GenericMethods.FloatFromString(tmpAry[1]);
                pointer.Z = GenericMethods.FloatFromString(tmpAry[2]);

                Vector3 rotation = pointer * GenericMethods.FloatFromString(tmpAry[3]);
                rotationData.baseRotationX += rotation.X;
                rotationData.baseRotationY += rotation.Y;
                rotationData.baseRotationZ += rotation.Z;
            }

            private void setTranslation(ref XmlTextReader reader)
            {
                reader.Read();
                string[] tmpAry = reader.Value.Split(' ');
                translationData.baseTranslation.X = GenericMethods.FloatFromString(tmpAry[0]);
                translationData.baseTranslation.Y = GenericMethods.FloatFromString(tmpAry[1]);
                translationData.baseTranslation.Z = GenericMethods.FloatFromString(tmpAry[2]);
            }

            public override void generate()
            {
                List<Matrix4> translationMatrices = translationData.generateMatrices(scene.frameCount);
                List<Matrix4> rotationMatrices = rotationData.generateMatrices(scene.frameCount);

                Matrix4 basePOffset = ParentBaseMatix;
                List<Matrix4> parentOffset = ParentAnimationMatrices;
                /*
               boneBaseMatrix = basePOffset; 
               boneBaseMatrix *= rotationData.baseMatrix;
               boneBaseMatrix *= translationData.baseMatrix;
               */

                boneBaseMatrix = rotationData.baseMatrix; 
                boneBaseMatrix *= translationData.baseMatrix;
                boneBaseMatrix *= basePOffset;

                invBaseMatrix = Matrix4.Invert(boneBaseMatrix);

                boneAnimationMatrices = new List<Matrix4>{};
                int frameCount = scene.frameCount;
                for (int i = 0; i < frameCount; i++)
                {
                    Matrix4 frameMatrix = Matrix4.Identity;
                    frameMatrix *= rotationMatrices[i];
                    frameMatrix *= translationMatrices[i];
                    frameMatrix *= parentOffset[i];

                    boneAnimationMatrices.Add(frameMatrix);
                }

                generateChilds();
            }

            public void generateChilds()
            {
                foreach (var node in childs)
                {
                    node.generate();
                }
            }

            public virtual List<Matrix4> ParentAnimationMatrices
            {
                get
                {
                    List<Matrix4> tmpList = new List<Matrix4> { };

                    int framecount = scene.frameCount;
                    for (int i = 0; i < framecount; i++)
                    {
                        tmpList.Add(Matrix4.Identity);
                    }

                    return tmpList;
                }
                set { ;}
            }
            public virtual Matrix4 ParentBaseMatix
            {
                get
                {
                    return Matrix4.Identity;
                }
                set
                {
                }
            }

            public string TreeName { 
                get { return null; }
                set
                {
                    this.treeName = value;
                    referenceName = treeName + "_" + sid;

                    foreach (var child in childs)
                    {
                        ColladaJoint jointChild = (ColladaJoint)child;
                        jointChild.TreeName = value;
                    }
                }
            }
        }

        class ColladaJoint : ColladaNode
        {
            public ColladaJoint(ref XmlTextReader reader, ColladaNode parent, ColladaScene scene)
                : base(ref reader, parent, scene)
            {
                //scene.nodes.Add(this);
            }

            protected override void specialHeaderAttributes(ref XmlTextReader reader)
            {
            }

            public override List<Matrix4> ParentAnimationMatrices
            {
                get
                {
                    ColladaNode parNode = (ColladaNode)parent;
                    return parNode.boneAnimationMatrices;
                }
                set
                {
                    base.ParentAnimationMatrices = value;
                }
            }

            public override Matrix4 ParentBaseMatix
            {
                get
                {
                    ColladaNode nodeParent = (ColladaNode)parent;
                    return nodeParent.boneBaseMatrix;
                }
                set
                {
                }
            }

        }

        class Collada : ColladaObject
        {
            public Collada(ref XmlTextReader reader, ColladaScene scene)
                : base(ref reader, null, scene, "COLLADA")
            {
            }
        }

        class ColladaAnimation : ColladaObject
        {
            private ColladaChannel channel;

            class ColladaChannel : ColladaObject
            {
                public ColladaNode targetObject;
                public Target targetValue;
                private string targetString;

                public enum Target
                {
                    location,
                    rotationX,
                    rotationY,
                    rotationZ
                }

                public ColladaChannel(ref XmlTextReader reader, ColladaObject parent, ColladaScene scene)
                    : base(ref reader, parent, scene)
                {
                }

                private ColladaNode getNodeByRefName(string name)
                {
                    foreach (var node in scene.nodes)
                    {
                        if (node.referenceName == name)
                            return node;
                    }
                    return null;
                }

                protected override void specialHeaderAttributes(ref XmlTextReader reader)
                {
                    if (reader.Name == "target")
                        targetString = reader.Value;
                }

                public void resolveTarget()
                {
                    string[] tmpAry = targetString.Split('/');
                    targetObject = getNodeByRefName(tmpAry[0]);

                    if (tmpAry[1] == "location")
                        targetValue = Target.location;

                    if (tmpAry[1] == "rotationX.ANGLE")
                        targetValue = Target.rotationX;

                    if (tmpAry[1] == "rotationY.ANGLE")
                        targetValue = Target.rotationY;

                    if (tmpAry[1] == "rotationZ.ANGLE")
                        targetValue = Target.rotationZ;
                }
            }

            public ColladaAnimation(ref XmlTextReader reader, ColladaObject parent, ColladaScene scene)
                : base(ref reader, parent, scene, "animation")
            {
                scene.animations.Add(this);
            }

            protected override void specialAttributes(ref XmlTextReader reader)
            {
                if (reader.Name == "channel")
                {
                    channel = new ColladaChannel(ref reader, this, scene);
                }
            }

            public override void generate()
            {
                //tell the channel to fill in objects
                channel.resolveTarget();

                //get inputs
                ColladaInput input = getInput("INPUT");
                ColladaInput output = getInput("OUTPUT");
                ColladaInput interpolation = getInput("INTERPOLATION");

                //get basic animation info
                float resolution = scene.stepSize;
                float[] timeSteps = input.FloatAry;
                int frameCount = timeSteps.Length;
                float endframe = timeSteps[timeSteps.Length - 1];
                if (scene.lastFrame < endframe) 
                    scene.lastFrame = endframe;

                //prepare variables
                int frame = 0;
                float frameTimeA;
                float frameTimeB;

                if (channel.targetValue == ColladaChannel.Target.location)
                {
                    //get raw data
                    List<Vector3> locationVecs = output.Vector3Data;
                    List<Vector3> locationOut = new List<Vector3> { };

                    Vector3 frameValueA;
                    Vector3 frameValueB;

                    for (float time = 0; time < endframe + resolution; time += resolution)
                    {
                        //find out what frame we are in
                        for (int i = frame; i < frameCount; i++)
                        {
                            frame = i;
                            if (timeSteps[i] >= time)
                                break;
                        }

                        if (time >= endframe)
                        {
                            locationOut.Add(locationVecs[frameCount - 1]);
                        }
                        else if (frame == 0)
                        {
                            locationOut.Add(locationVecs[0]);
                        }
                        else
                        {
                            frameTimeA = timeSteps[frame - 1];
                            frameTimeB = timeSteps[frame];

                            frameValueA = locationVecs[frame - 1];
                            frameValueB = locationVecs[frame];

                            float timeSpan = frameTimeB - frameTimeA;
                            float localTime = time - frameTimeA;

                            float weight = localTime / timeSpan;

                            locationOut.Add(frameValueA * (1 - weight) + frameValueB * weight);
                        }
                    }

                    channel.targetObject.translationData.animationTranslation = locationOut;

                    if (scene.frameCount < locationOut.Count)
                        scene.frameCount = locationOut.Count;
                }
                else
                {
                    //get raw data
                    float[] roatationFloats = output.FloatAry;
                    List<float> roatationOut = new List<float>{};

                    float frameValueA;
                    float frameValueB;

                    for (float time = 0; time < endframe + resolution; time += resolution)
                    {
                        //find out what frame we are in
                        for (int i = frame; i < frameCount; i++)
                        {
                            frame = i;
                            if (timeSteps[i] >= time)
                                break;
                        }

                        if (time >= endframe)
                        {
                            roatationOut.Add(roatationFloats[frameCount - 1]);
                        }
                        else if (frame == 0)
                        {
                            roatationOut.Add(roatationFloats[0]);
                        }
                        else
                        {
                            frameTimeA = timeSteps[frame - 1];
                            frameTimeB = timeSteps[frame];

                            frameValueA = roatationFloats[frame - 1];
                            frameValueB = roatationFloats[frame];

                            float timeSpan = frameTimeB - frameTimeA;
                            float localTime = time - frameTimeA;

                            float weight = localTime / timeSpan;

                            roatationOut.Add(frameValueA * (1 - weight) + frameValueB * weight);
                        }
                    }

                    switch (channel.targetValue)
                    {
                        case ColladaChannel.Target.rotationX:
                            channel.targetObject.rotationData.animationRotationX = roatationOut;
                            break;
                        case ColladaChannel.Target.rotationY:
                            channel.targetObject.rotationData.animationRotationY = roatationOut;
                            break;
                        case ColladaChannel.Target.rotationZ:
                            channel.targetObject.rotationData.animationRotationZ = roatationOut;
                            break;
                        default:
                            break;
                    }

                    if (scene.frameCount < roatationOut.Count)
                        scene.frameCount = roatationOut.Count;
                }
            }
        }

        class ColladaControler : ColladaObject
        {
            public ColladaControler(ref XmlTextReader reader, ColladaObject parent, ColladaScene scene)
                : base(ref reader, parent, scene, "controller")
            {
            }

            protected override void specialHeaderAttributes(ref XmlTextReader reader)
            {
                if (reader.Name == "name")
                {
                    scene.armatureName = reader.Value;
                }
            }
        }

        class ColladaPolys : ColladaObject
        {
            public List<Face> Polys = new List<Face> { }; 

            int[] vCounts;
            int[] rawIndices;

            public ColladaPolys(ref XmlTextReader reader, ColladaObject parent, ColladaScene scene)
                :base(ref reader,parent, scene, "polylist")
            {
                int position = 0;
                foreach (var vCount in vCounts)
                {
                    Polys.Add(new Face(vCount,position));
                    position += vCount;
                }

                int attributeCount = CalculateAttributeCount();

                ColladaInput vertIn = getInput("VERTEX");
                ColladaInput normalIn = getInput("NORMAL");
                ColladaInput texIn = getInput("TEXCOORD");

                scene.positionVboDataList = vertIn.Vector3Data;
                scene.normalVboDataList = normalIn.Vector3Data;
                scene.textureVboDataList = texIn.Vector2Data;

                int offset = vertIn.offset;
                int normaloffset = normalIn.offset;
                int texoffset = texIn.offset;

                foreach (var Poly in Polys)
                {
                    int basepos = Poly.position * attributeCount;
                    foreach (var vert in Poly.Vertice)
                    {
                        vert.Vi = rawIndices[basepos + offset];
                        vert.Ni = rawIndices[basepos + normaloffset];
                        vert.Ti = rawIndices[basepos + texoffset];
                        basepos += attributeCount;
                    }
                }

                scene.FaceList = Polys;
            }

            protected override void specialAttributes(ref XmlTextReader reader)
            {
                if (reader.Name == "vcount" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.Read();
                    string[] tmpAry = reader.Value.Split(seperator);

                    int tmpLenth = tmpAry.Length - 1;
                    vCounts = new int[tmpLenth];

                    for (int i = 0; i < tmpLenth; i++)
                    {
                        vCounts[i] = GenericMethods.IntFromString(tmpAry[i]);
                    }
                }

                if (reader.Name == "p" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.Read();
                    string[] tmpAry = reader.Value.Split(seperator);

                    int tmpLenth = tmpAry.Length;
                    rawIndices = new int[tmpLenth];

                    for (int i = 0; i < tmpLenth; i++)
                    {
                        rawIndices[i] = GenericMethods.IntFromString(tmpAry[i]);
                    }
                }
            }
        }

        class ColladaInput : ColladaObject
        {
            public string semantic = "";
            public int offset = 0;

            public ColladaInput(ref XmlTextReader reader,  ColladaObject parent, ColladaScene scene)
                : base(ref reader, parent, scene)
            {
            }

            protected override void specialHeaderAttributes(ref XmlTextReader reader)
            {

                if (reader.Name == "semantic")
                    semantic = reader.Value;

                if (reader.Name == "offset")
                    offset = GenericMethods.IntFromString(reader.Value);

                if (reader.Name == "source")
                {
                    string target = reader.Value.Remove(0, 1);
                    foreach (var colladaObject in scene.colladaObjects)
                    {
                        if (colladaObject.id == target)
                            source = colladaObject;
                    }
                }
            }
        }

        class ColladaVerts : ColladaObject
        {
            public ColladaVerts(ref XmlTextReader reader, ColladaObject parent, ColladaScene scene)
                : base(ref reader, parent, scene, "vertices")
            {
            }

            public override List<Vector3> Vector3Data
            {
                get
                {
                    return inputs[0].Vector3Data;
                }
                set { }
            }
        }

        class ColladaSource : ColladaObject
        {
            protected float[] floatAry;
            protected string[] nameAry;

            public ColladaSource(ref XmlTextReader reader, ColladaObject parent, ColladaScene scene)
                : base(ref reader, parent, scene, "source")
            {
            }

            protected override void specialAttributes(ref XmlTextReader reader)
            {
                if (reader.Name == "float_array" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.Read();
                    string[] tmpAry = reader.Value.Split(seperator);

                    int tmpLenth = tmpAry.Length;
                    floatAry = new float[tmpLenth];

                    for (int i = 0; i < tmpLenth; i++)
                    {
                        floatAry[i] = GenericMethods.FloatFromString(tmpAry[i]);
                    }
                }

                if (reader.Name == "Name_array" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.Read();
                    nameAry = reader.Value.Split(seperator);
                }
            }

            public override string[] NameAry { get { return nameAry; } set { } }
            public override float[] FloatAry { get { return floatAry; } set { } }

            public override List<Vector3> Vector3Data
            {
                get
                {
                    int vecCount = floatAry.Length / 3;
                    List<Vector3> tmpVec = new List<Vector3> { };
                    for (int i = 0; i < vecCount; i++)
                    {
                        int position = i * 3;
                        tmpVec.Add(
                            new Vector3(
                            floatAry[position],
                            floatAry[position + 1],
                            floatAry[position + 2]));
                    }
                    return tmpVec;
                }
                set { }
            }

            public override List<Vector2> Vector2Data
            {
                get
                {
                    int vecCount = floatAry.Length / 2;
                    List<Vector2> tmpVec = new List<Vector2> { };
                    for (int i = 0; i < vecCount; i++)
                    {
                        int position = i * 2;
                        tmpVec.Add(
                            new Vector2(
                            floatAry[position],
                            floatAry[position + 1]));
                    }
                    return tmpVec;
                }
                set { }
            }
        }

        class ColladaVertWeights : ColladaObject
        {
            int[] vCounts;
            int[] weightIndices;

            const int maxAffBones = 3;

            public ColladaVertWeights(ref XmlTextReader reader,  ColladaObject parent, ColladaScene scene)
                : base(ref reader, parent, scene, "vertex_weights")
            {
                int groupCount = 0;
                ColladaInput jointIn = getInput("JOINT");
                ColladaInput weightIn = getInput("WEIGHT");

                float[] weights = weightIn.FloatAry;

                int groupOffset = jointIn.offset;
                int weightOffset = weightIn.offset;

                int vertexCount = vCounts.Length;
                int attributeCount = CalculateAttributeCount();
                int readerPos = 0;

                string[] boneNames = jointIn.NameAry;
                scene.colladaAnimationData.boneNames = boneNames;

                groupCount = boneNames.Length;
                if (groupCount > maxAffBones)
                    groupCount = maxAffBones;

                float[][] tmpBoneWeights = new float[groupCount][];
                int[][] tmpBoneIds = new int[groupCount][];

                for (int i = 0; i < groupCount; i++)
                {
                    tmpBoneWeights[i] = new float[vertexCount];
                    tmpBoneIds[i] = new int[vertexCount];
                }

                for (int i = 0; i < vertexCount; i++)
                {
                    int curGroups = vCounts[i];
                    for (int j = 0; j < groupCount; j++)
                    {
                        if (j < curGroups)
                        {
                            int group = weightIndices[readerPos + groupOffset];
                            int weightIndex = weightIndices[readerPos + weightOffset];

                            tmpBoneWeights[j][i] = weights[weightIndex];
                            tmpBoneIds[j][i] = group + 1;

                            readerPos += attributeCount;
                        }
                        else
                        {
                            tmpBoneWeights[j][i] = 0;
                            tmpBoneIds[j][i] = 0;
                        }
                    }
                }

                scene.boneWeights = tmpBoneWeights;
                scene.boneIds = tmpBoneIds;
            }

            protected override void specialAttributes(ref XmlTextReader reader)
            {
                if (reader.Name == "vcount" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.Read();
                    string[] tmpAry = reader.Value.Split(seperator);

                    int tmpLenth = tmpAry.Length - 1;
                    vCounts = new int[tmpLenth];

                    for (int i = 0; i < tmpLenth; i++)
                    {
                        vCounts[i] = GenericMethods.IntFromString(tmpAry[i]);
                    }
                }

                if (reader.Name == "v" && reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.Read();
                    string[] tmpAry = reader.Value.Split(seperator);

                    int tmpLenth = tmpAry.Length;
                    weightIndices = new int[tmpLenth];

                    for (int i = 0; i < tmpLenth; i++)
                    {
                        weightIndices[i] = GenericMethods.IntFromString(tmpAry[i]);
                    }
                }
            }
        }

        public ColladaScene(string pointer)
        {
            colladaAnimationData = new AnimationDataGenerator();

            XmlTextReader reader = new XmlTextReader(pointer);

            stepSize = 1.0f / 25;

            while (reader.Read())
            {
                if (reader.Name == "COLLADA")
                    collada = new Collada(ref reader, this);
            }

            foreach (var animation in animations)
            {
                animation.generate();
            }

            if (nodes.Count > 0 && colladaAnimationData.boneNames != null)
            {
                foreach (var node in nodes)
                {
                    if (node.rootNode)
                        node.generate();
                }

                animationData.Matrices = new Matrix4[frameCount][];
                animationData.stepSize = stepSize;
                animationData.lastFrame = lastFrame;
                colladaAnimationData.generate(nodes,ref animationData);
            }

            if (pointer == "models\\untitled_2.dae")
            {
            }
        }

        internal void saveTo(ref Mesh target)
        {
            target.positionVboDataList = positionVboDataList;
            target.normalVboDataList = normalVboDataList;
            target.textureVboDataList = textureVboDataList;
            target.boneWeightList = boneWeights;
            target.FaceList = FaceList;
            target.boneIdList = boneIds;
            target.animationData = animationData;
        }

        class AnimationDataGenerator
        {
            public string[] boneNames;
            public ColladaNode[] referingNodes;

            internal void generate(List<ColladaNode> nodes,ref AnimationData animationData)
            {
                int boneCount = boneNames.Length;
                int frameCount = animationData.Matrices.Length;

                referingNodes = new ColladaNode[boneCount];
                for (int i = 0; i < boneCount; i++)
                {
                    string curName = boneNames[i];
                    foreach (var node in nodes)
                    {
                        if (node.sid == curName)
                        {
                            referingNodes[i] = node;
                            break;
                        }
                    }
                }

                Matrix4[][] allMatrices = new Matrix4[frameCount][];
                for (int curframe = 0; curframe < frameCount; curframe++)
                {
                    Matrix4[] curMatrices = new Matrix4[boneCount];
                    for (int j = 0; j < boneCount; j++)
                    {
                        curMatrices[j] = referingNodes[j].invBaseMatrix * referingNodes[j].boneAnimationMatrices[curframe];
                    }
                    allMatrices[curframe] = curMatrices;
                }

                animationData.Matrices = allMatrices;
            }
        }
    }

    public struct AnimationData
    {
        //public int BoneCount;
        public float stepSize;
        public float lastFrame;
        public float animationPos;

        //the animation Matrices[frame][bone]
        public Matrix4[][] Matrices;
    }
}
