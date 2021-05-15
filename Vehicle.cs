namespace WorldTens
{
    public enum VehicleType {
        Water,
        Ground,
        Air,
        None,
    }
    public class Vehicle
    {
        public float speed = 5;
        VehicleType type;

        public Vehicle(VehicleType vtype) {
            vtype = type;
        }

        public Vehicle(VehicleType vtype, float vspeed) {
            speed = vspeed;
            vtype = type;
        }
    }
}