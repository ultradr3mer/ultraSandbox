using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;
using System.IO;
using OpenTK;
using OpenTkProject.Game;

namespace OpenTkProject
{
    [Serializable]
    public struct Material
    {
        //generic stuff
        public string pointer;

        public enum Type { fromXml, fromCache };

        public enum TexType
        {
            baseTexture,
            base2Texture,
            base3Texture,
            normalTexture,
            emitTexture,
            reflectionTexture,
            emitMapTexture,
            specMapTexture,
            envMapTexture,
            envTexture,
            definfoTexture
        }

        public enum WorldTexture
        {
            lightMap,
            reflectionMap
        }

        public int identifier;

        public bool loaded;

        [Serializable]
        public struct Propertys
        {
            public bool envMapAlphaBaseTexture, useEnv;
            public bool envMapAlphaNormalTexture;
            public OpenTK.Vector3 envMapTint;
            public bool emitMapAlphaNormalTexture;
            public bool emitMapAlphaBaseTexture;
            public OpenTK.Vector3 emitMapTint;
            public bool useEmit;
            public bool useAlpha;
            public float refStrength;
            public float blurStrength;
            public float fresnelStrength;

            public bool useLight;
            public bool useSpec;
            public bool specMapAlphaNormalTexture;
            public bool specMapAlphaBaseTexture;
            public OpenTK.Vector3 specMapTint;
            public float specExp;
            public bool noCull;
            public bool noDepthMask;
            public bool additive;
        }

        public Type type;

        //stuff to be saved
        public string name;

        public Shader shader;
        public Shader ssnshader;
        public Shader selectionshader;
        public Shader shadowshader;
        public Shader definfoshader;

        public Propertys propertys;

        Texture[] textures;

        internal void cacheMaterial(ref List<Material> mList)
        {
            Material tmpMat = new Material();

            tmpMat.name = name;

            //propertys
            tmpMat.propertys = propertys;

            //shaders
            tmpMat.shader = shader.nameOnly();
            tmpMat.ssnshader = ssnshader.nameOnly();
            tmpMat.selectionshader = selectionshader.nameOnly();
            tmpMat.shadowshader = shadowshader.nameOnly();
            tmpMat.definfoshader = definfoshader.nameOnly();

            //textures
            int texCount = textures.Length;
            tmpMat.textures = new Texture[texCount];
            for (int i = 0; i < texCount; i++)
            {
                tmpMat.textures[i] = textures[i].nameOnly();
            }

            mList.Add(tmpMat);
        }

        public void setTexture(TexType type, Texture texture)
        {
            textures[(int)type] = texture;
        }

        public int getTextureId(TexType type)
        {
            return textures[(int)type].texture;
        }

        public string getTextureName(TexType type)
        {
            return textures[(int)type].name;
        }

        public void setArys()
        {
            int texCount = Enum.GetValues(typeof(TexType)).Length;
            if (textures == null)
                textures = new Texture[texCount];
        }

        /*
public Texture nameOnly()
{
    Texture tmpTex = new Texture();

    tmpTex.texture = texture;
    tmpTex.name = name;

    return tmpTex;
}
 */

        internal void resolveTextures(TextureLoader textureLoader)
        {
            int textureCount = textures.Length;
            for (int i = 0; i < textureCount; i++)
            {
                string texname = textures[i].name;
                if(texname != null)
                    textures[i] = textureLoader.getTexture(texname);
            }
        }

        internal void resolveShaders(ShaderLoader shaderLoader)
        {
            if (shader.name != null)
                shader = shaderLoader.getShader(shader.name);

            if (shadowshader.name != null)
                shadowshader = shaderLoader.getShader(shadowshader.name);

            if (ssnshader.name != null)
                ssnshader = shaderLoader.getShader(ssnshader.name);

            if (selectionshader.name != null)
                selectionshader = shaderLoader.getShader(selectionshader.name);

            if (definfoshader.name != null)
                definfoshader = shaderLoader.getShader(definfoshader.name);
        }
    }

    public class MaterialLoader : GameObject
    {
        public List<Material> materials = new List<Material> { };
        public Hashtable materialNames = new Hashtable();

        public MaterialLoader(OpenTkProjectWindow mGameWindow)
        {
            this.gameWindow = mGameWindow;
        }

        public Material getMaterial(string name)
        {
            int id = (int)materialNames[name];

            if (!materials[id].loaded)
                loadMaterial(materials[id]);

            return materials[id];
        }

        public void readCacheFile()
        {
            string filename = Settings.Instance.game.materialCacheFile;
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            List<Material> tmpMaterials;

            using (fileStream)
            {
                // Read the source file into a byte array.
                byte[] bytes = new byte[fileStream.Length];
                int numBytesToRead = (int)fileStream.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = fileStream.Read(bytes, numBytesRead, numBytesToRead);

                    // Break when the end of the file is reached.
                    if (n == 0)
                        break;

                    numBytesRead += n;
                    numBytesToRead -= n;
                }

                tmpMaterials = (List<Material>)GenericMethods.ByteArrayToObject(bytes);
                fileStream.Close();
            }

            int materialCount = tmpMaterials.Count;
            for (int i = 0; i < materialCount; i++)
            {
                Material curMat = tmpMaterials[i];
                string name = curMat.name;

                if (!materialNames.ContainsKey(name))
                {
                    curMat.type = Material.Type.fromCache;

                    int identifier = materials.Count;

                    curMat.identifier = identifier;

                    materialNames.Add(name, identifier);
                    materials.Add(curMat);
                }
            }

            gameWindow.log("loaded " + materialCount + " materials from cache");

        }

        public void writeCacheFile()
        {
            List<Material> SaveList = new List<Material> { };
            foreach (var material in materials)
            {
                material.cacheMaterial(ref SaveList);
            }

            string filename = Settings.Instance.game.materialCacheFile;

            FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);

            using (fileStream)
            {
                byte[] saveAry = GenericMethods.ObjectToByteArray(SaveList);
                fileStream.Write(saveAry, 0, saveAry.Length);
                fileStream.Close();
            }
        }

        public void fromXmlFile(string pointer)
        {
            string name = pointer.Replace(gameWindow.materialFolder, "");

            Material newMat = new Material();

            newMat.type = Material.Type.fromXml;
            newMat.name = name;
            newMat.pointer = pointer;
            newMat.setArys();

            register(newMat);
        }

        private void register(Material newMat)
        {
            if (!materialNames.Contains(newMat.name))
            {
                newMat.identifier = materials.Count;
                materials.Add(newMat);
                materialNames.Add(newMat.name, newMat.identifier);
            }
        }

        public void loadMaterials()
        {
            for (int i = 0; i < materials.Count; i++)
            {
                loadMaterial(materials[i]);
            }
        }

        public float loadSingleMaterials()
        {
            for (int i = 0; i < materials.Count; i++)
            {
                if (!materials[i].loaded)
                {
                    loadMaterial(materials[i]);
                    return (float)i / (float)materials.Count;
                }
            }
            return 1;
        }

        private void loadMaterial(Material material)
        {
            switch (material.type)
            {
                case Material.Type.fromXml:
                    loadMaterialXml(material);
                    break;
                case Material.Type.fromCache:
                    loadMaterialCache(material);
                    break;
                default:
                    break;
            }
        }

        private void loadMaterialCache(Material material)
        {
            //shaders
            material.resolveShaders(gameWindow.shaderLoader);

            //textures
            material.resolveTextures(gameWindow.textureLoader);

            material.loaded = true;

            materials[material.identifier] = material;
        }


        public void loadMaterialXml(Material target)
        {
            XmlTextReader reader = new XmlTextReader(target.pointer);

            //target.envMapAlphaBaseTexture = false;

            gameWindow.log("parsing material: " + target.name);

            while (reader.Read())
            {
                // parsing data in material tag
                if (reader.Name == "material" && reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.Name == "shader")
                            target.shader = gameWindow.shaderLoader.getShader(reader.Value);

                        else if (reader.Name == "ssnshader")
                            target.ssnshader = gameWindow.shaderLoader.getShader(reader.Value);

                        else if (reader.Name == "selection")
                            target.selectionshader = gameWindow.shaderLoader.getShader(reader.Value);

                        else if (reader.Name == "shadow")
                            target.shadowshader = gameWindow.shaderLoader.getShader(reader.Value);

                        else if (reader.Name == "definfo")
                            target.definfoshader = gameWindow.shaderLoader.getShader(reader.Value);
                    }
                    gameWindow.log("shader: " + target.shader.name);
                    reader.MoveToElement();
                }

                // parsing textures
                if (reader.Name == "textures" && reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        Texture tmpTex = gameWindow.textureLoader.getTexture(reader.Value);

                        if (reader.Name == "base")
                            target.setTexture(Material.TexType.baseTexture, tmpTex);

                        else if (reader.Name == "base2")
                            target.setTexture(Material.TexType.base2Texture, tmpTex);

                        else if (reader.Name == "base3")
                            target.setTexture(Material.TexType.base3Texture, tmpTex);

                        else if (reader.Name == "normal")
                            target.setTexture(Material.TexType.normalTexture, tmpTex);

                        else if (reader.Name == "reflection")
                            target.setTexture(Material.TexType.reflectionTexture, tmpTex);

                        else if (reader.Name == "definfo")
                            target.setTexture(Material.TexType.definfoTexture, tmpTex);

                        else if (reader.Name == "emit")
                            target.setTexture(Material.TexType.emitTexture, tmpTex);
                    }
                    gameWindow.log("base: " + target.getTextureName(Material.TexType.baseTexture));
                    gameWindow.log("normal: " + target.getTextureName(Material.TexType.normalTexture));
                    reader.MoveToElement();
                }

                // parsing envmap data
                if (reader.Name == "envmap")
                {
                    target.propertys.useEnv = true;
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "source")
                            {
                                if (reader.Value == "normalalpha")
                                    target.propertys.envMapAlphaNormalTexture = true;

                                else if (reader.Value == "basealpha")
                                    target.propertys.envMapAlphaBaseTexture = true;

                                else
                                    target.setTexture(Material.TexType.envMapTexture, gameWindow.textureLoader.getTexture(reader.Value));

                            }
                            else if (reader.Name == "tint")
                            {
                                target.propertys.envMapTint = GenericMethods.VectorFromString(reader.Value);
                            }
                        }
                        reader.MoveToElement();
                    }
                }

                // parsing specular data
                if (reader.Name == "specmap")
                {
                    target.propertys.useSpec = true;
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "source")
                            {
                                if (reader.Value == "normalalpha")
                                    target.propertys.specMapAlphaNormalTexture = true;

                                else if (reader.Value == "basealpha")
                                    target.propertys.specMapAlphaBaseTexture = true;

                                else
                                    target.setTexture(Material.TexType.specMapTexture, gameWindow.textureLoader.getTexture(reader.Value));

                            }
                            else if (reader.Name == "tint")
                            {
                                target.propertys.specMapTint = GenericMethods.VectorFromString(reader.Value);
                            }
                            else if (reader.Name == "exp")
                            {
                                target.propertys.specExp = GenericMethods.FloatFromString(reader.Value);
                            }
                        }
                        reader.MoveToElement();
                    }
                }

                // parsing emit data
                if (reader.Name == "emit")
                {
                    target.propertys.useEmit = true;
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "source")
                            {
                                if (reader.Value == "normalalpha")
                                    target.propertys.emitMapAlphaNormalTexture = true;

                                else if (reader.Value == "basealpha")
                                    target.propertys.emitMapAlphaBaseTexture = true;

                                else
                                    target.setTexture(Material.TexType.emitMapTexture, gameWindow.textureLoader.getTexture(reader.Value));

                            }
                            else if (reader.Name == "tint")
                            {
                                target.propertys.emitMapTint = GenericMethods.VectorFromString(reader.Value);
                            }
                        }
                        reader.MoveToElement();
                    }
                }

                // parsing transparency data
                if (reader.Name == "transparency")
                {
                    target.propertys.useAlpha = true;
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "refraction")
                                target.propertys.refStrength = GenericMethods.FloatFromString(reader.Value);

                            if (reader.Name == "blur")
                                target.propertys.blurStrength = GenericMethods.FloatFromString(reader.Value);

                            if (reader.Name == "fresnel")
                                target.propertys.fresnelStrength = GenericMethods.FloatFromString(reader.Value);
                        }
                        reader.MoveToElement();
                    }
                }

                // parsing lighting data
                if (reader.Name == "lighted")
                {
                    target.propertys.useLight = true;
                }

                // parsing nucull
                if (reader.Name == "nocull")
                {
                    target.propertys.noCull = true;
                }

                // parsing nucull
                if (reader.Name == "nodepthmask")
                {
                    target.propertys.noDepthMask = true;
                }

                // parsing additive
                if (reader.Name == "additive")
                {
                    target.propertys.additive = true;
                }


                target.loaded = true;
                materials[target.identifier] = target;
            }
        }
    }
}
