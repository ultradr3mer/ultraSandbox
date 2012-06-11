using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenTkProject.Game.Voxel
{
    class VoxelMeshGenerator : VoxelManager
    {
        public List<Vector3> positions;
        public List<Vector2> textureCoords;
        public List<Vector3> normals;
        public List<Face> Faces;

        public Vector3 resolution;
        private Mesh surfaceMesh;

        public VoxelMeshGenerator(VoxelManager parent, VoxelDataGenerator vdata) : base (parent)
        {
            resolution = vdata.resolution + Vector3.One; //add 3 (10 Faces mean 11 Vertecies)

            //prepareData();

            surfaceMesh = new Mesh();

            //generateMesh();
        }

        public Mesh generateMesh(VoxelDataGenerator vdata)
        {
            gameWindow.sw.Start();

            vdata.generateVoxelData();
            prepareData();

            //int[] data = vdata.data;
            float vSize = 0.5f / vdata.resolution.X;

            Vector3 up = new Vector3(0, 1, 0);
            Vector3 right = new Vector3(1, 0, 0);
            Vector3 front = new Vector3(0, 0, 1);
            Vector3 back = new Vector3(0, 0, -1);
            Vector3 down = new Vector3(0, -1, 0);
            Vector3 left = new Vector3(-1, 0, 0);

            for (int i = 0; i < vdata.voxelAmnt; i++)
            {
                Vector3 postion = vdata.idToNumber(i);

                if (vdata.getData(i) == 1)
                {
                    bool isBorder = vdata.isBorder(i);

                    if (vdata.getRelativeData(i,up) == 0)
                    {
                        generateTopFace(i, isBorder, postion);
                    }

                    if (vdata.getRelativeData(i,right) == 0)
                    {
                        generateRightFace(i, isBorder, postion);
                    }

                    if (vdata.getRelativeData(i,front) == 0)
                    {
                        generateFrontFace(i, isBorder, postion);
                    }

                    if (vdata.getRelativeData(i,back) == 0)
                    {
                        generateBackFace(i, isBorder, postion);
                    }

                    if (vdata.getRelativeData(i,left) == 0)
                    {
                        generateLeftFace(i, isBorder, postion);
                    }

                    if (vdata.getRelativeData(i,down) == 0)
                    {
                        generateDownFace(i, isBorder, postion);
                    }
                }
            }

            vdata.deleteCage();

            performSmoothing();
            performCubicMapping();

            surfaceMesh.positionVboDataList = positions;
            surfaceMesh.textureVboDataList = textureCoords;
            surfaceMesh.normalVboDataList = normals;

            surfaceMesh.FaceList = Faces;

            gameWindow.meshLoader.parseFaceList(ref surfaceMesh, false);
            gameWindow.meshLoader.generateVBO(ref surfaceMesh);

            gameWindow.sw.Stop();
            gameWindow.log("generated Chunk:" + gameWindow.sw.Elapsed);

            return surfaceMesh;
        }

        private void performCubicMapping()
        {
            //textureCoords = new List<Vector2> { };

            for (int i = 0; i < Faces.Count; i++)
            {
                Face curFace = Faces[i];

                Vector3 faceNormal = (normals[curFace.Vertice[0].Vi] +
                normals[curFace.Vertice[1].Vi] +
                normals[curFace.Vertice[2].Vi] +
                normals[curFace.Vertice[3].Vi])/4f;

                Vector3 tmpN = faceNormal;

                tmpN.X *= Math.Sign(tmpN.X);
                tmpN.Y *= Math.Sign(tmpN.Y);
                tmpN.Z *= Math.Sign(tmpN.Z);

                int mappingMethod = -1;

                if (tmpN.X >= tmpN.Y && tmpN.X >= tmpN.Z)
                {
                    if (faceNormal.X < 0)
                        mappingMethod = 0;
                    else
                        mappingMethod = 1;
                }

                if (tmpN.Y >= tmpN.Z && tmpN.Y >= tmpN.X)
                {
                    if (faceNormal.Y < 0)
                        mappingMethod = 2;
                    else
                        mappingMethod = 3;
                }

                if (tmpN.Z >= tmpN.Y && tmpN.Z >= tmpN.X)
                {
                    if (faceNormal.Z < 0)
                        mappingMethod = 4;
                    else
                        mappingMethod = 5;
                }

                

                for (int j = 0; j < 4; j++)
                {
                    Vector3 pos = positions[curFace.Vertice[j].Vi];

                    curFace.Vertice[j].Ti = textureCoords.Count;

                    pos = pos * 0.5f + Vector3.One * 0.5f;

                    float mod = pos.LengthFast * 0.001f;

                    pos += Vector3.One * mod;

                    switch (mappingMethod)
                    {
                        case 0:
                            textureCoords.Add(new Vector2(-pos.Z, pos.Y));
                            break;
                        case 1:
                            textureCoords.Add(new Vector2(pos.Z, pos.Y));
                            break;

                        case 2:
                            textureCoords.Add(new Vector2(-pos.X, pos.Z));
                            break;
                        case 3:
                            textureCoords.Add(new Vector2(pos.X, pos.Z));
                            break;

                        case 4:
                            textureCoords.Add(new Vector2(pos.X, pos.Y));
                            break;
                        case 5:
                            textureCoords.Add(new Vector2(-pos.X, pos.Y));
                            break;

                        default:
                            textureCoords.Add(new Vector2(pos.X, pos.Y));
                            break;
                    }
                }


                Faces[i] = curFace;
            }
        }

        private void performSmoothing()
        {
            List<Vector3> positionsRaw = positions;
            List<Vector3> normalsRaw = normals;

            int noPositions = positions.Count;

            //regenerating position and normal list
            positions = new List<Vector3> { };
            normals = new List<Vector3> { };
            for (int i = 0; i < noPositions; i++)
            {
                positions.Add(Vector3.Zero);
                normals.Add(Vector3.Zero);
            }

            int[] noUserFaces = new int[noPositions];

            for (int i = 0; i < Faces.Count; i++)
            {
                Face curFace = Faces[i];

                Vector3 faceCenter =
                    (positionsRaw[curFace.Vertice[0].Vi] +
                    positionsRaw[curFace.Vertice[1].Vi] +
                    positionsRaw[curFace.Vertice[2].Vi] +
                    positionsRaw[curFace.Vertice[3].Vi]) / 4.0f;

                Vector3 faceNormal =
                    (normalsRaw[curFace.Vertice[0].Vi] +
                    normalsRaw[curFace.Vertice[1].Vi] +
                    normalsRaw[curFace.Vertice[2].Vi] +
                    normalsRaw[curFace.Vertice[3].Vi]) / 4.0f;

                for (int j = 0; j < 4; j++)
                {
                    int indice = curFace.Vertice[j].Vi;

                    positions[indice] += faceCenter;
                    normals[indice] += faceNormal;
                    noUserFaces[indice]++;
                }
            }

            for (int i = 0; i < noPositions; i++)
            {
                int curNoUserFaces = noUserFaces[i];
                if (curNoUserFaces > 1)
                {
                    positions[i] /= curNoUserFaces;
                    normals[i] = Vector3.Normalize(normals[i]);
                }
            }
        }

        private void generateRightFace(int i, bool isBorder, Vector3 postion)
        {
            Vector3 position1 = postion + new Vector3(1, 0, 0);
            Vector3 position2 = postion + new Vector3(1, 1, 0);
            Vector3 position3 = postion + new Vector3(1, 1, 1);
            Vector3 position4 = postion + new Vector3(1, 0, 1);

            Vector3 normal = new Vector3(1, 0, 0);

            generateFace(position1, position2, position3, position4, 1, normal, isBorder);
        }

        private void generateLeftFace(int i, bool isBorder, Vector3 postion)
        {
            Vector3 position1 = postion + new Vector3(0, 0, 0);
            Vector3 position2 = postion + new Vector3(0, 0, 1);
            Vector3 position3 = postion + new Vector3(0, 1, 1);
            Vector3 position4 = postion + new Vector3(0, 1, 0);

            Vector3 normal = new Vector3(-1, 0, 0);

            generateFace(position1, position2, position3, position4, 1, normal, isBorder);
        }

        private void generateFrontFace(int i, bool isBorder, Vector3 postion)
        {
            Vector3 position1 = postion + new Vector3(0, 0, 1);
            Vector3 position2 = postion + new Vector3(1, 0, 1);
            Vector3 position3 = postion + new Vector3(1, 1, 1);
            Vector3 position4 = postion + new Vector3(0, 1, 1);

            Vector3 normal = new Vector3(0, 0, 1);

            generateFace(position1, position2, position3, position4, 2, normal, isBorder);
        }

        private void generateBackFace(int i, bool isBorder, Vector3 postion)
        {
            Vector3 position1 = postion + new Vector3(0, 0, 0);
            Vector3 position2 = postion + new Vector3(0, 1, 0);
            Vector3 position3 = postion + new Vector3(1, 1, 0);
            Vector3 position4 = postion + new Vector3(1, 0, 0);

            Vector3 normal = new Vector3(0, 0, -1);

            generateFace(position1, position2, position3, position4, 2, normal, isBorder);
        }

        private void generateTopFace(int i, bool isBorder, Vector3 postion)
        {
            Vector3 position1 = postion + new Vector3(0, 1, 0);
            Vector3 position2 = postion + new Vector3(0, 1, 1);
            Vector3 position3 = postion + new Vector3(1, 1, 1);
            Vector3 position4 = postion + new Vector3(1, 1, 0);

            Vector3 normal = new Vector3(0, 1, 0);

            generateFace(position1, position2, position3, position4, 0, normal, isBorder);
        }

        private void generateDownFace(int i, bool isBorder, Vector3 postion)
        {
            Vector3 position1 = postion + new Vector3(0, 0, 0);
            Vector3 position2 = postion + new Vector3(1, 0, 0);
            Vector3 position3 = postion + new Vector3(1, 0, 1);
            Vector3 position4 = postion + new Vector3(0, 0, 1);

            Vector3 normal = new Vector3(0, -1, 0);

            generateFace(position1, position2, position3, position4, 0, normal, isBorder);
        }

        private void generateFace(Vector3 position1, Vector3 position2, Vector3 position3, Vector3 position4, int mapping, Vector3 normal, bool isBorder)
        {
            int Index1 = numberToID(position1);
            int Index2 = numberToID(position2);
            int Index3 = numberToID(position3);
            int Index4 = numberToID(position4);

            int textureIndex1 = 0;
            int textureIndex2 = 0;
            int textureIndex3 = 0;
            int textureIndex4 = 0;

            if (mapping == 0)
            {
                textureIndex1 = numberToID(new Vector3(position1.X, position1.Z, 0));
                textureIndex2 = numberToID(new Vector3(position2.X, position2.Z, 0));
                textureIndex3 = numberToID(new Vector3(position3.X, position3.Z, 0));
                textureIndex4 = numberToID(new Vector3(position4.X, position4.Z, 0));
            }
            else if (mapping == 1)
            {
                textureIndex1 = numberToID(new Vector3(position1.Z, position1.Y, 0));
                textureIndex2 = numberToID(new Vector3(position2.Z, position2.Y, 0));
                textureIndex3 = numberToID(new Vector3(position3.Z, position3.Y, 0));
                textureIndex4 = numberToID(new Vector3(position4.Z, position4.Y, 0));
            }
            else if (mapping == 2)
            {
                textureIndex1 = numberToID(new Vector3(position1.X, position1.Y, 0));
                textureIndex2 = numberToID(new Vector3(position2.X, position2.Y, 0));
                textureIndex3 = numberToID(new Vector3(position3.X, position3.Y, 0));
                textureIndex4 = numberToID(new Vector3(position4.X, position4.Y, 0));
            }

            normals[Index1] += normal;
            normals[Index2] += normal;
            normals[Index3] += normal;
            normals[Index4] += normal;


            Vertice vert1 = new Vertice(Index1, textureIndex1, Index1);
            Vertice vert2 = new Vertice(Index2, textureIndex2, Index2);
            Vertice vert3 = new Vertice(Index3, textureIndex3, Index3);
            Vertice vert4 = new Vertice(Index4, textureIndex4, Index4);

            Face newFace = new Face(vert1, vert2, vert3, vert4);
            newFace.isTemp = isBorder;

            Faces.Add(newFace);

            
            //Faces.Add(new Face(vert3, vert2, vert1));
            //Faces.Add(new Face(vert2, vert3, vert4));
             
        }

        private void prepareData()
        {
            positions = new List<Vector3> { };
            textureCoords = new List<Vector2> { };
            normals = new List<Vector3> { };
            Faces = new List<Face> { };

            //positions = new Vector3[(int)resolution.X * (int)resolution.Y * (int)resolution.Z];
            int posAmnt = (int)resolution.X * (int)resolution.Y * (int)resolution.Z;
            for (int i = 0; i < posAmnt; i++)
            {
                Vector3 localPos = idToPos(i);
                positions.Add(localPos * 2f - Vector3.One);
                normals.Add(Vector3.Zero);
            }

            int texAmnt = (int)resolution.X * (int)resolution.Y;
            for (int i = 0; i < texAmnt; i++)
            {
                Vector3 localPos = idToPos(i);
                textureCoords.Add(localPos.Xy);
            }
        }

        public Vector3 idToPos(int id)
        {
            return Vector3.Divide(idToNumber(id) - Vector3.One, resolution - Vector3.One * 3);
        }

        public Vector3 idToNumber(int id)
        {
            Vector3 pos = new Vector3();

            int zStep = (int)(resolution.X * resolution.Y); // calculate how many voxels make up one Z layer
            pos.Z = (int)(id) / zStep; // calculate Z position by dividing by this value

            int tmpId = id % zStep; // calculate the remaining part / removing unused zLayers

            int yStep = (int)(resolution.X); // calculate how many voxels make up one Y layer
            pos.Y = (int)(tmpId) / yStep; // calculate Y by dividing by this

            int tmpId2 = tmpId % yStep; // calculate remaining part

            int xStep = 1; // oviously one one voxel equals one layer
            pos.X = (int)(tmpId2) / xStep; // ''

            return pos; // add 1 because of smoothing bounds
        }

        public int numberToID(Vector3 pos)
        {
            if (pos.X < 0 || pos.X >= resolution.X)
                return -1;

            if (pos.Y < 0 || pos.Y >= resolution.Y)
                return -1;

            if (pos.Z < 0 || pos.Z >= resolution.Z)
                return -1;

            return (int)(resolution.X * resolution.Y * pos.Z + resolution.X * pos.Y + pos.X);
        }
    }


}
