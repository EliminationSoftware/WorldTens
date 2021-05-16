using System;

using WorldTens;

namespace WorldTens
{
    class CreationSet
    {
        public int posX = 0;
        public int posY = 0;
        public int mind = 0;
        public PoliticalStatus politStatus;
        public uint count;

        public CreationSet() {}

        public CreationSet(int posX, int posY, int mind, string politStatus, uint count)
        {
            this.posX  = posX;
            this.posY  = posY;
            this.mind  = mind;
            this.count = count;

            switch (politStatus) {
                case "soldier":
                    this.politStatus = PoliticalStatus.Soldier;
                break;
                case "civilian":
                    this.politStatus = PoliticalStatus.Civilian;
                break;
                case "politic":
                    this.politStatus = PoliticalStatus.Politic;
                break;
                case "scientist":
                    this.politStatus = PoliticalStatus.Scientist;
                break;
                case "builder":
                    this.politStatus = PoliticalStatus.Builder;
                break;
                default:
                break;
            }
            // TODO: move the setting of the status to the Creation class
        }
    }
}