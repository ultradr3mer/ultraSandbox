using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenTkProject.Game.Voxel
{
    public class VoxelDataGenerator : VoxelManager
    {
        public Vector3 resolution;
        //int[][][] data;
        int[] data;

        public Vector3 start, end, scaling;

        public int voxelAmnt;
        public Vector3 voxelSize;

        public VoxelDataGenerator(VoxelManager parent, Vector3 start , Vector3 end)
            : base(parent)
        {
            this.start = start;
            this.end = end;

            scaling = (end - start);

            resolution = Vector3.One * 10f + Vector3.One * 2f;

            voxelAmnt = (int)resolution.X * (int)resolution.Y * (int)resolution.Z;

            voxelSize = Vector3.Divide(scaling, resolution - Vector3.One * 2f);
        }

        public void generateVoxelData()
        {
            data = new int[(int)resolution.X * (int)resolution.Y * (int)resolution.Z];
            for (int i = 0; i < data.Length; i++)
			{
                Vector3 globalPos = idToGPos(i);

                data[i] = Parent.GetData(globalPos);
            }
        }

        public Vector3 idToGPos(int id){
                Vector3 localPos = idToPos(id);
                return Vector3.Multiply(localPos, scaling) + start;
        }

        public Vector3 idToPos(int id){
            return Vector3.Divide(idToNumber(id) - Vector3.One + Vector3.One * 0.5f, resolution - Vector3.One * 2f);// +Vector3.Divide(Vector3.One * 0.5f, resolution);
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

            return pos;
        }

        public int numberToID(Vector3 pos)
        {
            if (pos.X < 0|| pos.X >= resolution.X)
                return -1;

            if (pos.Y < 0 || pos.Y >= resolution.Y)
                return -1;

            if (pos.Z < 0 || pos.Z >= resolution.Z)
                return -1;

            return (int)(resolution.X * resolution.Y * pos.Z + resolution.X * pos.Y + pos.X);
        }

        public int getData(int id)
        {
            if(data != null)
                return data[id];

            return Parent.GetData(idToGPos(id));
        }

        public int getRelativeData(int id,Vector3 shift)
        {
            if (data != null)
            {
                Vector3 number = idToNumber(id);
                number += shift;

                int cageId = numberToID(number);
                if (cageId != -1)
                    return data[cageId];
            }

            Vector3 pos = idToGPos(id);
            pos += Vector3.Multiply(voxelSize,shift);

            return Parent.GetData(pos);
        }

        internal bool isBorder(int id)
        {
            Vector3 pos = idToPos(id);

            if (pos.X < 0 || pos.X > 1)
                return true;

            if (pos.Y < 0 || pos.Y > 1)
                return true;

            if (pos.Z < 0 || pos.Z > 1)
                return true;

            return false;
        }

        internal void deleteCage()
        {
            data = null;
        }
    }
}
