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


//Pirmi lygiai V15
//Padariau gyvybiu pernasa
//Sutvarkiau 2 lygi
//Padariau 3 lygi, kad butu zaidziamas, kad pereidinetu normaliai
//Padaryt, kad galima butu galima restartint zaidimo metu - ne tik mirus
//Restartinus grazina gyvybes i max ir i to lygio, kuriame restartinta pradzia.
















namespace VeIsNaujo_NuoSokinejantis_1
{
    public partial class Program
    {
        Window mainWindow;
        Canvas layout;
        Text label;// Gyvybes
        Text label2;//Pabaigos Tekstas
        Text label3;// Restart Tekstas
        Text label4;// Pergales Tekstas
        Text scoreLabel;// Pergales Tekstas

        Rectangle player;
        Rectangle snowflake;

        public struct PlayerStruct
        {
            //Player parameters
            // public int playerTopPosition;
            // public int playerLeftPosition = 250;
            // public int playerHeight = 30;//30 buvo
            // public int playerWidth = 30;//30 buvo
            // public int jumpPower = 30;
            // public bool jumped = true;
            //public int power;
            public int playerTopPosition;
            // public int playerBottomPosition;
            public int playerLeftPosition;
            // public int playerRightPosition;
            public int playerHeight;
            public int playerWidth;
            public int jumpPower;
            public bool jumped;
            public int power;
            public bool antSpyglPavirsiaus;
            public bool antLavosPavirsiaus;
            public int lifeBarGyvybes;
            public int gyvybiuSkaitiklis;
            public bool neAntZemes;
            public bool mire;
            public bool laimejo;
            public int gyvybes;

            //  playerBottomPosition = playerTopPosition + playerHeight;
            public PlayerStruct(int playerTopPos, int playerLeftPos, int playerH, int playerW, int jumpP, bool jmp, int pow, bool antPavirsiaus, int barLives, int livesCount, bool neZeme, bool antLavos, bool die, bool win, int lives)
            {
                playerTopPosition = playerTopPos;

                playerLeftPosition = playerLeftPos;

                playerHeight = playerH;
                playerWidth = playerW;
                jumpPower = jumpP;
                jumped = jmp;
                power = pow;
                antSpyglPavirsiaus = antPavirsiaus;
                lifeBarGyvybes = barLives;
                gyvybiuSkaitiklis = livesCount;
                neAntZemes = neZeme;
                antLavosPavirsiaus = antLavos;
                mire = die;
                laimejo = win;
                gyvybes = lives;


                //playerBottomPosition = playerTopPosition + playerHeight;
                //playerRightPosition = playerLeftPosition + playerWidth;
            }
        }
        static int lifeBarSk = 7; // nekeist
        static int gyvybiuSk = 3;
        PlayerStruct playerStruct = new PlayerStruct(0, 0, 20, 20, 25, true, 0, false, lifeBarSk, 0, true, false, false, false, gyvybiuSk);



        //public struct SpyglysStruct
        //{
        //    public int spygliaiId;
        //    public int Heigth;
        //    public int Width;

        //    public SpyglysStruct(int id, int heigth, int width)
        //    {
        //        spygliaiId = id;
        //        Heigth = heigth;
        //        Width = width;
        //    }
        //}
        //SpyglysStruct spygl = new SpyglysStruct(0, 10, 10);

        //Spygliai
        //int spygliaiId = 0;
        //int spyglHeigth = 10;
        //int spyglWidth = 10;
        //bool spyglKrist = false;
        //Platform[] spygliaiMap = new Platform[4];

        //Platform parameters
        ////If platform parameters changes, position of platform also changes
        int platHeight = 25;// Tu
        int platWidth = 25;// buvo 30

        //Collision parameters
        bool stopLeft = false;// makes stop move left
        bool stopRight = false;// makes stop move right


        GT.Timer joystickTimer = new GT.Timer(30);
        GT.Timer jumpTimer = new GT.Timer(30);




        //Map generation
        // -88 this offset makes the platform align with botom of the screen
        // int platPraziosTop = platHeight * (-12);
        int basePositionTop = 0; //buvo -88, nustatau veliau
        int basePositionLeft = 0;
        int platformId = 0;


        Platform[] platformMap = new Platform[200];


        //Buvusi zaidejo pozicija, kolizijai geriau aptikti
        int buvusKaires;
        int buvusVirsaus;

        //Spygliai
        //bool spyglKrist = false;
        //bool pirmasKartas = true;
        //bool spyglNukrito = false;
        static int spygliuMaxSk = 20;
        int spygliaiId = 0;
        int spyglHeigth = 20;
        int spyglWidth = 10;
        bool[] spyglKrist = new bool[spygliuMaxSk];
        bool[] pirmasKartas = new bool[spygliuMaxSk];
        bool[] spyglNukrito = new bool[spygliuMaxSk];
        Platform[] spygliaiMap = new Platform[spygliuMaxSk];

        //Lava
        static int lavosMaxSk = 35;
        int lavaId = 0;
        int lavaHeigth = 25;
        int lavaWidth = 25;
        Platform[] lavaMap = new Platform[lavosMaxSk];

        //Lyugio Restartinimas
        bool restartinimas = false;

        //Durys i pergale
        static int duruMaxSk = 10;
        int duruId = 0;
        int duruHeigth = 35;
        int duruWidth = 25;
        Platform[] durysMap = new Platform[duruMaxSk];

        //Snaige
        int snowflakeLeftPosition = 150;
        int snowflakeTopPosition = 50;
        Random randomNumberGenerator = new Random();
        GT.Timer snowFlakeTimer = new GT.Timer(75);

        //Taskai
        int galutiniaiTaskai = 0;
        int snaigiuTaskai = 0;
        static int duotaLaiko = 1200;
        int laikoTaskai = duotaLaiko;
        
        //Lygiai
        int lygis = 1;
        bool perejoLygi = false;
        int buvesLygis = 1;
        int yraLygiu = 3;



        void ProgramStarted()
        {
            SetupUI();

            Canvas.SetLeft(player, playerStruct.playerLeftPosition);
            Canvas.SetTop(player, playerStruct.playerTopPosition);

            //joystickTimer.Tick += new GT.Timer.TickEventHandler(JoystickTimer_Tick);
            //joystickTimer.Start();
            if (!restartinimas)
            {
                jumpTimer.Tick += new GT.Timer.TickEventHandler(jumpTimer_Tick);
                jumpTimer.Start();

                snowFlakeTimer.Tick += new GT.Timer.TickEventHandler(SnowflakeTimer_Tick);
                snowFlakeTimer.Start();
            }



        }

        void SetupUI()
        {
            // initialize window

            //Labai lagina  
            //Startas(ref basePositionTop, basePositionLeft, platHeight, platWidth, spyglWidth, spyglHeigth, lavaWidth, lavaHeigth,  displayT43, ref layout, ref player, ref playerStruct, label, label2, label3, spyglKrist, pirmasKartas, spyglNukrito, platformMap, lavaMap, spygliaiMap, ref lavaId, ref spygliaiId, ref platformId, mainWindow);          
            // this.tunes.Play();
            // System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\mywavfile.wav");
            //var soundPlayer = new SoundPlayer(@"c:\Windows\Media\chimes.wav");

            //  player.Play();



            // setup the layout
            if (!restartinimas)
            {
                mainWindow = displayT43.WPFWindow;

                basePositionTop = (platHeight * (-12)) + displayT43.Height;

                layout = new Canvas();
                Border background = new Border();
                background.Background = new SolidColorBrush(Colors.Black);

                background.Height = 272;
                background.Width = 480;

                layout.Children.Add(background);
                Canvas.SetLeft(background, 0);
                Canvas.SetTop(background, 0);

                //add the player
                player = new Rectangle(playerStruct.playerWidth, playerStruct.playerHeight);
                player.Fill = new SolidColorBrush(Colors.Red);
                layout.Children.Add(player);

                //add the snowflake
                snowflake = new Rectangle(10, 10);
                snowflake.Fill = new SolidColorBrush(Colors.White);
                layout.Children.Add(snowflake);

                //label
                label = new Text();
                label.Height = 272;// buvo 240
                label.Width = 480;// buvo 320
                label.ForeColor = Colors.White;
                label.Font = Resources.GetFont(Resources.FontResources.NinaB);
                layout.Children.Add(label);
                Canvas.SetLeft(label, 0);
                Canvas.SetTop(label, 0);

                //Taskai
                scoreLabel = new Text();
                scoreLabel.Height = 272;// buvo 240
                scoreLabel.Width = 480;// buvo 320
                scoreLabel.ForeColor = Colors.White;
                scoreLabel.Font = Resources.GetFont(Resources.FontResources.NinaB);
                layout.Children.Add(scoreLabel);
                Canvas.SetLeft(scoreLabel, 0);
                Canvas.SetTop(scoreLabel, 0 + label.Font.Height);
            }

            ////Tekstas mirties uzrasui
            //label2 = new Text();
            //label2.Height = 272;// buvo 240
            //label2.Width = 480;// buvo 320          
            //label2.ForeColor = Colors.White;
            //label2.Font = Resources.GetFont(Resources.FontResources.NinaB);

            //layout.Children.Add(label2);
            //Canvas.SetLeft(label2, 180);
            //Canvas.SetTop(label2, 132);

            ////Tekstas mirties uzrasui
            //label3 = new Text();
            //label3.Height = 272;// buvo 240
            //label3.Width = 480;// buvo 320          
            //label3.ForeColor = Colors.White;
            //label3.Font = Resources.GetFont(Resources.FontResources.NinaB);

            //layout.Children.Add(label3);
            //Canvas.SetLeft(label3, 110);
            //Canvas.SetTop(label3, 132 + label2.Font.Height);

            //Spygliai
            for (int i = 0; i < spygliuMaxSk; i++)
            {
                spyglKrist[i] = false;
                pirmasKartas[i] = true;
                spyglNukrito[i] = false;

            }

            if (!perejoLygi)
            {
                ledStrip.SetLeds(lifeBarSk);
                led7R.SetLeds(gyvybiuSk);
            }

            string[] map = new string[12];
            if (lygis == 1)
            {
                // Pirmas Lygis           
               
                map[0] = ".....................................";
                map[1] = ".....................................";
                map[2] = ".....................................";
                map[3] = ".....................................";
                map[4] = ".................#...................";
                map[5] = "...........S...###...................";
                map[6] = ".#...S...S...S.......................";
                map[7] = ".......S.............................";
                map[8] = ".....................................";
                map[9] = ".....................................";
                map[10] = "...#................................";
                map[11] = "####LLLLLLLLL#...D..................";
            }
            else if (lygis == 2)
            {
                // Pirmas Lygis           
               // string[] map = new string[12];
                map[0] = ".....................................";
                map[1] = ".....................................";
                map[2] = ".....................................";
                map[3] = ".....................................";
                map[4] = ".....................................";
                map[5] = "............S........................";
                map[6] = "##..S....S...........................";
                map[7] = ".......S.............................";
                map[8] = "...............S.....................";
                map[9] = ".....................................";
                map[10] = "....................................";
                map[11] = "LLLLLLLLLLLLLLL...D.................";
            }
            else if (lygis == 3)
            {
                // Pirmas Lygis           
                // string[] map = new string[12];
                map[0] = ".....................................";
                map[1] = ".....................................";
                map[2] = ".....................................";
                map[3] = ".....................................";
                map[4] = ".....................................";
                map[5] = "...........S.........................";
                map[6] = "........S.....#......................";
                map[7] = "............S........................";
                map[8] = ".....#...S...........................";
                map[9] = "..S..................................";
                map[10] = "....................................";
                map[11] = "#LLLLLLLLLLLLLL...D.................";
            }

                
                for (int i = 0; i < map.Length; i++)
                {
                    for (int j = 0; j < map[i].Length; j++)
                    {
                        if (map[i][j] == '#')
                        {
                            platformMap[platformId] = new Platform(platWidth, platHeight);
                            platformMap[platformId].set(basePositionLeft + j * platWidth, basePositionTop + i * platHeight);
                            platformId++;
                        }

                        if (map[i][j] == 'S')
                        {
                            spygliaiMap[spygliaiId] = new Platform(spyglWidth, spyglHeigth);
                            spygliaiMap[spygliaiId].paint(Colors.Blue);
                            // * platWith ir platHeigth, kad spyglius deliojant mape galima butu orentuotis pagal platformu poz
                            spygliaiMap[spygliaiId].set(basePositionLeft + j * platWidth, basePositionTop + i * platHeight);
                            spygliaiId++;
                        }
                        if (map[i][j] == 'L')
                        {
                            lavaMap[lavaId] = new Platform(lavaWidth, lavaHeigth);
                            lavaMap[lavaId].paint(Colors.Magenta);
                            // * platWith ir platHeigth, kad spyglius deliojant mape galima butu orentuotis pagal platformu poz
                            lavaMap[lavaId].set(basePositionLeft + j * platWidth, basePositionTop + i * platHeight);
                            lavaId++;
                        }
                        if (map[i][j] == 'D')
                        {
                            durysMap[duruId] = new Platform(duruWidth, duruHeigth);
                            durysMap[duruId].paint(Colors.Green);
                            // * platWith ir platHeigth, kad spyglius deliojant mape galima butu orentuotis pagal platformu poz
                            durysMap[duruId].set(basePositionLeft + j * platWidth, basePositionTop + i * platHeight);
                            duruId++;
                        }
                    }
                }
            


            // Adding platforms to screen
            //if (!restartinimas)
            //{
                for (int i = 0; i < platformId; i++)
                {
                    layout.Children.Add(platformMap[i].get());
                }
                for (int i = 0; i < lavaId; i++)
                {
                    layout.Children.Add(lavaMap[i].get());
                }
                for (int i = 0; i < duruId; i++)
                {
                    layout.Children.Add(durysMap[i].get());
                }
            //}
            //Spygliai keicia pozicija, todel reikia kurt per naujo
            for (int i = 0; i < spygliaiId; i++)
            {
                layout.Children.Add(spygliaiMap[i].get());
            }

            if (!restartinimas)
            {
                //Perkeliau po visu kompunentu idejimo, kad mirties tekstas butu ant virsasu
                //Tekstas mirties uzrasui
                label2 = new Text();
                label2.Height = 272;// buvo 240
                label2.Width = 480;// buvo 320          
                label2.ForeColor = Colors.White;
                label2.Font = Resources.GetFont(Resources.FontResources.NinaB);

                layout.Children.Add(label2);
                Canvas.SetLeft(label2, 180);
                Canvas.SetTop(label2, 132);

                //Tekstas restartinimo uzrasui
                label3 = new Text();
                label3.Height = 272;// buvo 240
                label3.Width = 480;// buvo 320          
                label3.ForeColor = Colors.White;
                label3.Font = Resources.GetFont(Resources.FontResources.NinaB);

                layout.Children.Add(label3);
                Canvas.SetLeft(label3, 115);
                Canvas.SetTop(label3, 132 + label2.Font.Height);

                //Pergales tekstas
                label4 = new Text();
                label4.Height = 272;// buvo 240
                label4.Width = 480;// buvo 320          
                label4.ForeColor = Colors.White;
                label4.Font = Resources.GetFont(Resources.FontResources.NinaB);

                layout.Children.Add(label4);
                Canvas.SetLeft(label4, 180);//Tokiam paciam auksti, kaip ir mirties
                Canvas.SetTop(label4, 132);
            }

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
            if (snowflakeLeftPosition > 470) snowflakeLeftPosition = 470;// buvo 300
            Canvas.SetLeft(snowflake, snowflakeLeftPosition);
            Canvas.SetTop(snowflake, snowflakeTopPosition);
        }

        private void ResetSnowflake()
        {
            snowflakeTopPosition = 50;
            snowflakeLeftPosition = randomNumberGenerator.Next(300) + 10;
        }

        void jumpTimer_Tick(GT.Timer timer)
        {

            //-------------------------------------------------------------------------------------------------------
            // PLATFORMOS KOLIZIJA IR JUDEJIMAS
            //------------------------------------------------------------------------------------------------------
            //player.Fill = new SolidColorBrush(Colors.Red); //Pakeiciu zaidejo spauva is juodos i raudona - mirgsejimas

            if (!playerStruct.mire && !playerStruct.laimejo)
            {

                JudejimasY(ref  buvusVirsaus, ref  playerStruct, joystick);// Sutvarkyta

                Canvas.SetTop(player, playerStruct.playerTopPosition);

                VirsausKolizija(platformId, platformMap, buvusVirsaus, ref playerStruct, displayT43, ref ledStrip);//Pataisyta

                JudejimasX(ref buvusKaires, stopLeft, stopRight, ref playerStruct, joystick, displayT43);// Pataisyta

                Canvas.SetLeft(player, playerStruct.playerLeftPosition);// update player left posti

                SonuKolizija(platformId, platformMap, buvusKaires, ref playerStruct, ref stopRight, ref stopLeft, displayT43);// Sutvarkyta

                Canvas.SetTop(player, playerStruct.playerTopPosition); // update player top position
            }

            //-------------------------------------------------------------------------------------------------------
            // SPYGLIO KOLIZIJA IR JUDEJIMAS
            //------------------------------------------------------------------------------------------------------

            //Spygliu ciklas
            for (int i = 0; i < spygliaiId; i++)

                SpyglioKritimas(ref playerStruct, ref spygliaiMap, ref platformMap, spyglKrist, pirmasKartas, spyglNukrito, buvusVirsaus, i, ref lavaMap, displayT43, platformId, lavaId, ref ledStrip);


            //Lava
            Lava(ref playerStruct, ref spygliaiMap, ref lavaMap, buvusVirsaus, lavaId, ref player, ref ledStrip);

            //Durys
            Durys();

            //Snaiges kolizijos tikrinimas
            CheckForLanding();

            //Led bar ir gyvybiu veiksmai
            if (playerStruct.lifeBarGyvybes == 0)// Dalinai veikia
            {
                //int sk = gyvybiuSk;
                if (playerStruct.gyvybes > 0)
                {
                    playerStruct.lifeBarGyvybes = lifeBarSk;
                    ledStrip.SetLeds(lifeBarSk);
                }
                if (playerStruct.gyvybes > 0)
                {
                    led7R.TurnLedOff(playerStruct.gyvybes - 1);
                    playerStruct.gyvybes--;
                    // sk--;
                }
            }

            // if (button.Pressed) led7R.TurnLedOff(0);Testavimui D1 - 0, D2 - 1, D3 - 2 ...

            //  scoreLabel.TextContent = "Taskai: " + (galutiniaiTaskai*300 + playerStruct.gyvybes*150);
            //if (!playerStruct.mire) label.TextContent = "Gyvybes: " + playerStruct.lifeBarGyvybes;
            if (!playerStruct.mire) label.TextContent = "Gyvybes: " + playerStruct.gyvybes;
            if (!playerStruct.mire && !playerStruct.laimejo)
            {
                if (laikoTaskai > 0) laikoTaskai--;
                //galutiniaiTaskai = (snaigiuTaskai * 300 + playerStruct.lifeBarGyvybes * 150 + laikoTaskai);// Tasku formule
                galutiniaiTaskai = (snaigiuTaskai * 300 + playerStruct.lifeBarGyvybes * 35 + playerStruct.gyvybes * 450 + laikoTaskai);// Tasku formule
                scoreLabel.TextContent = "Taskai: " + galutiniaiTaskai;
            }
            // if (playerStruct.lifeBarGyvybes < 1) playerStruct.mire = true; 
            if (playerStruct.gyvybes < 1 && playerStruct.lifeBarGyvybes < 1) playerStruct.mire = true;// Taip dar neveikia - neveikia restart ir nujima visas gyvybes iskart.
            if (playerStruct.mire && !playerStruct.laimejo)
            {
                label2.TextContent = "Zaidimas Baigtas!";
                label3.TextContent = "Paspauskite valdikli, kad kartotumete";
            }
            if (!playerStruct.mire && !playerStruct.laimejo)
            {
                label2.TextContent = "";
                label3.TextContent = "";
                label4.TextContent = "";
            }
            ////LedStrip
            //if (playerStruct.minusGyvybe)
            //{
            //    ledStrip.SetLed(gyvybiuSk - (playerStruct.gyvybes + 1), false);
            //    playerStruct.minusGyvybe = false;
            //}
            if (playerStruct.laimejo && !playerStruct.mire)
            {
                label4.TextContent = "Jus perejote lygi!!!";
                label3.TextContent = "Paspauskite valdikli, kad kartotumete";

                if (joystick.IsPressed)
                {
                   // playerStruct.playerTopPosition = 240;
                    playerStruct.playerTopPosition = 0;
                    playerStruct.playerLeftPosition = 0;

                    if (!perejoLygi)
                    {
                        playerStruct.lifeBarGyvybes = lifeBarSk;
                        playerStruct.gyvybes = gyvybiuSk;
                    }
                   
                    laikoTaskai = duotaLaiko;
                    snaigiuTaskai = 0;
                    playerStruct.laimejo = false;

                    for (int i = 0; i < duruId; i++) layout.Children.Remove(durysMap[i].get());
                    for (int i = 0; i < platformId; i++) layout.Children.Remove(platformMap[i].get());
                    for (int i = 0; i < lavaId; i++) layout.Children.Remove(lavaMap[i].get());
                    for (int i = 0; i < spygliaiId; i++) layout.Children.Remove(spygliaiMap[i].get());

                    lavaId = 0;
                    platformId = 0;
                    spygliaiId = 0;
                    duruId = 0;
                    restartinimas = true;
                    player.Fill = new SolidColorBrush(Colors.Red);

                    ProgramStarted();
                    perejoLygi = false;

                }
            }
            if (playerStruct.mire && joystick.IsPressed && !playerStruct.laimejo)
            {

              
                playerStruct.playerTopPosition = 0;
                playerStruct.playerLeftPosition = 0;
                playerStruct.lifeBarGyvybes = lifeBarSk;
                playerStruct.gyvybes = gyvybiuSk;
                snaigiuTaskai = 0;
                laikoTaskai = duotaLaiko;
                playerStruct.mire = false;


                for (int i = 0; i < duruId; i++) layout.Children.Remove(durysMap[i].get());
                for (int i = 0; i < platformId; i++) layout.Children.Remove(platformMap[i].get());
                for (int i = 0; i < lavaId; i++) layout.Children.Remove(lavaMap[i].get());
                for (int i = 0; i < spygliaiId; i++) layout.Children.Remove(spygliaiMap[i].get());


                lavaId = 0;
                platformId = 0;
                spygliaiId = 0;
                duruId = 0;
                restartinimas = true;


                ProgramStarted();

            }
             if(kairysMygtukas.Pressed)
             {

                 playerStruct.playerTopPosition = 0;
                 playerStruct.playerLeftPosition = 0;
                 playerStruct.lifeBarGyvybes = lifeBarSk;
                 playerStruct.gyvybes = gyvybiuSk;
                 snaigiuTaskai = 0;
                 laikoTaskai = duotaLaiko;
                 playerStruct.mire = false;


                 for (int i = 0; i < duruId; i++) layout.Children.Remove(durysMap[i].get());
                 for (int i = 0; i < platformId; i++) layout.Children.Remove(platformMap[i].get());
                 for (int i = 0; i < lavaId; i++) layout.Children.Remove(lavaMap[i].get());
                 for (int i = 0; i < spygliaiId; i++) layout.Children.Remove(spygliaiMap[i].get());


                 lavaId = 0;
                 platformId = 0;
                 spygliaiId = 0;
                 duruId = 0;
                 restartinimas = true;


                 ProgramStarted();
             }

        }//jumpTimer end


        //-------------------------------------------------------------------------------------------------
        static void VirsausKolizija(int mapId, Platform[] map, int buvusVirsaus, ref PlayerStruct str, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43, ref LEDStrip ledStrip)
        {///DARAU PAKEITIMUS - reikia kad player butu ne struct, o platform klasej, kitaip nesigaus padaryt koliziju kitiem obj

            bool buvoVirsui = false;
            bool buvoApacioj = false;

            // Platform check loop                            
            for (int i = 0; i < mapId; i++)
            {

                buvoVirsui = false;
                buvoApacioj = false;


                if (buvusVirsaus < (int)map[i].posTop())
                {
                    buvoVirsui = true;
                }

                else if (buvusVirsaus > (int)map[i].posTop())
                {
                    buvoApacioj = true;
                }
                //------------------------------------------------------------------------------------------------------------------


                if ((str.playerLeftPosition + str.playerWidth) > map[i].posLeft()
                              && str.playerLeftPosition < map[i].posLeft() + map[i].Width())
                {
                    //TOP collision. (platHeight/2 -1) - even if player is in platform 
                    // (from top) and doesn't touch the middle he is set on top 

                    if (buvoVirsui && str.playerTopPosition + str.playerHeight >= map[i].posTop()
                        && str.playerTopPosition + str.playerHeight <= map[i].posTop() + map[i].Height() + System.Math.Abs(str.power))
                    {
                        str.playerTopPosition = map[i].posTop() - str.playerHeight;
                        str.jumped = false;

                        if (str.power < -40)
                        {
                            if (str.lifeBarGyvybes > 0)
                            {
                                for (int plattt = 3; plattt > 0; plattt--)
                                {
                                    ledStrip.SetLed(lifeBarSk - (str.lifeBarGyvybes), false);
                                    str.lifeBarGyvybes -= 1;//Kritimo is aukstai zala
                                }
                            }
                            //str.minusGyvybe = true;
                        }
                        str.power = 0;
                        break;

                    }

                    //BOTTOM collision. (platHeight/2 + 1) - even if player is in platform 
                    //(from bottom) and don't touch the middle he collides with bottom 

                    if (buvoApacioj && str.playerTopPosition <= map[i].posTop() + map[i].Height()
                        && str.playerTopPosition >= map[i].posTop() - System.Math.Abs(str.power))
                    {
                        str.playerTopPosition = map[i].posTop() + map[i].Height();
                        str.power = -1;// without this player gets stuck on platform bottom
                        // kolAplacia = true;
                        break;
                    }
                }
                // else str.jumped = true; // if player steps off platform - he falls
                else if (!str.antSpyglPavirsiaus && !str.antLavosPavirsiaus)
                    str.jumped = true;

            }

            // the very bottom collision
            if (str.playerTopPosition + str.playerHeight >= displayT43.Height)
            {
                str.playerTopPosition = displayT43.Height - str.playerHeight;
                str.jumped = false;
                str.neAntZemes = false;

                if (str.power < -40)
                {
                    if (str.lifeBarGyvybes > 0)
                    {
                        for (int platt = 3; platt > 0; platt--)
                        {
                            ledStrip.SetLed(lifeBarSk - (str.lifeBarGyvybes), false);
                            str.lifeBarGyvybes -= 1;// Kritimo is aukstai zala
                            //   str.minusGyvybe = true;
                        }
                    }
                }
                str.power = 0;
            }

        }// VirsasuKolizija end



        static void SonuKolizija(int mapId, Platform[] map, int buvusKaires, ref PlayerStruct str, ref bool stopRight, ref bool stopLeft, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43)
        {
            bool buvoKairej = false;
            bool buvoDesinej = false;

            for (int i = 0; i < mapId; i++)
            {
                buvoKairej = false;
                buvoDesinej = false;

                //-------------------------------------------------------------------------------------------------
                //Buvusi pozicija
                if (buvusKaires < map[i].posLeft() && buvusKaires + str.playerWidth < map[i].posLeft()
                   && ((str.playerTopPosition < map[i].posTop() || str.playerTopPosition + str.playerHeight < str.playerTopPosition) || (map[i].posTop() + map[i].Height() > str.playerTopPosition
                   || str.playerTopPosition + str.playerHeight < map[i].posTop() + map[i].Height())))
                {
                    buvoKairej = true;


                }
                else if (buvusKaires > map[i].posLeft() + map[i].Width() && buvusKaires + str.playerWidth > map[i].posLeft() + map[i].Width()
                   && ((str.playerTopPosition < map[i].posTop() || str.playerTopPosition + str.playerHeight < str.playerTopPosition) || (map[i].posTop() + map[i].Height() > str.playerTopPosition
                   || str.playerTopPosition + str.playerHeight < map[i].posTop() + map[i].Height())))
                {
                    buvoDesinej = true;
                }

                //---------------------------------------------------------------------------------------------------------------------

                if (str.playerTopPosition + str.playerHeight >= map[i].posTop() + 1 //Taisiau, kaip ir gerai
                                      && str.playerTopPosition <= map[i].posTop() + map[i].Height() - 1)
                {
                    //RIGHT collision. (platWidth / 2 - 1) - even if player is in platform 
                    // (from left) and don't touch the middle he collides with left

                    if (buvoKairej && (str.playerLeftPosition + str.playerWidth) + 1 >= map[i].posLeft()
                        && (str.playerLeftPosition + str.playerWidth) <= map[i].posLeft() + map[i].Width())
                    {
                        str.playerLeftPosition = map[i].posLeft() - str.playerWidth;
                        stopRight = true;// stops moving right

                        break;
                    }
                    else stopRight = false;

                    //LEFT collision. (platWidth / 2 - 1) - even if player is in platform 
                    // (from right) and don't touch the middle he collides with right

                    if (buvoDesinej && str.playerLeftPosition - 1 <= map[i].posLeft() + map[i].Width()
                        && str.playerLeftPosition >= map[i].posLeft())
                    {
                        str.playerLeftPosition = map[i].posLeft() + map[i].Width();
                        stopLeft = true;// stops moving left

                        break;
                    }
                    else stopLeft = false;

                }
                else
                {
                    stopRight = false;
                    stopLeft = false;
                }

            }

        }// SonuKolizija eind

        static void JudejimasX(ref int buvusKaires, bool stopLeft, bool stopRight, ref PlayerStruct str, Gadgeteer.Modules.GHIElectronics.Joystick joystick, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43)
        {
            //JUDEJIMAS I KAIRE/DESINE
            double x = joystick.GetPosition().X; // joystic x scale [-1;1]
            // move left
            if (x < -0.3 && str.playerLeftPosition > 0 && !stopLeft)
            {
                buvusKaires = str.playerLeftPosition;
                str.playerLeftPosition -= 5;
            }
            // move right
            else if (x > 0.7 && str.playerLeftPosition < displayT43.Width - str.playerWidth
                                                                     && !stopRight)
            {
                buvusKaires = str.playerLeftPosition;
                str.playerLeftPosition += 5;
            }
        }// JudejimasX end

        static void JudejimasY(ref int buvusVirsaus, ref PlayerStruct str, Gadgeteer.Modules.GHIElectronics.Joystick joystick)
        {
            double y = joystick.GetPosition().Y;

            //Jump part
            if (!str.jumped) // if didn't jump
            {
                if (y >= 0.7/*e.KeyCode == Keys.Space*/) // and jumps
                {
                    str.jumped = true;
                    str.power = str.jumpPower;
                }
            }

            //Jump
            if (str.jumped)
            {
                //Zaidejas.Top  -= power;
                //buvusVirsaus = Zaidejas.Top;

                buvusVirsaus = str.playerTopPosition;
                str.playerTopPosition -= str.power;
                str.power -= 3;
            }
        }// JudejimasY end

        static void SpyglioKritimas(ref PlayerStruct Zaidejas, ref Platform[] spygl, ref Platform[] platformMap, bool[] krist, bool[] pirmasKartas, bool[] spyglNukrito, int buvusZaidejoTop, int index, ref Platform[] lavaMap, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43, int platId, int lavaId, ref LEDStrip ledStrip)
        {
            int buvusVirsaus = spygl[index].posTop();// priskyrt buvusiai spyglio top possision


            if (spygl[index].isVisible())
            {

                if (Zaidejas.playerLeftPosition + Zaidejas.playerHeight >= spygl[index].posLeft() && Zaidejas.playerLeftPosition <= spygl[index].posRigth() && Zaidejas.playerTopPosition + Zaidejas.playerHeight >= spygl[index].posTop()) krist[index] = true;


                if (krist[index] && !spyglNukrito[index])// parasyt salyga dar viena, kad nevykdytu, jei nukritus
                {

                    if (pirmasKartas[index])
                    {
                        System.Threading.Thread.Sleep(20);
                        pirmasKartas[index] = false;
                    }



                    // spygl[index].posTop() = spygl[index].posTop() + 20;
                    spygl[index].set(spygl[index].posLeft(), spygl[index].posTop() + 10);//su 20 nepasoka nuo spygliu

                    Zaidejas.antSpyglPavirsiaus = false;// kai sitas yra neleidzia pasokt nuo spyglio

                }
            }//if(isVisible) end


            bool buvoApacioj = false;
            bool buvoVirsui = false;

            for (int i = 0; i < platId; i++)
            {

                buvoApacioj = false;
                buvoVirsui = false;

                //if (buvusVirsaus < platformMap[i].posTop() /*|| buvusVirsaus <= platformMap[i].posTop()*/) // zmogeliukas gali but ir prasmeges // Zinau, kad spyglys kris ant platformu is virsaus
                //{
                //    buvoVirsui = true;

                //}

                //else if (buvusVirsaus > platformMap[i].posTop())
                //{
                //    buvoApacioj = true;
                //}
                ////------------------------------------------------------------------------------------------------------------------

                if (spygl[index].isVisible())
                {
                    //Spyglio Kolizija Su platforma
                    if ((spygl[index].posRigth()) > platformMap[i].posLeft()
                        && spygl[index].posLeft() < platformMap[i].posRigth())
                    {
                        //Spyglio apacios su plat virsasu collision. 
                        if (/*buvoVirsui &&*/ spygl[index].posBottom() >= platformMap[i].posTop()
                            && spygl[index].posBottom() <= platformMap[i].posBottom() + System.Math.Abs(20))
                        {
                            //spygl[index].posTop() = platformMap[i].posTop() - spygl[index].Height();
                            spygl[index].set(spygl[index].posLeft(), platformMap[i].posTop() - spygl[index].Height());
                            krist[index] = false;
                            spyglNukrito[index] = true;
                            break;
                        }

                    }
                }// Spyglio Kolizija su platforma end
            }// platformu for end


            for (int i = 0; i < lavaId; i++)
            {

                buvoApacioj = false;
                buvoVirsui = false;

                if (buvusVirsaus < lavaMap[i].posTop()) // zmogeliukas gali but ir prasmeges // Zinau, kad spyglys kris ant platformu is virsaus
                {
                    buvoVirsui = true;

                }

                else if (buvusVirsaus > lavaMap[i].posTop())
                {
                    buvoApacioj = true;
                }
                ////------------------------------------------------------------------------------------------------------------------

                if (spygl[index].isVisible())
                {
                    //Spyglio Kolizija Su platforma
                    if ((spygl[index].posRigth()) > lavaMap[i].posLeft()
                        && spygl[index].posLeft() < lavaMap[i].posRigth())
                    {
                        //Spyglio apacios su plat virsasu collision. 
                        if (buvoVirsui && spygl[index].posBottom() >= lavaMap[i].posTop()
                            && spygl[index].posBottom() <= lavaMap[i].posBottom() + System.Math.Abs(20))
                        {
                            //spygl[index].Top = lavaMap[i].Top - spygl[index].Height;
                            spygl[index].set(spygl[index].posLeft(), lavaMap[i].posTop() - spygl[index].Height());
                            krist[index] = false;
                            spyglNukrito[index] = true;
                            break;
                        }

                    }
                }// Spyglio Kolizija su lava end
            }


            buvoApacioj = false;
            buvoVirsui = false;

            if (buvusVirsaus < buvusZaidejoTop && !spyglNukrito[index])
            {
                buvoVirsui = true;

            }
            else if ((buvusVirsaus + System.Math.Abs(spygl[index].Height() - Zaidejas.playerHeight) >= buvusZaidejoTop || buvusVirsaus > buvusZaidejoTop) && buvusVirsaus + spygl[index].Height() >= buvusZaidejoTop + Zaidejas.playerHeight)
            {
                buvoApacioj = true;
            }


            if (spygl[index].isVisible())
            {
                //Zaidejo kolizija su spygliu. DAR NEBAIGTA - bandau padaryt, kad galetum uzlipt ant spyglio, bet po kurio laiko jis pradetu krist
                if (Zaidejas.playerLeftPosition + Zaidejas.playerWidth > spygl[index].posLeft()
                    && Zaidejas.playerLeftPosition < spygl[index].posRigth()) // GALIMA pridet tikrinima ar nepasokes - bbus efektyviau
                {
                    //TOP collision. 
                    if (buvoApacioj && Zaidejas.playerTopPosition + Zaidejas.playerHeight >= spygl[index].posTop())
                    {
                        Zaidejas.playerTopPosition = spygl[index].posTop() - Zaidejas.playerHeight;
                        Zaidejas.jumped = false;
                        Zaidejas.antSpyglPavirsiaus = true;

                        if (Zaidejas.power < -40)
                        {
                            if (Zaidejas.lifeBarGyvybes > 0)
                            {
                                for (int spygg = 3; spygg > 0; spygg--)
                                {
                                    ledStrip.SetLed(lifeBarSk - (Zaidejas.lifeBarGyvybes), false);
                                    Zaidejas.lifeBarGyvybes -= 1;// Kritimo is aukstai zala
                                    // Zaidejas.minusGyvybe = true;
                                }
                            }
                        }
                        Zaidejas.power = 0;


                    }

                    //BOTTOM collision. 
                    if (buvoVirsui && spygl[index].posBottom() >= Zaidejas.playerTopPosition)
                    {
                        if (Zaidejas.lifeBarGyvybes > 0)
                        {
                            for (int spygl1 = 2; spygl1 > 0; spygl1--)
                            {
                                ledStrip.SetLed(lifeBarSk - (Zaidejas.lifeBarGyvybes), false);
                                Zaidejas.lifeBarGyvybes--;
                            }
                        }
                        // Zaidejas.minusGyvybe = true;
                        //      tunes.Play(500,200);
                        //.Play(200);
                        //.Stop();
                        // Form EkranasMetVid = new Form();// veikia taip pat kaip ir paduoti parametrai
                        //  spygl[index].Visible = false;
                        spygl[index].Hide();

                    }

                }
                else // Reikia Tvarkyt 
                {

                    Zaidejas.antSpyglPavirsiaus = false;

                }// Spyglio kol su zaideju end
            }//isVisible end
            //--------------------------------------------------------------------------------------------------------------------

            // the very bottom collision
            if (buvusVirsaus + spygl[index].Height() + 20 >= displayT43.Height || spygl[index].posBottom() >= displayT43.Height && Zaidejas.jumped)
            {
                //spygl[index].Top = Ekranas.Height - Zaidejas.Height;
                spygl[index].set(spygl[index].posLeft(), displayT43.Height - Zaidejas.playerHeight);
                spyglNukrito[index] = true;
            }

        }// SpyglioKritimas end

        //-------------------------------------------------------------------------------------------------------------
        static void Lava(ref PlayerStruct Zaidejas, ref Platform[] spygl, ref Platform[] lavaMap, int zaidejoBuvusTop, int lavaId, ref Rectangle player, ref LEDStrip ledStrip)
        {

            bool buvoVirsui;


            for (int i = 0; i < lavaId; i++)
            {


                buvoVirsui = false;

                if (zaidejoBuvusTop < lavaMap[i].posTop()) // zmogeliukas gali but ir prasmeges // Zinau, kad spyglys kris ant platformu is virsaus
                {
                    buvoVirsui = true;

                }

                if ((Zaidejas.playerLeftPosition + Zaidejas.playerWidth) > lavaMap[i].posLeft()
                    && Zaidejas.playerLeftPosition < lavaMap[i].posRigth())
                {
                    //TOP collision. (platHeight/2 -1) - even if player is in platform 
                    // (from top) and doesn't touch the middle he is set on top 

                    if (buvoVirsui && Zaidejas.playerTopPosition + Zaidejas.playerHeight >= lavaMap[i].posTop()
                        && Zaidejas.playerTopPosition + Zaidejas.playerHeight <= lavaMap[i].posBottom() + System.Math.Abs(Zaidejas.power))
                    {

                        Zaidejas.playerTopPosition = lavaMap[i].posTop() - Zaidejas.playerHeight;
                        // player.Fill =  new SolidColorBrush(Colors.Black);// Nebaigta - pradetas mirgsejimas
                        // System.Threading.Thread.Sleep(50);




                        Zaidejas.antLavosPavirsiaus = true;
                        Zaidejas.jumped = false;//Be sito nesokineja nat lavos

                        if (Zaidejas.power < -40)
                        {
                            if (Zaidejas.lifeBarGyvybes > 0)
                            {
                                for (int kint = 3; kint > 0; kint--)
                                {
                                    ledStrip.SetLed(lifeBarSk - (Zaidejas.lifeBarGyvybes), false);
                                    Zaidejas.lifeBarGyvybes -= 1;// Kritimo is aukstai zala
                                }
                            }
                            // Zaidejas.minusGyvybe = true;
                        }
                        Zaidejas.power = 0;

                        if (Zaidejas.gyvybiuSkaitiklis > 5)// Reguliuoja gyvybiu ateminejimo greiti
                        {
                            if (Zaidejas.lifeBarGyvybes > 0)
                                ledStrip.SetLed(lifeBarSk - (Zaidejas.lifeBarGyvybes), false);// cia luzta

                            Zaidejas.lifeBarGyvybes--;
                            Zaidejas.gyvybiuSkaitiklis = 0;
                            // Zaidejas.minusGyvybe = true;
                        }
                        Zaidejas.gyvybiuSkaitiklis++;

                        break;

                    }
                }
                else Zaidejas.antLavosPavirsiaus = false;
            }
        }// Lava end
        //---------------------------------------------------------------------------------------------------
        void Durys()
        {
            bool buvoKairej = false;
            bool buvoDesinej = false;

            for (int i = 0; i < duruId; i++)
            {
                // the very bottom collision
                if (durysMap[i].posBottom() >= displayT43.Height)
                {
                    durysMap[i].set(durysMap[i].posLeft(), displayT43.Height - durysMap[i].Height());

                }

                //Sonu kolizija


                buvoKairej = false;
                buvoDesinej = false;

                //-------------------------------------------------------------------------------------------------
                //Buvusi pozicija
                if (buvusKaires < durysMap[i].posLeft() && buvusKaires + playerStruct.playerWidth < durysMap[i].posLeft()
                   && ((playerStruct.playerTopPosition < durysMap[i].posTop() || playerStruct.playerTopPosition + playerStruct.playerHeight < playerStruct.playerTopPosition) || (durysMap[i].posTop() + durysMap[i].Height() > playerStruct.playerTopPosition
                   || playerStruct.playerTopPosition + playerStruct.playerHeight < durysMap[i].posTop() + durysMap[i].Height())))
                {
                    buvoKairej = true;


                }
                else if (buvusKaires > durysMap[i].posLeft() + durysMap[i].Width() && buvusKaires + playerStruct.playerWidth > durysMap[i].posLeft() + durysMap[i].Width()
                   && ((playerStruct.playerTopPosition < durysMap[i].posTop() || playerStruct.playerTopPosition + playerStruct.playerHeight < playerStruct.playerTopPosition) || (durysMap[i].posTop() + durysMap[i].Height() > playerStruct.playerTopPosition
                   || playerStruct.playerTopPosition + playerStruct.playerHeight < durysMap[i].posTop() + durysMap[i].Height())))
                {
                    buvoDesinej = true;
                }

                //---------------------------------------------------------------------------------------------------------------------

                if (playerStruct.playerTopPosition + playerStruct.playerHeight >= durysMap[i].posTop() + 1 //Taisiau, kaip ir gerai
                                      && playerStruct.playerTopPosition <= durysMap[i].posTop() + durysMap[i].Height() - 1)
                {
                    //RIGHT collision. (platWidth / 2 - 1) - even if player is in platform 
                    // (from left) and don't touch the middle he collides with left

                    if (buvoKairej && (playerStruct.playerLeftPosition + playerStruct.playerWidth) + 1 >= durysMap[i].posLeft()
                        && (playerStruct.playerLeftPosition + playerStruct.playerWidth) <= durysMap[i].posLeft() + durysMap[i].Width())
                    {
                        //playerStruct.playerLeftPosition = durysMap[i].posLeft() - playerStruct.playerWidth;
                        //stopRight = true;// stops moving right
                        player.Fill = new SolidColorBrush(Color.Black);
                        playerStruct.laimejo = true;

                        //if (lygis == 1)
                        //{   

                        //    lygis = 2;
                        //    perejoLygi = true;
                        //}

                        if (lygis < yraLygiu && !perejoLygi)
                        {
                            lygis++;
                            perejoLygi = true;
                        }
                        

                        break;
                    }
                    //else stopRight = false;

                    //LEFT collision. (platWidth / 2 - 1) - even if player is in platform 
                    // (from right) and don't touch the middle he collides with right

                    if (buvoDesinej && playerStruct.playerLeftPosition - 1 <= durysMap[i].posLeft() + durysMap[i].Width()
                        && playerStruct.playerLeftPosition >= durysMap[i].posLeft())
                    {
                        //playerStruct.playerLeftPosition = durysMap[i].posLeft() + durysMap[i].Width();
                        //stopLeft = true;// stops moving left
                        player.Fill = new SolidColorBrush(Color.Black);
                        playerStruct.laimejo = true;
                       
                        //if(lygis == 1)
                        //{
                        //    lygis = 2;
                        //    perejoLygi = true;
                        //}


                        if (lygis < yraLygiu && !perejoLygi)
                        {
                            lygis++;
                            perejoLygi = true;
                        }
                        break;
                    }
                    //else stopLeft = false;

                }
                //else
                //{
                //    stopRight = false;
                //    stopLeft = false;
                //}



            }
        }// void Durys end

        void CheckForLanding()
        {
            if (snowflakeTopPosition > playerStruct.playerTopPosition && snowflakeTopPosition < playerStruct.playerTopPosition + playerStruct.playerHeight)
            {
                if (snowflakeLeftPosition + 10 >= playerStruct.playerLeftPosition
                &&
                snowflakeLeftPosition <= playerStruct.playerLeftPosition + playerStruct.playerWidth)
                {
                    snaigiuTaskai++;
                    //label2.TextContent = "platTOPPosition: " + platTOPPosition;
                    ResetSnowflake();
                }

            }

        } //void CheckForLanding end
    }
}
