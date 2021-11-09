using System;
using System.Collections.Generic;
using System.Linq;

namespace Konsola
{
    class Program
    {
        private const int XScale = 13, YScale = 12;
        //startowa pozycja x i y |oraz| deklaracja ilości punktów
        private static int _xPos = 1, _yPos = 1, _points;
        private const string Player = " O ";
        private static readonly string[] BlockCollision = {"[ ]", "___", "/", "|", " # "};
        private const string BlockPoint = " + ";
        private const string BlockWin = " # ";
        private static bool _win, _start;
        private static int _level;

        private static readonly Dictionary<char, int[]> Vector = new() {
            {'w', new[] {0, -1}},
            {'a', new[] {-1, 0}},
            {'s', new[] {0, 1}},
            {'d', new[] {1, 0}}
        };
        
        //CanMove -> up, left, down, right
        private static readonly bool[] CanMove;
        static Program() {
            CanMove = new[] {true,true,true,true};
        }
        
        static string[,] gamePole = {
            {"/","___","___","___","___","___","___","___","___","___","___", "/", "|_________________"},
            {"|","   ","   ","   ","   ","   ","   ","   ","   ","   ","   ", "|", "/________________/|"},
            {"|","   ","   ","   ","   ","   ","   ","   ","   ","   ","   ", "|", " w -> go up     | |"},
            {"|","   ","   ","   ","   ","   ","   ","   ","   ","   ","   ", "|", " a -> go left   | |"},
            {"|","   ","   ","   ","   ","   ","   ","   ","   ","   ","   ", "|", " s -> go down   | |"},
            {"|","   ","   ","   ","   ","   ","   ","   ","   ","   ","   ", "|", " d -> go right  | |"},
            {"|","   ","   ","   ","   ","   ","   ","   ","   ","   ","   ", "|", " e -> exit      | |"},
            {"|","   ","   ","   ","   ","   ","   ","   ","   ","   ","   ", "|", " q -> enable    | |"},
            {"|","   ","   ","   ","   ","   ","   ","   ","   ","   ","   ", "|", " developer mode | /"},
            {"|","   ","   ","   ","   ","   ","   ","   ","   ","   ","   ", "|", "________________|/"},
            {"|","   ","   ","   ","   ","   ","   ","   ","   ","   ","   ", "|", "/"},
            {"|","___","___","___","___","___","___","___","___","___","___", "/", ""}
        };

        private static void Main() {
//Check platform compatibility - weryfikacja zgodności platformy -> wyłącza błąd o wielkości bufora konsoli
#pragma warning disable CA1416 
            Console.SetBufferSize(128, 128);
            bool developerMode = true;
            
            while (true){
                StartScreen();
                GenerateMap(_xPos, _yPos);

                int pointsCount = CountPointOnMap(_xPos, _yPos);

                while (true) 
                {
                    var vectorInput = Console.ReadKey(true).KeyChar;
                    if (_start) { Console.Clear(); _start = false; }

                    if (vectorInput == 'e')
                        return;

                    Console.SetCursorPosition(0, 0); //Refreshowanie ekranu
                    Console.CursorVisible = false; //Ukrycie kursora
                    
                    switch (vectorInput)
                    {
                        case 'a' when CanMove[1]:
                            _xPos += Vector['a'][0];
                            break;
                        case 'd' when CanMove[3]:
                            _xPos += Vector['d'][0];
                            break;
                        case 'w' when CanMove[0]:
                            _yPos += Vector['w'][1];
                            break;
                        case 's' when CanMove[2]:
                            _yPos += Vector['s'][1];
                            break;
                        case 'q':
                            if (developerMode)
                                developerMode = false;
                            else
                                developerMode = true;
                            Console.Clear();
                            break;
                    }

                    Check_Pos(_xPos, _yPos);
                    
                    if (_points >= pointsCount) { 
                        BlockCollision[4] = "None";
                        if(gamePole[_yPos,_xPos] == BlockWin)
                            _win = true;
                    }

                    if (gamePole[_yPos, _xPos] == BlockPoint){ _points++; gamePole[_yPos,_xPos] = "   "; }

                    var collisionDetecionItem = Check_collision(_xPos,_yPos,gamePole);

                    Check_movement(collisionDetecionItem);
                    
                    if (developerMode) { 
                        Console.Clear();
                        Console.WriteLine($"x: {_xPos}, y:{_yPos}"); 
                        Console.WriteLine($"CDI: {collisionDetecionItem}");
                        Console.WriteLine($"BC: \"{BlockCollision[4]}\"");
                        string[] words = {"up", "left", "down", "right" };
                        for (var i = 0; i < words.Length; i++) { Console.WriteLine($"{words[i]}: {CanMove[i]} "); }
                        Console.WriteLine("Press 'q' to disable flashing screen.");
                    }
                    gamePole[_yPos, _xPos] = Player;
                    
                    for (var height = 0; height < YScale; height++) {
                        for (var width = 0; width < XScale; width++) {
                            Console.Write(gamePole[height, width]);
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine($"Points: {_points}/{pointsCount}");
                    Console.WriteLine($"Actual level: {_level}");
                    
                    if (gamePole[_yPos,_xPos] != BlockWin)
                        gamePole[_yPos,_xPos] = "   ";

                    if (!_win)
                        continue;
                    WinGame();
                    _level += 1;
                    break;
                }
                Console.Clear();
                _win = false;
                _points = 0;
                BlockCollision[4] = " # ";
                _xPos = 1;
                _yPos = 1;
            }
        }

        private static void Check_Pos(int xPosP, int yPosP) {
            if (xPosP >= XScale - 1)
                _xPos = XScale - 1;
            if (yPosP >= YScale - 1)
                _yPos = YScale - 1;
            if (xPosP < 0)
                _xPos = 0;
            if (yPosP < 0)
                _yPos = 0;
        }

        private static int Check_collision(int xPosP, int yPosP, string[,] gamePoleP)
        {
            // [/1]                 
            // [*] or [/2][*] or [*] or [*][/8]
            //                   [/4]             
            //CanMove -> up, left, down, right      
            var collisionDetecionItem = 0;
            if (yPosP > 0)
                if (BlockCollision.Contains(gamePoleP[yPosP - 1, xPosP]))
                {
                    collisionDetecionItem += 1;
                }
            if (xPosP > 0)
                if (BlockCollision.Contains(gamePoleP[yPosP, xPosP - 1]))
                {
                    collisionDetecionItem += 2;
                }
            if (yPosP < YScale - 1)
                if (BlockCollision.Contains(gamePoleP[yPosP + 1, xPosP]))
                {
                    collisionDetecionItem += 4;
                }
            if (xPosP < XScale - 1)
                if (BlockCollision.Contains(gamePoleP[yPosP,xPosP + 1]))
                {
                    collisionDetecionItem += 8;
                }
            
            return collisionDetecionItem;
        }

        static void Check_movement(int collisionDetecionItemPrivate)
        {
            const int countOfCollisions = 16;
            Dictionary<int,bool[]> collisionDetect = new Dictionary<int,bool[]>() {
                {0, new[] {true, true, true, true}},
                {1, new[] {false, true, true, true}},
                {2, new[] {true, false, true, true}},
                {3, new[] {false, false, true, true}},
                {4, new[] {true, true, false, true}},
                {5, new[] {false, true, false, true}},
                {6, new[] {true, false, false, true}},
                {7, new[] {false, false, false, true}},
                {8, new[] {true, true, true, false}},
                {9, new[] {false, true, true, false}},
                {10, new[] {true, false, true, false}},
                {11, new[] {false, false, true, false}},
                {12, new[] {true, true, false, false}},
                {13, new[] {false, true, false, false}},
                {14, new[] {true, false, false, false}},
                {15, new[] {false, false, false, false}},
            };
            for (int collision = 0; collision < countOfCollisions; collision++) {
                if (collisionDetecionItemPrivate == collision) {
                    for (int i = 0; i < 4; i++)
                        CanMove[i] = collisionDetect[collision][i];
                }
            }
        }

        private static void GenerateMap(int xPos, int yPos)
        {
            int rand;
            Random randomBlock = new Random();
            string[] blocks = {"   ","[ ]"," + "};
            int xEnd = XScale - 3, yEnd = YScale - 2;
            bool isBlockWin = false;
            for (int width = xPos; width < xEnd; width++) {
                for (int height = yPos; height < yEnd; height++)
                {
                    var collisionItem = Check_collision(width, height, gamePole);
                    if (width > xPos && height > yPos)
                        rand = randomBlock.Next(0,blocks.Length);
                    else
                        rand = 0;
                    if (collisionItem < 1 && rand == 2)
                        gamePole[height,width] = blocks[2];
                    else
                        gamePole[height,width] = blocks[rand];
                }
            }

            while (!isBlockWin)
            {
                var xRand = randomBlock.Next(0, xEnd);
                var yRand = randomBlock.Next(0, yEnd);
                for (int width = xPos; width < xEnd;)
                {
                    for (int height = yPos; height < yEnd; height++)
                    {
                        var collisionItem = Check_collision(width, height, gamePole);
                        if (gamePole[yRand, xRand] != BlockPoint && !BlockCollision.Contains(gamePole[yRand, xRand]))
                        {
                            if (collisionItem < 7)
                                gamePole[yRand, xRand] = BlockWin;
                            if (yRand > 2 && xRand > 2)
                                for (int x = 0; x < 4; x++)
                                for (int i = 0; i < 4; i++)
                                    if (gamePole[yRand - x, xRand - i] == "[ ]")
                                        gamePole[yRand - x, xRand - i] = "   ";
                            isBlockWin = true;
                            break;
                        }
                    }

                    break;
                }
            }
        }

        private static int CountPointOnMap(int xPos, int yPos)
        {
            int xEnd = XScale - 2, yEnd = YScale - 1;
            int count = 0;
            for (int width = xPos; width < xEnd; width++)
            {
                for (int height = yPos; height < yEnd; height++)
                {
                    if (gamePole[height,width] == BlockPoint)
                        count++;
                }
            }
            return count;
        }

        private static void StartScreen()
        {
            //Console.SetWindowSize(35, 4);
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("      Click any key to start       ");
            Console.WriteLine("-----------------------------------");
            _start = true;
        }
        
        private static void WinGame()
        {
            Console.WriteLine("-------------------------------");
            Console.WriteLine("           You win!            ");
            Console.WriteLine("-------------------------------");
        }
    }

    class Enemy
    {
        
    }
}
