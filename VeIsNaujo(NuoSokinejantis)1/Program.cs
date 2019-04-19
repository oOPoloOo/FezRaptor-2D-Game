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
        Text label3;// tikrinimui
        Text label4;// tikrinimui
        Text label5;// tikrinimui
        Text label6;// tikrinimui
        Rectangle tongue;
        Rectangle platforma;
        Rectangle snowflake;



        int tongueLeftPosition = 250;
        int snowflakeLeftPosition = 150;
        int snowflakeTopPosition = 50;
        int tongueWidth = 30;

        //Mano parametrai
        //int pasokimoJiega: 30 orginaliai veikia gerai, 
        //40 - dingsta platformos virsasu kolizija, nes prasmenga platforma pries patikrinant 
        //50 - ta pati problema is  (jei prasmenga iki puses nustumia ikaire arba desine)
        int pasokimoJiega = 36;
        bool pasokes = true;
        int jega;
        int tongueTopPosition; // 31 - at zemes, -199 - liecia virsum lubas, 72 - po zeme, -240 - virs lubu
        int zaidejoAukstis = 30;// buvo 200, kur naudojamas dar?


        //Platforma
        int platLeftPosition = 100;
        int platTOPPosition = 142;
        int platAukstis = 30;// veike kai buvo 40. Kai 30 kai tik nukritau ant plat virsaus, galejau sokinet ant jo daug kartu ir viskas buvo gerai, bet nulipus neveike jokios kolizijos virsus kai 30 ir 40 veikia tik kol nenulipi
        int platPlotis = 30;// veike kai buvo 150, kai 30 ir plat aukstis 40 sonai veikia beveik gerai
        bool antPavirsiaus = false;
        bool sustojesK = false;
        bool sustojesD = false;
        bool ivykoKolizija = false;// sabdyt while, tikrinant visu plat kolizijas


        GT.Timer joystickTimer = new GT.Timer(30);
        GT.Timer snowFlakeTimer = new GT.Timer(75);
        GT.Timer pasokimoTimer = new GT.Timer(30); // pasokimui, kad pastoviai tikrintu ar pasokes ar ne

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

        Platform[] platformosMap = new Platform[200]; //Vytenis
        // Trap[] trapMap = new Trap[10]; //Vytenis Dar nedarau
        //-------------------------------------------------------------------------------------------------------------------


        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            SetupUI();

            Canvas.SetLeft(tongue, tongueLeftPosition);
            Canvas.SetTop(tongue, tongueTopPosition);

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
            background.Width = 800;// buvo 480, kad negaletu iseit uz ekr ribu. 800 - kad galetu, del kameros

            layout.Children.Add(background);
            Canvas.SetLeft(background, 0);
            Canvas.SetTop(background, 0);

            //add the tongue
            tongue = new Rectangle(tongueWidth, zaidejoAukstis);
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
            map[0] =  ".....................................";
            map[1] =  ".....................................";
            map[2] =  ".....................................";
            map[3] =  ".....................................";
            map[4] =  ".....................................";
            map[5] =  ".....................................";
            map[6] =  ".....................................";
            map[7] =  ".....................................";
            map[8] =  ".....................................";
            map[9] =  ".....................................";
            map[10] = ".......#..#..#.......................";
            map[11] = ".....................................";

            
            for (int i = 0; i < map.Length; i++)
            {
                for (int j = 0; j < map[i].Length; j++)
                {
                    if (map[i][j] == '#')
                    {
                        //platformosMap[platformosId] = new Platform(30, 30);
                        platformosMap[platformosId] = new Platform(platPlotis, platAukstis);// kai pakeiciau dingo plat


                        layout.Children.Add(platformosMap[platformosId].get());// pirma pridedam, tada settinnam

                       // platformosMap[platformosId].set(basePositionLeft + j * 30, basePositionTop + i * 30);
                        platformosMap[platformosId].set(basePositionLeft + j * platPlotis, basePositionTop + i * platAukstis);// kai pakeiciau dingo plat
                        platformosId++;
                    }
                    //else if (map[i][j] == 'T') Dar nedarau trapu
                    //{
                    //    trapMap[trapId] = new Trap(30, 30);
                    //    trapMap[trapId].set(basePositionLeft + j * 30, basePositionTop + i * 30);
                    //    trapId++;
                    //}
                }
            }

            //for (int i = 0; i < platformosId; i++) Manau reikia pirma pridet, tada settint (CanvasSetTop...)
            //{
            //    layout.Children.Add(platformosMap[i].get());
            //}
            //for (int i = 0; i < trapId; i++) Dar nedarau trapu
            //{
            //    layout.Children.Add(trapMap[i].get());
            //}
            //-------------------------------------------------------------------------------------------------------------------


            //plat
            //platforma = new Rectangle(platPlotis, platAukstis);
            //platforma.Fill = new SolidColorBrush(Colors.Purple);
            //layout.Children.Add(platforma);
            //Canvas.SetLeft(platforma, platLeftPosition);
            //Canvas.SetTop(platforma, platTOPPosition);//??





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

            //Tekstas y asies reiksmiu tikrinimui
            label3 = new Text();
            label3.Height = 272;// buvo 240
            label3.Width = 480;// buvo 320
            label3.ForeColor = Colors.White;
            label3.Font = Resources.GetFont(Resources.FontResources.NinaB);

            layout.Children.Add(label3);
            Canvas.SetLeft(label3, 0);
            Canvas.SetTop(label3, label.Font.Height * 2);

            //Tekstas y asies reiksmiu tikrinimui
            label4 = new Text();
            label4.Height = 272;// buvo 240
            label4.Width = 480;// buvo 320
            label4.ForeColor = Colors.White;
            label4.Font = Resources.GetFont(Resources.FontResources.NinaB);

            layout.Children.Add(label4);
            Canvas.SetLeft(label4, 0);
            Canvas.SetTop(label4, label.Font.Height * 3);
        
            //Tekstas y asies reiksmiu tikrinimui
            label5 = new Text();
            label5.Height = 272;// buvo 240
            label5.Width = 480;// buvo 320
            label5.ForeColor = Colors.White;
            label5.Font = Resources.GetFont(Resources.FontResources.NinaB);

            layout.Children.Add(label5);
            Canvas.SetLeft(label5, 0);
            Canvas.SetTop(label5, label.Font.Height * 4);
          
            //Tekstas y asies reiksmiu tikrinimui
            label6 = new Text();
            label6.Height = 272;// buvo 240
            label6.Width = 480;// buvo 320
            label6.ForeColor = Colors.White;
            label6.Font = Resources.GetFont(Resources.FontResources.NinaB);

            layout.Children.Add(label6);
            Canvas.SetLeft(label6, 0);
            Canvas.SetTop(label6, label.Font.Height * 5);

            mainWindow.Child = layout;
        }

        //void ColliderSide(int i) // ikeliau prie kitos kolizijos, kad galeciau naudot break
        //{

            

        //    // tikrina virsutines ribas sonu kolizijai
        //    //if (tongueTopPosition + zaidejoAukstis <= platTOPPosition + platAukstis && tongueTopPosition >= platTOPPosition)
        //    if (tongueTopPosition + zaidejoAukstis <= platformosMap[i].posTop() + platAukstis && tongueTopPosition >= platformosMap[i].posTop())// Platform.cs
        //    {


               
        //        //Jei zaidejas atsiduria net ties plat viduriu - 1pikselis, tai reiskia, kad jis susidure su plat is kaires. (tongueLeftPosition + tongueWidth) + 1, tikrina daugiau 1pix
        //        //if ((tongueLeftPosition + tongueWidth) + 1 >= platLeftPosition && (tongueLeftPosition + tongueWidth) <= platLeftPosition + (platPlotis / 2 - 1))// tikrinu riba is kaires
        //        if ((tongueLeftPosition + tongueWidth) + 1 >= platformosMap[i].posLeft() && (tongueLeftPosition + tongueWidth) <= platformosMap[i].posLeft() + (platPlotis / 2 - 1))// Platform.cs
        //        {
        //           // tongueLeftPosition = platLeftPosition - tongueWidth;
        //            tongueLeftPosition = platformosMap[i].posLeft() - tongueWidth;
        //            sustojesD = true;
        //            ivykoKolizija = true;
        //        }
        //        else sustojesD = false;


                
        //        //Jei zaidejas atsiduria net ties plat viduriu + 1 pikselis, tai reiskia, kad jis susidure su plat is desines. tongueLeftPosition -1 tikrina daugiau 1pix
        //      //  if (tongueLeftPosition - 1 <= platLeftPosition + platPlotis && tongueLeftPosition >= platLeftPosition + (platPlotis / 2 + 1))// tikrinu is desines
        //        if (tongueLeftPosition - 1 <= platformosMap[i].posLeft() + platPlotis && tongueLeftPosition >= platformosMap[i].posLeft() + (platPlotis / 2 + 1))// Platform.cs
        //        {
        //            tongueLeftPosition = platformosMap[i].posLeft() + platPlotis;
        //            sustojesK = true;
        //            ivykoKolizija = true;
        //        }
        //        else sustojesK = false;


        //    }
        //    else
        //    {
        //        sustojesD = false;
        //        sustojesK = false;
        //    }
        //}

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
            //double y = joystick.GetPosition().Y;
            //label.TextContent = "tongueTopPosition: " + tongueTopPosition;
            //label2.TextContent = "platTOPPosition: " + platTOPPosition;
            ////Pasokimas
            //if (!pasokes)
            //{
            //    if (y >= 0.7)
            //    {
            //        pasokes = true;
            //        antPavirsiaus = false;
            //        jega = pasokimoJiega;
            //    }
            //}
            //Canvas.SetTop(tongue, tongueTopPosition);

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

            //-------------------------------------------------------------------------------------------------------------------
            //                                                         Vytenis
            //-------------------------------------------------------------------------------------------------------------------
            //for (int i = 0; i < platformosId; i++)
            //{
            //    platformosMap[i].updatePosition(basePositionLeft, basePositionTop);
            //}
            //for (int i = 0; i < trapId; i++) Dar nedarau trapu
            //{
            //    trapMap[i].updatePosition(basePositionLeft, basePositionTop);
            //}
            //-------------------------------------------------------------------------------------------------------------------

            Canvas.SetLeft(tongue, tongueLeftPosition);
            CheckForLanding();


        }

        void PasokimoTimer_Tick(GT.Timer timer)
        {
            double y = joystick.GetPosition().Y;
            //Left/Right
            //label3.TextContent = "platRightPosition[1]: " + (platformosMap[1].posLeft() + platPlotis);
            //label.TextContent = "platRightPosition[0]:" + (platformosMap[0].posLeft() + platPlotis);
            //label4.TextContent = "platLeftPosition[1]: " + platformosMap[1].posLeft();
            //label2.TextContent = "platLeftPosition[0]: " + platformosMap[0].posLeft(); 
           
            //Top/Bottom
            label3.TextContent = "platBottomPosition[1]: " + (platformosMap[1].posTop() + platAukstis);
            label.TextContent =  "platBottomPosition[0]: " + (platformosMap[0].posTop() + platAukstis);
            label4.TextContent = "platTopPosition[1]:      " + platformosMap[1].posTop();
            label2.TextContent = "platTopPosition[0]:      " + platformosMap[0].posTop();

            label5.TextContent = "Zaidejo kaires poz:       " + tongueLeftPosition;
            label6.TextContent = "Zaidejo virsaus poz:     " + tongueTopPosition;
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
            
          
            //Pasokimas
            if (pasokes)
            {
                tongueTopPosition -= jega;
                jega -= 6;// geriausiai veikia (jiega -= 6, pasokimoJiega= 30 || 36) 
            }

            Canvas.SetTop(tongue, tongueTopPosition);
       
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

           
          
            for (int i = 0; i < platformosId; i++ )
            {
                //sustoja krist pasiekus apacia arba platforma
                // if ((tongueLeftPosition + tongueWidth) >= platLeftPosition && tongueLeftPosition <= platLeftPosition + platPlotis)//tikrinu, kad butu plat robose
                if ((tongueLeftPosition + tongueWidth) >= platformosMap[i].posLeft() && tongueLeftPosition <= platformosMap[i].posLeft() + platPlotis) // Platform.cs
                {
                    // (platAukstis/2 -1) - kad net susmigus iki vidurio be 1no pikselio, 
                    //vistiek butu ant virsaus. -1 kad nebutu situacijos, kai nesupranta ar turi stovet ant virsaus ar atsitrenkt i apacia, nes atsiduria ties viduriu
                    // if (tongueTopPosition + zaidejoAukstis >= platTOPPosition && tongueTopPosition + zaidejoAukstis <= platTOPPosition + (platAukstis / 2 - 1))
                    if (tongueTopPosition + zaidejoAukstis >= platformosMap[i].posTop() && tongueTopPosition + zaidejoAukstis <= platformosMap[i].posTop() + (platAukstis / 2 - 1))// Platform.cs
                    {
                        // tongueTopPosition = platTOPPosition - zaidejoAukstis;
                        tongueTopPosition = platformosMap[i].posTop() - zaidejoAukstis;
                        pasokes = false;
                        antPavirsiaus = true;
                        break;
                    }

                    // (platAukstis/2 + 1) - jei iki platform vidurio -1, tai atsitenkia i apacia
                    // if (tongueTopPosition <= platTOPPosition + platAukstis && tongueTopPosition >= (platTOPPosition + platAukstis) - (platAukstis / 2 + 1))
                    if (tongueTopPosition <= platformosMap[i].posTop() + platAukstis && tongueTopPosition >= platformosMap[i].posTop() + (platAukstis / 2 + 1))//Platform.cs
                    {

                        //tongueTopPosition = platTOPPosition + platAukstis;
                        tongueTopPosition = platformosMap[i].posTop() + platAukstis;
                        jega = -1;// #MrStickyFingers, jei sito nera trumpam prilimpa prie platformos apacios
                        break;
                    }
                }
                else pasokes = true; // jei nulipa nuo platformos, pradeda krist

                if (tongueTopPosition + zaidejoAukstis >= displayT43.Height) // tikrinu vienu daugiau
                {
                    tongueTopPosition = displayT43.Height - zaidejoAukstis;
                    pasokes = false;
                    antPavirsiaus = true;
                    break;
                }

                //--------------------------------------------------------------------------------------------------------------------------------------
                // ColliderSide() metodas, perkeltas cia, kad butu galima naudot break
                //--------------------------------------------------------------------------------------------------------------------------------------
                // tikrina virsutines ribas sonu kolizijai
                //if (tongueTopPosition + zaidejoAukstis <= platTOPPosition + platAukstis && tongueTopPosition >= platTOPPosition)
                if (tongueTopPosition + zaidejoAukstis <= platformosMap[i].posTop() + platAukstis && tongueTopPosition >= platformosMap[i].posTop())// Platform.cs
                {



                    //Jei zaidejas atsiduria net ties plat viduriu - 1pikselis, tai reiskia, kad jis susidure su plat is kaires. (tongueLeftPosition + tongueWidth) + 1, tikrina daugiau 1pix
                    //if ((tongueLeftPosition + tongueWidth) + 1 >= platLeftPosition && (tongueLeftPosition + tongueWidth) <= platLeftPosition + (platPlotis / 2 - 1))// tikrinu riba is kaires
                    if ((tongueLeftPosition + tongueWidth) + 1 >= platformosMap[i].posLeft() && (tongueLeftPosition + tongueWidth) <= platformosMap[i].posLeft() + (platPlotis / 2 - 1))// Platform.cs
                    {
                        // tongueLeftPosition = platLeftPosition - tongueWidth;
                        tongueLeftPosition = platformosMap[i].posLeft() - tongueWidth;
                        sustojesD = true;
                        break;
                    }
                    else sustojesD = false;



                    //Jei zaidejas atsiduria net ties plat viduriu + 1 pikselis, tai reiskia, kad jis susidure su plat is desines. tongueLeftPosition -1 tikrina daugiau 1pix
                    //  if (tongueLeftPosition - 1 <= platLeftPosition + platPlotis && tongueLeftPosition >= platLeftPosition + (platPlotis / 2 + 1))// tikrinu is desines
                    if (tongueLeftPosition - 1 <= platformosMap[i].posLeft() + platPlotis && tongueLeftPosition >= platformosMap[i].posLeft() + (platPlotis / 2 + 1))// Platform.cs
                    {
                        tongueLeftPosition = platformosMap[i].posLeft() + platPlotis;
                        sustojesK = true;
                        break;
                    }
                    else sustojesK = false;


                }
                else
                {
                    sustojesD = false;
                    sustojesK = false;
                }
               // ColliderSide(i);
               
            }

            //if (tongueTopPosition + zaidejoAukstis >= displayT43.Height) // tikrinu vienu daugiau
            //{
            //    tongueTopPosition = displayT43.Height - zaidejoAukstis;
            //    pasokes = false;
            //    antPavirsiaus = true;
            //}

            CheckForLanding();
            Canvas.SetTop(tongue, tongueTopPosition);

         

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
