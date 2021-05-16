using System;
using Raylib_cs;
using WorldTens.Map;

namespace WorldTens
{
    class Program
    {
        public static int screenWidth = 800;
        public static int screenHeight = 480;
        public static int iterations = 0;
        public static int iterTmp = 0;
        public static int iterMax = 50;
        static void Main(string[] args)
        {
            Raylib.InitWindow(screenWidth, screenHeight, "WorldTens");
            World world = new World();
            world.LoadMap("resources/default.bmp");

            Random random = new Random();
            for (int i = 0; i < 200; i++) {
                Creation creation = new Creation(new Vector2(265, 140), 10);
                world.detectors[0].creations.Add(creation);
                
            }
            
            for (int i = 0; i < 250; i++) {
                Creation creation = new Creation(new Vector2(250, 150), 10);
                creation.politStatus = PoliticalStatus.Builder;
                world.detectors[0].creations.Add(creation);
            }
            
            while (!Raylib.WindowShouldClose()) {
                world.DecreaseTens(Raylib.GetFrameTime());

                Raylib.BeginDrawing();

                if (iterTmp >= iterMax) {
                    Raylib.ClearBackground(Color.WHITE);
                    for (int i = 0; i < world.map.Count; i++) {
                        for (int j = 0; j < world.map[i].Count; j++) {
                            world.DrawMapPixel(new Vector2(i, j));
                        }
                    }
                    Raylib.DrawText("Good luck in WorldTens!", 10, 10, 14, Color.BLACK);
                    Raylib.DrawText(iterations.ToString(), 0, screenHeight - 20, 20, Color.BLACK);
                    Raylib.DrawText(world.GetTension().ToString(), screenWidth - 100, 10, 20, Color.BLACK);
                    iterTmp = 0;
                }

                for (int i = 0; i < world.detectors.Count; i++) {
                    for (int j = 0; j < world.detectors[i].creations.Count; j++) {
                        Creation creation = world.detectors[i].creations[j];
                        Vector2 prevPos = new Vector2(creation.position.x, creation.position.y);
                        creation.DoAction(Raylib.GetFrameTime(), world);
                        world.DrawMapPixel(prevPos);
                        
                        world.DrawMapPixel(creation.position);

                        if (creation.alive) {
                            Raylib.DrawPixel(creation.position.x, creation.position.y, Color.RED);
                        }
                        else {
                            Raylib.DrawPixel(creation.position.x, creation.position.y, Color.BROWN);
                            world.detectors[i].creations.Remove(creation);
                        }
                    }
                }
                iterations++;
                iterTmp++;
                if(Raylib.IsKeyDown(KeyboardKey.KEY_F))
                {
                    Raylib.DrawText("FPS: " + Raylib.GetFPS().ToString(), 350, 0, 20, Color.BLACK);
                }
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
