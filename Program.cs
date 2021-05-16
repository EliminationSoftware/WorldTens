using System;
using Raylib_cs;
using WorldTens.Map;

namespace WorldTens
{
    class Program
    {
        public static int screenWidth = 800;
        public static int screenHeight = 480;
        static void Main(string[] args)
        {
            Raylib.InitWindow(screenWidth, screenHeight, "WorldTens");
            World world = new World();
            world.LoadMap("resources/default.bmp");

            Random random = new Random();
            for (int i = 0; i < 100; i++) {
                Creation creation = new Creation(new Vector2(235, 140), 10);
                world.detectors[0].creations.Add(creation);
                
            }
            
            for (int i = 0; i < 50; i++) {
                Creation creation = new Creation(new Vector2(250, 150), 10);
                creation.politStatus = PoliticalStatus.Builder;
                world.detectors[0].creations.Add(creation);
            }
           
            Raylib.SetTargetFPS(60);
            
            while (!Raylib.WindowShouldClose()) {
                world.DecreaseTens(Raylib.GetFrameTime());

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.WHITE);

                for (int i = 0; i < world.map.Count; i++) {
                    for (int j = 0; j < world.map[i].Count; j++) {
                        MapPixel pixel = world.map[i][j];
                        if (pixel.grass) {
                            Raylib.DrawPixel(i, j, Color.GREEN);
                        }
                        else if (pixel.water) {
                            Raylib.DrawPixel(i, j, Color.BLUE);
                        }
                        if (pixel.city) {
                            Raylib.DrawPixel(i, j, Color.BLACK);
                        }
                        else if (pixel.road) {
                            Raylib.DrawPixel(i, j, Color.GRAY);
                        }
                        pixel = null;
                    }
                }

                for (int i = 0; i < world.detectors.Count; i++) {
                    for (int j = 0; j < world.detectors[i].creations.Count; j++) {
                        Creation creation = world.detectors[i].creations[j];
                        creation.DoAction(Raylib.GetFrameTime(), world);

                        if (creation.alive) {
                            Raylib.DrawPixel(creation.position.x, creation.position.y, Color.RED);
                        }
                        else {
                            Raylib.DrawPixel(creation.position.x, creation.position.y, Color.BROWN);
                            world.detectors[i].creations.Remove(creation);
                        }
                    }
                }
                Raylib.DrawText("Good luck in WorldTens!", 10, 10, 14, Color.BLACK);
                Raylib.DrawText(world.GetTension().ToString(), screenWidth - 100, 10, 20, Color.BLACK);
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
