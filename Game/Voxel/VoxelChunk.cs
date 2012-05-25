using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTkProject.Drawables;
using OpenTkProject.Drawables.Models;
using OpenTkProject.Drawables.Models.Paticles;

namespace OpenTkProject.Game.Voxel
{
    public class VoxelChunk : VoxelManager
    {
        VoxelDataGenerator voxelData;
        Model surface;
        VoxelMeshGenerator meshHelper;

        Vector3 start, end;

        public VoxelChunk(VoxelManager parent, Vector3 start ,Vector3 end)
            : base(parent)
        {
            this.start = start;
            this.end = end;

            voxelData = new VoxelDataGenerator(this, start, end);
            meshHelper = new VoxelMeshGenerator(this, voxelData);

            Position = (end + start) / 2f;

            generateSurface();

            wasUpdated = false;
            //generateDebugSurface();
            //generateSurfaceMesh();
            //scene.generateParticleSys();

            surface.isVisible = false;
        }

        private void generateSurface()
        {
            surface = new Model(this);
            surface.addMaterial("rock_face.xmf");

            surface.Color = new Vector4(0.8f, 0.3f, 0.8f, 1.0f) * 1f;
            surface.Size = (end - start) * 0.5f;

            surface.Position = Position;
        }

        public override void update(){
            if (wasUpdated)
            {
                surface.setMesh(meshHelper.generateMesh(voxelData));
                if(voulumetrics.Count >0)
                    surface.isVisible = true;
            }
        }

        internal bool isAffected(VoxelVolume voxelVolume)
        {
            Vector3 position = voxelVolume.Position;
            float range = voxelVolume.AffectionRadius;

            return (position.X > start.X - range && position.X < end.X + range &&
                position.Y > start.Y - range && position.Y < end.Y + range &&
                position.Z > start.Z - range && position.Z < end.Z + range);
        }
    }
}
