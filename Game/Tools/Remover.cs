using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jitter.Dynamics;
using OpenTkProject.Drawables;
using Jitter.LinearMath;
using OpenTK;
using OpenTkProject.Drawables.Models;

namespace OpenTkProject.Game.Tools
{
    class Remover : Tool
    {
        private Model muzzleModel;
        float muzzleFading = 0.9f;

        public Remover(Player parent, GameInput input)
            : base(parent, input)
        {
            icon.setMaterial("hud\\remover_icon.xmf");
            createMuzzleModel();
        }

        protected override void fireDown()
        {
            RigidBody body; JVector normal; float frac;

            bool result = Scene.world.CollisionSystem.Raycast(GenericMethods.FromOpenTKVector(Position), GenericMethods.FromOpenTKVector(PointingDirection),
                raycastCallback, out body, out normal, out frac);

            muzzleModel.Color = new Vector4(0.8f, 0.3f, 0.8f, 1.0f) * 2;

            if (body != null)
            {
                PhysModel selectedMod = (PhysModel)body.Tag;

                if (selectedMod != null)
                    selectedMod.dissolve();
            }
        }

        private void createMuzzleModel()
        {
            muzzleModel = new Model(Parent);
            muzzleModel.setMaterial("weapons\\toolgun_muzzle.xmf");
            muzzleModel.setMesh("weapons\\toolgun_muzzle.obj");

            muzzleModel.Color = Vector4.Zero;
            muzzleModel.isVisible = true;

            muzzleModel.renderlayer = Drawable.RenderLayer.Transparent;

            muzzleModel.Scene = Scene;
        }

        public override void update()
        {
            base.update();

            if (Parent.tool == this)
            {
                muzzleModel.isVisible = true;
                muzzleModel.Color = muzzleFading * muzzleModel.Color;
                muzzleModel.Orientation = weaponModel.Orientation;
                muzzleModel.Position = weaponModel.Position;
            }
            else
            {
                muzzleModel.isVisible = false;
            }
        }
    }
}
