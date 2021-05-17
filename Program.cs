using System;
using Raylib_cs;
using System.IO;

using WorldTens.Map;
using WorldTens;

namespace WorldTens
{
    class Program
    {
        public static int screenWidth;
        public static int screenHeight;
        public static uint iterations = 0;
        public static uint iterTmp = 0;
        public static uint iterMax = 200;

        static void Main(string[] args)
        {
            string modelName = args.Length > 0 ? args[0] : "default";

            string modelPath = "resources/models/" + modelName;
            if (!File.Exists(modelPath + "/map.bmp") || !File.Exists(modelPath + "/spawn.yml")) {
                Console.WriteLine("Wrong model title!");
                return;
            }
            World world = new World(modelPath + "/map.bmp");
            screenWidth = world.GetMapWidth();
            screenHeight = world.GetMapHeight();
            Raylib.InitWindow(screenWidth, screenHeight, "WorldTens");

            string spawnYaml = System.IO.File.ReadAllText("resources/models/" + modelName + "/spawn.yml");
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            CreationSet[] creationSets = deserializer.Deserialize<CreationSet[]>(spawnYaml);

            for (int i = 0; i < creationSets.Length; i++) {
                for (int j = 0; j < creationSets[i].count; j++) {
                    Creation creation = new Creation(new Vector2(
                        creationSets[i].posX, creationSets[i].posY
                    ), creationSets[i].mind);

                    creation.politStatus = creationSets[i].politStatus;
                    world.detectors[0].creations.Add(creation);
                }
            }

            while (!Raylib.WindowShouldClose()) {
                world.DecreaseTens(Raylib.GetFrameTime());

                Raylib.BeginDrawing();

                if (iterTmp >= iterMax || iterations == 0) {
                    Raylib.ClearBackground(Color.WHITE);
                    for (int i = 0; i < world.map.Count; i++) {
                        for (int j = 0; j < world.map[i].Count; j++) {
                            world.DrawMapPixel(new Vector2(i, j));
                        }
                    }
                    Raylib.DrawText("Good luck in WorldTens!", 10, 10, 14, Color.BLACK);
                    Raylib.DrawText(iterations.ToString(), 0, screenHeight - 20, 20, Color.BLACK);
                    Raylib.DrawText(world.GetTension().ToString(), screenWidth - 100, 10, 20, Color.BLACK);
                    Raylib.DrawText("FPS: " + Raylib.GetFPS().ToString(), 350, 0, 20, Color.BLACK);
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
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
