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
        Reproduce,
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
        public float capturingProgress = 0;
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
        private Vector2 destination = null;
        private MapDetectorSquare detectorLocation = null;

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

            CheckDetector(world);
            // Checks here
            if (world.map[position.x][position.y].status == PixelStatus.Water) {
                status = AIStatus.Move;
            }

            //List<Creation> creations = searchCreations(world);
            /*if (detectorLocation.creations.Count > 0) {
                status = AIStatus.Follow;
            }*/

            if (politStatus == PoliticalStatus.Civilian && detectorLocation.creations.Count > 1) {
                status = AIStatus.Reproduce;
            }

            if (world.map[position.x][position.y].status == PixelStatus.Grass && world.map[position.x][position.y].status != PixelStatus.City && politStatus == PoliticalStatus.Builder) {
                status = AIStatus.Build;
            }
            else if (politStatus == PoliticalStatus.Civilian && world.map[position.x][position.y].status != PixelStatus.City){
                status = AIStatus.Move;
            }
            else if (politStatus == PoliticalStatus.Builder || politStatus == PoliticalStatus.Soldier) {
                status = AIStatus.Move;
            }

            if (full <= 20 && world.map[position.x][position.y].status == PixelStatus.City) {
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
            //int followIndex = random.Next(creations.Count);
            //Actions here
            switch (status) {
                case AIStatus.Move:
                    if (politStatus == PoliticalStatus.Civilian) {
                        MoveToCity(world);
                    }
                    else if (politStatus == PoliticalStatus.Builder) {
                        /*if (destination == null) {
                            destination = SearchGrass(world);
                        }
                        if (destination != null) {
                            moveOnProgress(GetDirection(destination));
                        }*/
                        //else {
                            moveOnProgress(new Vector2(random.Next(-1, 2), random.Next(-1, 2)));
                        //}
                    }
                    else if (politStatus == PoliticalStatus.Soldier) {
                        full = 100;
                        if (destination == null) {
                            destination = SearchEnemy(world);   
                        }
                        if (destination != null) {
                            moveOnProgress(GetDirection(destination));
                        }

                        foreach (Creation creation in detectorLocation.creations) {
                            if (creation.country != country && creation.country != null && country != null){
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
                /*case AIStatus.Follow:
                    Vector2 direction = GetDirection(creations[followIndex].position);
                    position.x += direction.x;
                    position.y += direction.y;
                    break;*/
                case AIStatus.Build:
                    progress += speedBuid * delta;
                    if (progress >= 100) {
                        if (world.map[position.x][position.y].status == PixelStatus.Road) {
                            world.map[position.x][position.y].status = PixelStatus.City;
                        }
                        else {
                            world.map[position.x][position.y].status = PixelStatus.Road;
                        }
                        progress = 0;
                        world.IncreaseTens(0.01f, country);
                    }
                    break;
                case AIStatus.Eat:
                    if (world.map[position.x][position.y].status == PixelStatus.City) {
                        full += eatSpeed * delta;
                    }
                    else {
                        MoveToCity(world);
                    }
                    break;
                case AIStatus.Reproduce:
                    progress += 1.0f * delta;
                    if (progress >= 100 && detectorLocation != null) {
                        Creation child = new Creation(position, mind + 2);
                        child.country = country;
                        detectorLocation.creations.Add(child);
                        Console.WriteLine("Detector population is now {0}", detectorLocation.creations.Count);
                        progress -= 100;
                    }
                    break;
            }
        }

        public void MakeSolider() {
            politStatus = PoliticalStatus.Soldier;
            searchRadius = 30;
            hungerSpeed = 0;
            full = 100;
            destination = null;
        }

        private void moveOnProgress(Vector2 direction) {
            moveProgress += vehicle.speed * Raylib.GetFrameTime();
            full -= hungerSpeed * Raylib.GetFrameTime();
            if (moveProgress >= 100) {
                position.x += direction.x;
                position.y += direction.y;
                moveProgress -= 100;
            }
            if (position == destination) {
                destination = null;
            }
        }

        private void MoveToCity(World world) {
            Random random = new Random();
            if (destination == null) {
                destination = SearchCity(world);
            }
            Vector2 directionCity;
            if (destination != null) {
                directionCity = GetDirection(destination);
            }
            else {
                directionCity = new Vector2(random.Next(-1, 2), random.Next(-1, 2));
            }
            moveOnProgress(directionCity);
        }

        private Vector2 GetDirection(Vector2 targetPos) {
            Random random = new Random();
            float tan = ((position.x - targetPos.x) / (position.y - targetPos.y));

            if (random.NextDouble() < tan)
                return position.x < targetPos.x ? new Vector2(1, 0) : new Vector2(-1, 0);
            else
                return position.y < targetPos.y ? new Vector2(0, 1) : new Vector2(0, -1);
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
                    detectorLocation = world.detectors[i];
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

        private void CheckDetector(World world) {
            if (detectorLocation != null) {
                Rectangle detector = new Rectangle(detectorLocation.position.x, detectorLocation.position.y, detectorLocation.wh.x, detectorLocation.wh.y);
                if (Raylib.CheckCollisionPointRec(new System.Numerics.Vector2(position.x, position.y), detector)) {
                    return; 
                }
            }
            for (int i = 0; i < world.detectors.Count; i++) {
                Rectangle detector2 = new Rectangle(world.detectors[i].position.x, world.detectors[i].position.y, world.detectors[i].wh.x, world.detectors[i].wh.y);
                if (Raylib.CheckCollisionPointRec(new System.Numerics.Vector2(position.x, position.y), detector2)) {
                    if (!world.detectors[i].creations.Contains(this)) {
                        world.detectors[i].creations.Add(this);
                    }
                    detectorLocation = world.detectors[i];
                }
                else if (world.detectors[i].creations.Contains(this)) {
                    world.detectors[i].creations.Remove(this);
                }
            }
        }

        public Vector2 SearchCity(World world) {
            for (int i = position.x - searchRadius; i < position.x + searchRadius; i++) {
                for (int j = position.y - searchRadius; j < position.y + searchRadius; j++) {
                    if (world.map[i][j].status == PixelStatus.City) {
                        return new Vector2(i, j);
                    }
                }
            }
            return null;
        }

        private Vector2 SearchGrass(World world) { // Fix this
            Random random = new Random();
            if (random.Next(2) == 1) {
                for (int i = position.x - searchRadius + random.Next(searchRadius); i < position.x + searchRadius; i++) {
                    for (int j = position.y - searchRadius + random.Next(searchRadius); j < position.y + searchRadius; j++) {
                        if (world.map[i][j].status == PixelStatus.Grass && new Random().Next(40) == 1) {
                            return new Vector2(i, j);
                        }
                    }
                }
            }
            else {
                for (int i = position.x + searchRadius * 2; i > position.x; i--) {
                    for (int j = position.y - searchRadius + random.Next(searchRadius); j < position.y + searchRadius; j++) {
                        if (world.map[i][j].status == PixelStatus.Grass && new Random().Next(40) == 1) {
                            return new Vector2(i, j);
                        }
                    }
                }
            }
            return null;
        }

        public Vector2 SearchEnemy(World world) {
            foreach (MapDetectorSquare detector in world.detectors) {
                foreach (Creation creation in detector.creations) { 
                    if (creation.country != country && creation.country != null && country != null) {
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

        public Vector2 GetDesination() {
            return destination;
        }
    }
}