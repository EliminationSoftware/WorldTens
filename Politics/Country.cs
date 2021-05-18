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

        public Country() {
            Random random = new Random();
            ident = random.Next(100000);
            blue = random.Next(100000);
            green = random.Next(100000);
        }

        public void CalculateRequirements(World world) {
            requireSoldier = (int)world.GetTension() * 2;
        }

        public void ExecuteRequirements(List<Creation> citizens) {
            if (citizens == null) {
                return;
            }
            foreach (Creation citizen in citizens) {
                if (citizen.politStatus == PoliticalStatus.Civilian && requireSoldier > 0) {
                    citizen.MakeSolider();
                    requireSoldier--;
                    Console.WriteLine("Soldier created");
                }
            }
        }
    }
}