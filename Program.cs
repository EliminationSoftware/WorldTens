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
        public static System.Numerics.Vector2 mouseSelPos;
        public static System.Numerics.Vector2 mouseReleasePos;
        private static int yearCalc = 0;

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
                        //if (world.map[creation.position.x][creation.position.y].city) {
                            citizensCounter++;
                            citizens.Add(creation);
                        //}
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
                    if (citizensCounter > 2) {
                        if (world.countries.Count == 0) {
                            world.CreateCountry(citizens, i);
                        }
                        int enemyCounter = 0;
                        int citizenCounter = 0;
                        foreach (Creation citizen in citizens) {
                            if (citizen.country == null && world.detectors[i].country == null) {
                                world.CreateCountry(citizens, i);
                            }
                            if (world.detectors[i].country == citizen.country) {
                                citizenCounter++;
                            }
                            else {
                                enemyCounter++;
                            }
                        }
                        if (enemyCounter > citizenCounter) {
                            Country dominator = null;
                            foreach (Creation citizen in citizens) {
                                if (citizen.country != world.detectors[i].country) {
                                    world.detectors[i].country = citizen.country;
                                    dominator = citizen.country;
                                    break;
                                }
                            }
                            foreach (Creation citizen in citizens) {
                                if (citizen.country != world.detectors[i].country) {
                                    citizen.country = world.detectors[i].country;
                                }
                            }
                            if (dominator == null) {
                                world.CreateCountry(citizens, i);
                            }
                            Console.WriteLine("territory captured");
                        }
                    }
                    if (world.year != yearCalc) {
                        foreach (Country country in world.countries) {
                            if (world.detectors[i].country == country) {
                                country.CalculateWars(world);
                                country.CalculateRequirements(world);
                                country.ExecuteRequirements(citizens);
                            }
                        }
                    }
                    citizens = null;
                }

                if (world.year != yearCalc) {
                    yearCalc = world.year;
                }
                iterations++;
                iterTmp++;
                world.AddTime();

                if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_MULTIPLY)) {
                    world.IncreaseTens(100, world.countries[0]);
                }

                if (Raylib.IsKeyDown(KeyboardKey.KEY_J)) {
                    Console.WriteLine(world.GetTension());
                }

                if (Raylib.IsKeyDown(KeyboardKey.KEY_K)) {
                    foreach (MapDetectorSquare detector in world.detectors) {
                        if (detector.country != null) {
                            Random random = new Random(detector.country.ident);
                            int red = random.Next(255);
                            int blue = new Random(detector.country.blue).Next(255);
                            int green = new Random(detector.country.green).Next(255);
                            Raylib.DrawRectangle(detector.position.x, detector.position.y, detector.wh.x, detector.wh.y, new Color(red, blue, green, 100));
                        }
                    }
                }

                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON)) {
                    mouseSelPos = Raylib.GetMousePosition();
                }

                if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON)) {
                    mouseReleasePos = Raylib.GetMousePosition();

                    Rectangle rect = new Rectangle(mouseSelPos.X, mouseReleasePos.Y, Math.Abs(mouseReleasePos.X - mouseSelPos.X), Math.Abs(mouseReleasePos.Y - mouseSelPos.Y));
                    foreach (MapDetectorSquare detector in world.detectors) {
                        foreach (Creation creation in detector.creations) {
                            if (Raylib.CheckCollisionPointRec(new System.Numerics.Vector2(creation.position.x, creation.position.y), rect)) {
                                if (creation.country != null) {
                                    Console.WriteLine("Country: " + creation.country.ident);
                                }
                                else {
                                    Console.WriteLine("Country: null");
                                }
                                Console.WriteLine("PolitStatus: " + creation.politStatus);
                                Vector2 dest = creation.GetDesination();
                                if (dest != null) { 
                                    Console.WriteLine("Destination: {0}, {1}", dest.x, dest.y);
                                }
                                else {
                                    Console.WriteLine("Destination: None");
                                }
                            }
                        }
                    }
                }

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
