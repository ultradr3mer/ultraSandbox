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
        protected virtual void setupMatrices(ViewInfo curView, Shader curShader)
        {
            GL.UniformMatrix4(curShader.projectionMatrixLocation, false, ref curView.projectionMatrix);
            GL.UniformMatrix4(curShader.modelviewMatrixLocation, false, ref curView.modelviewMatrix);
            GL.UniformMatrix4(curShader.rotationMatrixLocation, false, ref orientation);
            GL.UniformMatrix4(curShader.modelMatrixLocation, false, ref modelMatrix);
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

        public Shader activateMaterial(Material curMat)
        {
            int texunit = 0;
            Shader shader = curMat.shader;
            int handle = shader.handle;


            if (!shader.loaded)
                return shader;

            GL.UseProgram(handle);

            if (curMat.baseTexture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, curMat.baseTexture);
                GL.Uniform1(GL.GetUniformLocation(handle, "baseTexture"), texunit);
                texunit++;
            }

            if (curMat.base2Texture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, curMat.base2Texture);
                GL.Uniform1(GL.GetUniformLocation(handle, "base2Texture"), texunit);
                texunit++;
            }

            if (curMat.base3Texture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, curMat.base3Texture);
                GL.Uniform1(GL.GetUniformLocation(handle, "base3Texture"), texunit);
                texunit++;
            }

            if (curMat.normalTexture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, curMat.normalTexture);
                GL.Uniform1(GL.GetUniformLocation(handle, "normalTexture"), texunit);
                texunit++;
            }

            if (curMat.mirrorTexture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, curMat.mirrorTexture);
                GL.Uniform1(GL.GetUniformLocation(handle, "reflectionTexture"), texunit);
                texunit++;
            }

            if (curMat.emitTexture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, curMat.emitTexture);
                GL.Uniform1(GL.GetUniformLocation(handle, "emitTexture"), texunit);
                texunit++;
            }

            int env = 0;
            if (curMat.useEnv)
            {
                texunit = initEnvTextures(texunit, handle);
                env = 1;

                int envBasealpha = 0;
                if (curMat.envMapAlphaBaseTexture)
                    envBasealpha = 1;

                GL.Uniform1(shader.envMapAlphaBaseTexture, 1, ref envBasealpha);

                int envNormalalpha = 0;
                if (curMat.envMapAlphaNormalTexture)
                    envNormalalpha = 1;

                GL.Uniform1(shader.envMapAlphaBaseTexture, 1, ref envNormalalpha);

                GL.Uniform3(shader.envTintLocation, ref curMat.envMapTint);

                /*
                if (curMat.envMapTexture != 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                    GL.BindTexture(TextureTarget.Texture2D, curMat.envMapTexture);
                    GL.Uniform1(GL.GetUniformLocation(handle, "envMapTexture"), texunit);
                    texunit++;
                }
                 * */
            }
            GL.Uniform1(shader.useEnvLocation, 1, ref env);

            int emit = 0;
            if (curMat.useEmit)
            {
                emit = 1;

                int emitBasealpha = 0;
                if (curMat.emitMapAlphaBaseTexture)
                    emitBasealpha = 1;

                GL.Uniform1(shader.emitMapAlphaBaseTexture, 1, ref emitBasealpha);

                int emitNormalalpha = 0;
                if (curMat.emitMapAlphaNormalTexture)
                    emitNormalalpha = 1;

                GL.Uniform1(shader.emitMapAlphaNormalTexture, 1, ref emitNormalalpha);

                GL.Uniform3(shader.emitColorLocation, ref curMat.emitMapTint);

                /*
                if (curMat.envMapTexture != 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                    GL.BindTexture(TextureTarget.Texture2D, curMat.envMapTexture);
                    GL.Uniform1(GL.GetUniformLocation(handle, "envMapTexture"), texunit);
                    texunit++;
                }
                 * */
            }
            GL.Uniform1(shader.useEmitLocation, 1, ref emit);

            int spec = 0;
            if (curMat.useSpec)
            {
                spec = 1;

                int specBasealpha = 0;
                if (curMat.specMapAlphaBaseTexture)
                    specBasealpha = 1;

                GL.Uniform1(shader.specMapAlphaBaseTexture, 1, ref specBasealpha);

                int specNormalalpha = 0;
                if (curMat.specMapAlphaNormalTexture)
                    specNormalalpha = 1;

                GL.Uniform1(shader.specMapAlphaNormalTexture, 1, ref specNormalalpha);

                GL.Uniform3(shader.specColorLocation, ref curMat.specMapTint);

                GL.Uniform1(shader.specExpLocation, 1, ref curMat.specExp);

                /*
                if (curMat.envMapTexture != 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                    GL.BindTexture(TextureTarget.Texture2D, curMat.envMapTexture);
                    GL.Uniform1(GL.GetUniformLocation(handle, "envMapTexture"), texunit);
                    texunit++;
                }
                 * */
            }
            GL.Uniform1(shader.useSpecLocation, 1, ref spec);

            int transparency = 0;
            if (curMat.useAlpha)
            {
                transparency = 1;

                GL.Uniform1(shader.refSizeLocation, 1, ref curMat.refStrength);

                GL.Uniform1(shader.blurSizeLocation, 1, ref curMat.blurStrength);

                GL.Uniform1(shader.fresnelStrLocation, 1, ref curMat.fresnelStrength);

                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, this.Scene.backdropTextures[0]);
                GL.Uniform1(GL.GetUniformLocation(handle, "backColorTexture"), texunit);
                texunit++;

                /*
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, this.Scene.backdropTextures[1]);
                GL.Uniform1(GL.GetUniformLocation(handle, "backDepthTexture"), texunit);
                texunit++;
                */
            }

            GL.Uniform1(shader.useAlphaLocation, 1, ref transparency);

            if (curMat.useLight)
            {
                Scene.sunLight.activate(shader, this);

                GL.Uniform1(GL.GetUniformLocation(handle, "sunShadowTexture"), texunit);
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, Scene.sunFrameBuffer.DepthTexture);
                texunit++;

                GL.Uniform1(GL.GetUniformLocation(handle, "sunInnerShadowTexture"), texunit);
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, Scene.sunInnerFrameBuffer.DepthTexture);
                texunit++;


                float quality = 0.5f;
                
                if(distToCamera < 10)
                    quality += 0.5f;

                GL.Uniform1(shader.LightCountLocation, 1, ref Scene.lightCount);
                GL.Uniform1(shader.shadowQualityLocation, 1, ref quality);

                for (int i = 0; i < Scene.spotlights.Count; i++)
                {
                    Scene.spotlights[i].activate(shader, i,this);
                }


                GL.Uniform3(shader.eyePosLocation, ref Scene.eyePos);
                GL.Uniform1(shader.waterLevelLocation, 1, ref Scene.waterLevel);

                GL.Uniform1(GL.GetUniformLocation(handle, "shadowTexture"), texunit);
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, gameWindow.shadowFramebuffer.ColorTexture);
                texunit++;

                GL.Uniform1(GL.GetUniformLocation(handle, "noiseTexture"), texunit);
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, Scene.noiseTexture);
                texunit++;
            }

            if (curMat.noCull)
            {
                GL.Disable(EnableCap.CullFace);
            }
            else
            {
                GL.Enable(EnableCap.CullFace);
            }

            if (curMat.noDepthMask)
            {
                GL.DepthMask(false);
            }
            else
            {
                GL.DepthMask(true);
            }

            if (curMat.additive)
            {
                GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
            }
            else
            {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }

            return shader;
        }

        public Shader activateMaterialSSN(Material curMat)
        {
            int texunit = 0;
            int handle = curMat.ssnshader.handle;

            if (!curMat.ssnshader.loaded)
                return curMat.ssnshader;

            GL.UseProgram(handle);

            if (curMat.normalTexture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, curMat.normalTexture);
                GL.Uniform1(GL.GetUniformLocation(handle, "normalTexture"), texunit);
                texunit++;
            }

            return curMat.ssnshader;
        }

        public Shader activateMaterialSelection(Material curMat)
        {
            int texunit = 0;
            int handle = curMat.selectionshader.handle;

            if (!curMat.selectionshader.loaded)
                return curMat.selectionshader;

            GL.UseProgram(handle);

            if (curMat.normalTexture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + texunit);
                GL.BindTexture(TextureTarget.Texture2D, curMat.normalTexture);
                GL.Uniform1(GL.GetUniformLocation(handle, "normalTexture"), texunit);
                texunit++;
            }

            return curMat.selectionshader;
        }

        public Shader activateMaterialShadow(Material curMat)
        {
            int handle = curMat.shadowshader.handle;

            if (!curMat.shadowshader.loaded)
                return curMat.shadowshader;

            GL.UseProgram(handle);

            GL.Uniform1(curMat.shadowshader.LightCountLocation, 1, ref Scene.lightCount);
            GL.Uniform1(curMat.shadowshader.curLightLocation, 1, ref Scene.currentLight);

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

            vaoHandle = new int[meshes.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                Shader curShader = materials[i].shader;
                int shaderHandle = curShader.handle;

                int normalIndex = GL.GetAttribLocation(shaderHandle, "in_normal");
                int positionIndex = GL.GetAttribLocation(shaderHandle, "in_position");
                int tangentIndex = GL.GetAttribLocation(shaderHandle, "in_tangent");
                int textureIndex = GL.GetAttribLocation(shaderHandle, "in_texture");

                // GL3 allows us to store the vertex layout in a "vertex array object" (VAO).
                // This means we do not have to re-issue VertexAttribPointer calls
                // every time we try to use a different vertex layout - these calls are
                // stored in the VAO so we simply need to bind the correct VAO.

                GL.GenVertexArrays(1, out vaoHandle[i]);
                GL.BindVertexArray(vaoHandle[i]);

                if (normalIndex != -1)
                {
                    GL.EnableVertexAttribArray(normalIndex);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, meshes[i].normalVboHandle);
                    GL.VertexAttribPointer(normalIndex, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
                    //GL.BindAttribLocation(shaderHandle, 0, "in_normal");
                }

                if (positionIndex != -1)
                {
                    GL.EnableVertexAttribArray(positionIndex);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, meshes[i].positionVboHandle);
                    GL.VertexAttribPointer(positionIndex, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
                    //GL.BindAttribLocation(shaderHandle, 1, "in_position");
                }

                if (tangentIndex != -1)
                {
                    GL.EnableVertexAttribArray(tangentIndex);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, meshes[i].tangentVboHandle);
                    GL.VertexAttribPointer(tangentIndex, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
                    //GL.BindAttribLocation(shaderHandle, 2, "in_tangent");
                }

                if (textureIndex != -1)
                {
                    GL.EnableVertexAttribArray(textureIndex);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, meshes[i].textureVboHandle);
                    GL.VertexAttribPointer(textureIndex, 2, VertexAttribPointerType.Float, true, Vector2.SizeInBytes, 0);
                    //GL.BindAttribLocation(shaderHandle, 3, "in_texture");
                }


                GL.BindBuffer(BufferTarget.ElementArrayBuffer, meshes[i].eboHandle);

                GL.BindVertexArray(0);
            }

            gameWindow.checkGlError("CreateVAOs");
        }

        #endregion mesh management


        public Vector3 EmissionColor
        {
            get { return materials[0].emitMapTint; }
            set
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].emitMapTint = value;
                }
            }
        }
    }
}
