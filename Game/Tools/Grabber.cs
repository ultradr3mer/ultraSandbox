using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jitter.Dynamics;
using Jitter.LinearMath;
using OpenTK;
using OpenTK.Input;
using OpenTkProject.Drawables;
using OpenTkProject.Drawables.Models;

namespace OpenTkProject.Game.Tools
{
    public class Grabber : Tool
    {
        private float grabDist;
        private PhysModel selectedMod;
        private RigidBody selectedBody;
        private Jitter.Dynamics.Constraints.SingleBody.PointOnPoint mConst;
        JVector selModRelPos;

        private Model muzzleModel;

        Pos2Model arcModel;
        private Vector3 weaponLocalHitCoords;
        private Vector4 modelLocalHitCoords;

        public Grabber(Player player, GameInput gameInput):base(player,gameInput)
        {
            createArcModel();
            createMuzzleModel();

            icon.setMaterial("hud\\grabber_icon.xmf");
        }

        private void createMuzzleModel()
        {
            muzzleModel = new Model(this);
            muzzleModel.setMaterial("weapons\\grabber_muzzle.xmf");
            muzzleModel.setMesh("weapons\\toolgun_muzzle.obj");

            muzzleModel.Color = new Vector4(0.6f, 0.7f, 1.0f, 1);
            muzzleModel.isVisible = false;

            muzzleModel.renderlayer = Drawable.RenderLayer.Transparent;

            muzzleModel.Scene = Scene;
        }

        private void createArcModel()
        {
            arcModel = new Pos2Model(this);
            arcModel.setMaterial("weapons\\toolgun_arc.xmf");
            arcModel.setMesh("weapons\\toolgun_arc.obj");

            arcModel.Color = new Vector4(0.6f, 0.7f, 1.0f, 1)*0.2f;
            arcModel.isVisible = false;

            arcModel.renderlayer = Drawable.RenderLayer.Transparent;

            arcModel.Scene = Scene;
            
        }

        protected override void fireDown()
        {
            RigidBody body; JVector normal; float frac;

            bool result = Scene.world.CollisionSystem.Raycast(GenericMethods.FromOpenTKVector(Position), GenericMethods.FromOpenTKVector(PointingDirection),
                raycastCallback, out body, out normal, out frac);

            Vector4 gpos = new Vector4(Position + PointingDirection * frac, 1);

            JVector hitCoords = GenericMethods.FromOpenTKVector(gpos.Xyz);
            weaponLocalHitCoords = GenericMethods.Mult( gpos, Matrix4.Invert(weaponModel.ModelMatrix)).Xyz;
            arcModel.Orientation2 = Matrix4.CreateTranslation(weaponLocalHitCoords - new Vector3(0, 0, -5));

            muzzleModel.isVisible = true;

            if (result && body.Tag != null)
            {
                PhysModel curMod = (PhysModel)body.Tag;

                if (curMod.grabable)
                {

                    arcModel.isVisible = true;

                    grabDist = frac;

                    selectedBody = body;
                    selectedMod = (PhysModel)body.Tag;
                    selectedMod.selected = 1;
                    selectedMod.Forceupdate = true;

                    Matrix4 localMaker = Matrix4.Invert(Matrix4.Mult(selectedMod.Orientation, selectedMod.ModelMatrix));
                    modelLocalHitCoords = GenericMethods.Mult(gpos, localMaker);

                    if (body.IsStatic)
                    {
                        selModRelPos = body.Position - hitCoords;
                    }
                    else
                    {
                        JVector lanchor = hitCoords - body.Position;
                        lanchor = JVector.Transform(lanchor, JMatrix.Transpose(body.Orientation));

                        body.IsActive = true;

                        //body.SetMassProperties(JMatrix.Identity, 0.1f, false);
                        //body.AffectedByGravity = false;

                        mConst = new Jitter.Dynamics.Constraints.SingleBody.PointOnPoint(body, lanchor);
                        mConst.Softness = 0.02f;
                        mConst.BiasFactor = 0.1f;
                        Scene.world.AddConstraint(mConst);
                    }
                }
            }
        }

        protected override void fireUp()
        {
            arcModel.isVisible = false;
            muzzleModel.isVisible = false;
            if (selectedMod != null)
            {
                //mConst.Body1.AffectedByGravity = true;
                if (mConst != null)
                {
                    Scene.world.RemoveConstraint(mConst);
                }
                selectedBody = null;
                selectedMod.selected = 0;
                selectedMod.Forceupdate = false;
                selectedMod = null;
                mConst = null;
            }
        }

        protected override void rotate()
        {
            if (gameInput.keyboard[Key.Q])
            {
                if (selectedMod != null && mConst == null)
                {
                    JMatrix rotMatA = JMatrix.CreateRotationY(gameInput.move.X * cameraRotSpeed);
                    JMatrix rotMatB = GenericMethods.FromOpenTKMatrix(Matrix4.CreateFromAxisAngle(-Parent.rightVec, gameInput.move.Y * cameraRotSpeed));

                    JMatrix rotMatFinal = JMatrix.Multiply(rotMatA, rotMatB);

                    selectedBody.Orientation = JMatrix.Multiply(selectedBody.Orientation, rotMatFinal);
                }
            }
            else
            {
                base.rotate();
            }
        }

        protected override void interactDown()
        {
            if (selectedMod == null)
            {
                RigidBody body; JVector normal; float frac;

                bool result = Scene.world.CollisionSystem.Raycast(GenericMethods.FromOpenTKVector(Position), GenericMethods.FromOpenTKVector(PointingDirection),
                    raycastCallback, out body, out normal, out frac);

                if (result && body.Tag != null)
                {
                    PhysModel curMod = (PhysModel)body.Tag;

                    if (curMod.grabable)
                    {
                        curMod.IsStatic = !curMod.IsStatic;
                        curMod.selectedSmooth = 1;
                    }
                }
            }
        }

        public override void update()
        {
            base.update();

            if (Parent.tool == this)
            {
                arcModel.Position = weaponModel.Position;
                //arcModel.Position2 = weaponModel.Position;
                muzzleModel.Position = weaponModel.Position;

                muzzleModel.Orientation = weaponModel.Orientation;
                arcModel.Orientation = weaponModel.Orientation;
                arcModel.Orientation2 = weaponModel.Orientation;

                // moving model

                if (selectedMod != null)
                {
                    JVector anchorCoords = GenericMethods.FromOpenTKVector(Position + PointingDirection * grabDist);
                    if (mConst != null)
                    {
                        mConst.Anchor = anchorCoords;
                    }
                    else if (selectedBody.IsStatic)
                    {
                        selectedBody.Position = anchorCoords + selModRelPos;
                    }

                    selectedMod.updateMatrix();
                    Matrix4 globalMaker = Matrix4.Mult(selectedMod.Orientation, selectedMod.ModelMatrix);
                    Vector4 gpos = GenericMethods.Mult(modelLocalHitCoords, globalMaker);

                    JVector hitCoords = GenericMethods.FromOpenTKVector(gpos.Xyz);
                    //weaponLocalHitCoords = GenericMethods.Mult(gpos, Matrix4.Invert(Parent.viewInfo.modelviewMatrix)).Xyz;

                    //arcModel.Position2 = GenericMethods.Mult(new Vector4(weaponLocalHitCoords - new Vector3(0, 0, 5), 1f), Matrix4.Invert(Parent.viewInfo.modelviewMatrix)).Xyz;
                    arcModel.Position2 = gpos.Xyz - 5 * Parent.viewInfo.PointingDirection;
                    //arcModel.Orientation2 = Matrix4.CreateTranslation(weaponLocalHitCoords - new Vector3(0, 0, -5));
                }
            }
            else
            {
                arcModel.isVisible = false;
                muzzleModel.isVisible = false;
                if (selectedMod != null)
                {
                    //mConst.Body1.AffectedByGravity = true;
                    if (mConst != null)
                    {
                        Scene.world.RemoveConstraint(mConst);
                    }
                    selectedBody = null;
                    selectedMod.selected = 0;
                    selectedMod.Forceupdate = false;
                    selectedMod = null;
                    mConst = null;
                }
            }
        }
        /*
        internal override void activate()
        {
            parent.tool = (Grabber)this;
        }
         * */
    }
}
