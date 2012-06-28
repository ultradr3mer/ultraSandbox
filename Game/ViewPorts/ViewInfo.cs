using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision;
using OpenTkProject.Drawables;

namespace OpenTkProject
{
    public class ViewInfo:GameObject
    {
        public Matrix4 modelviewMatrix = Matrix4.Identity;
        public Matrix4 projectionMatrix = Matrix4.Identity;

        public Vector3 upVec = new Vector3(0,1,0);

        public float zNear = 0.1f;
        public float zFar = 100f;

        public float focus = 10;

        public float fovy = (float)Math.PI / 4;
        public float aspect = 1;

        new public Vector3 pointingDirection;

        //public FrustrumCube frustrumCube;
        public Matrix4 modelviewProjectionMatrix = Matrix4.Identity;
        public Vector3 pointingDirectionRight;
        public Vector3 pointingDirectionUp;
        public bool sunPerspective;
        public Matrix4 invModelviewProjectionMatrix;
        public Matrix4 invModelviewMatrix;
        //static ViewInfo Zero = new ViewInfo();

        new public Vector3 position;

        public ViewInfo(GameObject parent)
        {
            Parent = parent;
            updateProjectionMatrix();
            //frustrumCube = new FrustrumCube(zFar, zNear, this);
        }

        public ViewInfo()
        {
        }

        public Matrix4 updateProjectionMatrix()
        {
            return projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fovy, aspect, zNear, zFar);
            //invProjectionMatrix = Matrix4.Invert(projectionMatrix);
        }

        private Matrix4 generateModelViewMatrix()
        {
            modelviewMatrix = Matrix4.LookAt(Position, Position + PointingDirection, upVec);
            invModelviewMatrix = Matrix4.Invert(modelviewMatrix);
            return modelviewMatrix;
        }

        public float getFocus()
        {
            RaycastCallback raycast = new RaycastCallback(RaycastCallback); RigidBody body; JVector normal; float frac;
            bool result = Scene.world.CollisionSystem.Raycast(GenericMethods.FromOpenTKVector(Position), GenericMethods.FromOpenTKVector(PointingDirection),
                raycast, out body, out normal, out frac);
            if (result)
            {
                return frac;
            }
            else
            {
                return zFar;
            }
        }

        public override Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        public float getFocus(float smoothing)
        {
            return focus = getFocus() * (1 - smoothing) + focus * smoothing;
        }

        private bool RaycastCallback(RigidBody body, JVector normal, float frac)
        {
            return (body != Parent.Body);
        }

        public virtual bool frustrumCheck(Drawable drawable)
        {
            Vector4 vSpacePos = GenericMethods.Mult(new Vector4(drawable.Position, 1), modelviewProjectionMatrix);

            float range = drawable.boundingSphere;

            float distToDrawAble = (position - drawable.Position).Length;

            if (distToDrawAble < range * 1.5f)
                return true;

            if (distToDrawAble - range > zFar)
                return false;

            if(vSpacePos.W <= 0)
                return false;

            range /= vSpacePos.W * 0.6f;

            if (float.IsNaN(range) || float.IsInfinity(range))
                return false;

            vSpacePos /= vSpacePos.W;
 
            return (
                vSpacePos.X < (1f + range) && vSpacePos.X > -(1f + range) &&
                vSpacePos.Y < (1f + range * aspect) && vSpacePos.Y > -(1f + range * aspect)
                );
        }

        public override void update()
        {
            if (Parent.PointingDirection != Vector3.Zero && Parent.PointingDirection != PointingDirection)
            {
                PointingDirection = Parent.PointingDirection;
                wasUpdated = true;
            }
            if (Parent.Position != Vector3.Zero && Parent.Position != position)
            {
                Position = Parent.Position;
                wasUpdated = true;
            }
            if (wasUpdated)
            {
                generateModelViewMatrix();
                generateViewProjectionMatrix();
                calculateVectors();

                updateChilds();
            }
        }

        public override Vector3 PointingDirection
        {
            get
            {
                return Vector3.Normalize(pointingDirection);
            }
            set
            {
                pointingDirection = value*zFar;
            }
        }

        private void calculateVectors()
        {
            Vector4 bottomLeft = new Vector4(-1, -1, 1, 1);
            bottomLeft = GenericMethods.Mult(bottomLeft, invModelviewProjectionMatrix);
            bottomLeft /= bottomLeft.W;

            Vector4 bottomRight = new Vector4(1, -1, 1, 1);
            bottomRight = GenericMethods.Mult(bottomRight, invModelviewProjectionMatrix);
            bottomRight /= bottomRight.W;

            Vector4 topLeft = new Vector4(-1, 1, 1, 1);
            topLeft = GenericMethods.Mult(topLeft, invModelviewProjectionMatrix);
            topLeft /= topLeft.W;

            pointingDirectionUp = topLeft.Xyz - bottomLeft.Xyz;
            pointingDirectionRight = bottomRight.Xyz - bottomLeft.Xyz;
        }

        public void generateViewProjectionMatrix()
        {
            modelviewProjectionMatrix = Matrix4.Mult(modelviewMatrix, projectionMatrix);
            invModelviewProjectionMatrix = Matrix4.Invert(modelviewProjectionMatrix);
        }
        /*
        internal void checkForUpdates()
        {
            throw new NotImplementedException();
        }
        */
        internal bool checkForUpdates(List<Drawable> drawables)
        {
            foreach (var drawable in drawables)
            {
                if (drawable.wasUpdated && frustrumCheck(drawable))
                {
                    wasUpdated = true;
                    break;
                }
            }
            return wasUpdated;
        }
    }
    /*
    public class FrustrumCube : GameObject
    {
        Vector3 farTopLeft;
        Vector3 farTopRight;

        Vector3 farBottomLeft;
        Vector3 farBottomRight;

        Vector3 nearTopLeft;
        Vector3 nearTopRight;

        Vector3 nearBottomLeft;
        Vector3 nearBottomRight;

        Vector3 center;

        Plane far;
        Plane near;
        Plane left;
        Plane right;
        Plane top;
        Plane bottom;

        Model marker;

        private float zFar;
        private float zNear;
        private Model marker2;
        private Model marker3;
        private Model grid;

        public new ViewInfo parent;

        public Vector3 pointingDirectionRight;
        public Vector3 pointingDirectionUp;
        private Model marker4;

        public FrustrumCube(float zFar, float zNear, ViewInfo parent)
        {
            setParent(parent);
            this.parent = parent;

            this.zFar = zFar;
            this.zNear = zNear;



        }

        public Model prepareMarker()
        {
            Model grid = new Model(this);
            grid.addMaterial("marker.xmf");
            grid.addMesh("sprite_plane.obj");

            grid.color = new Vector4(0.8f, 0.3f, 0.8f, 1.0f);

            grid.Position = Position;
            grid.addToScene(scene);
            grid.Size = Vector3.One * 0.2f;

            grid.Orientation = Matrix4.Identity;

            return grid;
        }

        public Model prepareGrid()
        {
            Model grid = new Model(this);
            grid.addMaterial("grid.xmf");
            grid.addMesh("grid.obj");

            grid.color = new Vector4(0.8f, 0.3f, 0.8f, 1.0f);

            grid.Position = Position;
            grid.addToScene(scene);
            grid.Size = Vector3.One * 1f;

            grid.Orientation = Matrix4.Identity;

            return grid;
        }

        public void generatePlanes()
        {
            far = new Plane(farTopLeft, farTopRight, farBottomLeft, center);
            near = new Plane(nearTopLeft, nearTopRight, nearBottomLeft, center);

            left = new Plane(farTopLeft, nearTopLeft, farBottomLeft, center);
            right = new Plane(farTopRight, nearTopRight, farBottomRight, center);

            top = new Plane(farTopLeft, nearTopLeft, farTopRight, center);
            bottom = new Plane(farBottomLeft, nearBottomLeft, farBottomRight, center);


        }

        public bool check(Vector3 vec)
        {
            return (near.check(vec) && far.check(vec) && left.check(vec) && right.check(vec) && top.check(vec) && bottom.check(vec));
        }

        internal bool check(Vector3 vec, float range)
        {
            return (near.check(vec, range) && far.check(vec, range) && left.check(vec, range) && right.check(vec, range) && top.check(vec, range) && bottom.check(vec, range));
        }

        public override void update()
        {
            position = parent.Position;
            pointingDirection = parent.pointingDirection;

            pointingDirectionRight = Vector3.Normalize(Vector3.Cross(parent.upVec, pointingDirection));
            pointingDirectionUp = Vector3.Normalize(Vector3.Cross(pointingDirectionRight, pointingDirection));

            generateWorldCoords();
            generatePlanes();

            updateChilds();
        }

        public void generateWorldCoords()
        {
            float scaling = (float)Math.Tan(parent.fovy*0.5);

            Vector2 nearSize = Vector2.One * zNear * scaling;
            nearSize.X *= parent.aspect;

            Vector2 farSize = Vector2.One * zFar * scaling;
            farSize.X *= parent.aspect;

            nearTopLeft = Position + zNear * pointingDirection + nearSize.X * pointingDirectionRight + nearSize.Y * pointingDirectionUp;
            nearTopRight = Position + zNear * pointingDirection - nearSize.X * pointingDirectionRight + nearSize.Y * pointingDirectionUp;

            nearBottomLeft = Position + zNear * pointingDirection + nearSize.X * pointingDirectionRight - nearSize.Y * pointingDirectionUp;
            nearBottomRight = Position + zNear * pointingDirection - nearSize.X * pointingDirectionRight - nearSize.Y * pointingDirectionUp;

            farTopLeft = Position + zFar * pointingDirection + farSize.X * pointingDirectionRight + farSize.Y * pointingDirectionUp;
            farTopRight = Position + zFar * pointingDirection - farSize.X * pointingDirectionRight + farSize.Y * pointingDirectionUp;

            farBottomLeft = Position + zFar * pointingDirection + farSize.X * pointingDirectionRight - farSize.Y * pointingDirectionUp;
            farBottomRight = Position + zFar * pointingDirection - farSize.X * pointingDirectionRight - farSize.Y * pointingDirectionUp;

            center = Position + (zFar + zNear) * 0.5f * pointingDirection;
        }
    }
*/
}
