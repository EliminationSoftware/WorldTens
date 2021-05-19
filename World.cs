using System.Drawing;
using System.Collections.Generic;
using WorldTens.Map;
using WorldTens.Politics;
using System;
using Raylib_cs;

namespace WorldTens
{
    public class World
    {
        private float tension = 0.0f;
        private float tensionFallTime = 1.0f;
        private float tensionFallAmount = 0.01f;
        private float speed = 1f;
        private float timePased = 0;
        public float yearTime = 10.0f;
        private Bitmap bmp;
        public List<List<MapPixel>> map = new List<List<MapPixel>>();
        public List<MapDetectorSquare> detectors = new List<MapDetectorSquare>();
        public List<Country> countries = new List<Country>();

        public World(string path) {
            LoadMap(path);
        }

        public void IncreaseTens(float amount, Country country) {
            tension += amount;
            if (country != null) {
                country.createdTension += amount;
            }
            if (tension > 120) {
                tension = 120;
            }
        }

        public void DecreaseTens(float delta) {
            tension -= tensionFallAmount * (delta * tensionFallTime) * speed;
            if (tension < 0) {
                tension = 0;
            }
        }

        public float GetTension() {
            return tension;
        }

        public int GetMapWidth() {
            return bmp == null ? 0 : bmp.Width;
        }

        public int GetMapHeight() {
            return bmp == null ? 0 : bmp.Height;
        }

        public void IncreaseTime() {
            speed *= 2;
        }

        public void DecreaseTime() {
            speed /= 2;
        }

        public void LoadMap(string path) {
            bmp = new Bitmap(path);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);

            MapColor ground = new MapColor(0, 255, 0);
            MapColor water = new MapColor(255, 255, 255);

            MapPixel waterPixel = new MapPixel();
            waterPixel.water = true;
            waterPixel.grass = false;
            for (int i = 0; i < bmp.Width; i++) {
                map.Add(new List<MapPixel>());
                for (int j = 0; j < bmp.Height; j++) {
                    System.Drawing.Color c = bmp.GetPixel(i, j);

                    

                    if (water.R == c.R && water.G == c.G && water.B == c.B) {
                        map[i].Add(waterPixel);
                    }
                    else  {
                       map[i].Add(new MapPixel());
                    }
                }
            }
            for (int i = 0; i < bmp.Width; i += 20) {
                for (int j = 0; j < bmp.Height; j += 20) {
                    detectors.Add(new MapDetectorSquare(new Vector2(i, j)));
                }
            }
        }

        public void DrawMapPixel(Vector2 pos) {
            MapPixel pixel = map[pos.x][pos.y];
            if (pixel.grass) {
                Raylib.DrawPixel(pos.x, pos.y, Raylib_cs.Color.GREEN);
            }
            else if (pixel.water) {
                Raylib.DrawPixel(pos.x, pos.y, Raylib_cs.Color.BLUE);
            }
            if (pixel.city) {
                Raylib.DrawPixel(pos.x, pos.y, Raylib_cs.Color.BLACK);
            }
            else if (pixel.road) {
                Raylib.DrawPixel(pos.x, pos.y, Raylib_cs.Color.GRAY);
            }
            pixel = null;
        }

        public void CreateCountry(List<Creation> citizens, int detectorIndex) {
            Country country = new Country();
            countries.Add(country);
            detectors[detectorIndex].country = country;
            foreach (Creation citizen in citizens) {
                citizen.country = country;
            }
            Console.WriteLine("New country created");
        }

        public void AddTime() {
            timePased += Raylib.GetFrameTime();
        }

        public float GetTime() {
            return timePased;
        }
    }
}