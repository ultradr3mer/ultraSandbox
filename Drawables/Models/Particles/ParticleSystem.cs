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

                    if (materials[i].useAlpha == targetLayer)
                    {
                        //Console.WriteLine("drawing: " + mMesh[i].name);

                        Shader curShader = activateMaterial(materials[i]);

                        if (curShader.loaded)
                        {
                            GL.Uniform2(curShader.screenSizeLocation, ref gameWindow.virtual_size);
                            GL.Uniform2(curShader.renderSizeLocation, ref gameWindow.currentSize);
                            GL.Uniform1(curShader.timeLocation, 1, ref gameWindow.frameTime);

                            GL.Uniform4(curShader.colorLocation, ref color);

                            //GL.Uniform1(curShader.nearLocation, 1, ref mGameWindow.mPlayer.zNear);
                            //GL.Uniform1(curShader.farLocation, 1, ref mGameWindow.mPlayer.zFar);

                            if (Scene != null)
                            {
                                setupMatrices(curView, curShader);
                            }

                            setSpecialUniforms(curShader);

                            GL.BindVertexArray(vaoHandle[i]);

                            for (int j = 0; j < particles.Length; j++)
                            {
                                Particle curPat = particles[j];
                                if (curPat.alive && curPat.rendertype == i)
                                {
                                    GL.Uniform3(curShader.particlePos, ref curPat.position);
                                    GL.Uniform1(curShader.particleSize, 1, ref curPat.size);
                                    GL.DrawElements(BeginMode.Triangles, meshes[i].indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
                                }
                            }
                            

                            gameWindow.checkGlError("--Drawing ERROR--" + meshes[i].name);
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
