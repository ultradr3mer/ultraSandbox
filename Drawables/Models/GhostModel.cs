using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTkProject.Drawables.Models
{
    public class GhostModel : Model
    {
        public GhostModel(GameObject parent) : base(parent)
        {
        }

        public override void draw(ViewInfo curView, bool targetLayer)
        {
        }

        public override void drawShadow(ViewInfo curView)
        {
        }

        public override void drawNormal(ViewInfo curView)
        {
        }

        public override void update()
        {
            updateSelection();
            updateChilds();
        }
    }
}
