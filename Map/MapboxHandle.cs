using System;
using System.IO;
using System.Net;
using System.Drawing;

namespace GameServer.Map
{
    public class MapboxHandle
    {
        private float maxHeight = 0;
        private float minHeight = 0;

        public const int zoom = 15;

        //For Translating Coords
        private double londiff;
        private double latdiff;
        private int tileX;
        private int tileY;

        private const int tileCountX = 1;

        public int GetTileCountX()
        {
            return tileCountX;
        }
        private const int tileCountY = 1;
        public int GetTileCountY()
        {
            return tileCountY;
        }

        private const int singleImageWidth = 256;
        private const int singleImageHeight = 256;
        private const int imageWidth = tileCountX * singleImageWidth;
        private const int imageHeight = tileCountY * singleImageHeight;
        private const int numberOfCellsPerTileX = 10;
        private const int numberOfCellsPerTileY = 10;

        public Image[,] landCoverMapImages = new Image[tileCountX, tileCountY];
        public Image[,] heightMapImages = new Image[tileCountX, tileCountY];

        public void fetchMap(double lat, double lon)
        {
            tileX = long2tilex(lon, zoom);
            tileY = lat2tiley(lat, zoom);

            //Fetch Maps
            FetchHeightMap2(tileX, tileY, zoom);
            FetchLandcoverMap2(tileX, tileY);

            //Construct Coords
            londiff = Math.Abs(tilex2long(tileX, zoom) - tilex2long(tileX + 1, zoom));
            latdiff = Math.Abs(tiley2lat(tileY, zoom) - tiley2lat(tileY + 1, zoom));
        }

        void FetchLandcoverMap2(int x, int y)
        {
            for (int i = -(tileCountX - 1) / 2; i < (tileCountX + 1) / 2; i++)
            {
                for (int j = -(tileCountY - 1) / 2; j < (tileCountY + 1) / 2; j++)
                {
                    string url = "https://api.mapbox.com/styles/v1" + "/huterguier/ckhklftc13x1k19o1hdnhnn5j" + "/tiles/256/" + zoom + "/" + (x + i) + "/" + (y - j) + "?access_token=pk.eyJ1IjoiaHV0ZXJndWllciIsImEiOiJja2g2Nm56cTEwOTV0MnhuemR1bHRianJtIn0.ViSkV78j-GHgC18pMnZfrQ";
                    WebRequest request = WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Console.WriteLine(response.StatusDescription);
                    Image image = Image.FromStream(response.GetResponseStream());
                    // string path = "C:\\Users\\User\\Desktop\\test.png";
                    // image.Save(path);
                    
                    this.landCoverMapImages[i, j] = image;
                }
            }
        }

        void FetchHeightMap2(int x, int y, int zoom)
        {
            for (int i = -(tileCountX - 1) / 2; i < (tileCountX + 1) / 2; i++)
            {
                for (int j = -(tileCountY - 1) / 2; j < (tileCountY + 1) / 2; j++)
                {
                    string url = "https://api.mapbox.com/v4/mapbox.terrain-rgb/" + zoom + "/" + (x + i) + "/" + (y - j) + ".png?access_token=pk.eyJ1IjoiaHVtYW5pemVyIiwiYSI6ImNraGdkc2t6YzBnYjYyeW1jOTJ0a3kxdGkifQ.SIpcsxeP7Xp2RTxDAEv3wA";
                    WebRequest request = WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Console.WriteLine(response.StatusDescription);
                    Image image = Image.FromStream(response.GetResponseStream());
                    // string path = "C:\\Users\\User\\Desktop\\test.png";
                    // image.Save(path);
                    this.heightMapImages[i, j] = image;
                }
            }
        }

        public static int long2tilex(double lon, int z)
        {
            return (int)(Math.Floor((lon + 180.0) / 360.0 * (1 << z)));
        }

        public static int lat2tiley(double lat, int z)
        {
            return (int)Math.Floor((1 - Math.Log(Math.Tan(ToRadians(lat)) + 1 / Math.Cos(ToRadians(lat))) / Math.PI) / 2 * (1 << z));
        }

        public static double tilex2long(int x, int z)
        {
            return (double)x / (double)(1 << z) * 360.0 - 180;
        }

        public static double tiley2lat(int y, int z)
        {
            double n = Math.PI - 2.0 * Math.PI * (double)y / (double)(1 << z);
            return 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)));
        }

        static double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        static uint[,] createMap(double lon, double lat, int tilesX, int tilesY)
        {
            Bitmap[,] landImages = FetchLandcoverMap(lon, lat, tilesX, tilesY);
            Bitmap[,] heightImages = FetchHeightMap(lon, lat, tilesX, tilesY);
            int cellCountX = tilesX * numberOfCellsPerTileX;
            int cellCountY = tilesY * numberOfCellsPerTileY;
            float hexWidth = HexMetrics.innerRadius + 2f * HexMetrics.innerRadius * (float)cellCountX;
            float hexHeight = 0.5f * HexMetrics.outerRadius + 1.5f * HexMetrics.outerRadius * (float)cellCountY;

            uint[,] map = new uint[cellCountX, cellCountY];

            for (int i = 0; i < cellCountX; i++)
            {
                for (int j = 0; j < cellCountY; j++)
                {
                    double x = (j + i * 0.5f - i / 2) * (HexMetrics.innerRadius * 2f);
                    double y = i * (HexMetrics.outerRadius * 1.5f);

                    int pixelX = (int)((x / hexWidth) * imageWidth);
                    int pixelZ = (int)((y / hexWidth) * imageHeight);

                    Color landColor = landImages[pixelX / 256, pixelZ / 256].GetPixel(pixelX % 256, pixelZ % 256);
                    Color heightColor = heightImages[pixelX / 256, pixelZ / 256].GetPixel(pixelX % 256, pixelZ % 256);

                    HexCellData tmp = new HexCellData();
                    map[i, j] = tmp.ToUInt();
                }
            }
            return map;
        }

        static Bitmap[,] FetchLandcoverMap(double lon, double lat, int tilesX, int tilesY)
        {
            Bitmap[,] images = new Bitmap[tilesX, tilesY];
            int x = long2tilex(lon, zoom);
            int y = lat2tiley(lat, zoom);
            for (int i = -(x - 1) / 2; i < (x + 1) / 2; i++)
            {
                for (int j = -(y - 1) / 2; j < (y + 1) / 2; j++)
                {
                    string url = "https://api.mapbox.com/styles/v1" + "/huterguier/ckhklftc13x1k19o1hdnhnn5j" + "/tiles/256/" + zoom + "/" + (x + i) + "/" + (y - j) + "?access_token=pk.eyJ1IjoiaHV0ZXJndWllciIsImEiOiJja2g2Nm56cTEwOTV0MnhuemR1bHRianJtIn0.ViSkV78j-GHgC18pMnZfrQ";
                    WebRequest request = WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Image image = Image.FromStream(response.GetResponseStream());
                    images[i, j] = new Bitmap(image);
                }
            }
            return images;
        }

        static Bitmap[,] FetchHeightMap(double lon, double lat, int tilesX, int tilesY)
        {
            Bitmap[,] images = new Bitmap[tilesX, tilesY];
            int x = long2tilex(lon, zoom);
            int y = lat2tiley(lat, zoom);
            for (int i = -(x - 1) / 2; i < (x + 1) / 2; i++)
            {
                for (int j = -(y - 1) / 2; j < (y + 1) / 2; j++)
                {
                    string url = "https://api.mapbox.com/v4/mapbox.terrain-rgb/" + zoom + "/" + (x + i) + "/" + (y - j) + ".png?access_token=pk.eyJ1IjoiaHVtYW5pemVyIiwiYSI6ImNraGdkc2t6YzBnYjYyeW1jOTJ0a3kxdGkifQ.SIpcsxeP7Xp2RTxDAEv3wA";
                    WebRequest request = WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Image image = Image.FromStream(response.GetResponseStream());
                    images[i, j] = new Bitmap(image);
                }
            }
            return images;
        }
    }
}