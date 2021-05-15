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
                world.detectors[0].creations.Add(new Creation(new Vector2(100, 15), 10));
            }

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
                    foreach (Creation creation in world.detectors[i].creations) {
                        creation.DoAction(Raylib.GetFrameTime(), world);
                        Raylib.DrawPixel(creation.position.x, creation.position.y, Color.RED);
                    }
                }
                Raylib.DrawText("Good luck in WorldTens!", 10, 10, 14, Color.BLACK);
                Raylib.DrawText(world.GetTension().ToString(), screenWidth - 100, 10, 20, Color.BLACK);
                
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
