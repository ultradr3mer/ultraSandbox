using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jitter.Collision;
using Jitter.Dynamics;
using Jitter.LinearMath;
using OpenTK.Input;
using OpenTK;
using OpenTkProject.Drawables.Models;

namespace OpenTkProject.Game.Tools
{
    public class Tool : GameObject
    {
        public GameInput gameInput;
        protected RaycastCallback raycastCallback;

        private Player parent;

        public float cameraRotSpeed = 0.001f;
        protected bool prevK;
        protected int slot;

        protected GuiElement icon;
        protected Vector2 iconPos, smoothIconPos;
        protected float iconWeight = 0.9f;
        protected float iconDist = 0.25f;

        Matrix4 weaponRMat = Matrix4.Mult(Matrix4.CreateRotationX((float)Math.PI / 2),Matrix4.CreateRotationY((float)Math.PI));

        protected Matrix4 spawOffset = Matrix4.CreateTranslation(new Vector3(0, -0.5f, 0));

        protected Model weaponModel;

       //Vector3 rawRotation;//, rotation;

        public Tool(Player parent, GameInput gameInput)
        {
            Parent = parent;

            this.gameInput = gameInput;

            raycastCallback = new RaycastCallback(mRaycastCallback);

            slot = getSlot();

            icon = new GuiElement(this.parent.hud);
            icon.setSizePix(new Vector2(256, 128));

            iconPos = new Vector2(-0.8f, 0.8f - slot * iconDist);
            smoothIconPos = iconPos;
            icon.Position = smoothIconPos;

            icon.setMaterial("hud\\blank_icon.xmf");

            createWeaponModel();
        }

        protected int getSlot()
        {
            return parent.tools.Count;
        }

        private void createWeaponModel()
        {
            weaponModel = new Model(parent);
            weaponModel.setMaterial("weapons\\toolgun.xmf");
            weaponModel.setMesh("weapons\\toolgun.obj");

            // kind of dirty (makes it possible to use the inverted viewpatrox for world poition)
            //weaponModel.Orientation = Matrix4.CreateTranslation(new Vector3(0.3f, -0.3f, -0.7f));

            weaponModel.isVisible = false;

            weaponModel.Scene = Scene;
        }

        protected bool mRaycastCallback(RigidBody hitbody, JVector normal, float frac)
        {
            return (hitbody != parent.Body);
        }

        public new Player Parent
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


        public override void update()
        {
            if (parent.tool == this)
            {
                iconPos = new Vector2(-0.8f, 0.8f - slot * iconDist);
                icon.Color = new Vector4(0.8f, 0.3f, 0.8f, 1.0f);
                Position = parent.Position;

                //weaponModel.updateModelMatrix();

                //weaponModel.ModelMatrix = Matrix4.Invert(parent.viewInfo.modelviewMatrix);
                weaponModel.isVisible = true;
                PointingDirection = parent.PointingDirection;

                /*
                weaponModel.Position = position +
                    parent.viewInfo.pointingDirection * 0.7f +
                    parent.viewInfo.pointingDirectionUp * 0.3f +
                    parent.viewInfo.pointingDirectionRight * -0.3f;
                 */

                Vector3 newPos = GenericMethods.Mult(new Vector4(0.3f, -0.3f, -0.7f, 1f), Matrix4.Invert(parent.viewInfo.modelviewMatrix)).Xyz;
                Matrix4 newOri = Matrix4.Mult(weaponRMat,GenericMethods.MatrixFromVector(PointingDirection));

                float smoothness = 0.5f;

                weaponModel.Position = weaponModel.Position * smoothness + newPos * (1 - smoothness);
                weaponModel.Orientation = GenericMethods.BlendMatrix(weaponModel.Orientation, newOri, smoothness);

                if (!wasActive)
                {
                    startUsing();
                }

                if (gameInput != null)
                {
                    //rotation
                    rotate();

                    //move
                    move();

                    // fire player fire
                    bool K = gameInput.mouse[MouseButton.Left];
                    if (K && !prevK && wasActive)
                    {
                        fireDown();
                    }
                    else if (!K && prevK)
                    {
                        fireUp();
                    }
                    prevK = K;

                    // fire player interact
                    bool E = gameInput.keyboard[Key.E];
                    if (E && !prevE)
                    {
                        interactDown();
                    }
                    else if (!E && prevE)
                    {
                        interactUp();
                    }
                    prevE = E;
                }

                wasActive = true;
                updateChilds();
            }
            else
            {
                iconPos = new Vector2(-0.85f, 0.8f - slot * iconDist);
                icon.Color = new Vector4(0.1f, 0.12f, 0.2f, 1.0f);

                weaponModel.isVisible = false;
                wasActive = false;
            }

            smoothIconPos = smoothIconPos * iconWeight + iconPos * (1 - iconWeight);
            icon.Position = smoothIconPos;
        }

        protected virtual void startUsing()
        {
            //weaponModel.ModelMatrix = Matrix4.Mult(spawOffset, Matrix4.Invert(parent.viewInfo.modelviewMatrix));

            weaponModel.Position = GenericMethods.Mult(new Vector4(0.3f, -0.7f, -0.7f, 1f), Matrix4.Invert(parent.viewInfo.modelviewMatrix)).Xyz;
            weaponModel.Orientation = Matrix4.Mult(weaponRMat, GenericMethods.MatrixFromVector(PointingDirection));
        }

        protected virtual void interactUp()
        {
        }

        protected virtual void interactDown()
        {
        }

        private void move()
        {
            float cameraSpeed = 10f;

            Vector3 moveVec = new Vector3();

            if (gameInput.keyboard[Key.W])
                moveVec.X = cameraSpeed;

            if (gameInput.keyboard[Key.S])
                moveVec.X = -cameraSpeed;

            if (gameInput.keyboard[Key.D])
                moveVec.Z = cameraSpeed;

            if (gameInput.keyboard[Key.A])
                moveVec.Z = -cameraSpeed;

            if (gameInput.keyboard[Key.Space])
                moveVec.Y = cameraSpeed;

            movePlayer(moveVec);
        }

        float smoothing = 0.7f;
        private bool prevE;
        private bool wasActive;

        public void rotatePlayer(float pitch, float yaw, float roll)
        {
            Vector4 tmpView = new Vector4(0, 0, -1, 1);

            parent.rawRotation.X -= pitch;
            parent.rawRotation.Y += yaw;

            parent.rotation = parent.rawRotation * (1 - smoothing) + parent.rotation * smoothing;

            Matrix4 tmpA = Matrix4.CreateRotationX(parent.rotation.X);
            Matrix4 tmpb = Matrix4.CreateRotationY(parent.rotation.Y);

            tmpView = GenericMethods.Mult(tmpA, tmpView);
            tmpView = GenericMethods.Mult(tmpb, tmpView);

            parent.PointingDirection = tmpView.Xyz;

            parent.fwdVec = parent.getFrontVec();
            parent.rightVec = parent.getRightVec();
        }

        public void movePlayer(Vector3 move)
        {
            RigidBody hitbody;
            JVector normal;
            float frac;

            bool result = Scene.world.CollisionSystem.Raycast(GenericMethods.FromOpenTKVector(Position), new JVector(0, -1f, 0),
                mRaycastCallback, out hitbody, out normal, out frac);

            //Console.WriteLine(frac);

            if (result && frac < 2.2f)
            {
                parent.Body.AddForce(GenericMethods.FromOpenTKVector(move.X * parent.fwdVec + parent.rightVec * move.Z));
                if (move.Y > 0)
                {
                    parent.Body.LinearVelocity = new JVector(parent.Body.LinearVelocity.X, 5, parent.Body.LinearVelocity.Z);
                }
            }
            else
            {
                parent.Body.AddForce(GenericMethods.FromOpenTKVector(move.X * parent.fwdVec + parent.rightVec * move.Z));
            }
        }

        protected virtual void fireUp()
        {
        }

        protected virtual void fireDown()
        {
        }

        protected virtual void rotate()
        {
            rotatePlayer(gameInput.move.Y * cameraRotSpeed, gameInput.move.X * cameraRotSpeed, 0);
        }

        /*
        internal virtual void activate()
        {
            parent.tool = this;
        }
         * */
    }
}
