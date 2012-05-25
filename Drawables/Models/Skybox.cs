using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTkProject.Drawables.Models
{
    class Skybox:Model
    {
        public Skybox(Scene mScene, OpenTkProjectWindow mGameWindow) : base(mScene)
        {
            Parent = mScene;
            this.Scene = mScene;
            mScene.drawables.Add(this);

            //setProgram("skybox/sky.fs");
            //mSkyModel.setMesh(mMeshLoader.loadObj("skybox/sky_1.obj"));
            //mSkyModel.setTextures(mTextureLoader.LoadTextures(new string[] { "skybox/sky_0001.png" }));

            for (int i = 0; i < 5; i++)
            {
                int sidetex = mGameWindow.textureLoader.getTexture("skybox\\sky_000" + (i + 1) + ".png");

                addMaterial("skybox\\sky" + (i + 1) + ".xmf");
                addMesh("skybox\\sky_" + (i + 1) + ".obj");
                //envTextures[i] = sidetex;
                //addTextures(new string[] { "skybox\\sky_000" + (i + 1) + ".png" });
            }

            float skyScale = mGameWindow.player.zFar / (float)Math.Sqrt(3);

            Size = new Vector3(skyScale, skyScale, skyScale);
            //updateModelMatrix();
        }

        public override void save(ref StringBuilder sb, int level)
        {
            saveChilds(ref sb, level);
        }

        public override void update()
        {
            Position = Scene.eyePos;

            updateChilds();
        }

        protected override void setSpecialUniforms(Shader shader)
        {
            Vector3 sunColor = Scene.sunLight.Color.Xyz; ;
            GL.Uniform3(shader.lightSunLocation, ref sunColor);
        }

        public override void drawNormal(ViewInfo curView)
        {
        }

        public override void drawSelection(ViewInfo curView)
        {
        }
    }
}
