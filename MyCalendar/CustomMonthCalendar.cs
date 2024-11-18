using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCalendar
{
    public class CustomMonthCalendar : MonthCalendar
    {
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_KEYDOWN = 0x0100;


        protected override void WndProc(ref Message m)
        {

            // Prevent all navigation events
            if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_LBUTTONDBLCLK || m.Msg == WM_KEYDOWN)
            {
                // Check if the event is related to navigation
                if (IsNavigationEvent(m))
                {
                    return;
                }
            }

            base.WndProc(ref m);
        }

        private bool IsNavigationEvent(Message m)
        {
            // Check for mouse clicks in the title bar or specific keystrokes
            if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_LBUTTONDBLCLK)
            {
                // ... (your existing mouse click check)
            }
            else if (m.Msg == WM_KEYDOWN)
            {
                // ... (your existing keystroke check)
            }

            // Additional checks for other potential navigation events
            // For example, you could check for messages related to scrolling or
            // using reflection to inspect the control's internal state.

            return true; // Assume it's a navigation event if not handled above
        }
    }

}