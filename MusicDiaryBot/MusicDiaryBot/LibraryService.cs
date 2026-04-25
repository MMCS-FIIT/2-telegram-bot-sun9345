using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDiaryBot
{
    public class TrackEntry
    {
        public string Artistname { get; set; } = "";
        public string Songtitle { get; set; } = "";

        public override string ToString()
        {
            return $"{Artistname},{Songtitle}";
        }

        public string ToStringForWrite()
        {
            return $"{Artistname} - {Songtitle}";
        }

        public TrackEntry(string artistname, string songtitle)
        {
            Artistname = artistname;
            Songtitle = songtitle;
        }
    }
    public class LibraryService
    {
        public LibraryService()
        {
            Directory.CreateDirectory("data"); // папка для .csv с библиотеками пользователей 
        }

        private string GetFilePath(long chatId) => Path.Combine("data", $"{chatId}.csv"); // путь к .csv юзера

        public void AddTrack(long chatId, TrackEntry track)
        {
            string filePath = GetFilePath(chatId);

            bool isDuplicate = GetTracks(chatId).Any(t =>   // проверка на дубликаты 
                string.Equals(t.Artistname, track.Artistname, StringComparison.OrdinalIgnoreCase) && 
                string.Equals(t.Songtitle, track.Songtitle, StringComparison.OrdinalIgnoreCase));

            if (isDuplicate) return;

            if (!File.Exists(filePath))
                File.WriteAllText(filePath, "Artist,Title\n");

            string line = $"\"{track.Artistname}\",\"{track.Songtitle}\"\n"; // запись в .csv с кавычками чтобы не сломалось если запятые в назвнияъ
            File.AppendAllText(filePath, line);
        }

        public bool DeleteTrack(long chatId, int index)
        {
            var tracks = GetTracks(chatId);

            if (index < 1 || index > tracks.Count) return false;

            tracks.RemoveAt(index - 1); // удаляем из списка

            string filePath = GetFilePath(chatId); // перезаписываем полностью весь .csv 
            File.WriteAllText(filePath, "Artist,Title\n");
            foreach (var track in tracks)
                File.AppendAllText(filePath, $"\"{track.Artistname}\",\"{track.Songtitle}\"\n");
            return true;
        }

        public List<TrackEntry> GetTracks(long chatId)
        {
            string filePath = GetFilePath(chatId);
            var tracks = new List<TrackEntry>();

            if (!File.Exists(filePath))
                return tracks;
            else
            {
                string[] lines = File.ReadAllLines(filePath); 
                foreach (string line in lines.Skip(1))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length < 2)
                        continue;

                    tracks.Add(new TrackEntry(parts[0].Trim('"'), parts[1].Trim('"'))); // убираем " " которые защищали от запятых
                }

                return tracks;
            }
        }
    }
}
