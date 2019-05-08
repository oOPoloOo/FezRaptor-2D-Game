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

//Sudejau Player duomenis i struct
//Sudejau judejima X ir Y i metodus
//Pastebejau, kad netinkamai veikia kairiausio kubelio kol.





namespace VeIsNaujo_NuoSokinejantis_1
{
    public partial class Program
    {
        Window mainWindow;
        Canvas layout;
       
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
            public int playerLeftPosition;
            public int playerHeight;
            public int playerWidth;
            public int jumpPower;
            public bool jumped;
            public int power;

           public PlayerStruct(int playerTopPos, int playerLeftPos, int playerH, int playerW, int jumpP, bool jmp, int pow)
            {
                playerTopPosition = playerTopPos;
                playerLeftPosition = playerLeftPos;
                playerHeight = playerH;
                playerWidth = playerW;
                jumpPower = jumpP;
                jumped = jmp;
                power = pow;
            }
        }
        PlayerStruct playerStruct = new PlayerStruct(0,250,30,30,30,true,0);
        
       
        


        //Platform parameters
        ////If platform parameters changes, position of platform also changes
        int platHeight = 30;// Tu
        int platWidth = 30;// buvo 30
        
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
        Platform[] spygliaiMap = new Platform[4]; 
        int spygliaiId = 0;
        int spyglHeight = 10;
        int spyglWidth  = 10;
        
        void ProgramStarted()
        {
            SetupUI();

            Canvas.SetLeft(player, playerStruct.playerLeftPosition);
            Canvas.SetTop(player, playerStruct.playerTopPosition);

            //joystickTimer.Tick += new GT.Timer.TickEventHandler(JoystickTimer_Tick);
            //joystickTimer.Start();
            
            jumpTimer.Tick += new GT.Timer.TickEventHandler(jumpTimer_Tick);
            jumpTimer.Start();

          

        }

        void SetupUI()
        {
            // initialize window
            mainWindow = displayT43.WPFWindow;

            basePositionTop = (platHeight * (-12)) + displayT43.Height;

            // setup the layout
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

            // Platformer map
            string[] map = new string[12];
            map[0] =  ".....................................";
            map[1] =  ".....................................";
            map[2] =  ".....................................";
            map[3] =  ".....................................";
            map[4] =  ".....................................";
            map[5] =  ".....................................";
            map[6] =  "......S..............................";
            map[7] =  "..#..................................";
            map[8] =  "........#............................";
            map[9] =  ".....#........#......................";
            map[10] = "...........####......................";
            map[11] = ".#...#..#............................";


            for (int i = 0; i < map.Length; i++)
            {
                for (int j = 0; j < map[i].Length; j++)
                {
                    if (map[i][j] == '#')
                    {
                        platformMap[platformId] = new Platform(platWidth, platHeight);
                        platformMap[platformId].set(basePositionLeft + j * platWidth , basePositionTop + i * platHeight);
                        platformId++;
                    }
                 
                    if (map[i][j] == 'S')
                    {
                        spygliaiMap[spygliaiId] = new Platform(spyglWidth, spyglHeight);
                        spygliaiMap[spygliaiId].paintBlue();
                        // * platWith ir platHeigth, kad spyglius deliojant mape galima butu orentuotis pagal platformu poz
                        spygliaiMap[spygliaiId].set(basePositionLeft + j * platWidth, basePositionTop + i * platHeight);
                        spygliaiId++;
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

            mainWindow.Child = layout;
        }


        void jumpTimer_Tick(GT.Timer timer)
        {
             
           //-------------------------------------------------------------------------------------------------------
           // PLATFORMOS KOLIZIJA IR JUDEJIMAS
           //------------------------------------------------------------------------------------------------------
            JudejimasY(ref  buvusVirsaus, ref  playerStruct,  joystick);// Sutvarkyta

            Canvas.SetTop(player, playerStruct.playerTopPosition);
            
            VirsausKolizija(platformId,  platformMap,  buvusVirsaus, ref playerStruct, displayT43);//Pataisyta

            JudejimasX(ref buvusKaires,  stopLeft, stopRight, ref playerStruct, joystick,  displayT43);// Pataisyta

            Canvas.SetLeft(player, playerStruct.playerLeftPosition);// update player left posti

            SonuKolizija(platformId, platformMap, buvusKaires, ref playerStruct, ref stopRight, ref stopLeft, displayT43);// Sutvarkyta

            Canvas.SetTop(player, playerStruct.playerTopPosition); // update player top position

            //-------------------------------------------------------------------------------------------------------
            // SPYGLIO KOLIZIJA IR JUDEJIMAS
            //------------------------------------------------------------------------------------------------------
            //... Veliau

        }//jumpTimer end
      
        
        //-------------------------------------------------------------------------------------------------
        static void VirsausKolizija(int mapId, Platform[] map, int buvusVirsaus, ref PlayerStruct str, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43)
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
                        //  kolVirsus = true;
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
                else str.jumped = true; // if player steps off platform - he falls
               
            }

            // the very bottom collision
            if (str.playerTopPosition + str.playerHeight >= displayT43.Height)
            {
                str.playerTopPosition = displayT43.Height - str.playerHeight;
                str.jumped = false;
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

                if (str.playerTopPosition + map[i].Height() >= map[i].posTop() + 1
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

    }
}
