using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public class GameMeuWrapper : GameMenu
    {

        private SocialPage page;

        private ClickableComponent npcSlot;

        public string targetName;
    
        public GameMeuWrapper() : base(2) // 2 = SocialPage
        { 
            page = (SocialPage) GetCurrentPage();
            foreach (ClickableComponent tab in tabs)
            {
                tab.visible = false;
            }
        }
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            if (npcSlot != null && npcSlot.bounds.Y > 155 && npcSlot.bounds.Y < 807)
                b.Draw(Game1.staminaRect, new Rectangle(npcSlot.bounds.X, npcSlot.bounds.Y, npcSlot.bounds.Width, npcSlot.bounds.Height), Color.Green * 0.25f);

        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (Game1.activeClickableMenu is ProfileMenu menu)
            {
                Game1.activeClickableMenu = this;
                targetName = menu.Current.Character.Name;
            }

            if (npcSlot != null && npcSlot.containsPoint(x, y))
            {
                Game1.playSound("reward");
                Game1.player.Money -= 2500;
                Game1.drawObjectDialogue(I18n.CupidStore_Success(targetName));
                LoveFestivalPatches.boughtCupidArrow = true;
                exitThisMenu();
            }

            for (int i = 0; i < page.characterSlots.Count; i++)
            {
                if (!page.characterSlots[i].containsPoint(x, y))
                {
                    continue;
                }

                npcSlot = page.characterSlots[i];
            }

        }
    }
}
