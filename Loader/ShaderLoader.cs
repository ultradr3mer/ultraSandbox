using System;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using System.Text;
using OpenTkProject.Game;

namespace OpenTkProject
{
    [Serializable]
    public struct Shader
    {
        //stuff to be saved
        public string name;
        public string vShader;
        public string fShader;

        //generic
        public string[] pointer;
        public Type type;
        public int handle;
        public int identifier;
        public bool loaded;

        public enum Type
        {
            fromFile,
            fromXml,
            fromCache
        }

        //light locations
        public int[] lightLocationsLocation;
        public int[] lightDirectionsLocation;
        public int[] lightColorsLocation;
        public int[] lightViewMatrixLocation;

        public int[] lightActiveLocation;
        public int[] lightTextureLocation;

        public int[] BoneMatixLocations;

        public int sunDirection;
        public int sunColor;
        public int sunMatrix;
        public int sunInnerMatrix;

        internal Shader nameOnly()
        {
            Shader tmpShader = new Shader();

            tmpShader.name = name;

            return tmpShader;
        }

        public enum Uniform
        {
            projection_matrix,
            projection_rev_matrix,
            modelview_matrix,
            rotation_matrix,
            model_matrix,
            rotation_matrix2,
            model_matrix2,

            in_eyepos,
            in_time,
            in_pass,
            in_waterlevel,
            in_vector,
            in_screensize,
            in_rendersize,
            in_lightambient,
            in_lightsun,
            shadow_quality,

            in_particlepos,
            in_particlesize,

            in_color,
            in_mod,

            use_emit,
            emit_a_base,
            emit_a_normal,
            in_emitcolor,

            use_spec,
            spec_a_base,
            spec_a_normal,
            in_speccolor,
            in_specexp,

            use_env,
            env_a_base,
            env_a_normal,
            env_tint,

            use_alpha,
            ref_size,
            blur_size,
            fresnel_str,

            in_near,
            in_far,

            in_hudsize,
            in_hudpos,
            in_hudcolor,
            in_hudvalue,

            in_no_lights,
            curLight,
            uni_no_bones
        }

        int[] locations;

        public void insertUniform(Uniform uni, ref float value)
        {
            int location = locations[(int)uni];
            if (location != -1)
                GL.Uniform1(location, 1, ref value);
        }

        internal void insertUniform(Uniform uni, ref int value)
        {
            int location = locations[(int)uni];
            if (location != -1)
                GL.Uniform1(location, 1, ref value);
        }

        internal void insertUniform(Uniform uni, ref Vector4 value)
        {
            int location = locations[(int)uni];
            if (location != -1)
                GL.Uniform4(location, ref value);
        }

        public void insertUniform(Uniform uni, ref Vector3 value)
        {
            int location = locations[(int)uni];
            if (location != -1)
                GL.Uniform3(location, ref value);
        }

        internal void insertUniform(Uniform uni, ref Vector2 value)
        {
            int location = locations[(int)uni];
            if (location != -1)
                GL.Uniform2(location, ref value);
        }

        public void insertUniform(Uniform uni, ref Matrix4 value)
        {
            int location = locations[(int)uni];
            if (location != -1)
                GL.UniformMatrix4(location, false, ref value);
        }

        public void generateLocations()
        {
            string[] names = Enum.GetNames(typeof(Uniform));
            Array values = Enum.GetValues(typeof(Uniform));

            int handlesCount = names.Length;
            locations = new int[handlesCount];

            for (int i = 0; i < handlesCount; i++)
            {
                locations[i] = GL.GetUniformLocation(handle, names[i]);
            }
        }

        internal void cache(ref ShaderCacheObject cacheObject)
        {
            Shader tmpShader = new Shader();

            tmpShader.name = name;
            tmpShader.vShader = vShader;
            tmpShader.fShader = fShader;

            cacheObject.shaders.Add(tmpShader);
        }
    }

    [Serializable]
    public struct Snippet
    {
        public string name;
        public string text;
        public string variables;
        public string functions;

        internal void cache(ref ShaderCacheObject cacheObject)
        {
            cacheObject.snippets.Add(this);
        }
    }

    [Serializable]
    public struct ShaderCacheObject
    {
        public List<Shader> shaders;
        public List<Snippet> snippets;
    }

    public class ShaderLoader : GameObject
    {
        int vertexShaderHandle,
            fragmentShaderHandle;

        public List<Shader> shaders = new List<Shader> { };
        public List<Snippet> snippets = new List<Snippet> { };

        public Hashtable shaderNames = new Hashtable();
        public const int maxNoLights = 10;

        const string varMarker = "#variables";
        const string codeMarker = "#code";
        const string includeMarker = "#include";
        const string functionsMarker = "#functions";
        private int maxNoBones = 64;

        enum Target { code, variable, function };

        internal void readCacheFile()
        {

            string filename = Settings.Instance.game.shaderCacheFile;
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            ShaderCacheObject cacheObject;

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

                cacheObject = (ShaderCacheObject)GenericMethods.ByteArrayToObject(bytes);
                fileStream.Close();
            }

            foreach (var shader in cacheObject.shaders)
            {
                Shader curShader = shader;

                string name = shader.name;

                if (!shaderNames.ContainsKey(shader.name))
                {
                    int identifier = shaders.Count;

                    curShader.type = Shader.Type.fromCache;
                    curShader.identifier = identifier;

                    shaders.Add(curShader);
                    shaderNames.Add(name, identifier);
                }
            }
            foreach (var newSnippet in cacheObject.snippets)
            {
                loadSnippetFromCache(newSnippet);
            }

            gameWindow.log("loaded " + cacheObject.shaders.Count + " shaders from cache");
            gameWindow.log("loaded " + cacheObject.snippets.Count + " shader-snippets from cache");

        }

        private void loadSnippetFromCache(Snippet newSnippet)
        {
            foreach (var snippet in snippets)
            {
                if (snippet.name == newSnippet.name)
                    return;
            } 

            snippets.Add(newSnippet);
        }

        internal void writeCacheFile()
        {
            ShaderCacheObject cacheObject = new ShaderCacheObject();
            cacheObject.shaders = new List<Shader> { };
            cacheObject.snippets = new List<Snippet> { };

            foreach (var shader in shaders)
            {
                shader.cache(ref cacheObject);
            }
            foreach (var snippet in snippets)
            {
                snippet.cache(ref cacheObject);
            }

            string filename = Settings.Instance.game.shaderCacheFile;

            FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);

            using (fileStream)
            {
                byte[] saveAry = GenericMethods.ObjectToByteArray(cacheObject);
                fileStream.Write(saveAry, 0, saveAry.Length);
                fileStream.Close();
            }
        }

        public void loadSnippet(string file)
        {
            Snippet newSnippet = new Snippet();
            newSnippet.name = file.Replace(gameWindow.shaderFolder, "");

            foreach (var snippet in snippets)
            {
                if (snippet.name == newSnippet.name)
                    return;
            }

            string line;
            StringBuilder codeSb = new StringBuilder();
            StringBuilder variableSb = new StringBuilder();
            StringBuilder functionSb = new StringBuilder();

            Target curtarget = Target.code;

            System.IO.StreamReader mFile =
               new System.IO.StreamReader(file);
            while ((line = mFile.ReadLine()) != null)
            {
                if (line.Contains(varMarker))
                {
                    curtarget = Target.variable;
                }
                else if (line.Contains(codeMarker))
                {
                    curtarget = Target.code;
                }
                else if (line.Contains(functionsMarker))
                {
                    curtarget = Target.function;
                }
                else
                {
                    if (curtarget == Target.code)
                        codeSb.AppendLine(line);
                    else if (curtarget == Target.variable)
                        variableSb.AppendLine(line);
                    else if (curtarget == Target.function)
                        functionSb.AppendLine(line);
                }
            }

            variableSb.AppendLine(varMarker);
            functionSb.AppendLine(functionsMarker);

            //Console.WriteLine(wholeFile.ToString());
            newSnippet.text =  codeSb.ToString();
            newSnippet.variables = variableSb.ToString();
            newSnippet.functions = functionSb.ToString();

            snippets.Add(newSnippet);
        }

        public Shader fromTextFile(string vfile, string ffile)
        {
            string name = ffile.Replace(gameWindow.shaderFolder, "");

            if (!shaderNames.ContainsKey(ffile))
            {
                Shader curShader = new Shader();

                int identifier = shaders.Count;

                curShader.type = Shader.Type.fromFile;
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

            if (!shaderNames.ContainsKey(name))
            {
                Shader curShader = new Shader();

                int identifier = shaders.Count;

                curShader.type = Shader.Type.fromXml;
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

            shaderNames.Add(newShader.name, newShader.identifier);
            shaders.Add(newShader);

        }

        public void loadShaders()
        {
            for (int i = 0; i < shaders.Count; i++)
            {
                loadShader(shaders[i]);
            }
        }

        public float loadSingleShaders()
        {
            for (int i = 0; i < shaders.Count; i++)
            {
                if (!shaders[i].loaded)
                {
                    loadShader(shaders[i]);
                    return (float)i / (float)shaders.Count;
                }
            }
            return 1;
        }

        private void loadShader(Shader shader)
        {
            switch (shader.type)
            {
                case Shader.Type.fromFile:
                    loadShaderFromFile(shader);
                    break;
                case Shader.Type.fromXml:
                    loadShaderXml(shader);
                    break;
                case Shader.Type.fromCache:
                    loadShaderFromCache(shader);
                    break;
                default:
                    break;
            }
        }

        public Shader getShader(string name)
        {
            if (name == null)
                return new Shader();

            int id = (int)shaderNames[name];
            return shaders[id];
        }

        public void loadShaderXml(Shader target)
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

            loadShaderFromFile(target);
        }

        public void loadShaderFromFile(Shader target)
        {
            string vfile = target.pointer[0];
            string ffile = target.pointer[1];

            target.vShader = readFile(vfile);
            target.fShader = readFile(ffile);

            loadShaderFromCache(target);
        }

        public void loadShaderFromCache(Shader target)
        {
            int shaderProgramHandle;

            vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShaderHandle, target.vShader);
            GL.ShaderSource(fragmentShaderHandle, target.fShader);

            string log;

            GL.CompileShader(vertexShaderHandle);
            GL.GetShaderInfoLog(vertexShaderHandle, out log);
            parseLog(log,"vertexShader: " + name, target.vShader);
            gameWindow.checkGlError("loadVertexShader (" + name + ")");

            GL.CompileShader(fragmentShaderHandle);
            GL.GetShaderInfoLog(fragmentShaderHandle, out log);
            parseLog(log, "fragmentShader: " + name, target.fShader);
            gameWindow.checkGlError("loadFragmentShader (" + name + ")");

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

            shaders[target.identifier] = target;
        }

        private void parseLog(string log, string name, string fullShader)
        {
            gameWindow.log("Shaderlog " + name + ":\n" + log);

            StringBuilder fullSb = new StringBuilder();
            string newline;

            string tmpShader = fullShader;

            int nextIndex;
            int curline = 1;
            while ((nextIndex = tmpShader.IndexOf("\r\n")) != -1 && tmpShader != "\r\n")
            {
                if (tmpShader.Length > nextIndex + 2)
                    newline = tmpShader.Remove(nextIndex + 2);
                else
                    newline = tmpShader;

                tmpShader = tmpShader.Remove(0, newline.Length);

                fullSb.Append(curline + "\t|" + newline);

                curline++;
            }

            if (log.Contains("ERROR") || log.Contains("error") || log.Contains("WARNING") || log.Contains("warning"))
                gameWindow.log(
                    "--------------------------------------------\n" + 
                    fullSb.ToString()+ 
                    "\n--------------------------------------------\n"
                    );
        }

        public void getHandles(ref Shader target)
        {
            int shaderProgramHandle = target.handle;

            // Set uniforms
            target.generateLocations();

            target.sunDirection = GL.GetUniformLocation(shaderProgramHandle, "sunLightStruct.direction");
            target.sunColor = GL.GetUniformLocation(shaderProgramHandle, "sunLightStruct.color");
            target.sunMatrix = GL.GetUniformLocation(shaderProgramHandle, "sunLightStruct.view_matrix");
            target.sunInnerMatrix = GL.GetUniformLocation(shaderProgramHandle, "sunLightStruct.inner_view_matrix");

            target.lightLocationsLocation = new int[maxNoLights];
            target.lightDirectionsLocation = new int[maxNoLights];
            target.lightColorsLocation = new int[maxNoLights];
            target.lightViewMatrixLocation = new int[maxNoLights];
            target.lightActiveLocation = new int[maxNoLights];
            target.lightTextureLocation = new int[maxNoLights];

            for (int i = 0; i < maxNoLights; i++)
            {
                target.lightActiveLocation[i] = GL.GetUniformLocation(shaderProgramHandle, "lightStructs[" + i + "].active");

                target.lightLocationsLocation[i] = GL.GetUniformLocation(shaderProgramHandle, "lightStructs[" + i + "].position");
                target.lightDirectionsLocation[i] = GL.GetUniformLocation(shaderProgramHandle, "lightStructs[" + i + "].direction");
                target.lightColorsLocation[i] = GL.GetUniformLocation(shaderProgramHandle, "lightStructs[" + i + "].color");

                target.lightTextureLocation[i] = GL.GetUniformLocation(shaderProgramHandle, "lightStructs[" + i + "].texture");

                target.lightViewMatrixLocation[i] = GL.GetUniformLocation(shaderProgramHandle, "lightStructs[" + i + "].view_matrix");
            }

            target.BoneMatixLocations = new int[maxNoBones];
            for (int i = 0; i < maxNoBones; i++)
            {
                target.BoneMatixLocations[i] = GL.GetUniformLocation(shaderProgramHandle, "bone_matrix[" + i + "]");
            }
        }

        private string readFile(string filename)
        {
            string line;
            StringBuilder wholeFile = new StringBuilder();

            System.IO.StreamReader file =
               new System.IO.StreamReader(filename);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains(includeMarker))
                {
                    string[] sline = line.Split(' ');
                    appendSnip(sline, ref wholeFile);
                }
                else
                {
                    wholeFile.AppendLine(line);
                }
            }

            wholeFile.Replace(varMarker, "");
            wholeFile.Replace(functionsMarker, "");

            //Console.WriteLine(wholeFile.ToString());
            return wholeFile.ToString();
        }

        private void appendSnip(string[] arguments, ref StringBuilder wholeFile)
        {
            int noArguments = arguments.Length;
             for (int i = 0; i < noArguments; i++)
			{
                if (arguments[i].Contains(includeMarker))
                {
                    string snipName = arguments[i + 1];

                    foreach (var snipet in snippets)
                    {
                        if (snipName == snipet.name)
                        {
                            string modText = snipet.text;

                            for (int j = 0; j < noArguments; j++)
                            {
                                string CurArgument = arguments[j];
                                if (CurArgument.Contains("replace:"))
                                {
                                    string[] subArguments = CurArgument.Split(':');
                                    modText = modText.Replace(subArguments[1], subArguments[2]);
                                }
                            }

                            wholeFile.Replace(varMarker, snipet.variables);
                            wholeFile.Replace(functionsMarker, snipet.functions);
                            wholeFile.AppendLine(modText);
                        }
                    }
                }
            }
        }

        public ShaderLoader(OpenTkProjectWindow mGameWindow)
        {
            this.gameWindow = mGameWindow;
        }
    }
}
