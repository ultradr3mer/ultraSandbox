using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Jitter.LinearMath;
using System.IO;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Jitter.Dynamics;
using Jitter.Collision.Shapes;
using System.Collections;
using OpenTkProject.Drawables;
using Jitter;
using Jitter.Collision;
using OpenTkProject.Game.Voxel;
using OpenTkProject.Drawables.Models;
using OpenTkProject.Drawables.Models.Paticles;
using System.Xml;

namespace OpenTkProject
{

    public class Scene:Drawable
    {
        //public OpenTkProjectWindow mGameWindow; 

        string dataSource = "WorldObjects.db";
        //List<GameObject> WorldObjects = new List<Model> { };
        public List<Drawable> drawables = new List<Drawable> { };
        public List<Gui> guis = new List<Gui> { };
        public List<LightSpot> spotlights = new List<LightSpot> { };

        public LightSun sunLight;
        public Framebuffer sunFrameBuffer;
        public Framebuffer sunInnerFrameBuffer;

        public World world;

        public Matrix4 projectionMatrix, modelviewMatrix;

        /*
        public Model 
            BloomViewboard, 
            BloomViewboard2, 
            CompositeViewboard, 
            AoViewboard, 
            AoBlrViewboard;
         */

        public Scene(OpenTkProjectWindow mGameWindow)
        {
            this.gameWindow = mGameWindow;
            Scene = this;

            sunLight = new LightSun(new Vector3(0.1f, 0.125f, 0.2f) * 3f, this);
            sunLight.lightAmbient = new Vector3(0.1f, 0.125f, 0.2f) * 0.5f;//new Vector3(0.2f, 0.125f, 0.1f);//new Vector3(0.1f, 0.14f, 0.3f);
            sunLight.PointingDirection = Vector3.Normalize(new Vector3(674, -674, 1024));
            sunFrameBuffer = gameWindow.framebufferCreator.createFrameBuffer("shadowFramebuffer", shadowRes * 2, shadowRes * 2, PixelInternalFormat.Rgba8, false);
            sunInnerFrameBuffer = gameWindow.framebufferCreator.createFrameBuffer("shadowFramebuffer", shadowRes * 2, shadowRes * 2, PixelInternalFormat.Rgba8, false);

            //sunLight.pointingDirection = Vector3.Normalize(new Vector3(674, 674, 1024));

            // creating a new collision system and adding it to the new world
            CollisionSystem collisionSystem = new CollisionSystemSAP();
            world = new World(collisionSystem);

            // Create the groundShape and the body.
            Shape groundShape = new BoxShape(new JVector(100, waterLevel * 2, 100));
            RigidBody groundBody = new RigidBody(groundShape);


            // make the body static, so it can't be moved
            groundBody.IsStatic = true;

            // add the ground to the world.
            world.AddBody(groundBody);
        }

        #region database

        public void saveObjects()
        {
            /*
            SQLiteConnection connection = new SQLiteConnection();

            connection.ConnectionString = "Data Source=" + dataSource;
            connection.Open();

            SQLiteCommand command = new SQLiteCommand(connection);

            //preparing tables for insertion

            // delete table
            command.CommandText = "DROP TABLE IF EXISTS WorldObjects;";
            command.ExecuteNonQuery();

            // recreate table
            command.CommandText = "CREATE TABLE IF NOT EXISTS WorldObjects ( id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                "name VARCHAR(100), position TEXT, rotation TEXT, material TEXT, meshes TEXT, pboxes TEXT, static INTEGER);";
            command.ExecuteNonQuery();

            // delete table
            command.CommandText = "DROP TABLE IF EXISTS WorldLights;";
            command.ExecuteNonQuery();

            // recreate table
            command.CommandText = "CREATE TABLE IF NOT EXISTS WorldLights ( id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                "name VARCHAR(100), position TEXT, direction TEXT, color TEXT, parent TEXT);";
            command.ExecuteNonQuery();



            command.CommandText = sw.ToString();
            command.ExecuteNonQuery();

            // Freigabe der Ressourcen.
            command.Dispose();

            connection.Close();
            connection.Dispose();
             */

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<scene>");
            //call tree to create string for saving
            saveChilds(ref sb, 1);

            sb.AppendLine("</scene>");

            using (StreamWriter outfile = new StreamWriter("scene.xsn"))
            {
                outfile.Write(sb.ToString());
            }
        }

        public void loadObjects()
        {
            XmlTextReader reader = new XmlTextReader("scene.xsn");

            reader.Read();

            load(ref reader,"scene");
        }

        #endregion database

        public Model getModelbyName(string name)
        {
            int identifier = (int)childNames[name];
            return (Model)childs[identifier];
        }
        
        Skybox mSkyModel;
        public float waterLevel = 0.3f;
        public int[] envTextures = new int[6];
        public Matrix4 mWaterMatrix;

        public void init()
        {

            mFilter2d = new Quad2d(this);

            mSkyModel = new Skybox(this, gameWindow);

            mGroundPlane = new GroundPlane(this);
            mGroundPlane.setMaterial("floor.xmf");

            mGroundPlane.setMesh("water_plane.obj");
            mGroundPlane.setMaterial("floor.xmf");

            //need to be fixed -- cant be executed after voxel Manager creation.
            generateParticleSys();

            voxelManager = new VoxelManager(this);

            particleAffectors.Add(new ParticleAffectorWind(new Vector3(1,-0.5f,0) * 0.01f));
            particleAffectors.Add(new ParticleAffectorFriction(0.1f));
            particleAffectors.Add(new ParticleAffectorFloorKiller(waterLevel));
            particleAffectors.Add(new ParticleAffectorLifeTimeKiller(this));

            /*
                waterModel.setTextures(mTextureLoader.fromMixed(new int[]{
                TextureGroup.TYPE_FRAMEBUFFER,
                TextureGroup.TYPE_FROMFILE},
                new string[] {
                    Convert.ToString(waterFramebuffer.ColorTexture), 
                    "noise.png"}));
             */

            mGroundPlane.Position = new Vector3(0,waterLevel,0);
            //mGroundPlane.updateModelMatrix();

            Matrix4 translate = Matrix4.CreateTranslation(0, -waterLevel * 2, 0);
            Matrix4 invert = Matrix4.Scale(1, -1, 1);
            Matrix4.Mult(ref translate, ref invert, out mWaterMatrix);
        }

        public void generateParticleSys()
        {
            ParticleSystem pSys = new ParticleSystem(scene);
            pSys.addMaterial("particles\\particle_a.xmf");
            pSys.addMesh("sprite_plane.obj");

            pSys.addMaterial("particles\\particle_b.xmf");
            pSys.addMesh("sprite_plane.obj");

            pSys.Color = new Vector4(0.8f, 0.3f, 0.8f, 1.0f);

            pSys.Position = scene.Position;
            pSys.Parent = scene;
            pSys.Size = OpenTK.Vector3.One * 1f;

            pSys.Orientation = Matrix4.Identity;

            pSys.generateParticles(1000);
        }

        public static int NULLPASS = 0;
        public static int WATERPASS = 1;
        public static int SELECTIONPASS = 2;
        public static int NORMALPASS = 3;
        public static int TRANSPARENTPASS = 4;
        public static int SHADOWPASS = 5;
        public Shader ssaoShader;
        public Shader ssaoBlrShader;
        public Shader bloomShader;
        public Shader compositeShader;
        public Quad2d mFilter2d;
        public int noiseTexture;
        public OpenTK.Vector3 eyePos;
        private GroundPlane mGroundPlane;
        public Shader dofpreShader;
        public Shader dofShader;
        public Shader ssaoBlendShader;
        public int[] backdropTextures;
        public Shader copycatShader;
        public int currentLight;
        public int lightCount;
        private int shadowRes = 512;
        public Shader ssaoMergeShader;
        public Shader ssaoBlrShaderA;
        public Shader wipingShader;
        public Shader ssNormalShader;
        public Shader ssNormalShaderNoTex;
        public VoxelManager voxelManager;
        public List<ParticleAffector> particleAffectors = new List<ParticleAffector> { };
        public int ClippingTexture;
        public Shader bloomCurveShader;
        public Shader selectionShaderAni;
        public Shader ssNormalShaderAni;

        public override void update()
        {
            gameWindow.checkGlError("--uncaught ERROR entering update--");

            //call physicengine to step
            world.Step((float)1.0f / 60.0f, true);

            //update scene tree
            updateChilds();

            //update shadowmap
            lightCount = spotlights.Count;
            int fbTargetRes = (int)gameWindow.shadowFramebuffer.Size.X / shadowRes;

            if (fbTargetRes != lightCount)
            {
                Console.WriteLine("updating Shadow Framebuffer: " + fbTargetRes + "->" + lightCount + " lights");
                gameWindow.shadowFramebuffer = gameWindow.framebufferCreator.createFrameBuffer("shadowFramebuffer", shadowRes * lightCount, shadowRes, PixelInternalFormat.Rgba16f, false);
                foreach (var light in spotlights)
                {
                    light.viewInfo.wasUpdated = true;
                }
            }

            //sort drawables by distance
            foreach (var drawable in drawables)
            {
                Vector3 vectorTobObject = drawable.Position - gameWindow.player.Position;
                drawable.distToCamera = vectorTobObject.Length;
            }
            drawables.Sort(CompareByDistance);

            gameWindow.checkGlError("--uncaught ERROR leaving update--");
        }

        public static int CompareByDistance(Drawable drawableA, Drawable drawableB)
        {
            return drawableA.distToCamera.CompareTo(drawableB.distToCamera);
        }

        #region draw

        public bool drawSceene(int currentPass, ViewInfo curView)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (currentPass == NULLPASS)
            {
                GL.Disable(EnableCap.Blend);
                foreach (Drawable curDrawable in drawables)
                {
                    curDrawable.draw(curView,false);
                }
            }

            if (currentPass == TRANSPARENTPASS)
            {
                GL.Enable(EnableCap.Blend);
                
                for (int i = drawables.Count - 1; i >= 0; i--)
                {
                    drawables[i].draw(curView, true);
                }
            }

            if (currentPass == SHADOWPASS)
            {
                foreach (Drawable curDrawable in drawables)
                {
                    curDrawable.drawShadow(curView);
                }
            }

            if (currentPass == SELECTIONPASS)
            {
                bool hasSelection = false;
                foreach (Drawable curDrawable in drawables)
                {
                    if (curDrawable.selectedSmooth > 0.01)
                    {
                        curDrawable.drawSelection(curView);
                        hasSelection = true;
                    }
                }
                return hasSelection;
            }

            if (currentPass == NORMALPASS)
            {
                GL.Disable(EnableCap.Blend);
                Shader SSNormalShader = ssNormalShader;

                mGroundPlane.draw(curView,ssNormalShaderNoTex);

                foreach (Drawable curDrawable in drawables)
                {
                    curDrawable.drawNormal(curView);
                }
            }

            GL.DepthMask(true);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            return false;
        }

        internal void draw(FramebufferSet curFramebuffers, ViewInfo curView)
        {
            gameWindow.checkGlError("--uncaught ERROR entering Scene--");

            RenderOptions renderOptions = curFramebuffers.renderOptions;

            if (renderOptions.postProcessing)
            {
                GL.Enable(EnableCap.Blend);

                curFramebuffers.sceeneFramebuffer.enable(true);
                curFramebuffers.sceeneFramebuffer.Multisampeling = false;

                GL.Enable(EnableCap.Blend);

                drawSceene(Scene.NORMALPASS, curView);

                GL.Disable(EnableCap.Blend);

                if (renderOptions.ssAmbientOccluison)
                {
                    curFramebuffers.aoFramebuffer.enable(false);
                    mFilter2d.draw(ssaoShader, new int[] { curFramebuffers.sceeneFramebuffer.ColorTexture, noiseTexture });

                    GL.Disable(EnableCap.Blend);
                    curFramebuffers.aoBlurFramebuffer.enable(false);
                    mFilter2d.draw(ssaoBlrShaderA, new int[] { curFramebuffers.aoFramebuffer.ColorTexture, curFramebuffers.aoBlurFramebuffer2.ColorTexture});

                    curFramebuffers.aoBlurFramebuffer2.enable(false);
                    mFilter2d.draw(ssaoBlrShader, new int[] { curFramebuffers.aoBlurFramebuffer.ColorTexture});
                }
                
                curFramebuffers.sceeneFramebuffer.enable(false);

                GL.Disable(EnableCap.Blend);

                drawSceene(Scene.NULLPASS, curView);

                GL.Enable(EnableCap.Blend);

                // copy scene to transparent fb -- we can do lookups
                curFramebuffers.sceeneFramebufferTrans.enable(true);
                mFilter2d.draw(copycatShader, new int[] { curFramebuffers.sceeneFramebuffer.ColorTexture });

                // switch back to scene fb
                curFramebuffers.sceeneFramebuffer.enable(false);

                backdropTextures = new int[] { 
                    curFramebuffers.sceeneFramebufferTrans.ColorTexture, 
                    curFramebuffers.sceeneFramebufferTrans.DepthTexture };

                if (renderOptions.ssAmbientOccluison)
                {
                    mFilter2d.draw(ssaoBlendShader, new int[] { curFramebuffers.aoBlurFramebuffer2.ColorTexture, curFramebuffers.sceeneFramebufferTrans.ColorTexture });
                }

                drawSceene(Scene.TRANSPARENTPASS, curView);

                curFramebuffers.selectionFb.enable(true);

                bool hasSelection = drawSceene(Scene.SELECTIONPASS, curView);

                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);

                Vector2 bloomSize = new Vector2(80, 20);

                if (hasSelection)
                {
                    curFramebuffers.selectionblurFb.enable(false);
                    mFilter2d.draw(bloomShader, new int[] { curFramebuffers.selectionFb.ColorTexture }, bloomSize);
                }

                curFramebuffers.selectionblurFb2.enable(true);

                if (hasSelection)
                    mFilter2d.draw(bloomShader, new int[] { curFramebuffers.selectionblurFb.ColorTexture }, bloomSize*0.2f);


                curFramebuffers.bloomFramebuffer2.Multisampeling = false;
                if (renderOptions.bloom)
                {
                    curFramebuffers.bloomFramebuffer2.enable(false);
                    mFilter2d.draw(bloomCurveShader, new int[] { curFramebuffers.sceeneFramebuffer.ColorTexture });

                    for (int i = 0; i < 2; i++)
                    {
                        curFramebuffers.bloomFramebuffer.enable(false);
                        mFilter2d.draw(bloomShader, new int[] { curFramebuffers.bloomFramebuffer2.ColorTexture }, bloomSize);

                        curFramebuffers.bloomFramebuffer2.enable(false);
                        mFilter2d.draw(bloomShader, new int[] { curFramebuffers.bloomFramebuffer.ColorTexture }, bloomSize * 0.2f);

                        bloomSize *= 2f;
                    }
                }
                curFramebuffers.bloomFramebuffer2.Multisampeling = true;

                if (renderOptions.depthOfField)
                {
                    curFramebuffers.dofPreFramebuffer.enable(false);
                    mFilter2d.draw(dofpreShader, new int[] { 
                        curFramebuffers.screenNormalFb.ColorTexture, 
                        curFramebuffers.sceeneFramebufferTrans.ColorTexture
                    }, new Vector2(curView.getFocus(0.9f), 0.01f));

                    curFramebuffers.dofFramebuffer.enable(false);
                    mFilter2d.draw(dofShader, new int[] { curFramebuffers.dofPreFramebuffer.ColorTexture, noiseTexture });

                    curFramebuffers.dofFramebuffer2.enable(false);
                    mFilter2d.draw(dofShader, new int[] { curFramebuffers.dofFramebuffer.ColorTexture, noiseTexture });
                }

                curFramebuffers.outputFb.enable(false);
                curFramebuffers.sceeneFramebuffer.Multisampeling = true;

                mFilter2d.draw(compositeShader, new int[] { 
                curFramebuffers.sceeneFramebuffer.ColorTexture,
                curFramebuffers.bloomFramebuffer2.ColorTexture,
                curFramebuffers.selectionFb.ColorTexture,
                curFramebuffers.selectionblurFb2.ColorTexture,
                curFramebuffers.dofFramebuffer2.ColorTexture,
                curFramebuffers.aoBlurFramebuffer2.ColorTexture});

                gameWindow.checkGlError("--uncaught ERROR leaving Scene--");
            }
            else
            {
                gameWindow.checkGlError("--uncaught ERROR entering Scene--");
                curFramebuffers.outputFb.enable(true);

                GL.Enable(EnableCap.Blend);
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);

                drawSceene(Scene.NULLPASS, curView);

                // copy scene to transparent fb -- we can do lookups
                curFramebuffers.sceeneFramebufferTrans.enable(true);
                mFilter2d.draw(copycatShader, new int[] { curFramebuffers.outputFb.ColorTexture });

                // switch back to scene fb
                curFramebuffers.outputFb.enable(false);

                backdropTextures = new int[] { 
                    curFramebuffers.sceeneFramebufferTrans.ColorTexture, 
                    curFramebuffers.sceeneFramebufferTrans.DepthTexture };

                drawSceene(Scene.TRANSPARENTPASS, curView);

                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);

                gameWindow.checkGlError("--uncaught ERROR leaving Scene--");
            }
        }

        internal void drawShadowBuffers(Framebuffer shadowFb)
        {
            shadowFb.enable(false);

            for (int i = 0; i < spotlights.Count; i++)
            {
                bool needsUpdate = spotlights[i].viewInfo.checkForUpdates(drawables);
                currentLight = i;

                if (needsUpdate)
                {
                    Console.WriteLine(i + " -> updating Shadowbuffer");

                    GL.Disable(EnableCap.Blend);
                    GL.Enable(EnableCap.DepthTest);
                    GL.Disable(EnableCap.CullFace);
                    //GL.CullFace(CullFaceMode.Front);

                    GL.DepthFunc(DepthFunction.Always);
                    mFilter2d.draw(wipingShader, new int[] { spotlights[i].ProjectionTexture },new Vector2(i,lightCount));
                    GL.DepthFunc(DepthFunction.Less);

                    GL.ColorMask(false, false, false, true);
                    drawSceene(Scene.SHADOWPASS, spotlights[i].viewInfo);
                    GL.ColorMask(true, true, true, true);
                }
            }

            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Front);

            //"disable" writing to certain uv space
            int tmpLightCount = lightCount;
            lightCount = 1;
            currentLight = 0;

            if (sunLight.viewInfo.wasUpdated)
            {
                //generating wide range sun shadows
                sunFrameBuffer.enable(true);

                GL.ColorMask(false, false, false, false);
                drawSceene(Scene.SHADOWPASS, sunLight.viewInfo);
                GL.ColorMask(true, true, true, true);
            }

            
            //generating short range sun shadows
            sunInnerFrameBuffer.enable(true);

            GL.ColorMask(false, false, false, false);
            drawSceene(Scene.SHADOWPASS, sunLight.innerViewInfo);
            GL.ColorMask(true, true, true, true);


            //clean up
            lightCount = tmpLightCount;
        }

        #endregion draw

        internal string getUniqueName()
        {
            int i = 0;
            name = "GameObject";
            while (childNames.ContainsKey(name))
            {
                name = "GameObject" + i;
                i++;
            }
            return name;
        }

        internal void drawGuis()
        {
            foreach (var gui in guis)
            {
                gui.draw();
            }
        }

        public int ShadowRes { get { return shadowRes; } set { shadowRes = value; } }
    }
}
