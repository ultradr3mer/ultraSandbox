#region License
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2009 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

using System;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

using Jitter;
using Jitter.Collision;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Jitter.Dynamics.Constraints;
using System.Windows.Forms;
using System.Drawing;
using OpenTkProject.Drawables;
using OpenTkProject.Game;

namespace OpenTkProject
{
    public class OpenTkProjectWindow : GameWindow
    {
        public Scene mScene;

        public TextureLoader textureLoader;
        public MeshLoader meshLoader;
        public ShaderLoader shaderLoader;
        public FramebufferCreator framebufferCreator;
        public MaterialLoader materialLoader;
        public TemplateLoader templateLoader;

        public Player player;

        public string modelFolder = "models\\";
        public string materialFolder = "materials\\";
        public string shaderFolder = "shaders\\";
        public string templateFolder = "templates\\";


        public Vector2 virtual_size, currentSize;

        public float frameTime, oldFrameTime;

        long spawnTime;

        //public Vector3 lightLocation = new Vector3(-3.8f, 3.8f, -6.0f);
        //public Vector3[] lightLocations = new Vector3[] { new Vector3(-3.8f, 3.8f, -6.0f) };
        //public Vector3 lightColor = new Vector3(0.8f, 0.7f, 0.6f);
        //public Vector3 lightDirection = new Vector3(3.8f, -3.8f, 6.0f);

        StringWriter mLog = new StringWriter();

        public Vector2 screenSize;

        float loadingPercentage = 0;

        public OpenTkProjectWindow(int pWidth, int pHeight, bool fullScr)
            : base(pWidth, pHeight, new GraphicsMode(), "ultraSandbox", fullScr ? GameWindowFlags.Fullscreen : GameWindowFlags.Default, DisplayDevice.Default, 3, 0,
            GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {


        }

        protected override void OnLoad(System.EventArgs e)
        {
            //generate stopwatch
            sw = new Stopwatch();

            //set size of the render
            virtual_size = new Vector2(1920, 1080);

            state = new GameState();

            string versionOpenGL = GL.GetString(StringName.Version);
            log(versionOpenGL);

            // make the Coursor disapear
            Cursor.Hide();

            // center it on the window
            Cursor.Position = new Point(
                (Bounds.Left + Bounds.Right) / 2,
                (Bounds.Top + Bounds.Bottom) / 2);

            // other state
            GL.ClearColor(System.Drawing.Color.Black);
            VSync = VSyncMode.On;
            screenSize = new Vector2(Width, Height);

            // create instances of necessary objects
            textureLoader = new TextureLoader(this);
            framebufferCreator = new FramebufferCreator(this);
            meshLoader = new MeshLoader(this);
            shaderLoader = new ShaderLoader(this);
            materialLoader = new MaterialLoader(this);
            templateLoader = new TemplateLoader(this);

            mScene = new Scene(this);

            // create gameInput
            gameInput = new GameInput(mScene, Keyboard, Mouse);

            // set files for displaying loading screen
            shaderLoader.fromXmlFile("shaders\\composite.xsp");

            shaderLoader.fromTextFile("shaders\\composite.vs", "shaders\\splash_shader.fs");

            meshLoader.fromObj("models\\sprite_plane.obj");

            textureLoader.fromFile("materials\\ultra_engine_back.png",true);
            textureLoader.fromFile("materials\\ultra_engine_back_h.png",true);

            materialLoader.fromXmlFile("materials\\composite.xmf");

            //loading noise manualy so we can disable multisampling
            textureLoader.fromFile("materials\\noise_pixel.png", false);

            // load files for loading screen
            textureLoader.LoadTextures();
            meshLoader.loadMeshes();
            shaderLoader.loadShaders();
            materialLoader.loadMaterials();

            // setup 2d filter (loading screen)
            splashFilter2d = new Quad2d(this);

            // set time to zero
            spawnTime = get_time();
        }
 
        long get_time()
        {
            return (DateTime.Now.Ticks - spawnTime);
        }

        #region draw

        double framerate_smoothness = 0.995;
        public double smoothframerate;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (state == GameState.Playing || state == GameState.Menu)
            {
                double framerate = 1 / e.Time;
                smoothframerate = framerate * (1 - framerate_smoothness) + smoothframerate * framerate_smoothness;
                

                // make water fb black
                waterFramebufferSet.outputFb.enable(true);

                // refresh one of the 6 cubemap textures
                if (currentCubemapSide > 5)
                    currentCubemapSide = 0;

                mScene.draw(mCubemapBufferSets.FrameBufferSets[currentCubemapSide], mCubemapBufferSets.cubeView[currentCubemapSide]);

                currentCubemapSide++;

                //render shadowbuffers
                mScene.drawShadowBuffers(shadowFramebuffer);

                // create viewInfo for water reflections
                waterViewInfo.projectionMatrix = player.viewInfo.projectionMatrix;
                waterViewInfo.modelviewMatrix = Matrix4.Mult(mScene.mWaterMatrix, player.viewInfo.modelviewMatrix);
                waterViewInfo.generateViewProjectionMatrix();

                // render water reflections
                GL.CullFace(CullFaceMode.Front);
                mScene.draw(waterFramebufferSet, waterViewInfo);

                // render main scene
                GL.CullFace(CullFaceMode.Back);
                mScene.draw(mainFramebufferSet, player.viewInfo);

                // draw Guis
                mScene.drawGuis();

                SwapBuffers();

                mScene.resetUpdateState();
            }
            else
            {
                // draw loading screen
                GL.Viewport(0, 0, Width, Height);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                splashFilter2d.draw(shaderLoader.getShader("splash_shader.fs"), new int[] { textureLoader.getTexture("ultra_engine_back.png"), textureLoader.getTexture("ultra_engine_back_h.png") }, Vector2.One * (loadingPercentage));

                SwapBuffers();
              
            }
        
        }


        #endregion draw


        #region update

        bool prevESC = false;
        private Framebuffer waterFramebuffer;
        private FramebufferSet mainFramebufferSet;
        private FramebufferSet waterFramebufferSet;
        private CubemapBufferSets mCubemapBufferSets;
        private  int currentCubemapSide = 0;
        private Quad2d splashFilter2d;
        private ViewInfo waterViewInfo;
        public Framebuffer shadowFramebuffer;
        private GameInput gameInput;
        public GameState state;
        public int shadowSize;
        public float lastFrameDuration;
        public Stopwatch sw;
        private bool prevF5;

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (state == GameState.Playing || state == GameState.Menu)
            {
                // calculate mouse movement
                //gameInput.calcDelta();

                // calculate frame Times
                frameTime += (float)e.Time;
                lastFrameDuration = (float)e.Time;

                // exit game?
                bool ESC = Keyboard[Key.Escape];
                if (ESC && !prevESC)
                {
                    if (state == GameState.Menu)
                    {
                        exitGame();
                    }
                    if (state == GameState.Playing)
                    {
                        enterMenu();
                    }
                }
                prevESC = ESC;

                bool F5 = Keyboard[Key.F5];
                if (F5 && !prevF5)
                {
                    if (state == GameState.Playing)
                    {
                        textureLoader.GrabScreenshot();
                    }
                }
                prevF5 = F5;



                // call scene tree to update
                mScene.update();
            }
            else if (state == GameState.Started)
            {
                //search for related files
                FileSeeker mFileSeeker = new FileSeeker(this);

                //create framebufferset for waterreflections
                waterFramebuffer = framebufferCreator.createFrameBuffer("waterFramebuffer", (int)(virtual_size.X * 0.5), (int)(virtual_size.Y * 0.5));

                RenderOptions mOptionsWater = new RenderOptions(virtual_size);
                mOptionsWater.postProcessing = true;
                mOptionsWater.ssAmbientOccluison = false;
                mOptionsWater.bloom = false;
                mOptionsWater.depthOfField = false;

                waterFramebufferSet = new FramebufferSet(framebufferCreator, waterFramebuffer, mOptionsWater);

                //crate shadow framebuffer
                shadowFramebuffer = framebufferCreator.createFrameBuffer("shadowFramebuffer", mScene.ShadowRes, mScene.ShadowRes, PixelInternalFormat.Rgba16f, false);

                //create main framebufferset
                RenderOptions mOptions = new RenderOptions(virtual_size);
                mOptions.postProcessing = true;
                mOptions.ssAmbientOccluison = true;
                mOptions.bloom = true;
                //mOptions.depthOfField = true;

                mainFramebufferSet = new FramebufferSet(framebufferCreator, framebufferCreator.defaultFb, mOptions);

                state = GameState.InitLoading;

            }
            else if (state == GameState.InitLoading)
            {
                //loading rutine - loads one object each time its called
                float percentageA = 0;
                float percentageB = 0;
                float percentageC = 0;
                float percentageD = 0;
                float percentageE = 0;

                percentageA = meshLoader.loadSingleMeshes();
                if (percentageA == 1)
                {
                    percentageB = shaderLoader.loadSingleShaders();
                }
                if (percentageB == 1)
                {
                    percentageC = textureLoader.loadSingleTextures();
                }
                if (percentageC == 1)
                {
                    percentageD = materialLoader.loadSingleMaterials();
                }
                if (percentageD == 1)
                {
                    percentageE = templateLoader.loadSingleTemplates();
                }

                loadingPercentage = (percentageA + percentageB + percentageC + percentageD + percentageE) / 5f;

                //loadingPercentage = (float)Math.Sqrt(loadingPercentage);

                if (loadingPercentage == 1)
                {
                    //create cubemap buffers
                    mCubemapBufferSets = new CubemapBufferSets(mScene, framebufferCreator, 128);
                    mScene.envTextures = mCubemapBufferSets.outTextures;

                    // create viewInfo for reflection
                    waterViewInfo = new ViewInfo();
                    waterViewInfo.aspect = (float)Width / (float)Height;
                    waterViewInfo.updateProjectionMatrix();

                    //set noise texture
                    mScene.noiseTexture = textureLoader.getTexture("noise_pixel.png");

                    //set shaders for postprocessing
                    mScene.ssaoShader = shaderLoader.getShader("ssao.xsp");
                    mScene.ssaoBlrShader = shaderLoader.getShader("ao_blur.xsp");
                    mScene.ssaoBlrShaderA = shaderLoader.getShader("ao_blur_a.xsp");
                    mScene.bloomCurveShader = shaderLoader.getShader("bloom_curve.xsp");
                    mScene.bloomShader = shaderLoader.getShader("bloom_shader.xsp");
                    mScene.dofpreShader = shaderLoader.getShader("dof_preshader.xsp");
                    mScene.dofShader = shaderLoader.getShader("dof_shader.xsp");
                    mScene.compositeShader = shaderLoader.getShader("composite.xsp");
                    mScene.ssaoBlendShader = shaderLoader.getShader("ssao_blend.xsp");
                    mScene.copycatShader = shaderLoader.getShader("backdrop.xsp");
                    mScene.wipingShader = shaderLoader.getShader("sMapWipe.xsp");

                    // spawn the player
                    player = new Player(mScene, new Vector3(0, 3, 10), new Vector3(0, 0, -1), gameInput);
                    mScene.sunLight.Parent = player;
                    //player.setInput(gameInput);

                    mScene.init();

                    // load objects saved in database
                    mScene.loadObjects();

                    state = GameState.Playing;
                }

            }
        }

        public void enterMenu()
        {
            state = GameState.Menu;
            player.hud.isVisible = false;
            player.tool = player.tools[0];
        }

        public void enterGame()
        {
            state = GameState.Playing;
            player.hud.isVisible = true;
            player.tool = player.tools[1];
        }

        [STAThread]
        public static void Main()
        {
            if (!File.Exists("settings.xml"))
            {
                // settings is not existing, lets save default ones
                Settings.Instance.SaveSettings("settings.xml");

                // we can create gui at this point when it is implemented
            }

            Settings.Instance.LoadSettings("settings.xml");


            int scrWidth = Settings.Instance.video.screenWidth;
            int scrHeight = Settings.Instance.video.screenHeight;
            bool fullScr = Settings.Instance.video.fullScreen;

            using (OpenTkProjectWindow game = new OpenTkProjectWindow(scrWidth, scrHeight, fullScr))
            {
                game.Run(60);
            }
        }

        #endregion update

        public void checkGlError(String op)
        {
            ErrorCode error;
            while ((error = GL.GetError()) != ErrorCode.NoError)
            {
                log(op + ": " + error);
            }
        }

        public void log(string text)
        {
            Console.WriteLine(text);
            mLog.WriteLine(text);
        }

        internal void exitGame()
        {
            using (StreamWriter outfile = new StreamWriter("log.txt"))
            {
                outfile.Write(mLog.ToString());
            }
            mScene.saveObjects();
            Exit();
        }
    }

    public struct GameState
    {
        private int curState;

        private GameState(int curState)
        {
            this.curState = curState;
        }

        public static GameState Started = new GameState(0);
        public static GameState InitLoading = new GameState(1);
        public static GameState Playing = new GameState(2);
        public static GameState Menu = new GameState(3);

        public static bool operator ==(GameState a, GameState b)
        {
            return a.curState == b.curState;
        }

        public static bool operator !=(GameState a, GameState b)
        {
            return a.curState != b.curState;
        }
    }
}