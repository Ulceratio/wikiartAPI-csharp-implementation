using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Threading;



namespace YourNameSpace
{

    class wikiArt
    {
        public class Artist
        {
            public string contentId { get; set; }
            public string artistName { get; set; }
            public string url { get; set; }
            public string lastNameFirst { get; set; }
            public string birthDay { get; set; }
            public string deathDay { get; set; }
            public string birthDayAsString { get; set; }
            public string deathDayAsString { get; set; }
            public string image { get; set; }
            public string wikipediaUrl { get; set; }
            public IList<string> dictonaries { get; set; }
        }

        public class Painting
        {       
            public string title { get; set; }
            public string contentId { get; set; }
            public string artistContentId { get; set; }
            public string artistName { get; set; }
            public string completitionYear { get; set; }
            public string yearAsString { get; set; }
            public string width { get; set; }
            public string image { get; set; }
            public string height { get; set; }
            public string artistUrl { get; set; }
            public string url { get; set; }
            public IList<string> dictonaries { get; set; }
            public string location { get; set; }
            public string period { get; set; }
            public string serie { get; set; }
            public string genre { get; set; }
            public string material { get; set; }
            public string technique { get; set; }
            public string sizeX { get; set; }
            public string sizeY { get; set; }
            public string diameter { get; set; }
            public string auction { get; set; }
            public string yearOfTrade { get; set; }
            public string lastPrice { get; set; }
            public string galleryName { get; set; }
            public string tags { get; set; }
            public string description { get; set; }
        }

        private string URL = "https://www.wikiart.org/en/App";

        HttpClient client;

        private int intraRequestCounter { get; set; }
        private int RequestCounter {
            get { return intraRequestCounter; }
            set
            {
                if (intraRequestCounter == 0 && value!=0)
                {
                    intraRequestCounter = value;
                    if(!timer.Enabled)
                    {
                        timer.Start();
                    }
                }
            }
        }
        public int requestCounter
        {
            get { return RequestCounter; }
        }

        private System.Timers.Timer timer;

        public wikiArt()
        {
            client = new HttpClient();
            RequestCounter = 0;
            timer = new System.Timers.Timer(5000);
            timer.Elapsed += (sender, e) =>
             {
                 if (RequestCounter != 0)
                 {
                     RequestCounter = 0;
                 }
             };
        }

        public async Task<List<Painting>> getArtistPaintings(Artist artist)
        {
            if(RequestCounter < 10)
            {
                try
                {
                    string responseFromServer = "";
                    using (Stream stream = await client.GetStreamAsync(URL + "/Painting/PaintingsByArtist?artistUrl=" + artist.url.Replace(" ", "") + "&json=2"))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                        {
                            responseFromServer = reader.ReadToEnd();
                        }
                    }
                    return JsonConvert.DeserializeObject<List<Painting>>(responseFromServer);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    RequestCounter++;
                }
            }
            else
            {
                throw new Exception("errorCode:1 |The limitation of requests was reached|");
            }
        }

        public async Task<List<Artist>> getAllArtists()
        {
            if (RequestCounter < 10)
            {
                try
                {
                    string responseFromServer = "";
                    using (Stream stream = await client.GetStreamAsync(URL + "/Artist/AlphabetJson"))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                        {
                            responseFromServer = reader.ReadToEnd();
                        }
                    }
                    return JsonConvert.DeserializeObject<List<Artist>>(responseFromServer);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    RequestCounter++;
                }
            }
            else
            {
                throw new Exception("errorCode:1 |The limitation of requests was reached|");
            }
        }

        public async Task<byte[]> getPaintingImage(Painting painting)
        {
            if(RequestCounter < 10)
            {
                try
                {
                    return await client.GetByteArrayAsync(painting.image.Split('!')[0]);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    RequestCounter++;
                }
            }
            else
            {
                throw new Exception("errorCode:1 |The limitation of requests was reached|");
            }
            
        }

        public async Task WriteImageIntoFile(byte[] image,string path)
        {
            await File.WriteAllBytesAsync(path, image);
        }

        public void WriteArtistsIntoFile(List<Artist> Artists,string path)
        { 
            foreach (var artist in Artists)
            {
                File.AppendAllText(path, JsonConvert.SerializeObject(artist));
            }
        }

        public void WritePaintingsIntoFile(List<Painting> Paintings, string path)
        {
            foreach (var painting in Paintings)
            {
                File.AppendAllText(path, JsonConvert.SerializeObject(painting));
            }
        }

        public async Task<List<Artist>> ReadArtistsFromFile(string path)
        {
            return JsonConvert.DeserializeObject<List<Artist>>(await File.ReadAllTextAsync(path));
        }

        public async Task<List<Painting>> ReadPaintingsFromFile(string path)
        {
            return JsonConvert.DeserializeObject<List<Painting>>(await File.ReadAllTextAsync(path));
        }

        public async Task<string> CreateWikiArtDataDump()
        {
            var allArtists = await getAllArtists();
            WriteArtistsIntoFile(allArtists, Windows.Storage.ApplicationData.Current.LocalFolder.Path+"\\artists.json");
            for (int i = 0; i < allArtists.Count;)
            {
                if (RequestCounter < 10)
                {
                    try
                    {
                        string pathToDir = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + allArtists[i].artistName.Trim();
                        if (!Directory.Exists(pathToDir))
                        {
                            DirectoryInfo directoryInfo = Directory.CreateDirectory(pathToDir);
                        }
                        var artistPaintings = await getArtistPaintings(allArtists[i]);
                        string pathToFile = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + allArtists[i].artistName.Trim() + "\\" + allArtists[i].artistName.Trim() + ".json";
                        if(!File.Exists(pathToFile))
                        {
                            WritePaintingsIntoFile(artistPaintings, pathToFile);
                        }
                        i++;
                    }
                    catch(Exception ex)
                    {
                        i++;
                    }
                }
            }
            return "done";
        }

        public async Task saveAllArtistPaintingsImages(Artist artist)
        {
            var artistPaintings = await getArtistPaintings(artist); // var artistPaintings = await ReadPaintingsFromFile(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\"+artist.artistName+"\\.json"); var artistPaintings = await ReadPaintingsFromFile(Environment.CurrentDirectory + "\\"+artist.artistName+"\\.json");
            string path = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + artist.artistName.Trim(); //Environment.CurrentDirectory + "\\" + artist.artistName.Trim();
            if (!Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
            }
            for (int i = 0; i < artistPaintings.Count;)
            {
                if (RequestCounter < 10)
                {
                    try
                    {
                        string pathToFile = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + artist.artistName.Trim() + "\\" + artistPaintings[i].title.Trim() + ".jpg";
                        if(!File.Exists(pathToFile))
                        {
                            await WriteImageIntoFile(await getPaintingImage(artistPaintings[i]), Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + artist.artistName.Trim() + "\\" + artistPaintings[i].title.Trim() + ".jpg");
                        }
                        i++;
                    }
                    catch (Exception ex)
                    {
                        i++;
                        throw ex;
                    }
                }                
            }
        }
    }
}
