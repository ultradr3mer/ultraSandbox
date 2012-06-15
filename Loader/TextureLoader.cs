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
using OpenTkProject.Game;

namespace OpenTkProject
{
    [Serializable]
    public struct Texture
    {
        public int texture;
        public string name;

        public string pointer;

        public Type type;
        public enum Type {fromFile, fromFramebuffer, fromCache};

        public bool loaded;
        public int identifier;

        public bool multisampling;

        public Bitmap bitmap;

        internal Texture nameOnly()
        {
            Texture tmpTexture = new Texture();

            tmpTexture.name = name;

            return tmpTexture;
        }

        public void cache(ref List<Texture> mList)
        {
            if (type != Type.fromFramebuffer)
            {
                Texture tmpTex = new Texture();

                tmpTex.name = name;
                tmpTex.bitmap = bitmap;
                tmpTex.multisampling = multisampling;

                mList.Add(tmpTex);
            }
        }
    }

    public class TextureLoader : GameObject
    {

        public List<Texture> textures = new List<Texture> { };
        public Hashtable textureNames = new Hashtable();

        public TextureLoader(OpenTkProjectWindow mGameWindow)
        {
            this.gameWindow = mGameWindow;
        }


        /*
        internal void readCacheFile()
        {

        }

        internal void writeCacheFile()
        {
            List<Texture> saveList = new List<Texture> { };
            foreach (var texture in textures)
            {
                texture.cache(ref saveList);
            }

            string filename = Settings.Instance.game.textureCacheFile;

            FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);

            using (fileStream)
            {
                byte[] saveAry = GenericMethods.ObjectToByteArray(saveList);
                fileStream.Write(saveAry, 0, saveAry.Length);
                fileStream.Close();
            }
        }
         */

        public Texture getTexture(string name)
        {
            if (name == "" || name == null)
                return new Texture();

            int identifier = (int)textureNames[name];
            return textures[identifier];
        }

        public int getTextureId(string name)
        {
            if (name == "")
                return 0;

            int identifier = (int)textureNames[name];
            return textures[identifier].texture;
        }

        public void fromFile(string file,bool sampling)
        {
            string name = file.Replace(gameWindow.materialFolder, "");

            if (!textureNames.ContainsKey(name))
            {
                Texture curTexture = new Texture();

                curTexture.identifier = textures.Count;
                curTexture.type = Texture.Type.fromFile;
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

            curTexture.identifier = textures.Count;
            curTexture.type = Texture.Type.fromFramebuffer;
            curTexture.loaded = true;
            curTexture.texture = texture;
            curTexture.name = name;

            registerTexture(curTexture);

            
        }

        public void registerTexture(Texture curTex)
        {
            if(textureNames.ContainsKey(curTex.name))
                    textureNames.Remove(curTex.name);

            textureNames.Add(curTex.name, curTex.identifier);
            textures.Add(curTex);
        }

        public void LoadTextures()
        {
            for (int i = 0; i < textures.Count; i++)
            {
                if (!textures[i].loaded)
                    loadTexture(textures[i]);
            }
        }

        public float loadSingleTextures()
        {
            for (int i = 0; i < textures.Count; i++)
            {
                if (!textures[i].loaded)
                {
                    loadTexture(textures[i]);
                    return (float)i / (float)textures.Count;
                }
            }
            return 1;
        }

        public void loadTexture(Texture target)
        {
            gameWindow.log("loading Texture: " + target.name);

            if (String.IsNullOrEmpty(target.pointer))
                throw new ArgumentException(target.pointer);

            target.texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, target.texture);

            Bitmap bmp = new Bitmap(target.pointer);
            target.bitmap = bmp;
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

            target.loaded = true;

            textures[target.identifier] = target;
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
