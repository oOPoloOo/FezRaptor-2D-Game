using System;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace SokinejantisSniegutis
{
    class Platfrom
    {
        private Rectangle platformRectangle { get; set; }
        int left = 0;
        int top = 0;

        public Platfrom()
        { }

        public Platfrom(int Width, int Height)
        {
            this.platformRectangle = new Rectangle(Width, Height);
            platformRectangle.Fill = new SolidColorBrush(Colors.Orange);
        }
        public void set(int leftPosition, int topPosition)
        {
            Canvas.SetLeft(platformRectangle, leftPosition);
            Canvas.SetTop(platformRectangle, topPosition);
            this.left = leftPosition;
            this.top = topPosition;
        }

        public void updatePosition(int leftPosition, int topPosition)
        {
            Canvas.SetLeft(platformRectangle, left + leftPosition);
            Canvas.SetTop(platformRectangle, top + topPosition);
        }

        public Rectangle get()
        {
            return platformRectangle;
        }

    }
}
