using System;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;
using System.Xml;
using OpenTkProject.Loader;
using System.Text;
using OpenTkProject.Game;
using System.Xml.Serialization;

namespace OpenTkProject
{
    [Serializable]
    public struct Mesh
    {
        //Lists for caching
        public List<Vector3> positionVboDataList;
        public List<Vector3> normalVboDataList;
        public List<Vector2> textureVboDataList;
        public List<Face> FaceList;
        public List<Vertice> FpIndiceList;

        public int affectingBonesCount;
        public float[][] boneWeightList;
        public int[][] boneIdList;

        //Meshlist id
        public int identifier;

        //type
        public enum Type { obj, generated, collada, empty, colladaManaged, fromCache }
        public Type type;

        //basic info
        public bool loaded;
        public bool containsVbo;

        //Vertex Buffer Object Handles
        public int normalVboHandle;
        public int positionVboHandle;
        public int textureVboHandle;
        public int tangentVboHandle;
        public int eboHandle;

        public int[] boneWeightVboHandles;
        public int[] boneIdVboHandles;

        public AnimationData curAnimationData;
        public bool animated;

        //stuff to be saved
        public string name;

        public Vector3[] positionVboData;
        public Vector3[] normalVboData;
        public Vector3[] tangentVboData;
        public Vector2[] textureVboData;
        public int[] indicesVboData;

        public int[][] boneIdVboData;
        public float[][] boneWeightVboData;

        public string pointer;
        public float boundingSphere;

        public List<AnimationData> animationData;

        public override string ToString()
        {
            return name;
        }

        internal void cacheMesh(ref List<Mesh> mList)
        {
            Mesh tmpMesh = new Mesh();

            if (type == Type.generated || !loaded)
                return;

            tmpMesh.name = name;

            if (type == Type.empty)
            {
                mList.Add(tmpMesh);
            }

            tmpMesh.positionVboData = positionVboData;
            tmpMesh.normalVboData = normalVboData;
            tmpMesh.tangentVboData = tangentVboData;
            tmpMesh.textureVboData = textureVboData;
            tmpMesh.indicesVboData = indicesVboData;

            tmpMesh.boneIdVboData = boneIdVboData;
            tmpMesh.boneWeightVboData = boneWeightVboData;

            tmpMesh.boundingSphere = boundingSphere;

            tmpMesh.animationData = animationData;

            mList.Add(tmpMesh);
        }
    }

    [Serializable]
    public struct AnimationData
    {
        //public int BoneCount;
        public float stepSize;
        public float lastFrame;
        public float animationPos;
        public string name;

        //the animation Matrices[frame][bone]
        public Matrix4[][] Matrices;
        public string pointer;

        public override string ToString()
        {
            return name;
        }
    }

    public class MeshLoader : GameObject
    {
        public List<Mesh> meshes = new List<Mesh> { };

        public Hashtable meshesNames = new Hashtable();

        NumberFormatInfo nfi = GenericMethods.getNfi();

        public void readCacheFile()
        {
            string filename = Settings.Instance.game.modelCacheFile;
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            List<Mesh> tmpMeshes;

            using (fileStream)
            {
                // Read the source file into a byte array.
                byte[] bytes = new byte[fileStream.Length];
                int numBytesToRead = (int)fileStream.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = fileStream.Read(bytes, numBytesRead, numBytesToRead);

                    // Break when the end of the file is reached.
                    if (n == 0)
                        break;

                    numBytesRead += n;
                    numBytesToRead -= n;
                }

                tmpMeshes = (List<Mesh>)GenericMethods.ByteArrayToObject(bytes);
                fileStream.Close();
            }

            int meshCount = tmpMeshes.Count;
            for (int i = 0; i < meshCount; i++)
            {
                Mesh curMesh = tmpMeshes[i];
                string name = curMesh.name;

                if (!meshesNames.ContainsKey(name))
                {
                    if (curMesh.indicesVboData != null)
                    {
                        curMesh.type = Mesh.Type.fromCache;
                    }
                    else
                    {
                        curMesh.type = Mesh.Type.empty;
                        curMesh.loaded = true;
                    }

                    int identifier = meshes.Count;
                    curMesh.identifier = identifier;

                    if (curMesh.animationData != null)
                        curMesh.curAnimationData = curMesh.animationData[0];

                    meshesNames.Add(name, identifier);
                    meshes.Add(curMesh);
                }
            }

            gameWindow.log("loaded "+meshCount+" meshes from cache");
        }

        public void writeCacheFile()
        {
            List<Mesh> SaveList = new List<Mesh> { };
            foreach (var mesh in meshes)
            {
                mesh.cacheMesh(ref SaveList);
            }

            string filename = Settings.Instance.game.modelCacheFile;
            FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);

            using (fileStream)
            {
                byte[] saveAry = GenericMethods.ObjectToByteArray(SaveList);
                fileStream.Write(saveAry, 0, saveAry.Length);
                fileStream.Close();
            }
        }

        public Mesh fromObj(string pointer)
        {
            string name = pointer.Replace(gameWindow.modelFolder, "");

            if (!meshesNames.ContainsKey(name))
            {
 
                Mesh curMesh = new Mesh();

                curMesh.type = Mesh.Type.obj;
                curMesh.pointer = pointer;
                curMesh.identifier = meshes.Count;
                curMesh.name = name;

                meshesNames.Add(curMesh.name, curMesh.identifier);

                meshes.Add(curMesh);

                return curMesh;
            }
            else
            {
                return getMesh(name);
            }

        }

        public Mesh fromCollada(string pointer)
        {
            string name = pointer.Replace(gameWindow.modelFolder, "");

            if (!meshesNames.ContainsKey(name))
            {

                Mesh curMesh = new Mesh();

                curMesh.type = Mesh.Type.collada;
                curMesh.pointer = pointer;
                curMesh.identifier = meshes.Count;
                curMesh.name = name;

                meshesNames.Add(curMesh.name, curMesh.identifier);

                meshes.Add(curMesh);

                return curMesh;
            }
            else
            {
                return getMesh(name);
            }
        }

        internal Mesh fromXml(string pointer)
        {
            string name = pointer.Replace(gameWindow.modelFolder, "");

            if (!meshesNames.ContainsKey(name))
            {

                Mesh curMesh = new Mesh();

                curMesh.type = Mesh.Type.colladaManaged;
                //curMesh.pointer = pointer;
                curMesh.identifier = meshes.Count;
                curMesh.name = name;
                curMesh.animationData = new List<AnimationData> { };

                XmlTextReader reader = new XmlTextReader(pointer);
                while (reader.Read())
                {
                    if (reader.Name == "mesh")
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "source")
                                curMesh.pointer = reader.Value;
                        }

                    if (reader.Name == "animation")
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "source")
                            {
                                AnimationData tmpAni = new AnimationData();
                                tmpAni.pointer = gameWindow.modelFolder +  reader.Value;
                                tmpAni.name = reader.Value;
                                curMesh.animationData.Add(tmpAni);
                            }
                        }
                }

                meshesNames.Add(curMesh.name, curMesh.identifier);

                meshes.Add(curMesh);

                return curMesh;
            }
            else
            {
                return getMesh(name);
            }
        }

        public Mesh getMesh(string name)
        {
            int id = (int)meshesNames[name];
            return meshes[id];
        }

        public void loadMeshes()
        {
            Mesh curMesh;
            for (int i = 0; i < meshes.Count; i++)
            {
                curMesh = meshes[i];
                if (!curMesh.loaded)
                {
                    loadMesh(curMesh);
                }
            }
        }

        public float loadSingleMeshes()
        {
            Mesh curMesh;
            for (int i = 0; i < meshes.Count; i++)
            {
                curMesh = meshes[i];
                if (!curMesh.loaded)
                {
                    loadMesh(curMesh);
                    return (float)i / (float)meshes.Count;
                }
            }
            return 1;
        }

        private void loadMesh(Mesh curMesh)
        {
            switch (curMesh.type)
            {
                case Mesh.Type.obj:
                    loadObj(curMesh);
                    break;
                case Mesh.Type.collada:
                    loadDae(curMesh);
                    break;
                case Mesh.Type.fromCache:
                    loadCached(curMesh);
                    break;
                case Mesh.Type.colladaManaged:
                    loadManagedCollada(curMesh);
                    break;
                default:
                    break;
            }
        }

        private void loadCached(Mesh target)
        {
            target.loaded = true;

            generateVBO(ref target);

            meshes[target.identifier] = target;
        }

        private void loadManagedCollada(Mesh target)
        {
            gameWindow.log("load Managed Collada: " + target.pointer);

            ColladaScene colladaScene = new ColladaScene(gameWindow.modelFolder + target.pointer);
            colladaScene.appendAnimations(target.animationData);
            colladaScene.saveTo(ref target);

            target.loaded = true;

            if (target.type != Mesh.Type.empty)
            {
                parseFaceList(ref target, false);

                generateVBO(ref target);
            }

            if (target.identifier != -1)
            {
                meshes[target.identifier] = target;
            }
        }

        private void loadDae(Mesh target)
        {
            gameWindow.log("load Collada: " + target.pointer);

            ColladaScene colladaScene = new ColladaScene(target.pointer);
            colladaScene.saveTo(ref target);

            target.loaded = true;

            if (target.type != Mesh.Type.empty)
            {
                parseFaceList(ref target, false);

                generateVBO(ref target);
            }

            if (target.identifier != -1)
            {
                meshes[target.identifier] = target;
            }
        }

        public void generateVBO(ref Mesh target)
        {
            int affectingBonesCount = 0;

            if (target.boneWeightVboData != null)
            {
                affectingBonesCount = target.boneWeightVboData.Length;

                target.affectingBonesCount = affectingBonesCount;
                target.boneWeightVboHandles = new int[affectingBonesCount];
                target.boneIdVboHandles = new int[affectingBonesCount];
            }

            if (!target.containsVbo)
            {
                GL.GenBuffers(1, out target.normalVboHandle);
                GL.GenBuffers(1, out target.positionVboHandle);
                GL.GenBuffers(1, out target.textureVboHandle);
                GL.GenBuffers(1, out target.tangentVboHandle);
                GL.GenBuffers(1, out target.eboHandle);

                if (affectingBonesCount > 0)
                {
                    GL.GenBuffers(affectingBonesCount, target.boneWeightVboHandles);
                    GL.GenBuffers(affectingBonesCount, target.boneIdVboHandles);
                }
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, target.normalVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(target.normalVboData.Length * Vector3.SizeInBytes),
                target.normalVboData, BufferUsageHint.StaticDraw);

            gameWindow.checkGlError("Create NormlBuffer");


            GL.BindBuffer(BufferTarget.ArrayBuffer, target.positionVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(target.positionVboData.Length * Vector3.SizeInBytes),
                target.positionVboData, BufferUsageHint.StaticDraw);

            gameWindow.checkGlError("Create positionBuffer");

            GL.BindBuffer(BufferTarget.ArrayBuffer, target.textureVboHandle);
            GL.BufferData<Vector2>(BufferTarget.ArrayBuffer,
                new IntPtr(target.textureVboData.Length * Vector2.SizeInBytes),
                target.textureVboData, BufferUsageHint.StaticDraw);

            gameWindow.checkGlError("Create uvBuffer");

            GL.BindBuffer(BufferTarget.ArrayBuffer, target.tangentVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(target.tangentVboData.Length * Vector3.SizeInBytes),
                target.tangentVboData, BufferUsageHint.StaticDraw);

            gameWindow.checkGlError("Create tangentBuffer");

            for (int i = 0; i < affectingBonesCount; i++)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, target.boneWeightVboHandles[i]);
                GL.BufferData<float>(BufferTarget.ArrayBuffer,
                    new IntPtr(target.boneWeightVboData[i].Length * sizeof(float)),
                    target.boneWeightVboData[i], BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ArrayBuffer, target.boneIdVboHandles[i]);
                GL.BufferData<int>(BufferTarget.ArrayBuffer,
                    new IntPtr(target.boneIdVboData[i].Length * sizeof(int)),
                    target.boneIdVboData[i], BufferUsageHint.StaticDraw);

                gameWindow.checkGlError("Create vertexGroup Buffer " + i);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, target.eboHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                new IntPtr(sizeof(uint) * target.indicesVboData.Length),
                target.indicesVboData, BufferUsageHint.StaticDraw);

            gameWindow.checkGlError("Create indice Buffer");

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // --- causes crash ---
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            target.containsVbo = true;
        }

        public void loadObj(Mesh target)
        {
            gameWindow.log("load Wavefront: " + target.pointer);
            List<Vector3> positionVboDataList = new List<Vector3> { };
            List<Vector3> normalVboDataList = new List<Vector3> { };
            List<Vector2> textureVboDataList = new List<Vector2> { };
            List<Face> FaceList = new List<Face> { };
            List<Vertice> FpIndiceList = new List<Vertice> { };

            // Read the file and display it line by line.
            string line;
            System.IO.StreamReader file =
               new System.IO.StreamReader(target.pointer);

            while ((line = file.ReadLine()) != null)
            {
                string[] sline = line.Split(new string[]{" "},10,StringSplitOptions.None);

                if (sline[0] == "v")
                {
                    float X = float.Parse(sline[1], nfi);
                    float Y = float.Parse(sline[2], nfi);
                    float Z = float.Parse(sline[3], nfi);
                    positionVboDataList.Add(new Vector3(X, Y, Z));
                }

                if (sline[0] == "vn")
                {
                    float X = float.Parse(sline[1], nfi);
                    float Y = float.Parse(sline[2], nfi);
                    float Z = float.Parse(sline[3], nfi);
                    normalVboDataList.Add(new Vector3(X, Y, Z));

                }

                if (sline[0] == "vt")
                {
                    float X = float.Parse(sline[1], nfi);
                    float Y = 1-float.Parse(sline[2], nfi);
                    textureVboDataList.Add(new Vector2(X, Y));

                }

                if (sline[0] == "f")
                {
                    string[] segment = sline[1].Split(new string[] { "/" }, 10, StringSplitOptions.None);
                    if (segment.Length == 3)
                    {
                        Vertice fp1 = new Vertice(int.Parse(segment[0]) - 1, int.Parse(segment[1]) - 1, int.Parse(segment[2]) - 1);

                        segment = sline[2].Split(new string[] { "/" }, 10, StringSplitOptions.None);
                        Vertice fp2 = new Vertice(int.Parse(segment[0]) - 1, int.Parse(segment[1]) - 1, int.Parse(segment[2]) - 1);

                        segment = sline[3].Split(new string[] { "/" }, 10, StringSplitOptions.None);
                        Vertice fp3 = new Vertice(int.Parse(segment[0]) - 1, int.Parse(segment[1]) - 1, int.Parse(segment[2]) - 1);

                        FaceList.Add(new Face(fp1, fp2, fp3));
                    }
                    else if (segment.Length == 3)
                    {
                        Vertice fp1 = new Vertice(int.Parse(segment[0]) - 1, int.Parse(segment[1]) - 1, int.Parse(segment[2]) - 1);

                        segment = sline[2].Split(new string[] { "/" }, 10, StringSplitOptions.None);
                        Vertice fp2 = new Vertice(int.Parse(segment[0]) - 1, int.Parse(segment[1]) - 1, int.Parse(segment[2]) - 1);

                        segment = sline[3].Split(new string[] { "/" }, 10, StringSplitOptions.None);
                        Vertice fp3 = new Vertice(int.Parse(segment[0]) - 1, int.Parse(segment[1]) - 1, int.Parse(segment[2]) - 1);

                        segment = sline[4].Split(new string[] { "/" }, 10, StringSplitOptions.None);
                        Vertice fp4 = new Vertice(int.Parse(segment[0]) - 1, int.Parse(segment[1]) - 1, int.Parse(segment[2]) - 1);

                        FaceList.Add(new Face(fp1, fp2, fp3, fp4));
                    }
                }
            }

            file.Close();

            target.positionVboDataList = positionVboDataList;
            target.normalVboDataList = normalVboDataList;
            target.textureVboDataList = textureVboDataList;
            target.FaceList = FaceList;

            parseFaceList(ref target, false);

            target.loaded = true;
            //Meshes[target.identifier] = target;

            generateVBO(ref target);

            if (target.identifier != -1)
            {
                meshes[target.identifier] = target;
            }
        }

        public void parseFaceList(ref Mesh target, bool genNormal)
        {
            List<Vector3> positionVboDataList = target.positionVboDataList;
            List<Vector3> normalVboDataList = target.normalVboDataList;
            List<Vector2> textureVboDataList = target.textureVboDataList;

            float[][] boneWeightList = target.boneWeightList;
            int[][] boneIdList = target.boneIdList;

            int affBones = 0;
            if (boneWeightList != null)
            {
                affBones = boneWeightList.Length;
            }

            List<Face> faceList = target.FaceList;

            removeTemp(ref faceList);
            convertToTri(ref faceList);

            Vector3[] tmpnormalVboData = new Vector3[normalVboDataList.Count];
            Vector3[] tmptangentVboData = new Vector3[normalVboDataList.Count];
            Vector3[] normalPositionData = new Vector3[normalVboDataList.Count];
            Vector2[] normalUvData = new Vector2[normalVboDataList.Count];

            List<Vector3> normalHelperList = new List<Vector3> { };
            List<Vector3> tangentHelperList = new List<Vector3> { };
            List<Vector2> normalUvHelperList = new List<Vector2> { };
            List<Vector3> positionHelperlist = new List<Vector3> { };

            int faceCount = faceList.Count;

            for (int i = 0; i < faceCount; i++)
            {
                // get all the information from Lists into Facelist
                Vector3[] vposition = new Vector3[3];
                Vector3[] vnormal = new Vector3[3];
                Vector2[] vtexture = new Vector2[3];
                for (int j = 0; j < 3; j++)
                {
                    vposition[j] = positionVboDataList[faceList[i].Vertice[j].Vi];
                    vnormal[j] = normalVboDataList[faceList[i].Vertice[j].Ni];
                    vtexture[j] = textureVboDataList[faceList[i].Vertice[j].Ti];

                    int id = i * 3 + j;

                    //FaceList[i].Vertice[j].position = vposition;
                    //FaceList[i].Vertice[j].normal = vnormal;
                    //FaceList[i].Vertice[j].texture = vtexture;
                }
                // calculating face normal and tangent
                Vector3 v1 = vposition[1] - vposition[0];
                Vector3 v2 = vposition[2] - vposition[0];

                Vector2 vtexture1 = vtexture[1] - vtexture[0];
                Vector2 vtexture2 = vtexture[2] - vtexture[0];

                Vector3 fnormal = Vector3.Cross(v1, v2);

                float s = 1f / (vtexture2.X - vtexture1.X * vtexture2.Y / vtexture1.Y);
                float r = 1f / (vtexture1.X - vtexture2.X * vtexture1.Y / vtexture2.Y);

                Vector3 tangent = Vector3.Normalize(r * v1 + s * v2);

                if(tangent == Vector3.Zero){
                    gameWindow.log("tangent generation ERROR");
                }

                Face curFace = faceList[i];

                // finding out if normal/tangent can be smoothed
                for (int j = 0; j < 3; j++)
                {
                    Vertice curVert = curFace.Vertice[j];

                    // if Normal[Normalindice] has not been assigned a uv coordinate do so and set normal
                    if (normalUvData[curVert.Ni] == Vector2.Zero)
                    {
                        normalUvData[curVert.Ni] = vtexture[j];
                        normalPositionData[curVert.Ni] = vposition[j];

                        tmpnormalVboData[curVert.Ni] = fnormal;
                        tmptangentVboData[curVert.Ni] = tangent;
                    }
                    else
                    {
                        // if Normal[Normalindice] is of the same Uv and place simply add
                        if (normalUvData[curVert.Ni] == vtexture[j] && normalPositionData[curVert.Ni] == vposition[j])
                        {
                            tmpnormalVboData[curVert.Ni] += fnormal;
                            tmptangentVboData[curVert.Ni] += tangent;
                        }
                        else
                        {
                            int helperCount = normalUvHelperList.Count;
                            for (int k = 0; k < helperCount; k++)
                            {
                                // if Normalhelper[Normalindice] is of the same Uv and position simply add
                                if (normalUvHelperList[k] == vtexture[j] && positionHelperlist[k] == vposition[j])
                                {
                                    tangentHelperList[k] += tangent;
                                    normalHelperList[k] += fnormal;

                                    curVert.Normalihelper = k;
                                }
                            }
                            // if matching Normalhelper has not been found create new one
                            if (faceList[i].Vertice[j].Normalihelper == -1)
                            {
                                normalUvHelperList.Add(vtexture[j]);

                                tangentHelperList.Add(tangent);
                                normalHelperList.Add(fnormal);
                                positionHelperlist.Add(vposition[j]);
                                curVert.Normalihelper = normalUvHelperList.Count - 1;
                            }
                        }
                    }
                }
            }

            // put Faces into DataSets (so we can easyly compare them)
            List<VerticeDataSet> vertList = new List<VerticeDataSet> { };

            for (int i = 0; i < faceCount; i++)
            {
                Face curFace = faceList[i];
                for (int j = 0; j < 3; j++)
                {
                    Vertice oldVert = curFace.Vertice[j];

                    VerticeDataSet curVert = new VerticeDataSet();

                    curVert.position = positionVboDataList[oldVert.Vi];
                    curVert.normal = normalVboDataList[oldVert.Ni];
                    if (oldVert.Normalihelper != -1)
                    {
                        if(genNormal)
                            curVert.normal = Vector3.Normalize(normalHelperList[oldVert.Normalihelper]); //-dont use calculated normal

                        curVert.tangent = Vector3.Normalize(tangentHelperList[oldVert.Normalihelper]);
                    }
                    else
                    {
                        if (genNormal)
                            curVert.normal = Vector3.Normalize(tmpnormalVboData[oldVert.Ni]); //-dont use calculated normal
                        curVert.tangent = Vector3.Normalize(tmptangentVboData[oldVert.Ni]);
                    }
                    if (affBones > 0)
                    {
                        curVert.boneWeight = new float[affBones];
                        curVert.boneId = new int[affBones];
                    }

                    for (int k = 0; k < affBones; k++)
                    {
                        curVert.boneWeight[k] = boneWeightList[k][oldVert.Vi];
                        curVert.boneId[k] = boneIdList[k][oldVert.Vi];
                    }
                    curVert.texture = textureVboDataList[oldVert.Ti];

                    vertList.Add(curVert);
                }
            }

            //Remove unneded verts
            int noVerts = vertList.Count;
            List<VerticeDataSet> newVertList = new List<VerticeDataSet> {};
            List<int> newIndiceList = new List<int> { };

            for (int i = 0; i < noVerts; i++)
            {
                VerticeDataSet curVert = vertList[i];
                int curNewVertCount = newVertList.Count;
                int index = -1;

                for (int j = curNewVertCount - 1; j >= 0; j--)
                {
                    if (newVertList[j].Equals(curVert))
                    {
                        index = j;
                    }
                }
                if (index < 0)
                {
                    index = curNewVertCount;
                    newVertList.Add(curVert);
                }

                newIndiceList.Add(index);
            }

            //put Faces into Arrays
            int newIndiceCount = newIndiceList.Count;
            int[] indicesVboData = new int[newIndiceCount];

            int newVertCount = newVertList.Count;
            Vector3[] positionVboData = new Vector3[newVertCount];
            Vector3[] normalVboData = new Vector3[newVertCount];
            Vector3[] tangentVboData = new Vector3[newVertCount];
            Vector2[] textureVboData = new Vector2[newVertCount];

            int[][] boneIdVboData = new int[affBones][];
            float[][] boneWeightVboData = new float[affBones][];

            gameWindow.log("removed Verts" + (noVerts - newVertCount));

            for (int i = 0; i < affBones; i++)
            {
                boneIdVboData[i] = new int[newVertCount];
                boneWeightVboData[i] = new float[newVertCount];
            }

            for (int i = 0; i < newVertCount; i++)
            {
                VerticeDataSet curVert = newVertList[i];

                positionVboData[i] = curVert.position;
                normalVboData[i] = curVert.normal;
                tangentVboData[i] = curVert.tangent;
                textureVboData[i] = curVert.texture;

                for (int k = 0; k < affBones; k++)
                {
                    boneWeightVboData[k][i] = curVert.boneWeight[k];
                    boneIdVboData[k][i] = curVert.boneId[k];
                }
            }

            for (int i = 0; i < newIndiceCount; i++)
            {
                indicesVboData[i] = newIndiceList[i];
            }

            //calculate a bounding Sphere
            float sphere = 0;
            foreach (var vec in positionVboData)
            {
                float length = vec.Length;
                if (length > sphere)
                    sphere = length;
            }

            //deleting unneded
            target.positionVboDataList = null;
            target.normalVboDataList = null;
            target.tangentVboData = null;
            target.indicesVboData = null;

            target.boneWeightList = null;
            target.boneIdList = null;

            //returning mesh info ... DONE :D
            target.positionVboData = positionVboData;
            target.normalVboData = normalVboData;
            target.tangentVboData = tangentVboData;
            target.textureVboData = textureVboData;
            target.indicesVboData = indicesVboData;

            target.boneIdVboData = new int[affBones][];
            target.boneWeightVboData = new float[affBones][];
            for (int i = 0; i < affBones; i++)
            {
                target.boneIdVboData[i] = boneIdVboData[i];
                target.boneWeightVboData[i] = boneWeightVboData[i];
            }

            target.boundingSphere = sphere;
        }

        private void removeTemp(ref List<Face> FaceList)
        {
            int i = 0;
            while (i < FaceList.Count)
            {
                Face curFace = FaceList[i];
                if (curFace.isTemp)
                    FaceList.Remove(curFace);
                else
                    i++;
            }
        }

        private void convertToTri(ref List<Face> FaceList)
        {
            int faces = FaceList.Count;
            for (int i = 0; i < faces; i++)
            {
                Face curFace = FaceList[i];
                if (curFace.Vertice.Length > 3)
                {
                    FaceList[i] = new Face(curFace.Vertice[0], curFace.Vertice[1], curFace.Vertice[2]);
                    FaceList.Add(new Face(curFace.Vertice[2], curFace.Vertice[3], curFace.Vertice[0]));
                }
            }
        }

        public MeshLoader(OpenTkProjectWindow mGameWindow)
        {
            this.gameWindow = mGameWindow;
        }
    }

    struct VerticeDataSet
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector3 tangent;
        public float[] boneWeight;
        public int[] boneId;
        public Vector2 texture;

        public bool Equals(VerticeDataSet vert)
        {
            if (GenericMethods.estSize(this.position - vert.position) < 0.0001f &&
                GenericMethods.estSize(this.texture - vert.texture) < 0.0001f &&
                GenericMethods.estSize(this.normal - vert.normal) < 0.001f)
                return true;
            else
                return false;
        }

        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Position: " + position.ToString());
            sb.AppendLine("Normal: " + normal.ToString());
            sb.AppendLine("Texture: " + texture.ToString());
            return sb.ToString();
        }
    }

    public class Vertice
    {
        public int Vi = 0;
        public int Ti = 0;
        public int Ni = 0;
        public int Normalihelper = -1;

        public Vertice(int vi, int ti, int ni)
        {
            Vi = vi;
            Ti = ti;
            Ni = ni;
        }

        public Vertice()
        {
        }
    }

    public class Face
    {
        public Vertice[] Vertice;
        public bool isTemp;
        public int position;

        public Face(Vertice ind1, Vertice ind2, Vertice ind3)
        {
            Vertice = new Vertice[3];
            Vertice[0] = ind1;
            Vertice[1] = ind2;
            Vertice[2] = ind3;
            // Log.e("VboCube",Vi+"/"+Ti+"/"+Ni);
        }

        public Face(Vertice ind1, Vertice ind2, Vertice ind3, Vertice ind4)
        {
            Vertice = new Vertice[4];
            Vertice[0] = ind1;
            Vertice[1] = ind2;
            Vertice[2] = ind3;
            Vertice[3] = ind4;
            // Log.e("VboCube",Vi+"/"+Ti+"/"+Ni);
        }

        public Face(int vCount, int position)
        {
            Vertice = new Vertice[vCount];
            this.position = position;

            for (int i = 0; i < vCount; i++)
                Vertice[i] = new Vertice();

        }
    }
}
