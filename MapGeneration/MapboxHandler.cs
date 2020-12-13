using System;
using System.IO;
using System.Net;
using System.Drawing;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.GameState;
using Shared.MapGeneration;

namespace Shared.MapGeneration
{
    public class MapboxHandler
    {
        public const int ZOOM = 15;        

        public static Bitmap[,] FetchLandcoverMap(double lon, double lat, int tilesX, int tilesY)
        {
            Bitmap[,] images = new Bitmap[tilesX, tilesY];
            int x = Slippy.long2tilex(lon, ZOOM);
            int y = Slippy.lat2tiley(lat, ZOOM);
            Console.WriteLine(lon + ", " + lat);
            for (int i = -(tilesX - 1) / 2; i < (tilesX + 1) / 2; i++)
            {
                for (int j = -(tilesY - 1) / 2; j < (tilesY + 1) / 2; j++)
                {
                    string url = "https://api.mapbox.com/styles/v1" + "/huterguier/ckhklftc13x1k19o1hdnhnn5j" + "/tiles/256/" + ZOOM + "/" + (x + i) + "/" + (y - j) + "?access_token=pk.eyJ1IjoiaHV0ZXJndWllciIsImEiOiJja2g2Nm56cTEwOTV0MnhuemR1bHRianJtIn0.ViSkV78j-GHgC18pMnZfrQ";
                    WebRequest request = WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Image image = Image.FromStream(response.GetResponseStream());
                    images[i + (tilesX - 1) / 2, j + (tilesY- 1) / 2] = new Bitmap(image);
                    Console.WriteLine(url);
                }
            }
            return images;
        }

        public static Bitmap[,] FetchHeightMap(double lon, double lat, int tilesX, int tilesY)
        {
            Bitmap[,] images = new Bitmap[tilesX, tilesY];
            int x = Slippy.long2tilex(lon, ZOOM);
            int y = Slippy.lat2tiley(lat, ZOOM);
            for (int i = -(tilesX - 1) / 2; i < (tilesX + 1) / 2; i++)
            {
                for (int j = -(tilesY - 1) / 2; j < (tilesY + 1) / 2; j++)
                {
                    string url = "https://api.mapbox.com/v4/mapbox.terrain-rgb/" + ZOOM + "/" + (x + i) + "/" + (y - j) + ".png?access_token=pk.eyJ1IjoiaHVtYW5pemVyIiwiYSI6ImNraGdkc2t6YzBnYjYyeW1jOTJ0a3kxdGkifQ.SIpcsxeP7Xp2RTxDAEv3wA";
                    WebRequest request = WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Image image = Image.FromStream(response.GetResponseStream());
                    images[i + (tilesX - 1) / 2, j + (tilesY - 1) / 2] = new Bitmap(image);
                    
                }
            }
            return images;
        }
    }
}