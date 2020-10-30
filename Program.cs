using System;
using System.Collections;
using System.Threading;
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
    public partial class Program
    {
        Window mainWindow;
        Canvas layout;
        Text label;// score
        Text label2;// tikrinimui
        Rectangle tongue;
        Rectangle snowflake;
        Rectangle kvad;

        int tongueLeftPosition = 150;
        int snowflakeLeftPosition = 150;
        int snowflakeTopPosition = 50;
        int tongueWidth = 30;
        int rectangleLfetPosition = 600;
        int rectangleWidth = 30;

        //Mano parametrai
        int gravitacija = 30;
        bool pasokes;
      
        int jiega;
        int tongueBottomPosition; // 31 - at zemes, -199 - liecia virsum lubas, 72 - po zeme, -240 - virs lubu
        int zaidejoAukstis = 41;
        int zaidejoPlotis = 40;
        int rectBottom = 31;
        int rectHeight = 100;

        GT.Timer joystickTimer = new GT.Timer(30);
        GT.Timer snowFlakeTimer = new GT.Timer(75);
        GT.Timer pasokimoTimer = new GT.Timer(30); // pasokimui, kad pastoviai tikrintu ar pasokes ar ne

        Random randomNumberGenerator = new Random();
        int score = 0;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            SetupUI();

            Canvas.SetLeft(tongue, tongueLeftPosition);
            Canvas.SetTop(tongue, tongueBottomPosition + zaidejoAukstis);
                       
            Canvas.SetLeft(snowflake, snowflakeLeftPosition);
            Canvas.SetTop(snowflake, snowflakeTopPosition);

            Canvas.SetLeft(kvad, rectangleLfetPosition);
            Canvas.SetBottom(kvad, 0);

            joystickTimer.Tick += new GT.Timer.TickEventHandler(JoystickTimer_Tick);
            joystickTimer.Start();

            snowFlakeTimer.Tick += new GT.Timer.TickEventHandler(SnowflakeTimer_Tick);
            snowFlakeTimer.Start();

            pasokimoTimer.Tick += new GT.Timer.TickEventHandler(PasokimoTimer_Tick);//Pasokimo taimeris
            pasokimoTimer.Start();


        }

        void SetupUI()
        {
            // initialize window
            mainWindow = displayT43.WPFWindow; // vietoj display - ziuret pagal auto generuota koda

            // setup the layout
            layout = new Canvas();
            Border background = new Border();
            background.Background = new SolidColorBrush(Colors.Black);// pikseliu matavimam rinktis balta
            background.Height = 272;
            background.Width = 800;

            layout.Children.Add(background);
            Canvas.SetLeft(background, 0);
            Canvas.SetTop(background, 0);

            //add the tongue
            tongue = new Rectangle(tongueWidth, zaidejoPlotis);
            tongue.Fill = new SolidColorBrush(Colors.Red);
            layout.Children.Add(tongue);

            //add the snowflake
            snowflake = new Rectangle(10, 10);
            snowflake.Fill = new SolidColorBrush(Colors.White);
            layout.Children.Add(snowflake);

            kvad = new Rectangle(rectangleWidth, rectHeight);
            kvad.Fill = new SolidColorBrush(Colors.Green);
            layout.Children.Add(kvad);

            // add the text area
            label = new Text();
            label.Height = 272;
            label.Width = 480;
            label.ForeColor = Colors.White;
            label.Font = Resources.GetFont(Resources.FontResources.NinaB);

            layout.Children.Add(label);
            Canvas.SetLeft(label, 0);
            Canvas.SetTop(label, 0);

            //Tekstas y asies reiksmiu tikrinimui
            label2 = new Text();
            label2.Height = 272;
            label2.Width = 480;
            label2.ForeColor = Colors.White;
            label2.Font = Resources.GetFont(Resources.FontResources.NinaB);

            layout.Children.Add(label2);
            Canvas.SetLeft(label2, 0);
            Canvas.SetTop(label2, label.Font.Height);

            mainWindow.Child = layout;
        }


        void SnowflakeTimer_Tick(GT.Timer timer)
        {
            snowflakeTopPosition += 5;
            if (snowflakeTopPosition >= 240)
            {
                ResetSnowflake();
            }
            snowflakeLeftPosition += (randomNumberGenerator.Next(15) - 7);
            if (snowflakeLeftPosition < 10) snowflakeLeftPosition = 0;
            if (snowflakeLeftPosition > 470) snowflakeLeftPosition = 470;
            Canvas.SetLeft(snowflake, snowflakeLeftPosition);
            Canvas.SetTop(snowflake, snowflakeTopPosition);
        }

        private void ResetSnowflake()
        {
            snowflakeTopPosition = 50;
            snowflakeLeftPosition = randomNumberGenerator.Next(300) + 10;
        }

        void JoystickTimer_Tick(GT.Timer timer)
        {
            double y = joystick.GetPosition().Y;
            label.TextContent = "Ekrano aukstis: " + displayT43.Height;
            //Pasokimas
             if (!pasokes)
            {
                if (y >= 0.7)
                {
                    pasokes = true;
                    jiega = gravitacija;
                }
            }
             Canvas.SetTop(tongue, tongueBottomPosition);// Pasokimo veiksmui isvest i ekrana
            
          
            //Orginalus kodas
            double x = joystick.GetPosition().X;// vietoj GetJoysticPosition().X;
            if (x < -0.3 && tongueLeftPosition > 0) // buvo 0.3 jutiklio skalė [-1;1], o ne [0;1]
            {
               
                rectangleLfetPosition += 20;
            }
            else if (x > 0.7 && tongueLeftPosition < 800 - tongueWidth && ((tongueLeftPosition + tongueWidth) != rectangleLfetPosition))
            {
                
                rectangleLfetPosition -= 20;
            }
            Canvas.SetLeft(tongue, tongueLeftPosition);
            Canvas.SetLeft(kvad, rectangleLfetPosition);
            CheckForLanding();
        }
        
        void PasokimoTimer_Tick(GT.Timer timer)
        {
            //Pasokimas
            if (pasokes)
            {
                tongueBottomPosition -= jiega;
                jiega -= 2;
            }
            //sustoja krist pasiekus apacia
            if (tongueBottomPosition >= displayT43.Height - 41) 
            {
                tongueBottomPosition = displayT43.Height - 41;
                pasokes = false;
            }
            else // jei sito nera, vaiksto, bet nepasoka ir nemigsi
            {
                tongueBottomPosition += 5;
            }
            Canvas.SetTop(tongue, tongueBottomPosition);
           
        }

        void CheckForLanding()
        {
            if (snowflakeTopPosition > tongueBottomPosition  && snowflakeTopPosition < tongueBottomPosition + zaidejoAukstis)
            {
                if( snowflakeLeftPosition + 10 >= tongueLeftPosition
                &&
                snowflakeLeftPosition <= tongueLeftPosition + tongueWidth)
            {
                score++;
                label2.TextContent = "Snowflakes Caught: " + score;
                ResetSnowflake();
            }

            }
               
        }
       
       
    }
}
