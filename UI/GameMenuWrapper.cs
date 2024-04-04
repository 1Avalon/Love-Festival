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

namespace LoveFestival.UI
{
    public class GameMenuWrapper : GameMenu
    {

        private SocialPage page;

        private ClickableComponent npcSlot;

        private string targetName;

        private string uiHint = I18n.CupidStore_Menu_UIHint(); //TODO padding
    
        public GameMenuWrapper() : base(2) // 2 = SocialPage
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
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen / 2 + height, this.width + 38, 200, false, true);
            Utility.drawTextWithShadow(b, uiHint, Game1.dialogueFont, new Vector2(this.xPositionOnScreen + this.width / 3f, this.yPositionOnScreen + height + 30), Color.Black);
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

            if (npcSlot != null && npcSlot.containsPoint(x, y) && npcSlot.bounds.Y > 155 && npcSlot.bounds.Y < 807)
            {
                Game1.playSound("reward");
                Game1.player.Money -= 2500;
                Game1.drawObjectDialogue(I18n.CupidStore_Success(targetName));
                LoveFestivalPatches.boughtCupidArrow = true;
                ModEntry.multiplier = new FriendshipMultiplier(targetName);
                exitThisMenu();
            }

            for (int i = 0; i < page.characterSlots.Count; i++)
            {
                if (!page.characterSlots[i].containsPoint(x, y))
                {
                    continue;
                }

                ClickableComponent slot = page.characterSlots[i];
                if (slot.bounds.Y > 155 && slot.bounds.Y < 807)
                    npcSlot = slot;
            }

        }
    }
}
