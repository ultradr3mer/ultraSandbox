using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.IO;

namespace OpenTkProject
{
    public struct Texture
    {
        public const int TYPE_FROMFILE = 1;
        //public const int TYPE_FROMFILE = 2;
        public const int TYPE_FRAMEBUFFER = 3;

        public int texture;
        public int type;
        public string pointer;

        public bool loaded;

        public int identifier;

        public string name;
        public bool multisampling;

        /*
        public Texture(string pointer)
        {
            type = TYPE_FROMFILE;
            loaded = false;

            this.pointer = pointer;
        }

        public Texture(int id)
        {
            type = TYPE_FRAMEBUFFER;
            loaded = true;

            this.texture = id;
        }
         */
    }

    public class TextureLoader : GameObject
    {

        public List<Texture> Textures = new List<Texture> { };
        public Hashtable TextureNames = new Hashtable();

        public TextureLoader(OpenTkProjectWindow mGameWindow)
        {
            this.gameWindow = mGameWindow;
        }

        public int getTexture(string name)
        {
            if (name == "")
                return 0;

            int identifier = (int)TextureNames[name];
            return Textures[identifier].texture;
        }

        public void fromFile(string file,bool sampling)
        {
            string name = file.Replace(gameWindow.materialFolder, "");

            if (!TextureNames.ContainsKey(name))
            {
                Texture curTexture = new Texture();

                curTexture.identifier = Textures.Count;
                curTexture.type = Texture.TYPE_FROMFILE;
                curTexture.loaded = false;
                curTexture.pointer = file;
                curTexture.name = name;
                curTexture.multisampling = sampling;

                registerTexture(curTexture);

            }
        }

        public void fromFramebuffer(string name,int texture)
        {
            Texture curTexture = new Texture();

            curTexture.identifier = Textures.Count;
            curTexture.type = Texture.TYPE_FRAMEBUFFER;
            curTexture.loaded = true;
            curTexture.texture = texture;
            curTexture.name = name;

            registerTexture(curTexture);

            
        }

        public void registerTexture(Texture curTex)
        {
            if(TextureNames.ContainsKey(curTex.name))
                    TextureNames.Remove(curTex.name);

            TextureNames.Add(curTex.name, curTex.identifier);
            Textures.Add(curTex);
        }

        public void LoadTextures()
        {
            for (int i = 0; i < Textures.Count; i++)
            {
                loadTexture(Textures[i]);
            }
        }

        public float loadSingleTextures()
        {
            for (int i = 0; i < Textures.Count; i++)
            {
                if (!Textures[i].loaded)
                {
                    loadTexture(Textures[i]);
                    return (float)i / (float)Textures.Count;
                }
            }
            return 1;
        }

        public void loadTexture(Texture target)
        {
            if (target.type == Texture.TYPE_FROMFILE)
            {
                if (String.IsNullOrEmpty(target.pointer))
                    throw new ArgumentException(target.pointer);

                target.texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, target.texture);

                Bitmap bmp = new Bitmap(target.pointer);
                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                bmp.UnlockBits(bmp_data);

                int sampling = 0;
                if (target.multisampling)
                {
                    sampling = (int)TextureMinFilter.Linear;
                }
                else
                {
                    sampling = (int)TextureMinFilter.Nearest;
                }

                // We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
                // On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
                // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                
                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, sampling);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, sampling);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            }

            target.loaded = true;

            Textures[target.identifier] = target;
        }

        // Returns a System.Drawing.Bitmap with the contents of the current framebuffer
        public Bitmap GrabScreenshot()
        {
            if (GraphicsContext.CurrentContext == null)
                throw new GraphicsContextMissingException();

            Bitmap bmp = new Bitmap(gameWindow.ClientSize.Width, this.gameWindow.Height);
            System.Drawing.Imaging.BitmapData data =
                bmp.LockBits(gameWindow.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, gameWindow.ClientSize.Width, gameWindow.ClientSize.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            int uid = 0;
            while (File.Exists("screenshot"+uid+".png"))
                uid++;

            // Save the image as a Png.
            bmp.Save("screenshot" + uid + ".png", System.Drawing.Imaging.ImageFormat.Png);

            return bmp;
        }
    }
}
