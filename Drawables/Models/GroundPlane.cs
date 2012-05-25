using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTkProject.Drawables.Models
{
    class GroundPlane:Model
    {
        public GroundPlane(Scene mScene) : base(mScene)
        {
        }

        public override void save(ref StringBuilder sb,int level)
        {
            saveChilds(ref sb, level);
        }
    }
}
