using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenTkProject.Game.Voxel
{
    public class VoxelManager : GameObject
    {
        public List<VoxelChunk> chunks = new List<VoxelChunk> { };

        VoxelManager parent;

        public List<VoxelVolume> voulumetrics = new List<VoxelVolume> { };

        public VoxelManager(GameObject parent)
            : base(parent)
        {
            float chunkAmnt = 2.5f;
            
            for (float i = -chunkAmnt; i <= chunkAmnt; i++)
            {
                for (float j = -chunkAmnt; j <= chunkAmnt; j++)
                {
                    for (int k = 0; k < chunkAmnt; k++)
                    {
                        Vector3 pos = new Vector3(i * 10, 5 + k * 10, j * 10);

                        Vector3 start = Vector3.One * -5 + pos;
                        Vector3 end = Vector3.One * 5 + pos;

                        chunks.Add(new VoxelChunk(this, start, end));
                    }
                }
            }
        }

        public override void update()
        {
            if (wasUpdated)
            {
                updateChilds();
                wasUpdated = false;
            }
        }



        public VoxelManager()
        {
        }

        public VoxelManager(VoxelManager parent)
            : base(parent)
        {
            Parent = parent;
        }

        public new VoxelManager Parent
        {
            get { return parent; }
            set
            {
                base.Parent = value;

                if (value != null)
                {
                    parent = value;
                }

                //parent.forceUpdate();
            }
        }

        public virtual int GetData(Vector3 pos)
        {
            foreach (var Volume in voulumetrics)
            {
                int type = Volume.check(pos);

                if (type != 0)
                    return type;
            }
            return 0;
        }

        internal void addVolume(VoxelVolume voxelVolume)
        {
            foreach (var chunk in chunks)
            {
                if(chunk.isAffected(voxelVolume)){
                    chunk.voulumetrics.Add(voxelVolume);
                    chunk.wasUpdated = true;
                    wasUpdated = true;
                }

            }
        }

        internal void removeVolume(VoxelVolume voxelVolume)
        {
            foreach (var chunk in chunks)
            {
                if (chunk.voulumetrics.Contains(voxelVolume))
                {
                    chunk.voulumetrics.Remove(voxelVolume);
                    chunk.wasUpdated = true;
                    wasUpdated = true;
                }
            }
        }
    }
}
