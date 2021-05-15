using System;
using System.Collections.Generic;
using Raylib_cs;
using WorldTens.Map;

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
        Army,
        Civilian,
        Politic,
        Scientist,
    }
    public class Creation
    {
        public Vector2 position = new Vector2(5, 5);
        public float mind = 1.0f;
        public Vehicle vehicle = new Vehicle(VehicleType.None, 1);
        private AIStatus status = AIStatus.Move;
        private bool alive = false;
        private float progress = 0.0f;
        private float speedBuid = 15.0f;

        public Creation() {

        }

        public Creation(Vector2 pos, float mind) {
            position = pos;
            this.mind = mind;
        }

        public void DoAction(float delta, World world) {
            // Checks here
            if (world.map[position.x][position.y].water) {
                status = AIStatus.Move;
            }

            List<Creation> creations = searchCreations(world);
            if (creations.Count > 0) {
                status = AIStatus.Follow;
            }

            if (world.map[position.x][position.y].grass && !world.map[position.x][position.y].city) {
                status = AIStatus.Build;
            }
            else {
                status = AIStatus.Move;
            }
            
            Random random = new Random();
            //Actions here
            switch (status) {
                case AIStatus.Move:
                    position.x += random.Next(2);
                    position.y += random.Next(2);
                    break;
                case AIStatus.Follow:
                    if (position.x < creations[0].position.x) {
                        position.x -= 1;
                    }
                    else {
                        position.x += 1;
                    }
                    if (position.y < creations[0].position.y) {
                        position.x -= 1;
                    }
                    else {
                        position.y += 1;
                    }
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
                    }
                    break;
            }
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
                    foreach (Creation creation in world.detectors[i].creations) {
                        creations.Add(creation);
                    }
                }
            }
            return creations;
        }
    }
}