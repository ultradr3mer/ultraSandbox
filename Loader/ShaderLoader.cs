using System;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Collections;
using System.Xml;

namespace OpenTkProject
{
    public struct Shader
    {
        public int
            modelviewMatrixLocation,
            projectionMatrixLocation,
            projectionRevMatrixLocation,
            modelMatrixLocation,
            rotationMatrixLocation,
            screenSizeLocation,
            lightLocationLocation,
            lightColorLocation,
            lightAmbientLocation,
            lightDirectionLocation,
            eyePosLocation,
            timeLocation,
            passLocation,
            waterLevelLocation,
            vectorLocation,
            nearLocation,
            farLocation,
            renderSizeLocation,
            hudElementSize,
            hudElementPos,
            useEnvLocation;

        public int[] lightLocationsLocation;
        public int[] lightDirectionsLocation;
        public int[] lightColorsLocation;
        public int[] lightViewMatrixLocation;

        public const int TYPE_FROMFILE = 1;
        public const int TYPE_FROMXML = 2;

        public int handle, type;
        public string[] pointer;

        public string name;

        public int identifier;

        public bool loaded;
        public int hudElementColor;
        public int hudElementValue;
        public int emitColorLocation;
        public int envMapAlphaBaseTexture;
        public int envMapAlphaNormalTexture;
        public int envTintLocation;
        public int useEmitLocation;
        public int emitMapAlphaBaseTexture;
        public int emitMapAlphaNormalTexture;
        public int useAlphaLocation;
        public int refSizeLocation;
        public int blurSizeLocation;
        public int fresnelStrLocation;
        public int useSpecLocation;
        public int specMapAlphaBaseTexture;
        public int specMapAlphaNormalTexture;
        public int specColorLocation;
        public int specExpLocation;
        public int rotationMatrixLocation2;
        public int modelMatrixLocation2;
        public int colorLocation;
        public int LightCountLocation;
        public int modLocation;
        public int sunDirection;
        public int sunColor;
        public int sunMatrix;
        public int lightSunLocation;
        public int curLightLocation;
        public int[] lightActiveLocation;
        public int shadowQualityLocation;
        public int particlePos;
        public int particleSize;
        public int[] lightTextureLocation;
        public int sunInnerMatrix;

    }

    public class ShaderLoader : GameObject
    {
        int vertexShaderHandle,
            fragmentShaderHandle;

        public List<Shader> Shaders = new List<Shader> { };

        public Hashtable ShaderNames = new Hashtable();
        public const int maxNoLights = 10;

        public Shader fromTextFile(string vfile, string ffile)
        {
            string name = ffile.Replace(gameWindow.shaderFolder, "");

            if (!ShaderNames.ContainsKey(ffile))
            {
                Shader curShader = new Shader();

                int identifier = Shaders.Count;

                curShader.type = Shader.TYPE_FROMFILE;
                curShader.pointer = new string[] { vfile, ffile };
                curShader.identifier = identifier;
                curShader.name = name;
                curShader.loaded = false;

                registerShader(curShader);
                return curShader;
            }
            else
            {
                return getShader(ffile);
            }
        }

        internal Shader fromXmlFile(string file)
        {
            string name = file.Replace(gameWindow.shaderFolder, "");

            if (!ShaderNames.ContainsKey(name))
            {
                Shader curShader = new Shader();

                int identifier = Shaders.Count;

                curShader.type = Shader.TYPE_FROMXML;
                curShader.pointer = new string[] { file };
                curShader.identifier = identifier;
                curShader.name = name;
                curShader.loaded = false;

                registerShader(curShader);
                return curShader;
            }
            else
            {
                return getShader(name);
            }
        }

        private void registerShader(Shader newShader)
        {

            ShaderNames.Add(newShader.name, newShader.identifier);
            Shaders.Add(newShader);

        }

        public void loadShaders()
        {
            for (int i = 0; i < Shaders.Count; i++)
            {
                loadShader(Shaders[i]);
            }
        }

        public float loadSingleShaders()
        {
            for (int i = 0; i < Shaders.Count; i++)
            {
                if (!Shaders[i].loaded)
                {
                    loadShader(Shaders[i]);
                    return (float)i / (float)Shaders.Count;
                }
            }
            return 1;
        }

        public Shader getShader(string name)
        {
            int id = (int)ShaderNames[name];
            return Shaders[id];
        }


        public void loadShader(Shader target)
        {
            if (target.type == Shader.TYPE_FROMXML)
            {
                XmlTextReader reader = new XmlTextReader(target.pointer[0]);

                string path = Path.GetDirectoryName(target.pointer[0]) + "\\";

                //target.envMapAlphaBaseTexture = false;

                gameWindow.log("parsing shader pair: " + target.name);

                target.pointer = new string[2];

                while (reader.Read())
                {
                    // parsing data in material tag
                    if (reader.Name == "shaderpair" && reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "vertex")
                                target.pointer[0] = path + reader.Value;

                            else if (reader.Name == "fragment")
                                target.pointer[1] = path + reader.Value;
                        }
                        reader.MoveToElement();
                    }
                }

                target.type = Shader.TYPE_FROMFILE;
                loadShader(target);
            }
            else if (target.type == Shader.TYPE_FROMFILE)
            {
                int shaderProgramHandle;

                string vfile = target.pointer[0];
                string ffile = target.pointer[1];

                vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
                fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

                GL.ShaderSource(vertexShaderHandle, readFile(vfile));
                GL.ShaderSource(fragmentShaderHandle, readFile(ffile));

                string log;

                GL.CompileShader(vertexShaderHandle);
                GL.GetShaderInfoLog(vertexShaderHandle, out log);
                gameWindow.log("Shaderlog (" + vfile + "):" + log);
                gameWindow.checkGlError("loadShader (" + vfile + ")");

                GL.CompileShader(fragmentShaderHandle);
                GL.GetShaderInfoLog(fragmentShaderHandle, out log);
                gameWindow.log("Shaderlog (" + ffile + "):" + log);
                gameWindow.checkGlError("loadShader (" + ffile + ")");

                Debug.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle));
                Debug.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle));

                // Create program
                shaderProgramHandle = GL.CreateProgram();

                GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
                GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);

                GL.LinkProgram(shaderProgramHandle);

                Debug.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));

                //GL.UseProgram(shaderProgramHandle);

                gameWindow.checkGlError("loadShader");

                target.handle = shaderProgramHandle;

                getHandles(ref target);

                target.loaded = true;

                Shaders[target.identifier] = target;
            }
        }

        public void getHandles(ref Shader target)
        {
            int shaderProgramHandle = target.handle;

            // Set uniforms
            target.projectionMatrixLocation = GL.GetUniformLocation(shaderProgramHandle, "projection_matrix");
            target.projectionRevMatrixLocation = GL.GetUniformLocation(shaderProgramHandle, "projection_rev_matrix");
            target.modelviewMatrixLocation = GL.GetUniformLocation(shaderProgramHandle, "modelview_matrix");
            target.rotationMatrixLocation = GL.GetUniformLocation(shaderProgramHandle, "rotation_matrix");
            target.modelMatrixLocation = GL.GetUniformLocation(shaderProgramHandle, "model_matrix");
            target.rotationMatrixLocation2 = GL.GetUniformLocation(shaderProgramHandle, "rotation_matrix2");
            target.modelMatrixLocation2 = GL.GetUniformLocation(shaderProgramHandle, "model_matrix2");

            target.eyePosLocation = GL.GetUniformLocation(shaderProgramHandle, "in_eyepos");
            target.timeLocation = GL.GetUniformLocation(shaderProgramHandle, "in_time");
            target.passLocation = GL.GetUniformLocation(shaderProgramHandle, "in_pass");
            target.waterLevelLocation = GL.GetUniformLocation(shaderProgramHandle, "in_waterlevel");
            target.vectorLocation = GL.GetUniformLocation(shaderProgramHandle, "in_vector");
            target.screenSizeLocation = GL.GetUniformLocation(shaderProgramHandle, "in_screensize");
            target.renderSizeLocation = GL.GetUniformLocation(shaderProgramHandle, "in_rendersize");
            target.lightAmbientLocation = GL.GetUniformLocation(shaderProgramHandle, "in_lightambient");
            target.lightSunLocation = GL.GetUniformLocation(shaderProgramHandle, "in_lightsun");
            target.shadowQualityLocation = GL.GetUniformLocation(shaderProgramHandle, "shadow_quality");

            target.particlePos = GL.GetUniformLocation(shaderProgramHandle, "in_particlepos");
            target.particleSize = GL.GetUniformLocation(shaderProgramHandle, "in_particlesize");

            target.colorLocation = GL.GetUniformLocation(shaderProgramHandle, "in_color");
            target.modLocation = GL.GetUniformLocation(shaderProgramHandle, "in_mod");

            target.useEmitLocation = GL.GetUniformLocation(shaderProgramHandle, "use_emit");
            target.emitMapAlphaBaseTexture = GL.GetUniformLocation(shaderProgramHandle, "emit_a_base");
            target.emitMapAlphaNormalTexture = GL.GetUniformLocation(shaderProgramHandle, "emit_a_normal");
            target.emitColorLocation = GL.GetUniformLocation(shaderProgramHandle, "in_emitcolor");

            target.useSpecLocation = GL.GetUniformLocation(shaderProgramHandle, "use_spec");
            target.specMapAlphaBaseTexture = GL.GetUniformLocation(shaderProgramHandle, "spec_a_base");
            target.specMapAlphaNormalTexture = GL.GetUniformLocation(shaderProgramHandle, "spec_a_normal");
            target.specColorLocation = GL.GetUniformLocation(shaderProgramHandle, "in_speccolor");
            target.specExpLocation = GL.GetUniformLocation(shaderProgramHandle, "in_specexp");

            target.useEnvLocation = GL.GetUniformLocation(shaderProgramHandle, "use_env");
            target.envMapAlphaBaseTexture = GL.GetUniformLocation(shaderProgramHandle, "env_a_base");
            target.envMapAlphaNormalTexture = GL.GetUniformLocation(shaderProgramHandle, "env_a_normal");
            target.envTintLocation = GL.GetUniformLocation(shaderProgramHandle, "env_tint");

            target.useAlphaLocation = GL.GetUniformLocation(shaderProgramHandle, "use_alpha");
            target.refSizeLocation = GL.GetUniformLocation(shaderProgramHandle, "ref_size");
            target.blurSizeLocation = GL.GetUniformLocation(shaderProgramHandle, "blur_size");
            target.fresnelStrLocation = GL.GetUniformLocation(shaderProgramHandle, "fresnel_str");

            target.nearLocation = GL.GetUniformLocation(shaderProgramHandle, "in_near");
            target.farLocation = GL.GetUniformLocation(shaderProgramHandle, "in_far");

            target.hudElementSize = GL.GetUniformLocation(shaderProgramHandle, "in_hudsize");
            target.hudElementPos = GL.GetUniformLocation(shaderProgramHandle, "in_hudpos");
            target.hudElementColor = GL.GetUniformLocation(shaderProgramHandle, "in_hudcolor");
            target.hudElementValue = GL.GetUniformLocation(shaderProgramHandle, "in_hudvalue");

            target.lightLocationsLocation = new int[maxNoLights];
            target.lightDirectionsLocation = new int[maxNoLights];
            target.lightColorsLocation = new int[maxNoLights];
            target.lightViewMatrixLocation = new int[maxNoLights];
            target.lightActiveLocation = new int[maxNoLights];
            target.lightTextureLocation = new int[maxNoLights];

            target.LightCountLocation = GL.GetUniformLocation(shaderProgramHandle, "in_no_lights");
            target.curLightLocation = GL.GetUniformLocation(shaderProgramHandle, "curLight");
            target.sunDirection = GL.GetUniformLocation(shaderProgramHandle, "sunLightStruct.direction");
            target.sunColor = GL.GetUniformLocation(shaderProgramHandle, "sunLightStruct.color");
            target.sunMatrix = GL.GetUniformLocation(shaderProgramHandle, "sunLightStruct.view_matrix");
            target.sunInnerMatrix = GL.GetUniformLocation(shaderProgramHandle, "sunLightStruct.inner_view_matrix");

            for (int i = 0; i < maxNoLights; i++)
            {
                target.lightActiveLocation[i] = GL.GetUniformLocation(shaderProgramHandle, "lightStructs[" + i + "].active");

                target.lightLocationsLocation[i] = GL.GetUniformLocation(shaderProgramHandle, "lightStructs[" + i + "].position");
                target.lightDirectionsLocation[i] = GL.GetUniformLocation(shaderProgramHandle, "lightStructs[" + i + "].direction");
                target.lightColorsLocation[i] = GL.GetUniformLocation(shaderProgramHandle, "lightStructs[" + i + "].color");

                target.lightTextureLocation[i] = GL.GetUniformLocation(shaderProgramHandle, "lightStructs[" + i + "].texture");

                target.lightViewMatrixLocation[i] = GL.GetUniformLocation(shaderProgramHandle, "lightStructs[" + i + "].view_matrix");
            }
        }

        private string readFile(string filename)
        {
            string line;
            StringWriter wholeFile = new StringWriter();

            System.IO.StreamReader file =
               new System.IO.StreamReader(filename);
            while ((line = file.ReadLine()) != null)
            {
                wholeFile.Write(line);
                wholeFile.Write(wholeFile.NewLine);
            }
            //Console.WriteLine(wholeFile.ToString());
            return wholeFile.ToString();
        }

        public ShaderLoader(OpenTkProjectWindow mGameWindow)
        {
            this.gameWindow = mGameWindow;
        }
    }

}
