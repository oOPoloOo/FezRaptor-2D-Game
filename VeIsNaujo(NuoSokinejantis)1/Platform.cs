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

namespace VeIsNaujo_NuoSokinejantis_1
{
    class Platform
    {
        private Rectangle platformRectangle { get; set; }
        
         int left = 0;
         int top = 0;
         int widthRect;
         int heightRect;
      
        public Platform()
        { 
        }

        public Platform(int Width, int Height)
        {
            this.platformRectangle = new Rectangle();
            this.platformRectangle.Height = Height;
            this.platformRectangle.Width = Width;
            widthRect = Width;
            heightRect = Height;
          
            platformRectangle.Fill = new SolidColorBrush(Colors.Orange);
        }
        public void set(int leftPosition, int topPosition)
        {
            Canvas.SetLeft(platformRectangle, leftPosition);
            Canvas.SetTop(platformRectangle, topPosition);
            this.left = leftPosition;
            this.top = topPosition;
        }
        
        public Rectangle get()
        {
            return platformRectangle;
        }
        public int posLeft()
        {
            return left;
        }
        public int posTop()
        {
            return top;
        }

        public int Height()
        {
            return heightRect;
        }
        public int Width()
        {
            return widthRect;
        }

        public void paintBlue()
        {
            platformRectangle.Fill = new SolidColorBrush(Colors.Blue);
        }
    }
}
