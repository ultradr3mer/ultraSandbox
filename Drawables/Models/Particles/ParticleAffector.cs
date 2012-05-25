using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenTkProject.Drawables.Models.Paticles
{
    public abstract class ParticleAffector : GameObject
    {
        public ParticleAffector()
        {
        }

        public virtual void affect(ref Particle[] particles)
        {
        }
    }

    public class ParticleAffectorWind : ParticleAffector
    {
        Vector3 strength;
        public ParticleAffectorWind(Vector3 strength)
        {
            this.strength = strength;
        }

        public override void affect(ref Particle[] particles)
        {
            int parts = particles.Length;
            for (int i = 0; i < parts; i++)
			{
                particles[i].vector += strength;
			}
        }
    }

    public class ParticleAffectorFriction : ParticleAffector
    {
        float strength;
        public ParticleAffectorFriction(float strength)
        {
            this.strength = (1-strength);
        }

        public override void affect(ref Particle[] particles)
        {
            int parts = particles.Length;
            for (int i = 0; i < parts; i++)
            {
                particles[i].vector *= strength;
            }
        }
    }

    public class ParticleAffectorFloorKiller : ParticleAffector
    {
        float level;

        public ParticleAffectorFloorKiller(float level)
        {
            this.level = level;
        }

        public override void affect(ref Particle[] particles)
        {
            int parts = particles.Length;
            for (int i = 0; i < parts; i++)
            {
                if (particles[i].position.Y < level)
                    particles[i].alive = false;
            }

        }
    }
    public class ParticleAffectorLifeTimeKiller : ParticleAffector
    {
        public ParticleAffectorLifeTimeKiller(GameObject parent)
        {
            Parent = parent;
        }

        public override void affect(ref Particle[] particles)
        {
            int parts = particles.Length;
            float timestamp = gameWindow.frameTime;
            for (int i = 0; i < parts; i++)
            {
                if (particles[i].spawnTime + particles[i].lifeTime < timestamp)
                    particles[i].alive = false;
            }
        }
    }


    public class ParticleAffectorGravity : ParticleAffectorWind
    {
        public ParticleAffectorGravity(float strength):base(new Vector3(0,strength,0))
        {
        }
    }
}
