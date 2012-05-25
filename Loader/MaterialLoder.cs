using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;
using System.IO;

namespace OpenTkProject
{
    public struct Material
    {
        public string name;
        public string pointer;

        public const int FROM_XML = 0;

        public Shader shader;
        public int baseTexture, normalTexture, emitTexture, mirrorTexture;
        public int identifier;

        public bool loaded;

        public int type;
        public Shader ssnshader;
        public Shader selectionshader;
        public int base2Texture;
        public int base3Texture;
        public bool envMapAlphaBaseTexture, useEnv;
        public bool envMapAlphaNormalTexture;
        public int envTexture;
        public int envMapTexture;
        public OpenTK.Vector3 envMapTint;
        public bool emitMapAlphaNormalTexture;
        public bool emitMapAlphaBaseTexture;
        public int emitMapTexture;
        public OpenTK.Vector3 emitMapTint;
        public bool useEmit;
        public bool useAlpha;
        public float refStrength;
        public float blurStrength;
        public float fresnelStrength;
        public Shader shadowshader;
        public bool useLight;
        public bool useSpec;
        public bool specMapAlphaNormalTexture;
        public bool specMapAlphaBaseTexture;
        public int specMapTexture;
        public OpenTK.Vector3 specMapTint;
        public float specExp;
        public bool noCull;
        public bool noDepthMask;
        public bool additive;
    }

    public class MaterialLoader : GameObject
    {
        public List<Material> Materials = new List<Material> { };
        public Hashtable MaterialNames = new Hashtable();

        public MaterialLoader(OpenTkProjectWindow mGameWindow)
        {
            this.gameWindow = mGameWindow;
        }

        public Material getMaterial(string name)
        {
            int id = (int)MaterialNames[name];
            return Materials[id];
        }

        public void fromXmlFile(string pointer)
        {
            string name = pointer.Replace(gameWindow.materialFolder, "");

            Material newMat = new Material();

            newMat.type = Material.FROM_XML;
            newMat.loaded = false;
            newMat.name = name;
            newMat.pointer = pointer;

            register(newMat);
        }

        private void register(Material newMat)
        {
            if (!MaterialNames.Contains(newMat.name))
            {
                newMat.identifier = Materials.Count;
                Materials.Add(newMat);
                MaterialNames.Add(newMat.name, newMat.identifier);
            }
        }

        public void loadMaterials()
        {
            for (int i = 0; i < Materials.Count; i++)
            {
                loadMaterial(Materials[i]);
            }
        }

        public float loadSingleMaterials()
        {
            for (int i = 0; i < Materials.Count; i++)
            {
                if (!Materials[i].loaded)
                {
                    loadMaterial(Materials[i]);
                    return (float)i / (float)Materials.Count;
                }
            }
            return 1;
        }


        public void loadMaterial(Material target)
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
                    }
                    gameWindow.log("shader: " + target.shader.name);
                    reader.MoveToElement();
                }

                // parsing textures
                if (reader.Name == "textures" && reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.Name == "base")
                            target.baseTexture = gameWindow.textureLoader.getTexture(reader.Value);

                        else if (reader.Name == "base2")
                            target.base2Texture = gameWindow.textureLoader.getTexture(reader.Value);

                        else if (reader.Name == "base3")
                            target.base3Texture = gameWindow.textureLoader.getTexture(reader.Value);

                        else if (reader.Name == "normal")
                            target.normalTexture = gameWindow.textureLoader.getTexture(reader.Value);

                        else if (reader.Name == "reflection")
                            target.mirrorTexture = gameWindow.textureLoader.getTexture(reader.Value);

                        else if (reader.Name == "emit")
                            target.emitTexture = gameWindow.textureLoader.getTexture(reader.Value);
                    }
                    gameWindow.log("base: " + target.baseTexture);
                    gameWindow.log("normal: " + target.normalTexture);
                    reader.MoveToElement();
                }

                // parsing envmap data
                if (reader.Name == "envmap")
                {
                    target.useEnv = true;
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "source")
                            {
                                if (reader.Value == "normalalpha")
                                    target.envMapAlphaNormalTexture = true;

                                else if (reader.Value == "basealpha")
                                    target.envMapAlphaBaseTexture = true;

                                else
                                    target.envMapTexture = gameWindow.textureLoader.getTexture(reader.Value);

                            }
                            else if (reader.Name == "tint")
                            {
                                target.envMapTint = GenericMethods.VectorFromString(reader.Value);
                            }
                        }
                        reader.MoveToElement();
                    }
                }

                // parsing specular data
                if (reader.Name == "specmap")
                {
                    target.useSpec = true;
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "source")
                            {
                                if (reader.Value == "normalalpha")
                                    target.specMapAlphaNormalTexture = true;

                                else if (reader.Value == "basealpha")
                                    target.specMapAlphaBaseTexture = true;

                                else
                                    target.specMapTexture = gameWindow.textureLoader.getTexture(reader.Value);

                            }
                            else if (reader.Name == "tint")
                            {
                                target.specMapTint = GenericMethods.VectorFromString(reader.Value);
                            }
                            else if (reader.Name == "exp")
                            {
                                target.specExp = GenericMethods.FloatFromString(reader.Value);
                            }
                        }
                        reader.MoveToElement();
                    }
                }

                // parsing emit data
                if (reader.Name == "emit")
                {
                    target.useEmit = true;
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "source")
                            {
                                if (reader.Value == "normalalpha")
                                    target.emitMapAlphaNormalTexture = true;

                                else if (reader.Value == "basealpha")
                                    target.emitMapAlphaBaseTexture = true;

                                else
                                    target.emitMapTexture = gameWindow.textureLoader.getTexture(reader.Value);

                            }
                            else if (reader.Name == "tint")
                            {
                                target.emitMapTint = GenericMethods.VectorFromString(reader.Value);
                            }
                        }
                        reader.MoveToElement();
                    }
                }

                // parsing transparency data
                if (reader.Name == "transparency")
                {
                    target.useAlpha = true;
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "refraction")
                                target.refStrength = GenericMethods.FloatFromString(reader.Value);

                            if (reader.Name == "blur")
                                target.blurStrength = GenericMethods.FloatFromString(reader.Value);

                            if (reader.Name == "fresnel")
                                target.fresnelStrength = GenericMethods.FloatFromString(reader.Value);
                        }
                        reader.MoveToElement();
                    }
                }

                // parsing lighting data
                if (reader.Name == "lighted")
                {
                    target.useLight = true;
                }

                // parsing nucull
                if (reader.Name == "nocull")
                {
                    target.noCull = true;
                }

                // parsing nucull
                if (reader.Name == "nodepthmask")
                {
                    target.noDepthMask = true;
                }

                // parsing additive
                if (reader.Name == "additive")
                {
                    target.additive = true;
                }


                target.loaded = true;
                Materials[target.identifier] = target;
            }
        }
    }
}
