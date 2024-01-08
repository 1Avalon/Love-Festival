using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public class BillboardWrapper : Billboard
    {
        private ClickableTextureComponent selectedDay;

        public int day;

        private ClickableTextureComponent okButton;

        private int counter = 0;
        public BillboardWrapper() : base(false)
        {
            okButton = new ClickableTextureComponent(new Rectangle(0,0,0,0), Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 1f);
            exitFunction = () => 
            {
                if (selectedDay == null)
                    Game1.activeClickableMenu = new BillboardWrapper();
            };
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (selectedDay != null && okButton.containsPoint(x, y))
            {
                this.exitThisMenu();
                return;
            }

            foreach (ClickableTextureComponent calendarDay in calendarDays)
            {
                counter++;
                if (calendarDay.containsPoint(x, y) && counter > Game1.dayOfMonth && !Utility.isFestivalDay(counter, "winter"))
                {
                    selectedDay = calendarDay;
                    day = counter;
                    break;
                }
            }
            counter = 0;
        }
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen + 650, this.width, 200, false, true);
            Utility.drawTextWithShadow(b, "Choose a Date for your date & double click", Game1.dialogueFont, new Vector2(this.xPositionOnScreen + 200, this.yPositionOnScreen + 760), Color.Black);
            if (selectedDay != null)
            {
                b.Draw(Game1.staminaRect, selectedDay.bounds, Color.Green * 0.5f);
                okButton.bounds = new Rectangle(selectedDay.bounds.X + selectedDay.bounds.Width / 4, selectedDay.bounds.Y + selectedDay.bounds.Height / 4, selectedDay.bounds.Width, selectedDay.bounds.Height);
                okButton.draw(b);
            }
            drawMouse(b);

        }
    }
}
