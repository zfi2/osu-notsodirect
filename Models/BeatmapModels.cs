using System.Text.Json.Serialization;

namespace osu_notsodirect_overlay.Models
{
    public class BeatmapSet
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("artist")]
        public string Artist { get; set; } = string.Empty;
        
        [JsonPropertyName("artist_unicode")]
        public string Artist_Unicode { get; set; } = string.Empty;
        
        [JsonPropertyName("creator")]
        public string Creator { get; set; } = string.Empty;
        
        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
        
        [JsonPropertyName("tags")]
        public string Tags { get; set; } = string.Empty;
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("title_unicode")]
        public string Title_Unicode { get; set; } = string.Empty;
        
        [JsonPropertyName("favourite_count")]
        public int Favourite_Count { get; set; }
        
        [JsonPropertyName("nsfw")]
        public bool Nsfw { get; set; }
        
        [JsonPropertyName("play_count")]
        public int Play_Count { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        
        [JsonPropertyName("bpm")]
        public double Bpm { get; set; }
        
        [JsonPropertyName("ranked")]
        public string Ranked { get; set; } = string.Empty;
        
        [JsonPropertyName("beatmaps")]
        public List<Beatmap> Beatmaps { get; set; } = new List<Beatmap>();

        public string DifficultyRange
        {
            get
            {
                if (Beatmaps == null || Beatmaps.Count == 0) return "Unknown";

                double min = double.MaxValue;
                double max = double.MinValue;

                foreach (var map in Beatmaps)
                {
                    min = Math.Min(min, map.Difficulty_Rating);
                    max = Math.Max(max, map.Difficulty_Rating);
                }

                return min == max ? $"{min:F2}★" : $"{min:F2}★ - {max:F2}★";
            }
        }
    }

    public class Beatmap
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("beatmapset_id")]
        public int Beatmapset_Id { get; set; }
        
        [JsonPropertyName("difficulty_rating")]
        public double Difficulty_Rating { get; set; }
        
        [JsonPropertyName("mode")]
        public string Mode { get; set; } = string.Empty;
        
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
        
        [JsonPropertyName("accuracy")]
        public double Accuracy { get; set; }
        
        [JsonPropertyName("ar")]
        public double Ar { get; set; }
        
        [JsonPropertyName("bpm")]
        public double Bpm { get; set; }
        
        [JsonPropertyName("cs")]
        public double Cs { get; set; }
        
        [JsonPropertyName("drain")]
        public double Drain { get; set; }
        
        [JsonPropertyName("hit_length")]
        public int Hit_Length { get; set; }
        
        [JsonPropertyName("total_length")]
        public int Total_Length { get; set; }
        
        [JsonPropertyName("mode_int")]
        public int Mode_Int { get; set; }

        public string FormattedLength
        {
            get
            {
                TimeSpan time = TimeSpan.FromSeconds(Total_Length);
                return $"{(int)time.TotalMinutes}:{time.Seconds:D2}";
            }
        }
    }
} 