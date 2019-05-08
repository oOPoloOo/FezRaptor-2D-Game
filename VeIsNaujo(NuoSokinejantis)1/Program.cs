﻿using System;
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

//Sudejau V6 kolizijos virsaus ir apacios kol kodus i metodus
//Reikia mazint metodu param sk
//SEnas kodas paliktas uzkomentuotas
//Padariau, kad nupiesu sygli





namespace VeIsNaujo_NuoSokinejantis_1
{
    public partial class Program
    {
        Window mainWindow;
        Canvas layout;
       
        Rectangle player;
     
       

        //Player parameters
        int playerTopPosition; 
        int playerLeftPosition = 250;
        int playerHeight = 30;//30 buvo
        int playerWidth = 30;//30 buvo
        int jumpPower = 30;
        bool jumped = true;
        int power;
       
        


        //Platform parameters
        //If platform parameters changes, position of platform also changes
        int platHeight = 30;// buvo 30
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
        bool buvoKairej = false;
        bool buvoDesinej = false;
        bool buvoVirsui = false;
        bool buvoApacioj = false;
        
        //Spygliai
        Platform[] spygliaiMap = new Platform[4]; 
        int spygliaiId = 0;
        int spyglHeight = 10;
        int spyglWidth  = 10;
        
        void ProgramStarted()
        {
            SetupUI();

            Canvas.SetLeft(player, playerLeftPosition);
            Canvas.SetTop(player, playerTopPosition);

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
            player = new Rectangle(playerWidth, playerHeight);
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

        //-------------------------------------------------------------------------------------------------
        static void VirsausKolizija(int mapId, Platform[] map, int buvusVirsaus, int playerLeftPosition, ref int playerTopPosition, int playerWidth, int playerHeight, int platWidth, int platHeight, ref bool jumped, ref int power, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43)
        {

            bool buvoVirsui = false;
            bool buvoApacioj = false;

            // Platform check loop                            
            for (int i = 0; i < mapId; i++)
            {

                buvoVirsui = false;
                buvoApacioj = false;


                // if (buvusVirsaus < platformMap[i].Top) // zmogeliukas gali but ir prasmeges
                if (buvusVirsaus < (int)map[i].posTop())
                {
                    buvoVirsui = true;

                    //buvoDesinej = false;
                    //buvoKairej = false;
                    //buvoApacioj = false;
                }

               // else if (buvusVirsaus > platformMap[i].Top)
                else if (buvusVirsaus > (int)map[i].posTop())
                {
                    buvoApacioj = true;

                    //buvoVirsui = false;
                    //buvoDesinej = false;
                    //buvoKairej = false;
                }
                //------------------------------------------------------------------------------------------------------------------


                //--------------------------------------------------------------------------------------------------
                if ((playerLeftPosition + playerWidth) > map[i].posLeft()
                              && playerLeftPosition < map[i].posLeft() + platWidth)
                {
                    //TOP collision. (platHeight/2 -1) - even if player is in platform 
                    // (from top) and doesn't touch the middle he is set on top 

                    if (buvoVirsui && playerTopPosition + playerHeight >= map[i].posTop()
                        && playerTopPosition + playerHeight <= map[i].posTop() + platHeight + System.Math.Abs(power))
                    {
                        playerTopPosition = map[i].posTop() - playerHeight;
                        jumped = false;
                        //  kolVirsus = true;
                        break;

                    }

                    //BOTTOM collision. (platHeight/2 + 1) - even if player is in platform 
                    //(from bottom) and don't touch the middle he collides with bottom 

                    if (buvoApacioj && playerTopPosition <= map[i].posTop() + platHeight
                        && playerTopPosition >= map[i].posTop() - System.Math.Abs(power))
                    {
                        playerTopPosition = map[i].posTop() + platHeight;
                        power = -1;// without this player gets stuck on platform bottom
                        // kolAplacia = true;
                        break;
                    }
                }
                else jumped = true; // if player steps off platform - he falls
                //}
                //--------------------------------------------------------------------------------------------------
            }

            // the very bottom collision
            if (playerTopPosition + playerHeight >= displayT43.Height)
            {
                playerTopPosition = displayT43.Height - playerHeight;
                jumped = false;
            }

            //--------------------------------------------------------------------------------------------------------- 

        }
        //-------------------------------------------------------------------------------------------------
    
        //-------------------------------------------------------------------------------------------------
        static void sonuKolizija(int mapId, Platform[] map, int buvusKaires, ref int playerLeftPosition, int playerTopPosition, int playerWidth, int playerHeight, int platWidth, int platHeight, ref bool stopRight, ref bool stopLeft, Gadgeteer.Modules.GHIElectronics.DisplayT43 displayT43)
        {
            bool buvoKairej = false;
            bool buvoDesinej = false;

            for (int i = 0; i < mapId; i++)
            {
                buvoKairej = false;
                buvoDesinej = false;

                //-------------------------------------------------------------------------------------------------
                //Buvusi pozicija
                if (buvusKaires < map[i].posLeft() && buvusKaires + playerWidth < map[i].posLeft()
           && ((playerTopPosition < map[i].posTop() || playerTopPosition + playerHeight < playerTopPosition) || (map[i].posTop() + platHeight > playerTopPosition
           || playerTopPosition + playerHeight < map[i].posTop() + platHeight)))
                {
                    buvoKairej = true;


                }
                else if (buvusKaires > map[i].posLeft() + platWidth && buvusKaires + playerWidth > map[i].posLeft() + platWidth
                   && ((playerTopPosition < map[i].posTop() || playerTopPosition + playerHeight < playerTopPosition) || (map[i].posTop() + platHeight > playerTopPosition
                   || playerTopPosition + playerHeight < map[i].posTop() + platHeight)))
                {
                    buvoDesinej = true;


                }


                //---------------------------------------------------------------------------------------------------------------------
                if (playerTopPosition + playerHeight >= map[i].posTop() + 1
                                      && playerTopPosition <= map[i].posTop() + platHeight - 1)
                {
                    //RIGHT collision. (platWidth / 2 - 1) - even if player is in platform 
                    // (from left) and don't touch the middle he collides with left

                    if (buvoKairej && (playerLeftPosition + playerWidth) + 1 >= map[i].posLeft()
                        && (playerLeftPosition + playerWidth) <= map[i].posLeft() + platWidth)
                    {
                        playerLeftPosition = map[i].posLeft() - playerWidth;
                        stopRight = true;// stops moving right
                        //   kolDesine = true;
                        break;
                    }
                    else stopRight = false;

                    //LEFT collision. (platWidth / 2 - 1) - even if player is in platform 
                    // (from right) and don't touch the middle he collides with right

                    if (buvoDesinej && playerLeftPosition - 1 <= map[i].posLeft() + platWidth
                        && playerLeftPosition >= map[i].posLeft())
                    {
                        playerLeftPosition = map[i].posLeft() + platWidth;
                        stopLeft = true;// stops moving left
                        //  kolKaire = true;
                        break;
                    }
                    else stopLeft = false;

                }
                else
                {
                    stopRight = false;
                    stopLeft = false;
                }

                //----------------------------------------------------------------------------------------------------------------------


            }

        }
        //-------------------------------------------------------------------------------------------------
        void jumpTimer_Tick(GT.Timer timer)
        {
            double y = joystick.GetPosition().Y;

            //Jump part
            if (!jumped) // if didn't jump
            {
                if (y >= 0.7/*e.KeyCode == Keys.Space*/) // and jumps
                {
                    jumped = true;
                    power = jumpPower;
                }
            }

            //Jump
            if (jumped)
            {
             //Zaidejas.Top  -= power;
             //buvusVirsaus = Zaidejas.Top;
                
                buvusVirsaus = playerTopPosition;
                playerTopPosition -= power;
                power -= 3;
            }

         
            Canvas.SetTop(player, playerTopPosition);
            
 //        // Platform check loop                            
            //for (int i = 0; i < platformId; i++)
            //{

            //    buvoVirsui = false;
            //    buvoApacioj = false;


            //    // if (buvusVirsaus < platformMap[i].Top) // zmogeliukas gali but ir prasmeges
            //    if (buvusVirsaus < (int)platformMap[i].posTop())
            //    {
            //        buvoVirsui = true;

            //        //buvoDesinej = false;
            //        //buvoKairej = false;
            //        //buvoApacioj = false;
            //    }

            //   // else if (buvusVirsaus > platformMap[i].Top)
            //    else if (buvusVirsaus > (int)platformMap[i].posTop())
            //    {
            //        buvoApacioj = true;

            //        //buvoVirsui = false;
            //        //buvoDesinej = false;
            //        //buvoKairej = false;
            //    }
            //    //------------------------------------------------------------------------------------------------------------------


            //    //--------------------------------------------------------------------------------------------------
            //    if ((playerLeftPosition + playerWidth) > platformMap[i].posLeft()
            //                  && playerLeftPosition < platformMap[i].posLeft() + platWidth)
            //    {
            //        //TOP collision. (platHeight/2 -1) - even if player is in platform 
            //        // (from top) and doesn't touch the middle he is set on top 

            //        if (buvoVirsui && playerTopPosition + playerHeight >= platformMap[i].posTop()
            //            && playerTopPosition + playerHeight <= platformMap[i].posTop() + platHeight + System.Math.Abs(power))
            //        {
            //            playerTopPosition = platformMap[i].posTop() - playerHeight;
            //            jumped = false;
            //            //  kolVirsus = true;
            //            break;

            //        }

            //        //BOTTOM collision. (platHeight/2 + 1) - even if player is in platform 
            //        //(from bottom) and don't touch the middle he collides with bottom 

            //        if (buvoApacioj && playerTopPosition <= platformMap[i].posTop() + platHeight
            //            && playerTopPosition >= platformMap[i].posTop() - System.Math.Abs(power))
            //        {
            //            playerTopPosition = platformMap[i].posTop() + platHeight;
            //            power = -1;// without this player gets stuck on platform bottom
            //            // kolAplacia = true;
            //            break;
            //        }
            //    }
            //    else jumped = true; // if player steps off platform - he falls
            //    //}
            //    //--------------------------------------------------------------------------------------------------
            //}

            //// the very bottom collision
            //if (playerTopPosition + playerHeight >= displayT43.Height)
            //{
            //    playerTopPosition = displayT43.Height - playerHeight;
            //    jumped = false;
            //}

            ////--------------------------------------------------------------------------------------------------------- 
    
           VirsausKolizija(platformId, platformMap,  buvusVirsaus, playerLeftPosition, ref playerTopPosition,  playerWidth,  playerHeight,  platWidth,  platHeight, ref  jumped, ref  power,  displayT43);
            
            //JUDEJIMAS I KAIRE/DESINE
           double x = joystick.GetPosition().X; // joystic x scale [-1;1]
            // move left
            if (x < -0.3 && playerLeftPosition > 0 && !stopLeft)
            {
                buvusKaires = playerLeftPosition;
                playerLeftPosition -= 5;
            }
            // move right
            else if (x > 0.7 && playerLeftPosition < displayT43.Width - playerWidth 
                                                                     && !stopRight)
            {
                buvusKaires = playerLeftPosition;
                playerLeftPosition += 5;
            }

            Canvas.SetLeft(player, playerLeftPosition);// update player left posti
     //---------------------------------------------------------------------------------------------------------
      
     // for (int i = 0; i < platformId; i++)
     // {
     //     buvoKairej = false;
     //     buvoDesinej = false;

     //     //-------------------------------------------------------------------------------------------------
     //     //Buvusi pozicija
     //     if (buvusKaires < platformMap[i].posLeft() && buvusKaires + playerWidth < platformMap[i].posLeft()
     //&& ((playerTopPosition < platformMap[i].posTop() || playerTopPosition + playerHeight < playerTopPosition) || (platformMap[i].posTop() + platHeight > playerTopPosition
     //|| playerTopPosition + playerHeight < platformMap[i].posTop() + platHeight)))
     //     {
     //         buvoKairej = true;

             
     //     }
     //     else if (buvusKaires > platformMap[i].posLeft() + platWidth && buvusKaires + playerWidth > platformMap[i].posLeft() + platWidth
     //        && ((playerTopPosition < platformMap[i].posTop() || playerTopPosition + playerHeight < playerTopPosition) || (platformMap[i].posTop() + platHeight > playerTopPosition
     //        || playerTopPosition + playerHeight < platformMap[i].posTop() + platHeight)))
     //     {
     //         buvoDesinej = true;

              
     //     }
                
         
     // //---------------------------------------------------------------------------------------------------------------------
     //  if (playerTopPosition + playerHeight >= platformMap[i].posTop() + 1
     //                        && playerTopPosition <= platformMap[i].posTop() + platHeight - 1)
     //           {
     //               //RIGHT collision. (platWidth / 2 - 1) - even if player is in platform 
     //               // (from left) and don't touch the middle he collides with left

     //               if (buvoKairej && (playerLeftPosition + playerWidth) + 1 >= platformMap[i].posLeft()
     //                   && (playerLeftPosition + playerWidth) <= platformMap[i].posLeft() + platWidth)
     //               {
     //                   playerLeftPosition = platformMap[i].posLeft() - playerWidth;
     //                   stopRight = true;// stops moving right
     //                   //   kolDesine = true;
     //                   break;
     //               }
     //               else stopRight = false;

     //               //LEFT collision. (platWidth / 2 - 1) - even if player is in platform 
     //               // (from right) and don't touch the middle he collides with right

     //               if (buvoDesinej && playerLeftPosition - 1 <= platformMap[i].posLeft() + platWidth
     //                   && playerLeftPosition >= platformMap[i].posLeft())
     //               {
     //                   playerLeftPosition = platformMap[i].posLeft() + platWidth;
     //                   stopLeft = true;// stops moving left
     //                   //  kolKaire = true;
     //                   break;
     //               }
     //               else stopLeft = false;

     //           }
     //           else
     //           {
     //               stopRight = false;
     //               stopLeft = false;
     //           }
     //           Canvas.SetTop(player, playerTopPosition); // update player top position
     //////----------------------------------------------------------------------------------------------------------------------
      
      
     // }


            sonuKolizija(platformId, platformMap,  buvusKaires, ref playerLeftPosition, playerTopPosition, playerWidth, playerHeight, platWidth,  platHeight, ref stopRight, ref  stopLeft,  displayT43);

            Canvas.SetTop(player, playerTopPosition); // update player top position
        }

       

    }
}
