using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Net.NetworkInformation;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;


namespace VeIsNaujo_NuoSokinejantis_1
{
    public partial class Program
    {
        #region Variables
        Window mainWindow;
        Canvas layout;
        Text label;// Lifes
        Text label2;//End Text
        Text label3;// Restart Text
        Text label4;// Win Text
        Text scoreLabel;// Score Text

        Rectangle player;
        Rectangle snowflake;

        public struct PlayerStruct
        {
            
            public int playerTopPosition;
            public int playerLeftPosition;            
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
            public bool laimejoZaidima;
            public int gyvybes;

           
            public PlayerStruct(int playerTopPos, int playerLeftPos, int playerH, int playerW, int jumpP, bool jmp, int pow, bool antPavirsiaus, int barLives, int livesCount, bool neZeme, bool antLavos, bool die, bool win, int lives, bool winGame)
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
                laimejoZaidima = winGame;

                //playerBottomPosition = playerTopPosition + playerHeight;
                //playerRightPosition = playerLeftPosition + playerWidth;
            }
        }

        #region OtherStuff
        static int lifeBarSk = 7; 
        static int gyvybiuSk = 3;
        PlayerStruct playerStruct = new PlayerStruct(0, 0, 20, 20, 25, true, 0, false, lifeBarSk, 0, true, false, false, false, gyvybiuSk, false);
        #endregion
           
        #region Platform parameters
        //If platform parameters changes, position of platform also changes
        int platHeight = 25;
        int platWidth = 25;
        #endregion
        #region Collision parameters
        bool stopLeft = false;// makes stop move left
        bool stopRight = false;// makes stop move right
        #endregion
        GT.Timer joystickTimer = new GT.Timer(30);
        GT.Timer jumpTimer = new GT.Timer(30);
        #region Map generation
        // -88 this offset makes the platform align with botom of the screen
        // int platPraziosTop = platHeight * (-12);
        int basePositionTop = 0; 
        int basePositionLeft = 0;
        int platformId = 0;
        #endregion
        Platform[] platformMap = new Platform[200];
        #region Previous player pozition, for collision
        int buvusKaires;
        int buvusVirsaus;
        #endregion
        #region Spikes        
        static int spygliuMaxSk = 20;
        int spygliaiId = 0;
        int spyglHeigth = 20;
        int spyglWidth = 10;
        bool[] spyglKrist = new bool[spygliuMaxSk];
        bool[] pirmasKartas = new bool[spygliuMaxSk];
        bool[] spyglNukrito = new bool[spygliuMaxSk];
        Platform[] spygliaiMap = new Platform[spygliuMaxSk];
        #endregion
        #region Lava
        static int lavosMaxSk = 35;
        int lavaId = 0;
        int lavaHeigth = 25;
        int lavaWidth = 25;
        Platform[] lavaMap = new Platform[lavosMaxSk];
        #endregion
        #region Level Restart
        bool restartinimas = false;
        #endregion
        #region Win Door
        static int duruMaxSk = 10;
        int duruId = 0;
        int duruHeigth = 35;
        int duruWidth = 25;
        Platform[] durysMap = new Platform[duruMaxSk];
        #endregion
        #region Snow Flake
        int snowflakeLeftPosition = 150;
        int snowflakeTopPosition = 50;
        Random randomNumberGenerator = new Random();
        GT.Timer snowFlakeTimer = new GT.Timer(75);
        #endregion
        #region Score
        int galutiniaiTaskai = 0;
        int snaigiuTaskai = 0;
        static int duotaLaiko = 1200;
        int laikoTaskai = duotaLaiko;
        #endregion
        #region Levels
        int level = 1;
        bool perejoLygi = false;
        int buveslevel = 1;
        int yraLygiu = 3;
        bool nextLevel = false;
        #endregion
        #region Confidential
        const string SSID = "HUA";
        const string PASSWORD = "12345678";

        //servername = "remotemysql.com";
        //username = "L6P59qwcqT";
        //password = "7Ngh9iUx0x";
        //dbname = "L6P59qwcqT";
        #endregion
        GT.Timer Kick;
        #endregion

        #region Setup
        void ProgramStarted()
        {
            SetupUI();

            Canvas.SetLeft(player, playerStruct.playerLeftPosition);
            Canvas.SetTop(player, playerStruct.playerTopPosition);
            
            if (!restartinimas)
            {
                #region wifi
                if (!wifiRS21.NetworkInterface.Opened)
                    wifiRS21.NetworkInterface.Open();

                if (!wifiRS21.NetworkInterface.IsDhcpEnabled)
                    wifiRS21.NetworkInterface.EnableDhcp();

                NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
                NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;

                Kick = new GT.Timer(100, GT.Timer.BehaviorType.RunOnce);
                Kick.Tick += Kick_Tick;
                Kick.Start();
                #endregion
                jumpTimer.Tick += new GT.Timer.TickEventHandler(jumpTimer_Tick);
                jumpTimer.Start();

                snowFlakeTimer.Tick += new GT.Timer.TickEventHandler(SnowflakeTimer_Tick);
                snowFlakeTimer.Start();

                gyro.MeasurementComplete += gyro_MeasurementComplete;
                gyro.Calibrate();
                gyro.StartTakingMeasurements();
            }



        }

     

        void SetupUI()
        {           

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
                label.Height = 272;
                label.Width = 480;
                label.ForeColor = Colors.White;
                label.Font = Resources.GetFont(Resources.FontResources.NinaB);
                layout.Children.Add(label);
                Canvas.SetLeft(label, 0);
                Canvas.SetTop(label, 0);

                //text
                scoreLabel = new Text();
                scoreLabel.Height = 272;
                scoreLabel.Width = 480;
                scoreLabel.ForeColor = Colors.White;
                scoreLabel.Font = Resources.GetFont(Resources.FontResources.NinaB);
                layout.Children.Add(scoreLabel);
                Canvas.SetLeft(scoreLabel, 0);
                Canvas.SetTop(scoreLabel, 0 + label.Font.Height);
            }           

            //Spikes
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
            if (level == 1)
            {
                // First level           

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
            else if (level == 2)
            {
                // Second level           
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
            else if (level == 3)
            {
                // Third level           
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
                        lavaMap[lavaId].set(basePositionLeft + j * platWidth, basePositionTop + i * platHeight);
                        lavaId++;
                    }
                    if (map[i][j] == 'D')
                    {
                        durysMap[duruId] = new Platform(duruWidth, duruHeigth);
                        durysMap[duruId].paint(Colors.Green);                       
                        durysMap[duruId].set(basePositionLeft + j * platWidth, basePositionTop + i * platHeight);
                        duruId++;
                    }
                }
            }

            // Adding platforms to screen            
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
            
            //Spikes change pozition, need to create again
            for (int i = 0; i < spygliaiId; i++)
            {
                layout.Children.Add(spygliaiMap[i].get());
            }

            if (!restartinimas)
            {
                
                //Death text
                label2 = new Text();
                label2.Height = 272;
                label2.Width = 480;          
                label2.ForeColor = Colors.White;
                label2.Font = Resources.GetFont(Resources.FontResources.NinaB);

                layout.Children.Add(label2);
                Canvas.SetLeft(label2, 180);
                Canvas.SetTop(label2, 132);

                //Restart text
                label3 = new Text();
                label3.Height = 272;
                label3.Width = 480;        
                label3.ForeColor = Colors.White;
                label3.Font = Resources.GetFont(Resources.FontResources.NinaB);

                layout.Children.Add(label3);
                Canvas.SetLeft(label3, 115);
                Canvas.SetTop(label3, 132 + label2.Font.Height);

                //Win text
                label4 = new Text();
                label4.Height = 272;
                label4.Width = 480;       
                label4.ForeColor = Colors.White;
                label4.Font = Resources.GetFont(Resources.FontResources.NinaB);

                layout.Children.Add(label4);
                Canvas.SetLeft(label4, 180);//Same height as death text
                Canvas.SetTop(label4, 132);
            }

            mainWindow.Child = layout;
        }
        #endregion

        #region wifi2
        void Kick_Tick(GT.Timer timer)
        {
            Debug.Print("Attempt to Join");
            try
            {
                wifiRS21.NetworkInterface.Join(SSID, PASSWORD);

            }
            catch (Exception ex)
            {
                Debug.Print("Exception: " + ex.Message);
            }

        }
        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            Debug.Print("Network Availability: " + e.IsAvailable.ToString());
        }
        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            Debug.Print("Address: " + wifiRS21.NetworkInterface.IPAddress);
        }
        #endregion

        #region Snow Flake
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
        #endregion

        #region Gyro
        void gyro_MeasurementComplete(Gyro sender, Gyro.MeasurementCompleteEventArgs e)
        {
            double X = e.X;
            double Y = e.Y;
            double Z = e.Z;

            if (!playerStruct.jumped)
            {

                if (Y > 25)
                {
                    playerStruct.jumped = true;                    
                    playerStruct.power = playerStruct.jumpPower;
                }
            }
            Canvas.SetTop(player, playerStruct.playerTopPosition);
            if (X < -10 && X != 0 && playerStruct.playerLeftPosition > 0)
            {                
                playerStruct.playerLeftPosition -= 5;
            }

            if (X > 5 && X != 0 && (playerStruct.playerLeftPosition + playerStruct.playerWidth) != 800)
            {                
                playerStruct.playerLeftPosition += 5;
            }
            Canvas.SetLeft(player, playerStruct.playerLeftPosition);
            CheckForLanding();
        }
        #endregion

        #region Timer
        void jumpTimer_Tick(GT.Timer timer)
        {

            //-------------------------------------------------------------------------------------------------------
            //PLATFORM COLLISION AND MOVEMENT
            //------------------------------------------------------------------------------------------------------
            
            if (!playerStruct.mire && !playerStruct.laimejo)
            {

                JudejimasY(ref  buvusVirsaus, ref  playerStruct, joystick);

                Canvas.SetTop(player, playerStruct.playerTopPosition);

                VirsausKolizija(platformId, platformMap, buvusVirsaus, ref playerStruct, displayT43, ref ledStrip);

                JudejimasX(ref buvusKaires, stopLeft, stopRight, ref playerStruct, joystick, displayT43);

                Canvas.SetLeft(player, playerStruct.playerLeftPosition);// update player left position

                SonuKolizija(platformId, platformMap, buvusKaires, ref playerStruct, ref stopRight, ref stopLeft, displayT43);

                Canvas.SetTop(player, playerStruct.playerTopPosition); // update player top position
            }

            //-------------------------------------------------------------------------------------------------------
            // SPIKE COLLISION AND MOVEMENT
            //------------------------------------------------------------------------------------------------------

            //Spygliu ciklas
            for (int i = 0; i < spygliaiId; i++)
            SpyglioKritimas(ref playerStruct, ref spygliaiMap, ref platformMap, spyglKrist, pirmasKartas, spyglNukrito, buvusVirsaus, i, ref lavaMap, displayT43, platformId, lavaId, ref ledStrip);

            //Lava
            Lava(ref playerStruct, ref spygliaiMap, ref lavaMap, buvusVirsaus, lavaId, ref player, ref ledStrip);

            //Door
            Durys();

            //Snowflake
            CheckForLanding();

            //Led bar and life action
            if (playerStruct.lifeBarGyvybes == 0)
            {
                
                if (playerStruct.gyvybes > 0)
                {
                    playerStruct.lifeBarGyvybes = lifeBarSk;
                    ledStrip.SetLeds(lifeBarSk);
                }
                if (playerStruct.gyvybes > 0)
                {
                    led7R.TurnLedOff(playerStruct.gyvybes - 1);
                    playerStruct.gyvybes--;                   
                }
            }
                       
            if (!playerStruct.mire) label.TextContent = "Gyvybes: " + playerStruct.gyvybes;
            if (!playerStruct.mire && !playerStruct.laimejo)
            {
                if (laikoTaskai > 0) laikoTaskai--;               
                galutiniaiTaskai = (snaigiuTaskai * 300 + playerStruct.lifeBarGyvybes * 35 + playerStruct.gyvybes * 450 + laikoTaskai);// Tasku formule
                scoreLabel.TextContent = "Taskai: " + galutiniaiTaskai;
            }
            
            if (playerStruct.gyvybes < 1 && playerStruct.lifeBarGyvybes < 1) playerStruct.mire = true;
           
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
           
            if (playerStruct.laimejo && !playerStruct.mire)
            {              

                int sk = 1;
                while (sk > 0)
                {
                    if (joystick.IsPressed)
                    {
                        nextLevel = true;
                        break;
                    }
                }
               
                if (nextLevel)
                {                    
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

                    if (playerStruct.laimejoZaidima)
                    {
                        postScore();
                        level = 1;
                        playerStruct.laimejoZaidima = false;
                    }

                    ProgramStarted();
                    perejoLygi = false;
                }
            }
            if (playerStruct.mire && joystick.IsPressed && !playerStruct.laimejo)
            {
                postScore();

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

                if (level == 2 || level == 3)
                    level = 1;

                ProgramStarted();
            }

            if (button.Pressed)
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

        }
        #endregion

        #region Post_Score
        private void postScore()
        {
            try
            {
                Debug.Print("**** Starting web request ****");

                string URL = "http://ktuprojektas.000webhostapp.com/add.php?rezultatas=" + galutiniaiTaskai + "&uid=" + 13;
                Debug.Print("Kreipiamasi i: " + URL);

                //if (succsess == true)
                //    break;

                HttpRequest request = HttpHelper.CreateHttpGetRequest(URL);
                request.ResponseReceived += postData;
                request.SendRequest();
            }
            catch
            {
                Debug.Print("Iðimtis: Exception during web request");
            }
        }

        private void postData(HttpRequest sender, HttpResponse response)
        {
            Debug.Print(response.Text);
        }
        //jumpTimer end
        #endregion

        #region Metodai
        //-------------------------------------------------------------------------------------------------
        static void VirsausKolizija(int mapId, Platform[] map, int buvusVirsaus, ref PlayerStruct str, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43, ref LEDStrip ledStrip)
        {
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
                                    str.lifeBarGyvybes -= 1;//Fall damage
                                }
                            }                         
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
                        break;
                    }
                }             
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
                            str.lifeBarGyvybes -= 1;//Fall damage                           
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
                //Previous position
                if (buvusKaires < map[i].posLeft() && buvusKaires + str.playerWidth < map[i].posLeft()
                   && ((str.playerTopPosition < map[i].posTop() || str.playerTopPosition + str.playerHeight < str.playerTopPosition) 
                   || (map[i].posTop() + map[i].Height() > str.playerTopPosition
                   || str.playerTopPosition + str.playerHeight < map[i].posTop() + map[i].Height())))
                {
                    buvoKairej = true;
                }
                else if (buvusKaires > map[i].posLeft() + map[i].Width() && buvusKaires + str.playerWidth > map[i].posLeft() + map[i].Width()
                   && ((str.playerTopPosition < map[i].posTop() || str.playerTopPosition + str.playerHeight < str.playerTopPosition)
                   || (map[i].posTop() + map[i].Height() > str.playerTopPosition
                   || str.playerTopPosition + str.playerHeight < map[i].posTop() + map[i].Height())))
                {
                    buvoDesinej = true;
                }

                //---------------------------------------------------------------------------------------------------------------------

                if (str.playerTopPosition + str.playerHeight >= map[i].posTop() + 1 
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

        }// SonuKolizija end

        static void JudejimasX(ref int buvusKaires, bool stopLeft, bool stopRight, ref PlayerStruct str, Gadgeteer.Modules.GHIElectronics.Joystick joystick, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43)
        {
            //Movement left/right
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
                if (y >= 0.7) // and jumps
                {
                    str.jumped = true;
                    str.power = str.jumpPower;
                }
            }

            //Jump
            if (str.jumped)
            {  
                buvusVirsaus = str.playerTopPosition;
                str.playerTopPosition -= str.power;
                str.power -= 3;
            }
        }// JudejimasY end

        static void SpyglioKritimas(ref PlayerStruct Zaidejas, ref Platform[] spygl, ref Platform[] platformMap, bool[] krist, bool[] pirmasKartas, bool[] spyglNukrito, int buvusZaidejoTop, int index, ref Platform[] lavaMap, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43, int platId, int lavaId, ref LEDStrip ledStrip)
        {
            int buvusVirsaus = spygl[index].posTop();// assign to previous spike top position


            if (spygl[index].isVisible())
            {

                if (Zaidejas.playerLeftPosition + Zaidejas.playerHeight >= spygl[index].posLeft() && Zaidejas.playerLeftPosition <= spygl[index].posRigth() && Zaidejas.playerTopPosition + Zaidejas.playerHeight >= spygl[index].posTop()) krist[index] = true;


                if (krist[index] && !spyglNukrito[index])
                {

                    if (pirmasKartas[index])
                    {
                        System.Threading.Thread.Sleep(20);
                        pirmasKartas[index] = false;
                    }
                   
                    spygl[index].set(spygl[index].posLeft(), spygl[index].posTop() + 10);

                    Zaidejas.antSpyglPavirsiaus = false;
                }
            }


            bool buvoApacioj = false;
            bool buvoVirsui = false;

            for (int i = 0; i < platId; i++)
            {

                buvoApacioj = false;
                buvoVirsui = false;
               

                if (spygl[index].isVisible())
                {
                    //Spike collision with platform
                    if ((spygl[index].posRigth()) > platformMap[i].posLeft()
                        && spygl[index].posLeft() < platformMap[i].posRigth())
                    {
                        //Spike bottom with plat top collision. 
                        if ( spygl[index].posBottom() >= platformMap[i].posTop()
                            && spygl[index].posBottom() <= platformMap[i].posBottom() + System.Math.Abs(20))
                        {                            
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

                if (buvusVirsaus < lavaMap[i].posTop()) 
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
                    //Spike collision with platform
                    if ((spygl[index].posRigth()) > lavaMap[i].posLeft()
                        && spygl[index].posLeft() < lavaMap[i].posRigth())
                    {
                        // Spike bottom with plat top collision
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
            else if ((buvusVirsaus + System.Math.Abs(spygl[index].Height() - Zaidejas.playerHeight) >= buvusZaidejoTop 
                    || buvusVirsaus > buvusZaidejoTop) && buvusVirsaus + spygl[index].Height() >= buvusZaidejoTop + Zaidejas.playerHeight)
            {
                buvoApacioj = true;
            }


            if (spygl[index].isVisible())
            {
                //Player collision with spike. 
                if (Zaidejas.playerLeftPosition + Zaidejas.playerWidth > spygl[index].posLeft()
                    && Zaidejas.playerLeftPosition < spygl[index].posRigth()) 
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
                                    ledStrip.SetLed(lifeBarSk - (Zaidejas.lifeBarGyvybes), false);
                                    Zaidejas.lifeBarGyvybes -= 1;// Fall damage                                    
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
                        spygl[index].Hide();
                    }
                }
                else 
                {

                    Zaidejas.antSpyglPavirsiaus = false;

                }// Spyglio kol su zaideju end
            }//isVisible end
            //--------------------------------------------------------------------------------------------------------------------

            // the very bottom collision
            if (buvusVirsaus + spygl[index].Height() + 20 >= displayT43.Height || spygl[index].posBottom() >= displayT43.Height && Zaidejas.jumped)
            {               
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

                if (zaidejoBuvusTop < lavaMap[i].posTop()) 
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

                        Zaidejas.antLavosPavirsiaus = true;
                        Zaidejas.jumped = false;

                        if (Zaidejas.power < -40)
                        {
                            if (Zaidejas.lifeBarGyvybes > 0)
                            {
                                for (int kint = 3; kint > 0; kint--)
                                {
                                    ledStrip.SetLed(lifeBarSk - (Zaidejas.lifeBarGyvybes), false);
                                    Zaidejas.lifeBarGyvybes -= 1;// Fall damage
                                }
                            }                        
                        }
                        Zaidejas.power = 0;

                        if (Zaidejas.gyvybiuSkaitiklis > 5)// Regulate lifes decrease rate
                        {
                            if (Zaidejas.lifeBarGyvybes > 0)
                                ledStrip.SetLed(lifeBarSk - (Zaidejas.lifeBarGyvybes), false);

                            Zaidejas.lifeBarGyvybes--;
                            Zaidejas.gyvybiuSkaitiklis = 0;                           
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

                //Sides collison

                buvoKairej = false;
                buvoDesinej = false;

                //-------------------------------------------------------------------------------------------------
                //Previous position
                if (buvusKaires < durysMap[i].posLeft() && buvusKaires + playerStruct.playerWidth < durysMap[i].posLeft()
                   && ((playerStruct.playerTopPosition < durysMap[i].posTop() || playerStruct.playerTopPosition + playerStruct.playerHeight < playerStruct.playerTopPosition) 
                   || (durysMap[i].posTop() + durysMap[i].Height() > playerStruct.playerTopPosition
                   || playerStruct.playerTopPosition + playerStruct.playerHeight < durysMap[i].posTop() + durysMap[i].Height())))
                {
                    buvoKairej = true;
                }
                else if (buvusKaires > durysMap[i].posLeft() + durysMap[i].Width() && buvusKaires + playerStruct.playerWidth > durysMap[i].posLeft() + durysMap[i].Width()
                   && ((playerStruct.playerTopPosition < durysMap[i].posTop() || playerStruct.playerTopPosition + playerStruct.playerHeight < playerStruct.playerTopPosition) 
                   || (durysMap[i].posTop() + durysMap[i].Height() > playerStruct.playerTopPosition
                   || playerStruct.playerTopPosition + playerStruct.playerHeight < durysMap[i].posTop() + durysMap[i].Height())))
                {
                    buvoDesinej = true;
                }

                //---------------------------------------------------------------------------------------------------------------------

                if (playerStruct.playerTopPosition + playerStruct.playerHeight >= durysMap[i].posTop() + 1 
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

                        if (level == yraLygiu)
                        playerStruct.laimejoZaidima = true;
                       
                        if (level < yraLygiu && !perejoLygi)
                        {
                            level++;
                            perejoLygi = true;
                        }
                        break;
                    }
                   

                    //LEFT collision. (platWidth / 2 - 1) - even if player is in platform 
                    // (from right) and don't touch the middle he collides with right

                    if (buvoDesinej && playerStruct.playerLeftPosition - 1 <= durysMap[i].posLeft() + durysMap[i].Width()
                        && playerStruct.playerLeftPosition >= durysMap[i].posLeft())
                    {
                        //playerStruct.playerLeftPosition = durysMap[i].posLeft() + durysMap[i].Width();
                        //stopLeft = true;// stops moving left
                        player.Fill = new SolidColorBrush(Color.Black);
                        playerStruct.laimejo = true;

                        if (level == yraLygiu)
                            playerStruct.laimejoZaidima = true;     

                        if (level < yraLygiu && !perejoLygi)
                        {
                            level++;
                            perejoLygi = true;
                        }
                        break;
                    }               

                }              

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
                    ResetSnowflake();
                }

            }

        } //void CheckForLanding end
        #endregion
    }
}
