using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTkProject.Drawables.Models;

namespace OpenTkProject.Drawables
{
    public class Drawable : GameObject
    {
        private float biggestMeshSphere;

        private List<string> materialList = new List<string> { };
        private List<string> meshList = new List<string> { };
        private List<string[]> textureList = new List<string[]> { };

        public float selectedSmooth;
        public float selected;

        public bool isVisible = true;

        public float boundingSphere = 1f;

        protected Material[] materials = new Material[0];

        public Vector2 shaderVector;

        protected Mesh[] meshes = new Mesh[0];

        protected Matrix4 modelMatrix = Matrix4.Identity;
        protected Matrix4 orientation = Matrix4.Identity;

        protected int[]
            vaoHandle = new int[0];

        public float distToCamera;

        public RenderLayer renderlayer;

        public enum RenderLayer
        {
            Solid,
            Both,
            Transparent
        }

        public override Scene Scene
        {
            get
            { return scene; }
            set
            {
                if (scene != null)
                    scene.drawables.Remove(this);
                scene = value;
                scene.drawables.Add(this);
            }
        }

        #region matices/position
        protected virtual void setupMatrices(ref ViewInfo curView,ref Shader shader, ref Mesh curMesh)
        {
            shader.insertUniform(Shader.Uniform.projection_matrix, ref curView.projectionMatrix);
            shader.insertUniform(Shader.Uniform.modelview_matrix, ref curView.modelviewMatrix);
            shader.insertUniform(Shader.Uniform.rotation_matrix, ref orientation);
            shader.insertUniform(Shader.Uniform.model_matrix, ref modelMatrix);
        }

        public override Matrix4 Orientation
        {
            get { return orientation; }
            set
            {
                if (orientation != value)
                {
                    orientation = value;
                    Vector4 dir = new Vector4(0, 1, 0, 0);

                    pointingDirection = GenericMethods.Mult(dir, value).Xyz;

                    //wasUpdated = true;
                }
            }
        }

        public override Vector3 PointingDirection
        {
            get { return pointingDirection; }
            set
            {
                if (pointingDirection != value)
                {
                    pointingDirection = value;
                    orientation = GenericMethods.MatrixFromVector(value);

                    //wasUpdated = true;
                }
            }
        }

        public Matrix4 ModelMatrix
        {
            get { return modelMatrix; }
            set
            {
                if (modelMatrix != value)
                {
                    modelMatrix = value;
                    position = GenericMethods.Mult(new Vector4(0, 0, 0, 1), value).Xyz;

                    //wasUpdated = true;
                }
            }
        }

        public override Vector3 Position
        {
            get { return position; }
            set
            {
                if (position != value)
                {
                    position = value;
                    updateModelMatrix();

                    //wasUpdated = true;
                }
            }
        }

        public override Vector3 Size
        {
            get { return size; }
            set
            {
                if (size != value)
                {
                    size = value;
                    updateModelMatrix();
                    updateBSphere();

                    wasUpdated = true;
                }
            }
        }

        protected virtual void updateBSphere()
        {
            if (size.X >= size.Y && size.X >= size.Z)
            {
                boundingSphere = biggestMeshSphere * size.X;
                return;
            }

            if (size.Y >= size.X && size.Y >= size.Z)
            {
                boundingSphere = biggestMeshSphere * size.Y;
                return;
            }
            if (size.Z >= size.Y && size.Z >= size.X)
            {
                boundingSphere = biggestMeshSphere * size.Z;
                return;
            }

            boundingSphere = biggestMeshSphere * size.X;
        }

        private void updateModelMatrix()
        {
            //modelMatrix = Matrix4.Identity;
            Matrix4 scaleMatrix = Matrix4.Scale(Size);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);

            //Matrix4.Mult(ref translationMatrix, ref modelMatrix, out modelMatrix);
            Matrix4.Mult(ref scaleMatrix, ref translationMatrix, out modelMatrix);
            //Matrix4.Mult(ref orientation, ref modelMatrix, out modelMatrix);
        }

        #endregion matices/position

        #region material

        public List<string> Materials
        {
            get
            {
                return materialList;
            }
            set
            {
                materialList = value;
                int materialAmnt = value.Count;
                materials = new Material[materialAmnt];
                for (int i = 0; i < materialAmnt; i++)
                {
                    materials[i] = gameWindow.materialLoader.getMaterial(materialList[i]);
                }
            }
        }

        public void setMaterial(string name)
        {
            materialList.Add(name);

            materials = new Material[] { gameWindow.materialLoader.getMaterial(name) };
        }

        internal void addMaterial(string name)
        {
            materialList.Add(name);

            Material[] tmpMaterials = new Material[materials.Length + 1];
            for (int i = 0; i < materials.Length; i++)
            {
                tmpMaterials[i] = materials[i];
            }
            tmpMaterials[materials.Length] = gameWindow.materialLoader.getMaterial(name);

            materials = tmpMaterials;
        }

        public Shader activateMaterial(ref Material curMat)
        {
            int texunit = 0;
            Shader shader = curMat.shader;
            int handle = shader.handle;
            Material.Propertys propertys = curMat.propertys;


            if (!shader.loaded)
                return shader;

            GL.UseProgram(handle);

            activateTexture(Material.TexType.baseTexture,ref curMat, ref texunit, handle);
            activateTexture(Material.TexType.base2Texture, ref curMat, ref texunit, handle);
            activateTexture(Material.TexType.base3Texture, ref curMat, ref texunit, handle);

            //activateTexture(Material.TexType.normalTexture, ref curMat, ref texunit, handle);
            activateTexture(Material.TexType.definfoTexture, ref curMat, ref texunit, handle);
            activateTexture(Material.TexType.reflectionTexture, ref curMat, ref texunit, handle);

            activateWorldTexture(Material.WorldTexture.reflectionMap, ref texunit, handle);

            shader.insertUniform(Shader.Uniform.in_eyepos, ref Scene.eyePos);
            activateWorldTexture(Material.WorldTexture.lightMap, ref texunit, handle);

            int emit = 0;
            if (propertys.useEmit)
            {
                emit = 1;

                //activateTexture(Material.TexType.emitTexture, ref curMat, ref texunit, handle);
                shader.insertUniform(Shader.Uniform.in_emitcolor, ref propertys.emitMapTint);

                /*
                int emitBasealpha = 0;
                if (propertys.emitMapAlphaBaseTexture)
                    emitBasealpha = 1;

                int emitNormalalpha = 0;
                if (propertys.emitMapAlphaNormalTexture)
                    emitNormalalpha = 1;

                shader.insertUniform(Shader.Uniform.emit_a_normal, ref emitNormalalpha);
                shader.insertUniform(Shader.Uniform.emit_a_base, ref emitBasealpha);
                shader.insertUniform(Shader.Uniform.in_emitcolor, ref propertys.emitMapTint);
                 
                if (curMat.envMapTexture != 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                    GL.BindTexture(TextureTarget.Texture2D, curMat.envMapTexture);
                    GL.Uniform1(GL.GetUniformLocation(handle, "envMapTexture"), texunit);
                    texunit++;
                }
                 * */
            }
            shader.insertUniform(Shader.Uniform.use_emit, ref emit);

            /*
            int transparency = 0;
            if (propertys.useAlpha)
            { 
                transparency = 1;

                shader.insertUniform(Shader.Uniform.ref_size, ref propertys.refStrength);
                shader.insertUniform(Shader.Uniform.blur_size, ref propertys.blurStrength);
                shader.insertUniform(Shader.Uniform.fresnel_str, ref propertys.fresnelStrength);

                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, this.Scene.backdropTextures[0]);
                GL.Uniform1(GL.GetUniformLocation(handle, "backColorTexture"), texunit);
                texunit++;

                
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, this.Scene.backdropTextures[1]);
                GL.Uniform1(GL.GetUniformLocation(handle, "backDepthTexture"), texunit);
                texunit++;
                
            }
            shader.insertUniform(Shader.Uniform.use_alpha, ref transparency);
            */



            if (propertys.noCull)
            {
                GL.Disable(EnableCap.CullFace);
            }
            else
            {
                GL.Enable(EnableCap.CullFace);
            }

            if (propertys.noDepthMask)
            {
                GL.DepthMask(false);
            }
            else
            {
                GL.DepthMask(true);
            }

            if (propertys.additive)
            {
                GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
            }
            else
            {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }

            return shader;
        }

        private void activateWorldTexture(Material.WorldTexture type, ref int texunit, int handle)
        {
            string name = type.ToString();
            int texid = scene.getTextureId(type);

            if (texid != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, texid);
                GL.Uniform1(GL.GetUniformLocation(handle, name), texunit);
                texunit++;
            }
        }

        private static void activateTexture(Material.TexType type, ref Material curMat, ref int texunit, int handle)
        {
            string name = type.ToString();
            int texid = curMat.getTextureId(type);

            if (texid != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, texid);
                GL.Uniform1(GL.GetUniformLocation(handle, name), texunit);
                texunit++;
            }
        }

        public Shader activateMaterialNormal(Material curMat)
        {
            int texunit = 0;
            Shader shader = curMat.ssnshader;
            int handle = shader.handle;


            if (!shader.loaded)
                return shader;

            GL.UseProgram(handle);

            shader.insertUniform(Shader.Uniform.in_specexp, ref curMat.propertys.specExp);

            int normalTexture = curMat.getTextureId(Material.TexType.normalTexture);
            if (normalTexture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, normalTexture);
                GL.Uniform1(GL.GetUniformLocation(handle, "normalTexture"), texunit);
                texunit++;
            }

            return shader;
        }

        public Shader activateMaterialSelection(Material curMat)
        {
            int texunit = 0;
            int handle = curMat.selectionshader.handle;

            if (!curMat.selectionshader.loaded)
                return curMat.selectionshader;

            GL.UseProgram(handle);

            activateTexture(Material.TexType.normalTexture , ref curMat, ref texunit, handle);

            return curMat.selectionshader;
        }

        public Shader activateMaterialShadow(Material curMat)
        {
            Shader shader = curMat.shadowshader;
            int handle = curMat.shadowshader.handle;

            if (!shader.loaded)
                return shader;

            GL.UseProgram(handle);

            shader.insertUniform(Shader.Uniform.in_no_lights, ref Scene.lightCount);
            shader.insertUniform(Shader.Uniform.curLight, ref Scene.currentLight);

            return curMat.shadowshader;
        }

        #endregion material

        #region textures

        protected int initEnvTextures(int texunit, int handle)
        {
            for (int i = 0; i < 6; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, Scene.envTextures[i]);
                GL.Uniform1(GL.GetUniformLocation(handle, "EnvTexture" + (i + 1)), texunit);
                texunit++;
            }
            return texunit;
        }

        protected void initTextures(int[] texures, int handle, string target)
        {
            for (int i = 0; i < texures.Length; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                GL.BindTexture(TextureTarget.Texture2D, texures[i]);
                GL.Uniform1(GL.GetUniformLocation(handle, target + (i + 1)), i);
            }
        }

        #endregion textures

        #region draw (virtual)
        public virtual void draw()
        {
        }

        public virtual void draw(ViewInfo curView)
        {
        }

        public virtual void draw(ViewInfo curView, bool transparencyLayer)
        {
        }

        public virtual void draw(ViewInfo curView, Shader curShader)
        {
        }

        public virtual void drawSelection(ViewInfo curView)
        {
        }

        public virtual void drawNormal(ViewInfo curView)
        {
        }

        public virtual void drawDefInfo(ViewInfo curView)
        {
        }

        public virtual void drawShadow(ViewInfo curView)
        {
        }

        #endregion draw (virtual)

        #region mesh management

        public List<string> Meshes
        {
            get
            {
                return meshList;
            }
            set
            {
                meshList = value;
                int meshAmnt = value.Count;
                meshes = new Mesh[meshAmnt];
                for (int i = 0; i < meshAmnt; i++)
                {
                    Mesh newMesh = gameWindow.meshLoader.getMesh(meshList[i]);

                    if (newMesh.boundingSphere > biggestMeshSphere)
                        biggestMeshSphere = newMesh.boundingSphere;

                    meshes[i] = newMesh;
                }

                updateBSphere();

                CreateVAOs();
            }
        }

        public void setMesh(string name)
        {
            setMesh(gameWindow.meshLoader.getMesh(name));
        }

        public void setMesh(Mesh newMesh)
        {
            meshList = new List<string> { newMesh.name };

            meshes = new Mesh[1];
            meshes[0] = newMesh;

            biggestMeshSphere = newMesh.boundingSphere;

            updateBSphere();

            CreateVAOs();
        }

        public void addMesh(string name)
        {
            addMesh(gameWindow.meshLoader.getMesh(name));
        }

        public virtual void addMesh(Mesh newMesh)
        {
            meshList.Add(newMesh.name);

            Mesh[] tmpMesh = new Mesh[meshes.Length + 1];
            for (int i = 0; i < meshes.Length; i++)
            {
                tmpMesh[i] = meshes[i];
            }
            tmpMesh[meshes.Length] = newMesh;

            meshes = tmpMesh;

            if (newMesh.boundingSphere > biggestMeshSphere)
                biggestMeshSphere = newMesh.boundingSphere;

            updateBSphere();

            CreateVAOs();
        }

        public void CreateVAOs()
        {
            gameWindow.checkGlError("uncaughtERROR");

            int [] newHandle = new int[meshes.Length];
            for (int i = 0; i < newHandle.Length && i < vaoHandle.Length; i++)
            {
                newHandle[i] = vaoHandle[i];
            }
            vaoHandle = newHandle;

            for (int i = 0; i < meshes.Length; i++)
            {
                Shader curShader = materials[i].shader;
                Mesh curMesh = meshes[i];
                int shaderHandle = curShader.handle;

                int normalIndex = GL.GetAttribLocation(shaderHandle, "in_normal");
                int positionIndex = GL.GetAttribLocation(shaderHandle, "in_position");
                int tangentIndex = GL.GetAttribLocation(shaderHandle, "in_tangent");
                int textureIndex = GL.GetAttribLocation(shaderHandle, "in_texture");

                // GL3 allows us to store the vertex layout in a "vertex array object" (VAO).
                // This means we do not have to re-issue VertexAttribPointer calls
                // every time we try to use a different vertex layout - these calls are
                // stored in the VAO so we simply need to bind the correct VAO.

                if (vaoHandle[i] == 0)
                {
                    GL.GenVertexArrays(1, out vaoHandle[i]);
                }

                GL.BindVertexArray(vaoHandle[i]);

                int affectingBonesCount = curMesh.affectingBonesCount;
                for (int j = 0; j < affectingBonesCount; j++)
                {
                    int boneIdIndex = GL.GetAttribLocation(shaderHandle, "in_joint_"+j);
                    int boneWeightIndex = GL.GetAttribLocation(shaderHandle, "in_weight_"+j);

                    if (boneIdIndex != -1)
                    {
                        GL.EnableVertexAttribArray(boneIdIndex);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, curMesh.boneIdVboHandles[j]);
                        GL.VertexAttribPointer(boneIdIndex, 1, VertexAttribPointerType.Int, false, sizeof(int), 0);
                    }

                    if (boneWeightIndex != -1)
                    {
                        GL.EnableVertexAttribArray(boneWeightIndex);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, curMesh.boneWeightVboHandles[j]);
                        GL.VertexAttribPointer(boneWeightIndex, 1, VertexAttribPointerType.Float, true, sizeof(float), 0);
                    }
                }

                if (normalIndex != -1)
                {
                    GL.EnableVertexAttribArray(normalIndex);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, curMesh.normalVboHandle);
                    GL.VertexAttribPointer(normalIndex, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
                    //GL.BindAttribLocation(shaderHandle, 0, "in_normal");
                }

                if (positionIndex != -1)
                {
                    GL.EnableVertexAttribArray(positionIndex);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, curMesh.positionVboHandle);
                    GL.VertexAttribPointer(positionIndex, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
                    //GL.BindAttribLocation(shaderHandle, 1, "in_position");
                }

                if (tangentIndex != -1)
                {
                    GL.EnableVertexAttribArray(tangentIndex);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, curMesh.tangentVboHandle);
                    GL.VertexAttribPointer(tangentIndex, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
                    //GL.BindAttribLocation(shaderHandle, 2, "in_tangent");
                }

                if (textureIndex != -1)
                {
                    GL.EnableVertexAttribArray(textureIndex);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, curMesh.textureVboHandle);
                    GL.VertexAttribPointer(textureIndex, 2, VertexAttribPointerType.Float, true, Vector2.SizeInBytes, 0);
                    //GL.BindAttribLocation(shaderHandle, 3, "in_texture");
                }


                GL.BindBuffer(BufferTarget.ElementArrayBuffer, curMesh.eboHandle);

                GL.BindVertexArray(0);
            }

            gameWindow.checkGlError("CreateVAOs");
        }

        #endregion mesh management


        public Vector3 EmissionColor
        {
            get { return materials[0].propertys.emitMapTint; }
            set
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].propertys.emitMapTint = value;
                }
            }
        }
    }

    public class UniformPairList
    {
        public List<Shader.Uniform> uniforms = new List<Shader.Uniform> { };
        public List<object> objects = new List<object> { };

        public UniformPairList(Shader.Uniform uni1, object obj1, Shader.Uniform uni2, object obj2)
        {
            objects.Add(obj1);
            objects.Add(obj2);

            uniforms.Add(uni1);
            uniforms.Add(uni2);
        }

        public UniformPairList(Shader.Uniform uni1, object obj1)
        {
            objects.Add(obj1);

            uniforms.Add(uni1);
        }

        internal void insert(ref Shader shader)
        {
            int length = uniforms.Count;
            for (int i = 0; i < length; i++)
            {
                shader.insertGenUniform(uniforms[i], objects[i]);
            }
        }
    }
}
