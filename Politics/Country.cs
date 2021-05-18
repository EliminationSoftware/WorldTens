using System.Collections.Generic;
using WorldTens.Map;
using System;

namespace WorldTens.Politics
{
    public class Country
    {
        private int requireSoldier = 0;
        public int ident = 0;
        public int blue = 0;
        public int green = 0;
        public Dictionary<Country, byte> warLevel = new Dictionary<Country, byte>();
        public float createdTension = 0;

        public Country() {
            Random random = new Random();
            ident = random.Next(100000);
            blue = random.Next(100000);
            green = random.Next(100000);
        }

        public void CalculateRequirements(World world) {
            requireSoldier = (int)world.GetTension();
            foreach (byte level in warLevel.Values) {
                requireSoldier += level * 4;
            }
        }

        public void ExecuteRequirements(List<Creation> citizens) {
            if (citizens == null) {
                return;
            }
            Console.WriteLine("Reqruiting {0} soldiers...", requireSoldier);
            foreach (Creation citizen in citizens) {
                if (citizen.politStatus == PoliticalStatus.Civilian && requireSoldier > 0) {
                    citizen.MakeSolider();
                    requireSoldier--;
                }
            }
        }

        public void CalculateWars(World world) {
            foreach (Country country in world.countries) {
                if (country != this && world.GetTension() / country.createdTension < 2.5 && world.GetTension() > 15 && !warLevel.ContainsKey(country)) {
                    warLevel.Add(country, 1);
                    if (!country.warLevel.ContainsKey(this)) {
                        country.warLevel.Add(this, 10);
                    }
                    else {
                        country.warLevel[this] += 3;
                    }
                    Console.WriteLine("War declared from {0} to {1}", ident, country.ident);
                }
            }
        }
    }
}