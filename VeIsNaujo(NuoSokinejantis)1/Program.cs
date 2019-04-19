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
        //  Rectangle platforma;
        Rectangle snowflake;
        //private Rectangle[] visosPlatformos;// Platformu masyvas kolizijai


        int tongueLeftPosition = 250;
        int snowflakeLeftPosition = 150;
        int snowflakeTopPosition = 50;
        int tongueWidth = 30;

        //Mano parametrai
        //int pasokimoJiega: 30 orginaliai veikia gerai, 
        //40 - dingsta platformos virsasu kolizija, nes prasmenga platforma pries patikrinant 
        //50 - ta pati problema is  (jei prasmenga iki puses nustumia ikaire arba desine)
        int pasokimoJiega = 36;//36
        bool pasokes = true;
        int jega;
        int tongueTopPosition; // 31 - at zemes, -199 - liecia virsum lubas, 72 - po zeme, -240 - virs lubu
        int zaidejoAukstis = 30;// buvo 200, kur naudojamas dar?


        //Platforma
        // int platLeftPosition = 100;
        //  int platTOPPosition = 142;
        int platAukstis = 30;// 30 orginaliai veikia gerai KLAIDAAAA kai didini keistai keiciasi koordinates
        int platPlotis = 30
;
        bool antPavirsiaus = false;
        bool sustojesK = false;
        bool sustojesD = false;


        GT.Timer joystickTimer = new GT.Timer(30);
        GT.Timer snowFlakeTimer = new GT.Timer(75);
        GT.Timer pasokimoTimer = new GT.Timer(30); // pasokimui, kad pastoviai tikrintu ar pasokes ar ne

        Random randomNumberGenerator = new Random();
        int score = 0;

        //-------------------------------------------------------------------------------------------------------------------
        //                                                         Vytenis
        //-------------------------------------------------------------------------------------------------------------------
        //int basePositionTop = -60;
        //int basePositionLeft = -60;
        //int basePositionTop = -44;// apacioj kairej map = apacioj kairej ekrane
        //int basePositionLeft = 0;//kai yra Update metodas
        int basePositionTop = -88;//be updata metodo, ekrano kairej apacioj kai -88, bet kai padini platAukst, jis nepakinta, o pllatTopPos padideja 90 vienetu
        int basePositionLeft = 0;//basePositionTop = -188
        // int iteration = 0;
        int platformosId = 0;
        //  int trapId = 0;

        // int lives = 3;

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
            map[10] = ".....................................";
            map[11] = ".....#..##.#.........................";


            for (int i = 0; i < map.Length; i++)
            //for (int i = 1; i <= map.Length; i++)
            {
                for (int j = 0; j < map[i].Length; j++)
                //for (int j = 1; j <= map[i].Length; j++)
                {
                    if (map[i][j] == '#')
                    {
                        platformosMap[platformosId] = new Platform(platPlotis, platAukstis);
                        //platformosMap[platformosId] = new Platform(30, 30);
                        //platformosMap[platformosId].set(basePositionLeft + (j * 30), basePositionTop + (i * 30));
                         platformosMap[platformosId].set(basePositionLeft + j * platPlotis , basePositionTop + i * platAukstis /*-88*/);
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

            //-------------------------------------------------------------------------------------------------------------------------
            //PAULIAUS
            //-------------------------------------------------------------------------------------------------------------------------
            //visosPlatformos = new Rectangle[platformosId];// Kuriu, numatant, kad visi map[i].Length vienodo ilgio, nerek turbut

            //--------------------------------------------------------------------------------------------------------------------

            for (int i = 0; i < platformosId; i++)
            {
                // layout.Children.Add(platformosMap[i].get());

                layout.Children.Add(platformosMap[i].get());
            }
            //for (int i = 0; i < trapId; i++) Dar nedarau trapu
            //{
            //    layout.Children.Add(trapMap[i].get());
            //}
            //-------------------------------------------------------------------------------------------------------------------


            //plat laikinai uzkomentuota, tikrinant generuojamas platformas
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

            mainWindow.Child = layout;
        }

        void ColliderSide(int i)
        {

            //// netinka, nes platformos ir zaidejo dydis gali skirtis
            ////if (tongueTopPosition + zaidejoAukstis  == platTOPPosition + platAukstis && tongueTopPosition  == platTOPPosition) 

            //// tikrina virsutines ribas sonu kolizijai
            ////if (tongueTopPosition + zaidejoAukstis <= platTOPPosition + platAukstis && tongueTopPosition >= platTOPPosition)
            //if (tongueTopPosition + zaidejoAukstis <= platformosMap[i].posTop() + platAukstis && tongueTopPosition >= platformosMap[i].posTop())
            //{


            //    //if ((tongueLeftPosition + tongueWidth) == platLeftPosition)// tikrinu riba is kaires

            //    //Jei zaidejas atsiduria net ties plat viduriu - 1pikselis, tai reiskia, kad jis susidure su plat is kaires. (tongueLeftPosition + tongueWidth) + 1, tikrina daugiau 1pix
            //    //if ((tongueLeftPosition + tongueWidth) + 1 >= platLeftPosition && (tongueLeftPosition + tongueWidth) <= platLeftPosition + (platPlotis / 2 - 1))// tikrinu riba is kaires
            //    if ((tongueLeftPosition + tongueWidth) + 1 >= platformosMap[i].posLeft() && (tongueLeftPosition + tongueWidth) <= platformosMap[i].posLeft() + (platPlotis / 2 - 1))
            //    {
            //        //tongueLeftPosition = platLeftPosition - tongueWidth;
            //        tongueLeftPosition = platformosMap[i].posLeft() - tongueWidth;// jei tongueLeftPosition = platformosMap[i].posLeft() - tongueWidth -5, tai sustoja laiku, bet atleidus valdikli atsitraukia per toli
            //        sustojesD = true;
            //        break;
            //    }
            //    else sustojesD = false;


            //    //if (tongueLeftPosition == platLeftPosition + platPlotis )// tikrinu is desines
            //    //Jei zaidejas atsiduria net ties plat viduriu + 1 pikselis, tai reiskia, kad jis susidure su plat is desines. tongueLeftPosition -1 tikrina daugiau 1pix
            //    //if (tongueLeftPosition - 1 <= platLeftPosition + platPlotis && tongueLeftPosition >= platLeftPosition + (platPlotis / 2 + 1))// tikrinu is desines
            //    if (tongueLeftPosition - 1 <= platformosMap[i].posLeft() + platPlotis && tongueLeftPosition >= platformosMap[i].posLeft() + (platPlotis / 2 + 1))
            //    {
            //        //tongueLeftPosition = platLeftPosition + platPlotis;
            //        tongueLeftPosition = platformosMap[i].posLeft() + platPlotis;
            //        sustojesK = true;
            //        break;
            //    }
            //    else sustojesK = false;


            //}
            //else
            //{
            //    sustojesD = false;
            //    sustojesK = false;
            //    break;
            //}
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
            //double y = joystick.GetPosition().Y;
            ////label.TextContent = "tongueTopPosition: " + tongueTopPosition;
            ////label2.TextContent = "platTOPPosition: " + platTOPPosition;
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
            //for (int i = 0; i < platformosId; i++) // AR CIA KAMERAI??
            //{
            //    platformosMap[i].updatePosition(basePositionLeft, basePositionTop);
            ////}
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
            //label.TextContent = "tongueTopPosition: " + tongueTopPosition;
            //label2.TextContent = "platTOPPosition: " + platTOPPosition;
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
                jega -= 8;// geriausiai veikia (jiega -= 6, pasokimoJiega= 30 || 36) 
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
            //pasokes = true;// bandymas
            //antPavirsiaus = false;
            //----------------------------------------------------------------------------------------------------------------------------------------
            //LABELSSSSSS
            //---------------------------------------------------------------------------------------------------------------------------------------
            //label.TextContent = "tongueRightPosition: " + (tongueLeftPosition + tongueWidth);
            //label2.TextContent = "platLeftPosition[0]: " + platformosMap[0].posLeft();
            label.TextContent = "platLeftPosition[0]: " + platformosMap[0].posLeft();
            label2.TextContent = "platLeftPosition[1]: " + platformosMap[1].posLeft();
            //label.TextContent = "platTopPosition[0]: " + platformosMap[0].posTop();
            //label2.TextContent = "platTopPosition[1]: " + platformosMap[1].posTop();

            for (int i = 0; i < platformosId; i++)
            {


                //sustoja krist pasiekus apacia arba platforma
                // if ((tongueLeftPosition + tongueWidth) >= platLeftPosition && tongueLeftPosition <= platLeftPosition + platPlotis)//tikrinu, kad butu plat robose
                if ((tongueLeftPosition + tongueWidth) >= platformosMap[i].posLeft() && tongueLeftPosition <= platformosMap[i].posLeft() + platPlotis) // Platform.cs
                {
                    // (platAukstis/2 -1) - kad net susmigus iki vidurio be 1no pikselio, 
                    //vistiek butu ant virsaus. -1 kad nebutu situacijos, kai nesupranta ar turi stovet ant virsaus ar atsitrenkt i apacia, nes atsiduria ties viduriu

                    //if (tongueTopPosition + zaidejoAukstis >= platTOPPosition && tongueTopPosition + zaidejoAukstis <= platTOPPosition + (platAukstis / 2 - 1))
                    if (tongueTopPosition + zaidejoAukstis >= platformosMap[i].posTop()
                            && tongueTopPosition + zaidejoAukstis <= platformosMap[i].posTop() + (platAukstis / 2 - 1))// Platform.cs
                    {
                        tongueTopPosition = platformosMap[i].posTop() - zaidejoAukstis;
                        pasokes = false;
                        antPavirsiaus = true;

                       // break;//pridetas naujai
                    }

                    // (platAukstis/2 + 1) - jei iki platform vidurio -1, tai atsitenkia i apacia
                    //if (tongueTopPosition <= platTOPPosition + platAukstis && tongueTopPosition >= (platTOPPosition + platAukstis) - (platAukstis / 2 + 1))
                    if (tongueTopPosition <= platformosMap[i].posTop() + platAukstis && tongueTopPosition >= platformosMap[i].posTop() + (platAukstis / 2 + 1))
                    {

                        tongueTopPosition = platformosMap[i].posTop() + platAukstis;
                        jega = -1;// #MrStickyFingers, jei sito nera trumpam prilimpa prie platformos apacios
                       // break; // pridetas naujai

                    }
                }
                else pasokes = true; // jei nulipa nuo platformos, pradeda krist



                if (tongueTopPosition + zaidejoAukstis >= displayT43.Height) // tikrinu vienu daugiau
                {
                    tongueTopPosition = displayT43.Height - zaidejoAukstis;
                    pasokes = false;
                    antPavirsiaus = true;
                  //  break;
                }
                //-------------------------------------------------------------------------------------------------------------------------
                //COLLIDERSIDE()
                //-------------------------------------------------------------------------------------------------------------------------
                // netinka, nes platformos ir zaidejo dydis gali skirtis
                //if (tongueTopPosition + zaidejoAukstis  == platTOPPosition + platAukstis && tongueTopPosition  == platTOPPosition) 

                // tikrina virsutines ribas sonu kolizijai
                //if (tongueTopPosition + zaidejoAukstis <= platTOPPosition + platAukstis && tongueTopPosition >= platTOPPosition)
                if (tongueTopPosition + zaidejoAukstis <= platformosMap[i].posTop() + platAukstis && tongueTopPosition >= platformosMap[i].posTop())
                {


                    //if ((tongueLeftPosition + tongueWidth) == platLeftPosition)// tikrinu riba is kaires

                    //Jei zaidejas atsiduria net ties plat viduriu - 1pikselis, tai reiskia, kad jis susidure su plat is kaires. (tongueLeftPosition + tongueWidth) + 1, tikrina daugiau 1pix
                    //if ((tongueLeftPosition + tongueWidth) + 1 >= platLeftPosition && (tongueLeftPosition + tongueWidth) <= platLeftPosition + (platPlotis / 2 - 1))// tikrinu riba is kaires
                    if ((tongueLeftPosition + tongueWidth) + 1 >= platformosMap[i].posLeft() && (tongueLeftPosition + tongueWidth) <= platformosMap[i].posLeft() + (platPlotis / 2 - 1))
                    {
                        //tongueLeftPosition = platLeftPosition - tongueWidth;
                        tongueLeftPosition = platformosMap[i].posLeft() - tongueWidth;// jei tongueLeftPosition = platformosMap[i].posLeft() - tongueWidth -5, tai sustoja laiku, bet atleidus valdikli atsitraukia per toli
                        sustojesD = true;
                      //  break;
                    }
                    else sustojesD = false;


                    //if (tongueLeftPosition == platLeftPosition + platPlotis )// tikrinu is desines
                    //Jei zaidejas atsiduria net ties plat viduriu + 1 pikselis, tai reiskia, kad jis susidure su plat is desines. tongueLeftPosition -1 tikrina daugiau 1pix
                    //if (tongueLeftPosition - 1 <= platLeftPosition + platPlotis && tongueLeftPosition >= platLeftPosition + (platPlotis / 2 + 1))// tikrinu is desines
                    if (tongueLeftPosition - 1 <= platformosMap[i].posLeft() + platPlotis && tongueLeftPosition >= platformosMap[i].posLeft() + (platPlotis / 2 + 1))
                    {
                        //tongueLeftPosition = platLeftPosition + platPlotis;
                        tongueLeftPosition = platformosMap[i].posLeft() + platPlotis;
                        sustojesK = true;
                       // break;
                    }
                    else sustojesK = false;


                }
                else
                {
                    sustojesD = false;
                    sustojesK = false;
                   // break;
                }
                //ColliderSide(i);
                //Canvas.SetTop(tongue, tongueTopPosition);
                Canvas.SetTop(tongue, tongueTopPosition);
            }

            CheckForLanding();

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
