using System;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Jitter;
using Jitter.Collision;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Jitter.Dynamics.Constraints;
using Jitter.Dynamics.Constraints.SingleBody;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Input;
using OpenTkProject.Game;
using System.Collections.Generic;
using OpenTkProject.Drawables.Models;
using OpenTkProject.Game.Tools;

namespace OpenTkProject
{
    public class Player : GameObject
    {
        public Vector3 rawRotation, rotation, upVector, fwdVec, rightVec;

        public ViewInfo viewInfo;

        public Tool tool;

        public List<Tool> tools = new List<Tool> { };

        protected RigidBody body;

        public Player(Scene parent, Vector3 spawnPos, Vector3 viewDir, GameInput gameInput)
        {
            Parent = parent;
            this.gameInput = gameInput;

            //create hud renderer
            hud = new Hud(this);
            parent.guis.Add(hud);

            Position = spawnPos;
            PointingDirection = viewDir;

            upVector = new Vector3(0, 1, 0);

            Shape boxShape = new BoxShape(new JVector(0.5f, 2, 0.5f));

            Body = new RigidBody(boxShape);
            Body.Position = new JVector(Position.X, Position.Y, Position.Z);
            Body.AllowDeactivation = false;

            JMatrix mMatrix = JMatrix.Identity;

            //mBody.SetMassProperties(mMatrix, 2,false);

            Jitter.Dynamics.Constraints.SingleBody.FixedAngle mConstraint = new Jitter.Dynamics.Constraints.SingleBody.FixedAngle(Body);

            parent.world.AddConstraint(mConstraint);
            parent.world.AddBody(Body);

            viewInfo = new ViewInfo(this);
            viewInfo.aspect = (float)gameWindow.Width / (float)gameWindow.Height;
            viewInfo.updateProjectionMatrix();

            tools.Add(new GameMenu(this, gameInput));
            tools.Add(new Spawner(this, gameInput));
            tools.Add(new TerrainGun(this, gameInput));
            tools.Add(new Grabber(this, gameInput));
            tools.Add(new Remover(this, gameInput));

            tool = tools[1];
        }

        public void setInput(GameInput gameInput)
        {
            /*
            tool.gameInput = gameInput;
            foreach (var curTool in tools)
            {
                curTool.gameInput = gameInput;
            }
             */
            this.gameInput = gameInput;
        }
        /*
        public Matrix4 createProjectrionMatrix()
        {
            return viewInfo.generateProjectionMatrix();
        }

        public Matrix4 createLookAtMatrix()
        {
            return viewInfo.generateModelViewMatrix();
        }
        */
        public Vector3 getFrontVec() 
        {
            return Vector3.Normalize(new Vector3(PointingDirection.X, 0, PointingDirection.Z)); 
        }

        public Vector3 getRightVec() 
        {
            return Vector3.Normalize(Vector3.Cross(Vector3.Normalize(PointingDirection), upVector)); 
        }

        #region actions

        private GameInput gameInput;
        public Hud hud;

        private bool mRaycastCallback(RigidBody hitbody, JVector normal, float frac)
        {
            return (hitbody != Body);
        }

        #endregion actions

        public override void update()
        {                            
            Position = GenericMethods.ToOpenTKVector(Body.Position);
            Position += new Vector3(0, 1, 0);

            if(Settings.Instance.game.debugMode)
                hud.fpsCounter.setValue((float)gameWindow.smoothframerate);

            if (gameWindow.state == GameState.Playing)
            {
                if (gameInput.keyboard[Key.Number1])
                {
                    if (tools.Count > 1)
                        tool = tools[1];
                }

                if (gameInput.keyboard[Key.Number2])
                {
                    if (tools.Count > 2)
                        tool = tools[2];
                }

                if (gameInput.keyboard[Key.Number3])
                {
                    if (tools.Count > 3)
                        tool = tools[3];
                }

                if (gameInput.keyboard[Key.Number4])
                {
                    if (tools.Count > 4)
                        tool = tools[4];
                }

                if (gameInput.keyboard[Key.Number5])
                {
                    if(tools.Count > 5)
                        tool = tools[5];
                }
            }

            updateChilds();

            Scene.eyePos = position;
        }


        public override RigidBody Body { get { return body; } set { body = value; forceUpdate(); } }
    }

}
