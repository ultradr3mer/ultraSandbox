using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTkProject.Drawables;
using Jitter.Dynamics;
using Jitter.LinearMath;
using OpenTkProject.Drawables.Models;

namespace OpenTkProject.Game.Tools
{
    public class Spawner : Tool
    {
        private Model ghost, grid;

        private Template template;

        int tempId = 0;

        public Spawner(Player player, GameInput gameInput) : base(player, gameInput)
        {
            template = gameWindow.templateLoader.getTemplate(tempId);
            generateGhost();

            generateGrid();

            icon.setMaterial("hud\\spawner_icon.xmf");
        }

        private void generateGrid()
        {
            grid = new Model(this);
            grid.setMaterial("grid.xmf");
            grid.setMesh("grid.obj");

            grid.Color = new Vector4(0.8f, 0.3f, 0.8f, 1.0f) * 0.3f;

            grid.Position = Position;
            grid.Scene = Scene;
        }

        private void generateGhost()
        {
            if (ghost != null)
            {
                ghost.kill();
            }

            ghost = new GhostModel(this);

            ghost.Materials = template.materials;
            ghost.Meshes = template.meshes;
            ghost.Position = ghost.Position;

            ghost.Position = Position;
            ghost.selected = 1;
            ghost.selectedSmooth = 0;

            ghost.Scene = Scene;
        }

        protected override void startUsing()
        {
            base.startUsing();
            ghost.selectedSmooth = 0;
        }

        public override void update()
        {
            base.update();

            if (Parent.tool == this)
            {
                ghost.isVisible = true;
                grid.isVisible = true;

                RigidBody body; JVector normal; float frac;

                bool result = Scene.world.CollisionSystem.Raycast(GenericMethods.FromOpenTKVector(Position), GenericMethods.FromOpenTKVector(PointingDirection),
                    raycastCallback, out body, out normal, out frac);

                Vector3 hitCoords = Position + PointingDirection * frac;

                if (result && ghost != null)
                {
                    float smoothness = 0.9f;

                    //Matrix4 newOri = Matrix4.Mult(Matrix4.CreateRotationX((float)Math.PI / 2), Conversion.MatrixFromVector(normal));
                    Matrix4 newOri = GenericMethods.MatrixFromVector(normal);
                    Vector3 newPos = hitCoords + GenericMethods.ToOpenTKVector(normal) * template.positionOffset;

                    grid.Position = smoothness * grid.Position + (1 - smoothness) * hitCoords;
                    grid.Orientation = GenericMethods.BlendMatrix(grid.Orientation, newOri, smoothness);

                    ghost.Position = smoothness * ghost.Position + (1 - smoothness) * newPos;

                    if (template.normal)
                        ghost.Orientation = newOri;
                }
            }
            else
            {
                ghost.isVisible = false;
                grid.isVisible = false;
            }
        }

        protected override void interactDown()
        {
            stepTemplateId();

            while ((template = gameWindow.templateLoader.getTemplate(tempId)).useType != "pmodel")
                stepTemplateId();

            generateGhost();
        }

        private void stepTemplateId()
        {
            tempId++;
            if (tempId >= gameWindow.templateLoader.templates.Count)
                tempId = 0;
        }

        protected override void fireDown()
        {
            objectFromTemplate();
        }

        private void objectFromTemplate()
        {
            PhysModel curModel = new PhysModel(Scene);

            curModel.Materials = template.materials;
            curModel.Meshes = template.meshes;
            curModel.Position = ghost.Position;
            curModel.PhysBoxes = template.pmeshes;

            curModel.IsStatic = template.isStatic;

            curModel.setName(Scene.getUniqueName());

            curModel.Orientation = ghost.Orientation;

            if (template.hasLight && Scene.lightCount < ShaderLoader.maxNoLights)
            {
                Light mLight = new LightSpot(curModel);
                mLight.Color = new Vector4(template.lightColor, 1);
            }
        }
    }
}
