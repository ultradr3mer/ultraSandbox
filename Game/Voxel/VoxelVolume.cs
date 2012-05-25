using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenTkProject.Game.Voxel
{
    public abstract class VoxelVolume : VoxelManager
    {
        private float affectionRadius;

        public VoxelVolume(VoxelManager parent)
        {
            Parent = parent;
        }

        public virtual int check(Vector3 pos)
        {
            return 1;
        }

        public override Vector3 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                Parent.removeVolume(this);
                base.Position = value;
                Parent.addVolume(this);
            }
        }

        public override void kill()
        {
            Parent.removeVolume(this);
            killChilds();
        }

        public float AffectionRadius
        {
            get { return affectionRadius; }
            set
            {
                Parent.removeVolume(this);
                affectionRadius = value;
                Parent.addVolume(this);
            }
        }
    }

    public class VoxelVolumeSphere : VoxelVolume
    {
        public VoxelVolumeSphere(VoxelManager parent, float radius) : base(parent)
        {
            this.AffectionRadius = radius;
        }

        public override int check(Vector3 pos)
        {
            if ((pos - Position).Length < AffectionRadius)
                return 1;

            return 0;
        }
    }
}
