using System;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Collections;

namespace OpenTkProject
{
    public class FramebufferCreator : GameObject
    {
        public List<Framebuffer> Framebuffers = new List<Framebuffer> { };
        public Hashtable FramebufferNames = new Hashtable();

        public Framebuffer defaultFb;

        public FramebufferCreator(OpenTkProjectWindow gameWindow)
        {
            this.gameWindow = gameWindow;
            //defaultFb = createFrameBuffer( mGameWindow.Width, mGameWindow.Height);
            defaultFb = new DefaultFramebuffer(new Vector2(gameWindow.Width, gameWindow.Height), this);
        }

        #region Randoms

        Random rnd = new Random();
        public const float rScale = 3f;

        /// <summary>Returns a random Float in the range [-0.5*scale..+0.5*scale]</summary>
        public float GetRandom()
        {
            return (float)(rnd.NextDouble() - 0.5) * rScale;
        }

        /// <summary>Returns a random Float in the range [0..1]</summary>
        public float GetRandom0to1()
        {
            return (float)rnd.NextDouble();
        }

        #endregion Randoms

        public Framebuffer createFrameBuffer(string name, int FboWidth, int FboHeight)
        {
            return createFrameBuffer(name, FboWidth, FboHeight, PixelInternalFormat.Rgba8, true);
        }

        public Framebuffer createFrameBuffer(int FboWidth, int FboHeight)
        {
 	        return createFrameBuffer( null,  FboWidth,  FboHeight, PixelInternalFormat.Rgba8, true);
        }

        public Framebuffer createFrameBuffer(int FboWidth, int FboHeight, PixelInternalFormat precision, bool multisampling)
        {
            return createFrameBuffer(null, FboWidth, FboHeight, precision, multisampling);
        }

        public Framebuffer createFrameBuffer(string name, int FboWidth, int FboHeight, PixelInternalFormat precision, bool multisampling)
        {
            int FboHandle;
            int ColorTexture;
            int DepthTexture;

            int sampling = 0;
            if (multisampling)
            {
                sampling = (int)TextureMinFilter.Linear;
            }
            else
            {
                sampling = (int)TextureMinFilter.Nearest;
            }

            // Create Color Tex
            GL.GenTextures(1, out ColorTexture);
            GL.BindTexture(TextureTarget.Texture2D, ColorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, precision, FboWidth, FboHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, sampling);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, sampling);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            // GL.Ext.GenerateMipmap( GenerateMipmapTarget.Texture2D );


            // Create Depth Tex
            GL.GenTextures(1, out DepthTexture);
            GL.BindTexture(TextureTarget.Texture2D, DepthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, (PixelInternalFormat)All.DepthComponent32, FboWidth, FboHeight, 0, PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
            // things go horribly wrong if DepthComponent's Bitcount does not match the main Framebuffer's Depth
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, sampling);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, sampling);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            // GL.Ext.GenerateMipmap( GenerateMipmapTarget.Texture2D );
            
            // Create a FBO and attach the textures
            GL.Ext.GenFramebuffers(1, out FboHandle);

            #region Test for Error

            switch (GL.Ext.CheckFramebufferStatus(FramebufferTarget.FramebufferExt))
            {
                case FramebufferErrorCode.FramebufferCompleteExt:
                    {
                        gameWindow.log("FBO: The framebuffer is complete and valid for rendering.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteAttachmentExt:
                    {
                        gameWindow.log("FBO: One or more attachment points are not framebuffer attachment complete. This could mean there’s no texture attached or the format isn’t renderable. For color textures this means the base format must be RGB or RGBA and for depth textures it must be a DEPTH_COMPONENT format. Other causes of this error are that the width or height is zero or the z-offset is out of range in case of render to volume.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteMissingAttachmentExt:
                    {
                        gameWindow.log("FBO: There are no attachments.");
                        break;
                    }
                /* case  FramebufferErrorCode.GL_FRAMEBUFFER_INCOMPLETE_DUPLICATE_ATTACHMENT_EXT: 
                     {
                         Console.WriteLine("FBO: An object has been attached to more than one attachment point.");
                         break;
                     }*/
                case FramebufferErrorCode.FramebufferIncompleteDimensionsExt:
                    {
                        gameWindow.log("FBO: Attachments are of different size. All attachments must have the same width and height.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteFormatsExt:
                    {
                        gameWindow.log("FBO: The color attachments have different format. All color attachments must have the same format.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteDrawBufferExt:
                    {
                        gameWindow.log("FBO: An attachment point referenced by GL.DrawBuffers() doesn’t have an attachment.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteReadBufferExt:
                    {
                        gameWindow.log("FBO: The attachment point referenced by GL.ReadBuffers() doesn’t have an attachment.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferUnsupportedExt:
                    {
                        gameWindow.log("FBO: This particular FBO configuration is not supported by the implementation.");
                        break;
                    }
                default:
                    {
                        gameWindow.log("FBO: Status unknown. (yes, this is really bad.)");
                        break;
                    }
            }

            // using FBO might have changed states, e.g. the FBO might not support stereoscopic views or double buffering
            int[] queryinfo = new int[6];
            GL.GetInteger(GetPName.MaxColorAttachmentsExt, out queryinfo[0]);
            GL.GetInteger(GetPName.AuxBuffers, out queryinfo[1]);
            GL.GetInteger(GetPName.MaxDrawBuffers, out queryinfo[2]);
            GL.GetInteger(GetPName.Stereo, out queryinfo[3]);
            GL.GetInteger(GetPName.Samples, out queryinfo[4]);
            GL.GetInteger(GetPName.Doublebuffer, out queryinfo[5]);
            //Console.WriteLine("max. ColorBuffers: " + queryinfo[0] + " max. AuxBuffers: " + queryinfo[1] + " max. DrawBuffers: " + queryinfo[2] + "\nStereo: " + queryinfo[3] + " Samples: " + queryinfo[4] + " DoubleBuffer: " + queryinfo[5]);

            Console.WriteLine("Last GL Error: " + GL.GetError());

            #endregion Test for Error

            gameWindow.checkGlError("createFrameBuffer");

            Framebuffer myFramebuffer = new Framebuffer(FboHandle, ColorTexture, DepthTexture, new Vector2(FboWidth, FboHeight), this);
            myFramebuffer.name = name;

            if (name != null)
            {
                registerFramebuffer(myFramebuffer);
            }

            return myFramebuffer;
        }

        private void registerFramebuffer(Framebuffer newFb)
        {
            Framebuffers.Add(newFb);

            int identifier = Framebuffers.Count;
            if (FramebufferNames.ContainsKey(newFb.name))
                FramebufferNames.Remove(newFb.name);

            FramebufferNames.Add(newFb.name, identifier);

            gameWindow.textureLoader.fromFramebuffer(newFb.name + "color", newFb.ColorTexture);
        }

        public void createLightBuffer(string name, int size, bool multisampling)
        {
            int FboHandle;
            int ColorTexture;
            int DepthTexture;

            int sampling = 0;
            if (multisampling)
            {
                sampling = (int)TextureMinFilter.Linear;
            }
            else
            {
                sampling = (int)TextureMinFilter.Nearest;
            }
              /*
            // Create Color Tex
            GL.GenTextures(1, out ColorTexture);
            GL.BindTexture(TextureTarget.Texture2D, ColorTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, precision, size, size, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, sampling);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, sampling);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            // GL.Ext.GenerateMipmap( GenerateMipmapTarget.Texture2D );
            */

            // Create Depth Tex
            GL.GenTextures(1, out DepthTexture);
            GL.BindTexture(TextureTarget.Texture2D, DepthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, (PixelInternalFormat)All.DepthComponent32, size, size, 0, PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
            // things go horribly wrong if DepthComponent's Bitcount does not match the main Framebuffer's Depth
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, sampling);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, sampling);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            // GL.Ext.GenerateMipmap( GenerateMipmapTarget.Texture2D );

            // Create a FBO and attach the textures
            GL.Ext.GenFramebuffers(1, out FboHandle);

            #region Test for Error

            switch (GL.Ext.CheckFramebufferStatus(FramebufferTarget.FramebufferExt))
            {
                case FramebufferErrorCode.FramebufferCompleteExt:
                    {
                        gameWindow.log("FBO: The framebuffer is complete and valid for rendering.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteAttachmentExt:
                    {
                        gameWindow.log("FBO: One or more attachment points are not framebuffer attachment complete. This could mean there’s no texture attached or the format isn’t renderable. For color textures this means the base format must be RGB or RGBA and for depth textures it must be a DEPTH_COMPONENT format. Other causes of this error are that the width or height is zero or the z-offset is out of range in case of render to volume.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteMissingAttachmentExt:
                    {
                        gameWindow.log("FBO: There are no attachments.");
                        break;
                    }
                /* case  FramebufferErrorCode.GL_FRAMEBUFFER_INCOMPLETE_DUPLICATE_ATTACHMENT_EXT: 
                     {
                         Console.WriteLine("FBO: An object has been attached to more than one attachment point.");
                         break;
                     }*/
                case FramebufferErrorCode.FramebufferIncompleteDimensionsExt:
                    {
                        gameWindow.log("FBO: Attachments are of different size. All attachments must have the same width and height.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteFormatsExt:
                    {
                        gameWindow.log("FBO: The color attachments have different format. All color attachments must have the same format.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteDrawBufferExt:
                    {
                        gameWindow.log("FBO: An attachment point referenced by GL.DrawBuffers() doesn’t have an attachment.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteReadBufferExt:
                    {
                        gameWindow.log("FBO: The attachment point referenced by GL.ReadBuffers() doesn’t have an attachment.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferUnsupportedExt:
                    {
                        gameWindow.log("FBO: This particular FBO configuration is not supported by the implementation.");
                        break;
                    }
                default:
                    {
                        gameWindow.log("FBO: Status unknown. (yes, this is really bad.)");
                        break;
                    }
            }

            // using FBO might have changed states, e.g. the FBO might not support stereoscopic views or double buffering
            int[] queryinfo = new int[6];
            GL.GetInteger(GetPName.MaxColorAttachmentsExt, out queryinfo[0]);
            GL.GetInteger(GetPName.AuxBuffers, out queryinfo[1]);
            GL.GetInteger(GetPName.MaxDrawBuffers, out queryinfo[2]);
            GL.GetInteger(GetPName.Stereo, out queryinfo[3]);
            GL.GetInteger(GetPName.Samples, out queryinfo[4]);
            GL.GetInteger(GetPName.Doublebuffer, out queryinfo[5]);
            //Console.WriteLine("max. ColorBuffers: " + queryinfo[0] + " max. AuxBuffers: " + queryinfo[1] + " max. DrawBuffers: " + queryinfo[2] + "\nStereo: " + queryinfo[3] + " Samples: " + queryinfo[4] + " DoubleBuffer: " + queryinfo[5]);

            Console.WriteLine("Last GL Error: " + GL.GetError());

            #endregion Test for Error

            gameWindow.checkGlError("createFrameBuffer");

            int identifier = Framebuffers.Count;

            ColorTexture = 0;

            Framebuffer myFramebuffer = new Framebuffer(FboHandle, ColorTexture, DepthTexture, new Vector2(size, size), this);

            gameWindow.textureLoader.fromFramebuffer(name + "light", myFramebuffer.DepthTexture);

            Framebuffers.Add(myFramebuffer);
            FramebufferNames.Add(name, identifier);

            //return new Framebuffer(FboHandle, ColorTexture, DepthTexture, new Vector2(FboWidth, FboHeight), mGameWindow);
        }

        public Framebuffer getFrameBuffer(string name)
        {
            int id = (int)FramebufferNames[name];
            return Framebuffers[id];
        }
    }

    public struct RenderOptions
    {
        public Vector2 size;
        public float quality;
        public bool postProcessing;
        public bool ssAmbientOccluison;
        public bool depthOfField;
        public bool bloom;

        public RenderOptions(Vector2 size)
        {
            this.size = size;
            quality = 0.5f;
            postProcessing = false;
            ssAmbientOccluison = false;
            depthOfField = false;
            bloom = false;
        }
    }

    public class FramebufferSet : GameObject
    {
        public Framebuffer 
            screenNormalFb, 
            aoFramebuffer,
            aoBlurFramebuffer, 
            aoBlurFramebuffer2, 
            sceeneFramebuffer, 
            selectionFb,
            bloomFramebuffer,
            bloomFramebuffer2,
            outputFb, 
            selectionblurFb,
            selectionblurFb2,
            dofFramebuffer,
            dofPreFramebuffer,
            dofFramebuffer2,
            sceeneBackdropFb,
            reflectionFramebuffer,
            lightFramebuffer,
            lightBlurFramebuffer,
            sceeneDefInfoFb,
            aoPreFramebuffer;

        public RenderOptions renderOptions;

        public FramebufferSet(FramebufferCreator mFramebufferCreator, Vector2 size, Framebuffer outputFb)
        {
            RenderOptions mOptions = new RenderOptions(size);
            createFramebufferSet(mFramebufferCreator, outputFb, mOptions);
        }

        public FramebufferSet(FramebufferCreator mFramebufferCreator, Framebuffer outputFb, RenderOptions mOptions)
        {

            createFramebufferSet(mFramebufferCreator, outputFb, mOptions);
        }

        private void createFramebufferSet(FramebufferCreator mFramebufferCreator, Framebuffer outputFb, RenderOptions mOptions)
        {
            Vector2 size = mOptions.size;
            Vector2 size2 = size * mOptions.quality;
            Vector2 size3 = size2 * 0.3f;

            //sceeneFramebuffer = mFramebufferCreator.createFrameBuffer((int)size.X, (int)size.Y);
            sceeneBackdropFb = mFramebufferCreator.createFrameBuffer((int)size.X, (int)size.Y, PixelInternalFormat.Rgba8, false);
            sceeneDefInfoFb = mFramebufferCreator.createFrameBuffer((int)size.X, (int)size.Y, PixelInternalFormat.Rgba16, false);
            sceeneDefInfoFb.clearColor = new Color4(0f, 0f, 0f, 1f);

            lightFramebuffer = mFramebufferCreator.createFrameBuffer((int)size.X, (int)size.Y, PixelInternalFormat.Rgba8, false);
            lightFramebuffer.clearColor = new Color4(0f, 0f, 0f, 0f);

            lightBlurFramebuffer = mFramebufferCreator.createFrameBuffer((int)size.X, (int)size.Y, PixelInternalFormat.Rgba8, false);

            reflectionFramebuffer = mFramebufferCreator.createFrameBuffer((int)size.X, (int)size.Y, PixelInternalFormat.Rgba8, false);

            sceeneFramebuffer = mFramebufferCreator.createFrameBuffer((int)size.X, (int)size.Y, PixelInternalFormat.Rgba16, false);
            sceeneFramebuffer.clearColor = new Color4(0f, 0f, 0f, 1f);

            selectionFb = mFramebufferCreator.createFrameBuffer((int)size.X, (int)size.Y, PixelInternalFormat.Rgba8, false);


            //screenNormalFb = mFramebufferCreator.createFrameBuffer((int)size2.X, (int)size2.Y, PixelInternalFormat.Rgba16f, false);
            //screenNormalFb.clearColor = new Color4(0f, 0f, 0f, 100f);
            //lightFramebuffer = mFramebufferCreator.createFrameBuffer((int)size.X, (int)size.Y, PixelInternalFormat.Rgba16f, false);

            if (mOptions.depthOfField)
            {
                // depth of field buffers
                dofPreFramebuffer = mFramebufferCreator.createFrameBuffer((int)size2.X, (int)size2.Y, PixelInternalFormat.Rgba8, false);
                dofFramebuffer = mFramebufferCreator.createFrameBuffer((int)size2.X, (int)size2.Y, PixelInternalFormat.Rgba8, false);
            }

            dofFramebuffer2 = mFramebufferCreator.createFrameBuffer((int)size2.X, (int)size2.Y, PixelInternalFormat.Rgba8, true);

            aoBlurFramebuffer2 = mFramebufferCreator.createFrameBuffer((int)size2.X, (int)size2.Y, PixelInternalFormat.Rgba8, true);
            aoBlurFramebuffer2.clearColor = new Color4(0f, 0f, 0f, 0.5f);

            if (mOptions.ssAmbientOccluison)
            {
                aoPreFramebuffer = mFramebufferCreator.createFrameBuffer((int)size2.X, (int)size2.Y, PixelInternalFormat.Rgba16, false);
                aoFramebuffer = mFramebufferCreator.createFrameBuffer((int)size2.X, (int)size2.Y, PixelInternalFormat.Rgba8, false);
                aoBlurFramebuffer = mFramebufferCreator.createFrameBuffer((int)size2.X, (int)size2.Y, PixelInternalFormat.Rgba8, false);
            }
            else
            {
                // if ao is set of make the buffer grey 
                //aoBlurFramebuffer2.enable();
            }


            selectionblurFb = mFramebufferCreator.createFrameBuffer((int)size3.X, (int)size3.Y, PixelInternalFormat.Rgba8, false);
            selectionblurFb2 = mFramebufferCreator.createFrameBuffer((int)size3.X, (int)size3.Y, PixelInternalFormat.Rgba8, true);
            bloomFramebuffer = mFramebufferCreator.createFrameBuffer((int)size3.X, (int)size3.Y, PixelInternalFormat.Rgba8, false);
            bloomFramebuffer2 = mFramebufferCreator.createFrameBuffer((int)size3.X, (int)size3.Y, PixelInternalFormat.Rgba8, false);

            renderOptions = mOptions;
            this.outputFb = outputFb;
        }
    }

    public class CubemapBufferSets : GameObject
    {
        private Framebuffer[] outFrameBuffers = new Framebuffer[6];
        public FramebufferSet[] FrameBufferSets = new FramebufferSet[6];

        public ViewInfo[] cubeView = new ViewInfo[6];

        public Vector3[] viewDirections = new Vector3[] {
            new Vector3(-1,0,0),
            new Vector3(0,0,1),
            new Vector3(1,0,0),
            new Vector3(0,0,-1),
            new Vector3(0,1,0),
            new Vector3(0,-1,0)
        };

        public Vector3[] upDirections = new Vector3[] {
            new Vector3(0,-1,0),
            new Vector3(0,-1,0),
            new Vector3(0,-1,0),
            new Vector3(0,-1,0),
            new Vector3(0,0,-1),
            new Vector3(0,0,1)
        };

        public int[] outTextures = new int[6];

        public CubemapBufferSets(Scene parent, FramebufferCreator mFramebufferCreator, int size)
        {
            this.Scene = parent;
            Parent = parent;

            Vector2 vecSize = new Vector2(size,size);

            float fovy = (float)Math.PI/2;

            for (int i = 0; i < 6; i++)
			{
                outFrameBuffers[i] = mFramebufferCreator.createFrameBuffer(size, size, PixelInternalFormat.Rgba8, true);
                outTextures[i] = outFrameBuffers[i].ColorTexture;
                FrameBufferSets[i] = new FramebufferSet(mFramebufferCreator, vecSize, outFrameBuffers[i]);

                cubeView[i] = new ViewInfo(this);
                cubeView[i].PointingDirection = viewDirections[i];
                cubeView[i].upVec = upDirections[i];

                cubeView[i].aspect = 1f;
                cubeView[i].fovy = fovy;
                cubeView[i].updateProjectionMatrix();
			}
        }

        public override void update()
        {
            Position = Scene.eyePos;

            updateChilds();
        }
    }

    public class Framebuffer : GameObject
    {
        public int FboHandle;
        public int ColorTexture;
        public int DepthTexture;

        public Color4 clearColor = new Color4(0f, 0f, 0f, 0f);

        public bool isDefaultFb = false;

        protected new Vector2 size;

        new public Vector2 Size { get { return size; } set { size = value; } }

        public Framebuffer(
            int FboHandle, 
            int ColorTexture,
            int DepthTexture, 
            Vector2 size,
            FramebufferCreator parent)
        {
            this.FboHandle = FboHandle;
            this.ColorTexture = ColorTexture;
            this.DepthTexture = DepthTexture;
            this.size = size;

            Parent = parent;
        }

        public virtual void enable(bool wipe)
        {
            gameWindow.checkGlError("--uncaught ERROR enabeling framebuffer--" + name);

            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, FboHandle);
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, ColorTexture, 0);
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, TextureTarget.Texture2D, DepthTexture, 0);

            GL.Viewport(0, 0, (int)(size.X), (int)(size.Y));
            // clear the screen in red, to make it very obvious what the clear affected. only the FBO, not the real framebuffer
            if (wipe)
            {
                GL.ClearColor(clearColor);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }

            gameWindow.currentSize = size;

            gameWindow.checkGlError("--ERROR enabeling framebuffer--" + name);
        }

        public bool Multisampeling
        {
            get { return false; }
            set
            {
                int sampling = 0;
                if (value)
                {
                    sampling = (int)TextureMinFilter.Linear;
                }
                else
                {
                    sampling = (int)TextureMinFilter.Nearest;
                }

                GL.BindTexture(TextureTarget.Texture2D, ColorTexture);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, sampling);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, sampling);
            }
        }

        public Framebuffer() { }
    }

    public class DefaultFramebuffer : Framebuffer
    {
        public DefaultFramebuffer(Vector2 size, FramebufferCreator parent)
        {
            Parent = parent;
            this.size = size;
        }

        public override void enable(bool wipe)
        {
            gameWindow.checkGlError("--uncaught ERROR enabeling framebuffer--defaultfb");

            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // disable rendering into the FBO

            GL.Viewport(0, 0, (int)(size.X), (int)(size.Y));

            if (wipe)
            {
                GL.ClearColor(clearColor);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }

            //GL.Enable(EnableCap.Texture2D); // enable Texture Mapping
            //GL.BindTexture(TextureTarget.Texture2D, 0); // bind default texture

            gameWindow.currentSize = size;

            gameWindow.checkGlError("--ERROR enabeling framebuffer--defaultfb");
        }
    }
}
