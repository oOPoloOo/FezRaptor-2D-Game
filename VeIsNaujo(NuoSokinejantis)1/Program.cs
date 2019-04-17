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
    public partial class Program
    {
        Window mainWindow;
        Canvas layout;
        Text label;// iteration
        Text label2;// lives
        Rectangle tongue;
        //Rectangle snowflake;
        //Rectangle kvad;

        int tongueLeftPosition = 150;
        int snowflakeLeftPosition = 150;
        int snowflakeTopPosition = 50;
        int tongueWidth = 30;
        //int rectangleLfetPosition = 600;
        //int rectangleWidth = 30;


        //Mano parametrai
        int gravitacija = 30;
        bool pasokes;
        //bool eina = false;
        int jiega;
        int tongueBottomPosition; // 31 - at zemes, -199 - liecia virsum lubas, 72 - po zeme, -240 - virs lubu
        int zaidejoAukstis = 41;// buvo 200, kur naudojamas dar?
        int zaidejoPlotis = 40;
        //int rectBottom = 31;
        //int rectHeight = 100;



        GT.Timer joystickTimer = new GT.Timer(150);
        GT.Timer snowFlakeTimer = new GT.Timer(75);
        GT.Timer pasokimoTimer = new GT.Timer(30); // pasokimui, kad pastoviai tikrintu ar pasokes ar ne
        //DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()


        Random randomNumberGenerator = new Random();
        int score = 0;

        //-------------------------------------------------------------------------------------------------------------------
        //                                                         Vytenis
        //-------------------------------------------------------------------------------------------------------------------
        int basePositionTop = -60;
        int basePositionLeft = -60;
        int iteration = 0;
        int platformosId = 0;
        int trapId = 0;

        int lives = 3;

        Platfrom[] platformosMap = new Platfrom[1000]; //Vytenis
        Trap[] trapMap = new Trap[10]; //Vytenis
        //-------------------------------------------------------------------------------------------------------------------



        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            SetupUI();

            Canvas.SetLeft(tongue, tongueLeftPosition);
            Canvas.SetTop(tongue, tongueBottomPosition + zaidejoAukstis);// pakeista vietoj 200 - tongueBottomPosition + zaidejoAukstis

            /*Canvas.SetLeft(snowflake, snowflakeLeftPosition);
            Canvas.SetTop(snowflake, snowflakeTopPosition);*/


            //Canvas.SetLeft(kvad, rectangleLfetPosition);
            //Canvas.SetBottom(kvad, 0);



            joystickTimer.Tick += new GT.Timer.TickEventHandler(JoystickTimer_Tick);
            joystickTimer.Start();

            /*snowFlakeTimer.Tick += new GT.Timer.TickEventHandler(SnowflakeTimer_Tick);
            snowFlakeTimer.Start();*/

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
            background.Height = 272;// buvo 240
            background.Width = 800;// buvo 320

            layout.Children.Add(background);
            Canvas.SetLeft(background, 0);
            Canvas.SetTop(background, 0);

            //add the tongue
            tongue = new Rectangle(tongueWidth, zaidejoPlotis);// zaidejo plotis, buvo 40 pakeiciau i zaidejoPlotis
            tongue.Fill = new SolidColorBrush(Colors.Red);
            layout.Children.Add(tongue);

            //-------------------------------------------------------------------------------------------------------------------
            //                                                         Vytenis
            //-------------------------------------------------------------------------------------------------------------------
            //                                                      Map aprasymas
            //-------------------------------------------------------------------------------------------------------------------
            /*masyvas[n] = new Platfrom(plotis, aukstis);
              masyvas[n].set(NuoKaires, NuoVirsaus);*/
            //-------------------------------------------------------------------------------------------------------------------
            string[] map = new string[12];
            map[0] = "#####################################";
            map[1] = "#...................................#";
            map[2] = "#...................................#";
            map[3] = "#.......................######......#";
            map[4] = "#...####............................#";
            map[5] = "#...................................#";
            map[6] = "#....................@..............#";
            map[7] = "#.................########..........#";
            map[8] = "#...................................#";
            map[9] = "######....................###TT######";
            map[10] = ".....................################";
            map[11] = ".....####............................";

            for (int i = 0; i < map.Length; i++)
            {
                for (int j = 0; j < map[i].Length; j++)
                {
                    if (map[i][j] == '#')
                    {
                        platformosMap[platformosId] = new Platfrom(30, 30);
                        platformosMap[platformosId].set(basePositionLeft + j * 30, basePositionTop + i * 30);
                        platformosId++;
                    }
                    else if (map[i][j] == 'T')
                    {
                        trapMap[trapId] = new Trap(30, 30);
                        trapMap[trapId].set(basePositionLeft + j * 30, basePositionTop + i * 30);
                        trapId++;
                    }
                }
            }

            for (int i = 0; i < platformosId; i++)
            {
                layout.Children.Add(platformosMap[i].get());
            }
            for (int i = 0; i < trapId; i++)
            {
                layout.Children.Add(trapMap[i].get());
            }
            //-------------------------------------------------------------------------------------------------------------------

            //add the snowflake
            /*snowflake = new Rectangle(10, 10);
        snowflake.Fill = new SolidColorBrush(Colors.White);
        layout.Children.Add(snowflake);*/

            //kvad = new Rectangle(rectangleWidth, rectHeight);
            //kvad.Fill = new SolidColorBrush(Colors.Green);
            //layout.Children.Add(kvad);

            // add the text area
            label = new Text();
            label.Height = 272;// buvo 240
            label.Width = 480;// buvo 320
            label.ForeColor = Colors.White;
            label.Font = Resources.GetFont(Resources.FontResources.NinaB);

            layout.Children.Add(label);
            Canvas.SetLeft(label, 0);
            Canvas.SetTop(label, 0);

            //Tekstas y asies reiksmiu tikrinimui
            label2 = new Text();
            label2.Height = 272;// buvo 240
            label2.Width = 480;// buvo 320
            label2.ForeColor = Colors.White;
            label2.Font = Resources.GetFont(Resources.FontResources.NinaB);

            layout.Children.Add(label2);
            Canvas.SetLeft(label2, 0);// nieks nesikeicia, keiciant reiksmes
            Canvas.SetTop(label2, label.Font.Height);// nieks nesikeicia, keiciant reiksmes

            mainWindow.Child = layout;
        }


        /*void SnowflakeTimer_Tick(GT.Timer timer)
        {
            snowflakeTopPosition += 5;
            if (snowflakeTopPosition >= 240)
            {
                ResetSnowflake();
            }
            snowflakeLeftPosition += (randomNumberGenerator.Next(15) - 7);
            if (snowflakeLeftPosition < 10) snowflakeLeftPosition = 0;
            if (snowflakeLeftPosition > 470) snowflakeLeftPosition = 470;// buvo 300
            Canvas.SetLeft(snowflake, snowflakeLeftPosition);
            Canvas.SetTop(snowflake, snowflakeTopPosition);
        }

        private void ResetSnowflake()
        {
            snowflakeTopPosition = 50;
            snowflakeLeftPosition = randomNumberGenerator.Next(300) + 10;
        }*/

        void JoystickTimer_Tick(GT.Timer timer)
        {
            double y = joystick.GetPosition().Y;
            label.TextContent = "Iteration: " + iteration;
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
            // arAntZemes();//tikrina ar krist ar sustot, nes pasieke zeme

            //Orginalus kodas
            double x = joystick.GetPosition().X;// vietoj GetJoysticPosition().X;
            if (x < -0.3 && tongueLeftPosition > 0) // buvo 0.3 jutiklio skalė [-1;1], o ne [0;1]
            {
                //eina = true;
                //tongueLeftPosition -= 5;
                //rectangleLfetPosition += 20;
                basePositionLeft += 40;
            }
            else if (x > 0.7 && tongueLeftPosition < 800 - tongueWidth /*&& ((tongueLeftPosition + tongueWidth) != rectangleLfetPosition)*/)
            {
                //eina = true;
                //tongueLeftPosition += 5;
                //rectangleLfetPosition -= 20;
                basePositionLeft -= 40;
            }
            //-------------------------------------------------------------------------------------------------------------------
            //                                                         Vytenis
            //-------------------------------------------------------------------------------------------------------------------
            for (int i = 0; i < platformosId; i++)
            {
                platformosMap[i].updatePosition(basePositionLeft, basePositionTop);
            }
            for (int i = 0; i < trapId; i++)
            {
                trapMap[i].updatePosition(basePositionLeft, basePositionTop);
            }
            //-------------------------------------------------------------------------------------------------------------------
            Canvas.SetLeft(tongue, tongueLeftPosition);
            //Canvas.SetLeft(kvad, rectangleLfetPosition);
            //CheckForLanding();
            CheckForTraps();
            iteration++;
        }

        void PasokimoTimer_Tick(GT.Timer timer)
        {
            //Pasokimas
            if (pasokes)// buvo !pasokes
            {
                tongueBottomPosition -= jiega;
                jiega -= 2;
            }
            //sustoja krist pasiekus apacia
            if (tongueBottomPosition >= displayT43.Height - 41) // vietoj displayT43.He
            {
                tongueBottomPosition = displayT43.Height - 41;// vietoj displayT43.Height - zaidejoAukstis, rasiau 31, tada zaidejas ramiai nenustovi ant zemes ir pasoka, bet zeme prie ekrano virsaus
                pasokes = false;
            }
            else // jei sito nera, vaiksto, bet nepasoka ir nemigsi
            {
                tongueBottomPosition += 5;//kritimas, nuo lango virsaus zemina, kas 5
            }
            Canvas.SetTop(tongue, tongueBottomPosition);
            //CheckForLanding();
            //CheckForTraps();
        }

        void CheckForTraps()
        {
            //label2.TextContent = "Lives left: " + (lives);
            for (int i = 0; i < trapId; i++)
                //if (trapMap[i].getPositionTop() > tongueBottomPosition && (trapMap[i].getPositionTop() < tongueBottomPosition + zaidejoAukstis))
                //{
                if (trapMap[i].getPositionLeft() + 30 > tongueLeftPosition
                && trapMap[i].getPositionLeft() < tongueLeftPosition + tongueWidth)
                {
                    label2.TextContent = "Lives left: " + (lives);
                    lives--;
                }
            //}
        }

        void CheckForLanding()
        {
            if (snowflakeTopPosition > tongueBottomPosition && snowflakeTopPosition < tongueBottomPosition + zaidejoAukstis)
            {
                if (snowflakeLeftPosition + 10 >= tongueLeftPosition
                &&
                snowflakeLeftPosition <= tongueLeftPosition + tongueWidth)
                {
                    score++;
                    label2.TextContent = "Map dydis: " + (platformosId + trapId);
                    //ResetSnowflake();
                }

            }

        }


    }
}
