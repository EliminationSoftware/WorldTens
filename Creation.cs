using System;
using System.Collections.Generic;
using Raylib_cs;
using WorldTens.Map;
using WorldTens.Politics;

namespace WorldTens
{
    public enum AIStatus {
        Attack,
        Move,
        Hide,
        Defend,
        Rage, // Use this when world tension is >= 100
        Eat,
        Build,
        Panic,
        Research,
        Follow,
    }

    public enum PoliticalStatus {
        Soldier,
        Civilian,
        Politic,
        Scientist,
        Builder,
    }
    public class Creation
    {
        public Country country = null;
        public Vector2 position = new Vector2(5, 5);
        public float mind = 1.0f;
        public Vehicle vehicle = new Vehicle(VehicleType.None, 100);
        private AIStatus status = AIStatus.Move;
        public PoliticalStatus politStatus = PoliticalStatus.Civilian;
        public bool alive = true;
        private float progress = 0.0f;
        private float moveProgress = 0.0f;
        private float speedBuid = 15.0f;
        private float full = 100;
        private float eatSpeed = 30;
        private int searchRadius = 20;
        private float hungerSpeed = 5.0f;

        public Creation() {

        }

        public Creation(Vector2 pos, float mind) {
            position = pos;
            this.mind = mind;
        }

        public void DoAction(float delta, World world) {
            if (!alive) {
                return;
            }
            // Checks here
            if (world.map[position.x][position.y].water) {
                status = AIStatus.Move;
            }

            List<Creation> creations = searchCreations(world);
            if (creations.Count > 0) {
                status = AIStatus.Follow;
            }

            if (world.map[position.x][position.y].grass && !world.map[position.x][position.y].city && politStatus == PoliticalStatus.Builder) {
                status = AIStatus.Build;
            }
            else {
                status = AIStatus.Move;
            }

            if (full <= 20 && world.map[position.x][position.y].city) {
                status = AIStatus.Eat;
            }

            if (full <= 0) {
                if (politStatus == PoliticalStatus.Soldier) {
                    Console.WriteLine("Soldier dies from starvation");
                }
                alive = false;
                return;
            }
            
            Random random = new Random();
            int followIndex = random.Next(creations.Count);
            //Actions here
            switch (status) {
                case AIStatus.Move:
                    if (politStatus == PoliticalStatus.Civilian) {
                        MoveToCity(world);
                    }
                    else if (politStatus == PoliticalStatus.Builder) {
                        moveOnProgress(new Vector2(random.Next(-1, 2), random.Next(-1, 2)));
                    }
                    else if (politStatus == PoliticalStatus.Soldier) {
                        full = 100;
                        Vector2 dir = SearchEnemy(world);
                        if (dir != null) {
                            moveOnProgress(GetDirection(dir));
                        }

                        foreach (Creation creation in creations) {
                            if (creation.country != country) {
                                if (!country.warLevel.TryGetValue(creation.country, out byte level)) {
                                    continue;
                                }
                                progress += (float)random.NextDouble() * mind;
                                if (progress > 100) {
                                    creation.alive = false;
                                    progress = 0;
                                    world.IncreaseTens(0.001f, country);
                                    if (!country.warLevel.ContainsKey(creation.country)) {
                                        country.warLevel.Add(creation.country, 1);
                                    }
                                    else {
                                        if (country.warLevel[creation.country] < byte.MaxValue) {
                                            country.warLevel[creation.country]++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case AIStatus.Follow:
                    Vector2 direction = GetDirection(creations[followIndex].position);
                    position.x += direction.x;
                    position.y += direction.y;
                    break;
                case AIStatus.Build:
                    progress += speedBuid * delta;
                    if (progress >= 100) {
                        if (world.map[position.x][position.y].road) {
                            world.map[position.x][position.y].city = true;
                        }
                        else {
                            world.map[position.x][position.y].road = true;
                        }
                        progress = 0;
                        world.IncreaseTens(0.01f, country);
                    }
                    break;
                case AIStatus.Eat:
                    if (world.map[position.x][position.y].city) {
                        full += eatSpeed * delta;
                    }
                    else {
                        MoveToCity(world);
                    }
                    break;
            }
        }

        public void MakeSolider() {
            politStatus = PoliticalStatus.Soldier;
            searchRadius = 30;
            hungerSpeed = 0;
            full = 100;
        }

        private void moveOnProgress(Vector2 direction) {
            moveProgress += vehicle.speed * Raylib.GetFrameTime();
            full -= hungerSpeed * Raylib.GetFrameTime();
            if (moveProgress >= 100) {
                position.x += direction.x;
                position.y += direction.y;
                moveProgress -= 100;
            }
        }

        private void MoveToCity(World world) {
            Random random = new Random();
            Vector2 cityPos = SearchCity(world);
            Vector2 directionCity;
            if (cityPos != null) {
                directionCity = GetDirection(cityPos);
            }
            else {
                directionCity = new Vector2(random.Next(-1, 2), random.Next(-1, 2));
            }
            moveOnProgress(directionCity);
        }

        private Vector2 GetDirection(Vector2 targetPos) {
            Vector2 dir = new Vector2(0, 0);
            if (position.x < targetPos.x) {
                dir.x += 1;
            }
            else {
                dir.x -= 1;
            }
            if (position.y < targetPos.y) {
                dir.y += 1;
            }
            else {
                dir.y -= 1;
            }
            return dir;
        }

        public List<Creation> searchCreations(World world) {
            List<Creation> creations = new List<Creation>();
            /*for (int i = position.x - 5; i < position.x + 5; i++) {
                for (int j = position.y - 5; j < position.y + 5; j++) {
                    for (int a = 0; a < world.map[i][j].creations.Count; a++) {
                        creations.Add(world.map[i][j].creations[a]);
                    }
                }
            }*/
            for (int i = 0; i < world.detectors.Count; i++) {
                Rectangle detector = new Rectangle(world.detectors[i].position.x, world.detectors[i].position.y, world.detectors[i].wh.x, world.detectors[i].wh.y);
                if (Raylib.CheckCollisionPointRec(new System.Numerics.Vector2(position.x, position.y), detector)) {
                    if (!world.detectors[i].creations.Contains(this)) {
                        world.detectors[i].creations.Add(this);
                    }
                    foreach (Creation creation in world.detectors[i].creations) {
                        if (creation != this) {
                            creations.Add(creation);
                        }
                    }
                }
                else if (world.detectors[i].creations.Contains(this)) {
                    world.detectors[i].creations.Remove(this);
                }
            }
            return creations;
        }

        public Vector2 SearchCity(World world) {
            for (int i = position.x - searchRadius; i < position.x + searchRadius; i++) {
                for (int j = position.y - searchRadius; j < position.y + searchRadius; j++) {
                    if (world.map[i][j].city) {
                        return new Vector2(i, j);
                    }
                }
            }
            return null;
        }

        public Vector2 SearchEnemy(World world) {
            foreach (MapDetectorSquare detector in world.detectors) {
                foreach (Creation creation in detector.creations) { 
                    if (creation.country != country) {
                        if (country.warLevel.TryGetValue(creation.country, out byte level)) {
                            if (level > 0) {
                                return creation.position;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public List<MapPixel> GetPixelsInRadius(World world) {
            List<MapPixel> pixels = new List<MapPixel>();
            for (int i = position.x - searchRadius; i < position.x + searchRadius; i++) {
                for (int j = position.y - searchRadius; j < position.y + searchRadius; j++) {
                    pixels.Add(world.map[i][j]);
                }
            }
            return pixels;
        }
    }
}