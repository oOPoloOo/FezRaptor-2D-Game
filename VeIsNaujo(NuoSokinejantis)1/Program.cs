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

namespace VeIsNaujo_NuoSokinejantis_1
{
    public partial class Program
    {
        Window mainWindow;
        Canvas layout;
        Text label;// score
        Text label2;// tikrinimui
        Rectangle tongue;
        Rectangle platforma;
         Rectangle snowflake;


        
        int tongueLeftPosition = 250;
        int snowflakeLeftPosition = 150;
        int snowflakeTopPosition = 50;
        int tongueWidth = 30;

        //Mano parametrai
        int pasokimoJiega = 30;// 30 orginaliai veikia gerai
        bool pasokes = true;
        int jega;
        int tongueTopPosition; // 31 - at zemes, -199 - liecia virsum lubas, 72 - po zeme, -240 - virs lubu
        int zaidejoAukstis = 30;// buvo 200, kur naudojamas dar?
       

        //Platforma
        int platLeftPosition=100;
        int platTOPPosition = 142;
        int platAukstis = 40;// 30 orginaliai veikia gerai
        int platPlotis = 150;
        bool antPavirsiaus = false;
        bool sustojesK = false;
        bool sustojesD = false;
        

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
            Canvas.SetTop(tongue, tongueTopPosition );

            Canvas.SetLeft(snowflake, snowflakeLeftPosition);
            Canvas.SetTop(snowflake, snowflakeTopPosition);
            
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
            background.Height = 272;// buvo 240
            background.Width = 480;// buvo 320

            layout.Children.Add(background);
            Canvas.SetLeft(background, 0);
            Canvas.SetTop(background, 0);

            //add the tongue
            tongue = new Rectangle(tongueWidth, zaidejoAukstis);
            tongue.Fill = new SolidColorBrush(Colors.Red);
            layout.Children.Add(tongue);

            //plat
            platforma = new Rectangle(platPlotis, platAukstis);
            platforma.Fill = new SolidColorBrush(Colors.Purple);
            layout.Children.Add(platforma);
            Canvas.SetLeft(platforma, platLeftPosition);
            Canvas.SetTop(platforma, platTOPPosition);//??

          
            
            

            //add the snowflake
            snowflake = new Rectangle(10, 10);
            snowflake.Fill = new SolidColorBrush(Colors.White);
            layout.Children.Add(snowflake);

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
            Canvas.SetLeft(label2, 0);
            Canvas.SetTop(label2, label.Font.Height);

            mainWindow.Child = layout;
        }

        void ColliderSide()
        {
           
           
            // tikrina virsutines ribas sonu kolizijai
            //if (tongueTopPosition + zaidejoAukstis  == platTOPPosition + platAukstis && tongueTopPosition  == platTOPPosition) 
            if (tongueTopPosition + zaidejoAukstis <= platTOPPosition + platAukstis && tongueTopPosition >= platTOPPosition) 
            {
                

                    if ((tongueLeftPosition + tongueWidth) == platLeftPosition)// tikrinu riba is kaires
                    {
                        tongueLeftPosition = platLeftPosition - tongueWidth;
                        sustojesD = true;
                    }
                    else sustojesD = false;


                    if (tongueLeftPosition == platLeftPosition + platPlotis )// tikrinu is desines
                    {
                        tongueLeftPosition = platLeftPosition + platPlotis ;
                        sustojesK = true;
                    }
                    else sustojesK = false;
              

            }
            else
            { 
             sustojesD = false;
             sustojesK = false;
            }
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
            if (snowflakeLeftPosition > 470) snowflakeLeftPosition = 470;// buvo 300
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
            label.TextContent = "tongueTopPosition: " + tongueTopPosition;
            label2.TextContent = "platTOPPosition: " + platTOPPosition;
            //Pasokimas
            if (!pasokes)
            {
                if (y >= 0.7)
                {
                    pasokes = true;
                    antPavirsiaus = false;
                    jega = pasokimoJiega;
                }
            }
            Canvas.SetTop(tongue, tongueTopPosition ); 
            
            //Orginalus kodas
            double x = joystick.GetPosition().X;// vietoj GetJoysticPosition().X;
            if (x < -0.3 && tongueLeftPosition > 0 && !sustojesK) // buvo 0.3 jutiklio skalė [-1;1], o ne [0;1]
            {
                tongueLeftPosition -= 5;
            }
             else if (x > 0.7 && tongueLeftPosition < displayT43.Width - tongueWidth && !sustojesD)
            {
                tongueLeftPosition += 5;
            }
            Canvas.SetLeft(tongue, tongueLeftPosition);
            CheckForLanding();
           
          
        }

        void PasokimoTimer_Tick(GT.Timer timer)
        {
            //Pasokimas
            if (pasokes)
            {
                tongueTopPosition -= jega;
                jega -= 4;// orginaliai 2 veikia gerai, 3 - veikia, bet labau matosi sonu problema, 1 - visiskai neapti
            }

            //if ((tongueLeftPosition + tongueWidth) >= platLeftPosition && (tongueLeftPosition + tongueWidth) <= platLeftPosition + platPlotis)// Plaukimas
            //{
            //    if (tongueTopPosition >= platTOPPosition)
            //    {
            //        tongueTopPosition = displayT43.Height - (zaidejoAukstis + platAukstis);
            //        pasokes = false;
            //    }
            //    else // jei sito nera, vaiksto, bet nepasoka ir nemigsi
            //    {
            //        tongueTopPosition += 5;//kritimas, nuo lango virsaus zemina, kas 5
            //    }
            //}
          
            
           
            //sustoja krist pasiekus apacia arba platforma
            if ((tongueLeftPosition + tongueWidth) >= platLeftPosition && tongueLeftPosition <= platLeftPosition + platPlotis)//tikrinu, kad butu plat robose
            {
                // (platAukstis/2 -1) - kad net susmigus iki vidurio, 
                //vistiek butu ant virsaus. -1 kad nebutu situacijos, kai nesupranta ar turi stovet ant virsaus ar atsitrenkt i apacia
                if (tongueTopPosition + zaidejoAukstis >= platTOPPosition && tongueTopPosition + zaidejoAukstis <= platTOPPosition + (platAukstis / 2 -1))
                                                                                                                                                          
                {
                   tongueTopPosition =  platTOPPosition - zaidejoAukstis;
                    pasokes = false;
                    antPavirsiaus = true;
                }

                // (platAukstis/2 + 1) - jei iki platform vidurio -1, tai atsitenkia i apacia
                if (tongueTopPosition <= platTOPPosition + platAukstis && tongueTopPosition >= (platTOPPosition + platAukstis) - (platAukstis / 2 + 1))
               {
                  
                   tongueTopPosition = platTOPPosition + platAukstis;
                   jega = -1;
                 
               }
            } else pasokes = true; // jei nulipa nuo platformos, pradeda krist
          
          
            
                if (tongueTopPosition + zaidejoAukstis >= displayT43.Height) // tikrinu vienu daugiau
                {
                    tongueTopPosition = displayT43.Height - zaidejoAukstis;
                    pasokes = false;
                    antPavirsiaus = true;
                }
               
                              
           ColliderSide();
           CheckForLanding();
           Canvas.SetTop(tongue, tongueTopPosition );
            
        }

        void CheckForLanding()
        {
            if (snowflakeTopPosition > tongueTopPosition && snowflakeTopPosition < tongueTopPosition + zaidejoAukstis)
            {
                if (snowflakeLeftPosition + 10 >= tongueLeftPosition
                &&
                snowflakeLeftPosition <= tongueLeftPosition + tongueWidth)
                {
                    score++;
                    //label2.TextContent = "platTOPPosition: " + platTOPPosition;
                    ResetSnowflake();
                }

            }

        }
    }
}
