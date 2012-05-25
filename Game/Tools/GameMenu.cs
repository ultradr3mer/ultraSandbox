using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;
using OpenTkProject.Drawables.Models;

namespace OpenTkProject.Game.Tools
{
    class GameMenu : Tool
    {
        private Menu MenuUi;

        public GameMenu(Player parent, GameInput input)
            : base(parent, input)
        {
            weaponModel.isVisible = false;
            icon.isVisible = false;

            MenuUi = new Menu(this);
            MenuUi.isVisible = false;
            Scene.guis.Add(MenuUi);
        }

        public override void update()
        {
            if (Parent.tool == this)
            {
                MenuUi.isVisible = true;
                MenuUi.moveCursor(gameInput.move);

                // fire player fire
                bool K = gameInput.mouse[MouseButton.Left];
                if (K && !prevK)
                {
                    MenuUi.performClick();
                }
                else if (!K && prevK)
                {
                    //fireUp();
                }
                prevK = K;

                updateChilds();
            }
            else
            {
                MenuUi.isVisible = false;
            }
        }
    }
}
