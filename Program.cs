using System;
using Raylib_cs;
using System.IO;
using System.Collections.Generic;
using WorldTens.Map;
using WorldTens;
using WorldTens.Politics;

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
                    int citizensCounter = 0;
                    List<Creation> citizens = new List<Creation>();
                    for (int j = 0; j < world.detectors[i].creations.Count; j++) {
                        Creation creation = world.detectors[i].creations[j];
                        if (world.map[creation.position.x][creation.position.y].city) {
                            citizensCounter++;
                            citizens.Add(creation);
                        }
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
                    if (citizensCounter > 30) {
                        if (world.countries.Count == 0) {
                            world.CreateCountry(citizens, i);
                        }
                        int enemyCounter = 0;
                        int citizenCounter = 0;
                        foreach (Creation citizen in citizens) {
                            if (citizen.country == null) {
                                foreach (Country country in world.countries) {
                                    if (world.detectors[i].country == country) {
                                        citizen.country = country;
                                    }
                                }
                            }
                            else {
                                if (world.detectors[i].country == citizen.country) {
                                    citizenCounter++;
                                }
                                else {
                                    enemyCounter++;
                                }
                            }
                        }
                        if (enemyCounter > citizenCounter) {
                            Country dominator = null;
                            foreach (Creation citizen in citizens) {
                                if (citizen.country != world.detectors[i].country) {
                                    world.detectors[i].country = citizen.country;
                                    dominator = citizen.country;
                                }
                            }
                            if (dominator == null) {
                                world.CreateCountry(citizens, i);
                            }
                            Console.WriteLine("territory captured");
                        }
                    }
                    else {
                        citizens = null;
                    }
                    if (world.GetTime() % 10 < 0.3) {
                        foreach (Country country in world.countries) {
                            if (world.detectors[i].country == country) {
                                country.CalculateRequirements(world);
                                country.ExecuteRequirements(citizens);
                            }
                        }
                    }
                }
                iterations++;
                iterTmp++;
                world.AddTime();
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
