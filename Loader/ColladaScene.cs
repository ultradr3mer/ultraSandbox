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

        NumberFormatInfo nfi = GenericMethods.getNfi();
        static char[] seperator = " ".ToCharArray();

        private List<ColladaObject> colladaObjects = new List<ColladaObject> { };

        class ColladaObject
        {
            public string id;
            public List<ColladaInput> inputs = new List<ColladaInput> { };
            public ColladaObject source;

            protected string nodename;

            public virtual List<Vector3> Vector3Data { get { return source.Vector3Data; } set { } }
            public virtual List<Vector2> Vector2Data { get { return source.Vector2Data; } set { } }
        }

        class ColladaPolys : ColladaObject
        {
            public List<Face> Polys = new List<Face> { }; 

            int[] vCounts;
            int[] rawIndices;

            new string nodename = "polylist";

            public ColladaPolys(ref XmlTextReader reader, ColladaScene scene)
            {
                

                while (reader.Read() && reader.Name != nodename)
                {
                    if (reader.Name == "input")
                        inputs.Add(new ColladaInput(ref reader, scene));

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

                int position = 0;
                foreach (var vCount in vCounts)
                {
                    Polys.Add(new Face(vCount,position));
                    position += vCount;
                }

                int attributeCount = 3;

                foreach (var input in inputs)
                {
                    if (input.semantic == "VERTEX")
                    {
                        scene.positionVboDataList = input.Vector3Data;

                        int offset = input.offset;
                        foreach (var Poly in Polys)
                        {
                            int basepos = Poly.position * attributeCount;
                            foreach (var vert in Poly.Vertice)
                            {
                                vert.Vi = rawIndices[basepos + offset];
                                basepos += attributeCount;
                            }
                        }
                    }
                    if (input.semantic == "NORMAL")
                    {
                        scene.normalVboDataList = input.Vector3Data;

                        int offset = input.offset;
                        foreach (var Poly in Polys)
                        {
                            int basepos = Poly.position * attributeCount;
                            foreach (var vert in Poly.Vertice)
                            {
                                vert.Ni = rawIndices[basepos + offset];
                                basepos += attributeCount;
                            }
                        }
                    }
                    if (input.semantic == "TEXCOORD")
                    {
                        scene.textureVboDataList = input.Vector2Data;

                        int offset = input.offset;
                        foreach (var Poly in Polys)
                        {
                            int basepos = Poly.position * attributeCount;
                            foreach (var vert in Poly.Vertice)
                            {
                                vert.Ti = rawIndices[basepos + offset];
                                basepos += attributeCount;
                            }
                        }
                    }
                }

                scene.FaceList = Polys;
            }
        }

        class ColladaInput : ColladaObject
        {
            public string semantic = "";
            public int offset = 0;

            public ColladaInput(ref XmlTextReader reader, ColladaScene scene)
            {
                while (reader.MoveToNextAttribute())
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
        }

        class ColladaVerts : ColladaObject
        {
            new string nodename = "vertices";

            public ColladaVerts(ref XmlTextReader reader, ColladaScene scene)
            {
                while (reader.Read() && reader.Name != nodename)
                {
                    if (reader.Name == "input")
                        inputs.Add(new ColladaInput(ref reader, scene));
                }
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
            public float[] floatAry;

            new string nodename = "source";

            public ColladaSource(ref XmlTextReader reader)
            {
                while (reader.Read() && reader.Name != nodename)
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
                }
            }

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

        public ColladaScene(string pointer)
        {
            XmlTextReader reader = new XmlTextReader(pointer);

            while (reader.Read())
            {
                if (reader.Name == "source")
                {
                    string objId = "";
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.Name == "id")
                        {
                            objId = reader.Value;
                        }
                    }
                    ColladaObject newObj = new ColladaSource(ref reader);
                    newObj.id = objId;
                    colladaObjects.Add(newObj);
                }

                if (reader.Name == "vertices")
                {
                    string objId = "";
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.Name == "id")
                        {
                            objId = reader.Value;
                        }
                    }
                    ColladaObject newObj = new ColladaVerts(ref reader, this);
                    newObj.id = objId;
                    colladaObjects.Add(newObj);
                }

                if (reader.Name == "polylist")
                {
                    ColladaObject newObj = new ColladaPolys(ref reader, this);
                    colladaObjects.Add(newObj);
                }
            }
        }

        internal void saveTo(ref Mesh target)
        {
            target.positionVboDataList = positionVboDataList;
            target.normalVboDataList = normalVboDataList;
            target.textureVboDataList = textureVboDataList;
            target.FaceList = FaceList;
        }
    }
}
