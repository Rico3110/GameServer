namespace GameServer.Map
{
    public class HexCell
    {
        Biome biome;
        ushort elevation;
        Ressource ressource;
        public HexCell()
        {
            this.biome = Biome.GRASS;
            this.elevation = 0;
            this.ressource = Ressource.NONE;
        }

        public HexCell(Biome biome, ushort elevation, Ressource ressource)
        {
            this.biome = biome;
            this.elevation = elevation;
            this.ressource = ressource;
        }

        public string ToString()
        {
            return this.biome.ToString() + ", " + this.elevation.ToString() + ", " + this.ressource.ToString();
        }
    }
}