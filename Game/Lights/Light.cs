using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Jitter.Dynamics;
using OpenTkProject.Drawables;
using OpenTK.Graphics.OpenGL;
using OpenTkProject.Drawables.Models;

namespace OpenTkProject
{
    public class Light : Drawable
    {
        //public Vector3 lightDirection;

        //public Vector3 lightPosition;

        //public Framebuffer shadowMap;
        public ViewInfo viewInfo;

        public Matrix4 shadowMatrix;

        protected Matrix4 bias = new Matrix4(	
			0.5f, 0.0f, 0.0f, 0.0f, 
			0.0f, 0.5f, 0.0f, 0.0f,
			0.0f, 0.0f, 0.5f, 0.0f,
		0.5f, 0.5f, 0.5f, 1.0f);
		
        //private Vector4 ssposition;
        //private Vector4 ssdirection;

        public LightVolume drawable;

        /*
        public Light(Vector3 location, Vector3 direction, Vector3 color)
        {
            viewInfo = new ViewInfo(this);
            Position = location;
            pointingDirection = direction;
            this.colorRgb = color;
        }
        */

        public Light()
        {
        }

        public virtual void activate(Shader shader, Drawable drawable) { }

        public virtual void activateDeffered(Shader shader) { }
    }
}
