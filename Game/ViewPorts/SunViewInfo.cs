using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenTkProject.Drawables
{
    public class SunViewInfo : ViewInfo
    {
        float widith;

        public SunViewInfo(GameObject parent, float widith, float height)
            : base(parent)
        {
            this.widith = widith;
            projectionMatrix = Matrix4.CreateOrthographic(widith, widith, -height * 0.5f, height * 0.5f);
        }

        public override void update()
        {
            position = Parent.Position;
            PointingDirection = Parent.PointingDirection;

            modelviewMatrix = Matrix4.LookAt(position, position+PointingDirection, new Vector3(0, 1, 0));

            generateViewProjectionMatrix();
        }
        
        public override bool frustrumCheck(Drawable drawable)
        {
            Vector4 vSpacePos = GenericMethods.Mult(new Vector4(drawable.Position, 1), modelviewProjectionMatrix);

            float range = drawable.boundingSphere * 2f / widith;

            if (float.IsNaN(range) || float.IsInfinity(range))
                return false;

            vSpacePos /= vSpacePos.W;

            return (
                vSpacePos.X < (1f + range) && vSpacePos.X > -(1f + range) &&
                vSpacePos.Y < (1f + range) && vSpacePos.Y > -(1f + range) &&
                vSpacePos.Z < (1f) && vSpacePos.Z > -(1f)
                );
        }
      
    }
}
