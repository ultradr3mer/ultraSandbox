using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTkProject.Game.Voxel;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTkProject.Drawables.Models;

namespace OpenTkProject.Drawables.Models.Paticles
{
    public class ParticleSystem : Model
    {
        Random rnd = new Random();

        private int spreadRadius;
        private int maxParticles = 700;

        protected Particle[] particles;

        private int particlesPerFrame = 30;
        private float particleLifeTime = 7;
        private float particleSize = 1f;

        public ParticleSystem(GameObject parent)
            : base(parent)
        {
            spreadRadius = 30;
            particles = new Particle[maxParticles];
        }

        public void generateParticles(int amnt)
        {
            int curPos = 0;

            for (int i = 0; i < amnt; i++)
            {
                bool placed = false;

                while (!placed)
                {
                    if (!particles[curPos].alive)
                    {
                        Vector3 randomPos = new Vector3((float)(0.5 - rnd.NextDouble()), (float)(rnd.NextDouble() * 0.5f), (float)(0.5 - rnd.NextDouble())) * spreadRadius * 2;
                        Particle curPat = new Particle(randomPos);

                        curPat.rendertype = rnd.Next(meshes.Length);
                        curPat.size = particleSize;
                        curPat.spawnTime = gameWindow.frameTime;
                        curPat.lifeTime = particleLifeTime;
                        curPat.alive = true;

                        particles[curPos] = curPat;

                        placed = true;
                    }

                    curPos++;

                    if (curPos == particles.Length)
                    {
                        return;
                    }
                }
            }
        }

        public override void update()
        {
            float frametime = gameWindow.frameTime;
            float lastFrameDur = gameWindow.lastFrameDuration;

            generateParticles(particlesPerFrame);

            foreach (var affector in scene.particleAffectors)
            {
                affector.affect(ref particles);
            }

            for (int i = 0; i < maxParticles; i++)
            {
                particles[i].position += particles[i].vector;
            }

            updateChilds();

        }

        public override void draw(ViewInfo curView, bool targetLayer)
        {
            if (vaoHandle != null && isVisible)
            {
                //Vector4 screenpos;
                for (int i = 0; i < vaoHandle.Length; i++)
                {
                    gameWindow.checkGlError("--uncaught ERROR drawing Model--" + meshes[i].name);

                    if (materials[i].propertys.useAlpha == targetLayer)
                    {
                        //Console.WriteLine("drawing: " + mMesh[i].name);

                        Material curMat = materials[i];
                        Shader shader = activateMaterial(ref curMat);
                        Mesh curMesh = meshes[i];

                        if (shader.loaded)
                        {
                            shader.insertUniform(Shader.Uniform.in_screensize, ref gameWindow.virtual_size);
                            shader.insertUniform(Shader.Uniform.in_rendersize, ref gameWindow.currentSize);
                            shader.insertUniform(Shader.Uniform.in_time, ref gameWindow.frameTime);
                            shader.insertUniform(Shader.Uniform.in_color, ref color);

                            //GL.Uniform1(curShader.nearLocation, 1, ref mGameWindow.mPlayer.zNear);
                            //GL.Uniform1(curShader.farLocation, 1, ref mGameWindow.mPlayer.zFar);

                            if (Scene != null)
                            {
                                setupMatrices(ref curView, ref shader, ref curMesh);
                            }

                            setSpecialUniforms(ref shader,ref curMesh);

                            GL.BindVertexArray(vaoHandle[i]);

                            for (int j = 0; j < particles.Length; j++)
                            {
                                Particle curPat = particles[j];
                                if (curPat.alive && curPat.rendertype == i)
                                {
                                    shader.insertUniform(Shader.Uniform.in_particlepos, ref curPat.position);
                                    shader.insertUniform(Shader.Uniform.in_particlesize, ref curPat.size);
                                    GL.DrawElements(BeginMode.Triangles, curMesh.indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
                                }
                            }


                            gameWindow.checkGlError("--Drawing ERROR--" + curMesh.name);
                        }
                    }
                }
            }
        }
    }

    public struct Particle
    {
        public Vector3 position;
        public Vector3 vector;
        public int rendertype;
        public float size;
        public float spawnTime;
        public float lifeTime;
        public bool alive;

        public Particle(Vector3 position)
        {
            this.position = position;
            rendertype = 0;
            vector = Vector3.Zero;
            size = 1;
            spawnTime = 0;
            lifeTime = 10;
            alive = false;
        }
    }
}
