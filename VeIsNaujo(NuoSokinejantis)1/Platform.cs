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

//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Media;
//using System.Windows.Shapes;

namespace VeIsNaujo_NuoSokinejantis_1
{
    class Platform
    {
        //private Rectangle platformRectangle { get; set; }
        private Rectangle platformRectangle { get; set; }
        
        
         int left = 0;
         int top = 0;
      

        public Platform()
        { 
        }

        public Platform(int Width, int Height)
        {
            //this.platformRectangle = new Rectangle(Height, Width);
            this.platformRectangle = new Rectangle(Width, Height);
            //Rect myRect2 = new Rect();
            //myRect2.Size = new Size(50, 200);
            //myRect2.Location = new Point(300, 100);
            //platformRectangle.Fill = new SolidColorBrush(Colors.Orange);
          
            platformRectangle.Fill = new SolidColorBrush(Colors.Orange);
        }
        public void set(int leftPosition, int topPosition)
        {
            Canvas.SetLeft(platformRectangle, leftPosition);
            Canvas.SetTop(platformRectangle, topPosition);
            this.left = leftPosition;
            this.top = topPosition;
        }

        //public void updatePosition(int leftPosition, int topPosition)
        //{
        //    Canvas.SetLeft(platformRectangle, left + leftPosition);
        //    Canvas.SetTop(platformRectangle, top + topPosition);
        //}

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
    }
}
