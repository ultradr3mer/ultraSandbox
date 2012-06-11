using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTkProject.Drawables.Models;
using OpenTK;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace OpenTkProject.Game.Tools
{
    class TerrainGun :Tool
    {
        GhostModel ghost;
        Template[] voulmeTemplates = new Template[2];
        private Template template;
        private int tempId = 0;

        public TerrainGun(Player player, GameInput gameInput)
            : base(player, gameInput)
        {
            icon.setMaterial("hud\\terrain_icon.xmf");

            ensureType();

            generateGhost();
        }

        private void generateGhost()
        {
            if (ghost != null)
                ghost.kill();

            ghost = new GhostModel(this);

            ghost.Materials = template.materials;

            ghost.Meshes = template.meshes;


            ghost.Position = Position;
            ghost.selected = 1;
            ghost.selectedSmooth = 0;

            ghost.Scene = Scene;
        }

        protected override void fireDown()
        {
            MetaModel curModel = new MetaModel(Scene);

            curModel.Materials = template.materials;
            curModel.Meshes = template.meshes;
            curModel.PhysBoxes = template.pmeshes;
            curModel.volume.AffectionRadius = template.volumeRadius;

            curModel.Position = ghost.Position;
          
            curModel.setName(Scene.getUniqueName());
        }

        protected override void interactDown()
        {
            stepTemplateId();

            ensureType();

            generateGhost();
        }

        private void ensureType()
        {
            while ((template = gameWindow.templateLoader.getTemplate(tempId)).useType != Template.UseType.Meta)
                stepTemplateId();
        }

        private void stepTemplateId()
        {
            tempId++;
            if (tempId >= gameWindow.templateLoader.templates.Count)
                tempId = 0;
        }

        public override void update()
        {
            base.update();

            if (Parent.tool == this)
            {
                RigidBody body; JVector normal; float frac;

                bool result = Scene.world.CollisionSystem.Raycast(GenericMethods.FromOpenTKVector(Position), GenericMethods.FromOpenTKVector(PointingDirection),
                    raycastCallback, out body, out normal, out frac);

                Vector3 hitCoords = Position + PointingDirection * frac;

                if (result && ghost != null)
                {
                    float smoothness = 0.9f;

                    //Matrix4 newOri = Matrix4.Mult(Matrix4.CreateRotationX((float)Math.PI / 2), Conversion.MatrixFromVector(normal));
                    Matrix4 newOri = GenericMethods.MatrixFromVector(normal);
                    Vector3 newPos = hitCoords;

                    ghost.Position = smoothness * ghost.Position + (1 - smoothness) * newPos;
                    //ghost.updateModelMatrix();
                }

                ghost.isVisible = true;
            }
            else
            {
                ghost.isVisible = false;
            }
        }
    }
}
