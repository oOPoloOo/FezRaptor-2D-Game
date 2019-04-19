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
       
        Rectangle player;
     
       

        //Player parameters
        int playerTopPosition; 
        int playerLeftPosition = 250;
        int playerHeight = 30;
        int playerWidth = 30;
        int jumpPower = 36;
        bool jumped = true;
        int power;
       
        


        //Platform parameters
        // If platform parameters changes, position of platform also changes
        int platHeight = 30;
        int platWidth = 30;
        
        //Collision parameters
        bool onSuface = false;
        bool stopLeft = false;// makes stop move left
        bool stopRight = false;// makes stop move right


        GT.Timer joystickTimer = new GT.Timer(30);
        GT.Timer jumpTimer = new GT.Timer(30); 

       
       

        //Map generation
        int basePositionTop = -88;// this offset makes the platform align with botom of the screen
        int basePositionLeft = 0;
        int platformId = 0;
        Platform[] platformMap = new Platform[200]; 

      
        
        void ProgramStarted()
        {
            SetupUI();

            Canvas.SetLeft(player, playerLeftPosition);
            Canvas.SetTop(player, playerTopPosition);

            joystickTimer.Tick += new GT.Timer.TickEventHandler(JoystickTimer_Tick);
            joystickTimer.Start();
            
            jumpTimer.Tick += new GT.Timer.TickEventHandler(jumpTimer_Tick);
            jumpTimer.Start();

        }

        void SetupUI()
        {
            // initialize window
            mainWindow = displayT43.WPFWindow; 

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
            map[6] =  ".....................................";
            map[7] =  ".....................................";
            map[8] =  ".....................................";
            map[9] =  ".....................................";
            map[10] = ".....................................";
            map[11] = ".....#..#............................";


            for (int i = 0; i < map.Length; i++)
            {
                for (int j = 0; j < map[i].Length; j++)
                {
                    if (map[i][j] == '#')
                    {
                        platformMap[platformId] = new Platform(platWidth, platHeight);
                        platformMap[platformId].set(basePositionLeft + j * platWidth , basePositionTop + i * platHeight /*-88*/);
                        platformId++;
                    }
                }
            }

            // Adding platforms to screen
            for (int i = 0; i < platformId; i++)
            {               
                layout.Children.Add(platformMap[i].get());
            }
                               
            mainWindow.Child = layout;
        }

      
        void JoystickTimer_Tick(GT.Timer timer)
        {
            double x = joystick.GetPosition().X; // joystic x scale [-1;1]
            if (x < -0.3 && playerLeftPosition > 0 && !stopLeft) 
            {
                playerLeftPosition -= 5;
            }
            else if (x > 0.7 && playerLeftPosition < displayT43.Width - playerWidth && !stopRight)
            {
                playerLeftPosition += 5;
            }

            Canvas.SetLeft(player, playerLeftPosition);
       }

        void jumpTimer_Tick(GT.Timer timer)
        {
            double y = joystick.GetPosition().Y;
            
            //Jump part
            if (!jumped) // if didn't jump
            {
                if (y >= 0.7) // and jumps
                {
                    jumped = true;
                    onSuface = false;
                    power = jumpPower;
                }
            }

            //Jump
            if (jumped)
            {
                playerTopPosition -= power;
                power -= 8;
            }
            Canvas.SetTop(player, playerTopPosition);
                             
            for (int i = 0; i < platformId; i++)
            {
                //TOP AND BOTTOM COLLISION
                //Check if player is in platform bounds x axis
                if ((playerLeftPosition + playerWidth) >= platformMap[i].posLeft() && playerLeftPosition <= platformMap[i].posLeft() + platWidth) 
                {
                    // (platHeight/2 -1) - even if player is in platform (from top) and doesn't touch the middle he is set on top 
                    if (playerTopPosition + playerHeight >= platformMap[i].posTop() && playerTopPosition + playerHeight <= platformMap[i].posTop() + (platHeight / 2 - 1))
                            
                    {
                        playerTopPosition = platformMap[i].posTop() - playerHeight;
                        jumped = false;
                        onSuface = true;
                                              
                    }

                    // (platHeight/2 + 1) - even if player is in platform (from bottom) and don't touch the middle he collides with bottom 
                    if (playerTopPosition <= platformMap[i].posTop() + platHeight && playerTopPosition >= platformMap[i].posTop() + (platHeight / 2 + 1))
                    {
                        playerTopPosition = platformMap[i].posTop() + platHeight;
                        power = -1;// without this player gets stuck on platform bottom
                    }
                }
                else jumped = true; // if player steps off platform - he falls



                if (playerTopPosition + playerHeight >= displayT43.Height) // the very bottom collision
                {
                    playerTopPosition = displayT43.Height - playerHeight;
                    jumped = false;
                    onSuface = true;
                }
              
                //SIDES COLLISION
                //Check if player is in platform bounds y axis
                if (playerTopPosition + playerHeight <= platformMap[i].posTop() + platHeight && playerTopPosition >= platformMap[i].posTop())
                {
                    if ((playerLeftPosition + playerWidth) + 1 >= platformMap[i].posLeft() && (playerLeftPosition + playerWidth) <= platformMap[i].posLeft() + (platWidth / 2 - 1))
                    {
                        playerLeftPosition = platformMap[i].posLeft() - playerWidth;
                        stopRight = true;
                    }
                    else stopRight = false;

                    if (playerLeftPosition - 1 <= platformMap[i].posLeft() + platWidth && playerLeftPosition >= platformMap[i].posLeft() + (platWidth / 2 + 1))
                    {
                        playerLeftPosition = platformMap[i].posLeft() + platWidth;
                        stopLeft = true;
                    }
                    else stopLeft = false;

                }
                else
                {
                    stopRight = false;
                    stopLeft = false;               
                }
              
                Canvas.SetTop(player, playerTopPosition);
            }

        }
          
    }
}
