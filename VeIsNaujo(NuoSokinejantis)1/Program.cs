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


//Pirmas lygis V8
//Pradejau daryt, kad mirgsetu ant lavos








namespace VeIsNaujo_NuoSokinejantis_1
{
    public partial class Program
    {
        Window mainWindow;
        Canvas layout;
        Text label;// Gyvybes
        Text label2;//Pabaigos Tekstas
        Text label3;// Score
        
        Rectangle player;

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
            public int gyvybes;
            public int gyvybiuSkaitiklis;
            public bool neAntZemes;
            public bool mire;
          //  playerBottomPosition = playerTopPosition + playerHeight;
           public PlayerStruct(int playerTopPos, int playerLeftPos, int playerH, int playerW, int jumpP, bool jmp, int pow, bool antPavirsiaus, int lives, int livesCount, bool neZeme, bool antLavos, bool die)
            {
                playerTopPosition = playerTopPos;
                
                playerLeftPosition = playerLeftPos;
                
                playerHeight = playerH;
                playerWidth = playerW;
                jumpPower = jumpP;
                jumped = jmp;
                power = pow;
                antSpyglPavirsiaus = antPavirsiaus;
                gyvybes = lives;
                gyvybiuSkaitiklis = livesCount;
                neAntZemes = neZeme;
                antLavosPavirsiaus = antLavos;
                mire = die;

                //playerBottomPosition = playerTopPosition + playerHeight;
                //playerRightPosition = playerLeftPosition + playerWidth;
            }
        }
        PlayerStruct playerStruct = new PlayerStruct(0,0,20,20,25,true,0,false,300,0,true,false, false);



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
        static int spygliuMaxSk = 10;
        int spygliaiId = 0;
        int spyglHeigth = 20;
        int spyglWidth = 10;
        bool[] spyglKrist = new bool[spygliuMaxSk];
        bool[] pirmasKartas = new bool[spygliuMaxSk];
        bool[] spyglNukrito = new bool[spygliuMaxSk];
        Platform[] spygliaiMap = new Platform[spygliuMaxSk];

        //Lava
        static int lavosMaxSk = 10;
        int lavaId = 0;
        int lavaHeigth = 25;
        int lavaWidth = 25;
        Platform[] lavaMap = new Platform[lavosMaxSk];
       
        //Lyugio Restartinimas
        bool restartinimas = false;
        
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

                //label
                label = new Text();
                label.Height = 272;// buvo 240
                label.Width = 480;// buvo 320
                label.ForeColor = Colors.White;
                label.Font = Resources.GetFont(Resources.FontResources.NinaB);
                layout.Children.Add(label);
                Canvas.SetLeft(label, 0);
                Canvas.SetTop(label, 0);
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



            // Platformer map
            //Sunki versija
            //string[] map = new string[12];
            //map[0] = ".....................................";
            //map[1] = ".....................................";
            //map[2] = ".....................................";
            //map[3] = ".....................................";
            //map[4] = ".................#...................";
            //map[5] = "...........S...###...................";
            //map[6] = ".#...S...S...S.......................";
            //map[7] = "........S............................";
            //map[8] = ".....................................";
            //map[9] = ".....................................";
            //map[10] = "...#................................";
            //map[11] = "####LLLLLLLLL#......................";

            //Lengva versija
            string[] map = new string[12];
            map[0] = ".....................................";
            map[1] = ".....................................";
            map[2] = ".....................................";
            map[3] = ".................#...................";
            map[4] = "...........S...###...................";
            map[5] = ".....S...S...S.......................";
            map[6] = ".#.....S.............................";
            map[7] = ".....................................";
            map[8] = ".....................................";
            map[9] = ".....................................";
            map[10] = "...#................................";
            map[11] = "####LLLLLLLLL#......................";



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
                }
            }



            // Adding platforms to screen
            for (int i = 0; i < platformId; i++)
            {
                layout.Children.Add(platformMap[i].get());
            }

            for (int i = 0; i < spygliaiId; i++)
            {
                layout.Children.Add(spygliaiMap[i].get());
            }

            for (int i = 0; i < lavaId; i++)
            {
                layout.Children.Add(lavaMap[i].get());
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

                //Tekstas mirties uzrasui
                label3 = new Text();
                label3.Height = 272;// buvo 240
                label3.Width = 480;// buvo 320          
                label3.ForeColor = Colors.White;
                label3.Font = Resources.GetFont(Resources.FontResources.NinaB);

                layout.Children.Add(label3);
                Canvas.SetLeft(label3, 110);
                Canvas.SetTop(label3, 132 + label2.Font.Height);
            }
           
            mainWindow.Child = layout;
        }


        void jumpTimer_Tick(GT.Timer timer)
        {
             
           //-------------------------------------------------------------------------------------------------------
           // PLATFORMOS KOLIZIJA IR JUDEJIMAS
           //------------------------------------------------------------------------------------------------------
            //player.Fill = new SolidColorBrush(Colors.Red); //Pakeiciu zaidejo spauva is juodos i raudona - mirgsejimas
         
            if (!playerStruct.mire) JudejimasY(ref  buvusVirsaus, ref  playerStruct,  joystick);// Sutvarkyta

            Canvas.SetTop(player, playerStruct.playerTopPosition);
            
            VirsausKolizija(platformId,  platformMap,  buvusVirsaus, ref playerStruct, displayT43);//Pataisyta

            if (!playerStruct.mire) JudejimasX(ref buvusKaires, stopLeft, stopRight, ref playerStruct, joystick, displayT43);// Pataisyta

            Canvas.SetLeft(player, playerStruct.playerLeftPosition);// update player left posti

            SonuKolizija(platformId, platformMap, buvusKaires, ref playerStruct, ref stopRight, ref stopLeft, displayT43);// Sutvarkyta

            Canvas.SetTop(player, playerStruct.playerTopPosition); // update player top position

            //-------------------------------------------------------------------------------------------------------
            // SPYGLIO KOLIZIJA IR JUDEJIMAS
            //------------------------------------------------------------------------------------------------------
            
            //Spygliu ciklas
            for (int i = 0; i < spygliaiId; i++)
               
                SpyglioKritimas(ref playerStruct, ref spygliaiMap, ref platformMap, spyglKrist, pirmasKartas, spyglNukrito, buvusVirsaus, i, ref lavaMap, displayT43, platformId, lavaId);
               

            //Lava
            Lava(ref playerStruct, ref spygliaiMap, ref lavaMap, buvusVirsaus, lavaId, ref player);
           
            
            if (!playerStruct.mire) label.TextContent = "Gyvybes: " + playerStruct.gyvybes;
            if (playerStruct.gyvybes < 1) playerStruct.mire = true;
            if (playerStruct.mire)
            {
                label2.TextContent = "Zaidimas Baigtas!";
                label3.TextContent = "Paspauskite valdikli, kad kartotumete";
            }
            if (!playerStruct.mire)
            {
                label2.TextContent = "";
                label3.TextContent = "";
            }
            if (playerStruct.mire && joystick.IsPressed)
            {
               
                // nieko nedaro
                //ProgramStarted();
                //SetupUI();
                // base.Run();
                // Application.Current.Run();
                playerStruct.playerTopPosition = 240;
                playerStruct.playerLeftPosition = 0;
                playerStruct.gyvybes = 3;
                playerStruct.mire = false;
             
                
                //mainWindow.Child = null;
                //mainWindow = null;
               
                //layout.Children.Remove(label);
                //layout.Children.Remove(label2); //Blogai nes 2tra kart mirus neismeta teksto
                //layout.Children.Remove(label3);

                for (int i = 0; i < platformId; i++) layout.Children.Remove(platformMap[i].get());
                for (int i = 0; i < lavaId; i++) layout.Children.Remove(lavaMap[i].get());
                for (int i = 0; i < spygliaiId; i++) layout.Children.Remove(spygliaiMap[i].get());
             
                lavaId = 0;
                platformId = 0;
                spygliaiId = 0;
                restartinimas = true;
      
                ProgramStarted();

                //label2.TextContent = "";
                //label3.TextContent = "";
                
                //SetupUI(); sito ne tik nevykdo, bet ji idejus neveikia visas if
            }


        }//jumpTimer end
      
        
        //-------------------------------------------------------------------------------------------------
        static void VirsausKolizija(int mapId, Platform[] map, int buvusVirsaus, ref PlayerStruct str, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43)
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

                        if (str.power < -40) str.gyvybes -= 20;//Kritimo is aukstai zala
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

                if (str.power < -40) str.gyvybes -= 20;// Kritimo is aukstai zala
                str.power = 0;
            }

        }// VirsasuKolizija end
       

       
        static void SonuKolizija(int mapId, Platform[] map, int buvusKaires,ref PlayerStruct str, ref bool stopRight, ref bool stopLeft, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43)
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

        static void JudejimasX(ref int buvusKaires,  bool stopLeft, bool stopRight, ref PlayerStruct str, Gadgeteer.Modules.GHIElectronics.Joystick joystick, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43)
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

        static void SpyglioKritimas(ref PlayerStruct Zaidejas, ref Platform[] spygl, ref Platform[] platformMap, bool[] krist, bool[] pirmasKartas, bool[] spyglNukrito, int buvusZaidejoTop, int index, ref Platform[] lavaMap, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43, int platId, int lavaId)
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
                    spygl[index].set(spygl[index].posLeft(), spygl[index].posTop() + 10);// buvo 20 bet per greitai krenta

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

                        if (Zaidejas.power < -40) Zaidejas.gyvybes -= 20;// Kritimo is aukstai zala
                        Zaidejas.power = 0;


                    }

                    //BOTTOM collision. 
                    if (buvoVirsui && spygl[index].posBottom() >= Zaidejas.playerTopPosition)
                    {
                        Zaidejas.gyvybes--;
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
        static void Lava(ref PlayerStruct Zaidejas, ref Platform[] spygl, ref Platform[] lavaMap, int zaidejoBuvusTop, int lavaId, ref Rectangle player)
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

                        if (Zaidejas.power < -40) Zaidejas.gyvybes -= 20;// Kritimo is aukstai zala
                        Zaidejas.power = 0;
                       
                        if (Zaidejas.gyvybiuSkaitiklis > 5)// Reguliuoja gyvybiu ateminejimo greiti
                        {
                            Zaidejas.gyvybes--;
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
       
    }
}
