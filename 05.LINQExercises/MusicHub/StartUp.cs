namespace MusicHub
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Data;
    using Initializer;

    public class StartUp
    {
        public static void Main(string[] args)
        {
            MusicHubDbContext context =
                new MusicHubDbContext();

            DbInitializer.ResetDatabase(context);

            Console.WriteLine(ExportSongsAboveDuration(context, 4));
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var albums = context.Producers
                .FirstOrDefault(x => x.Id == producerId)
                .Albums
                .Select(x => new
                {
                    AlbumName = x.Name,
                    x.ReleaseDate,
                    ProducerName = x.Producer.Name,
                    Songs = x.Songs
                    .Select(x => new
                    {
                        SongName = x.Name,
                        x.Price,
                        WriterName = x.Writer.Name
                    }).OrderByDescending(x => x.SongName).ThenBy(x => x.WriterName).ToList(),
                    AlbumPrice = x.Price
                })
                .OrderByDescending(x => x.AlbumPrice)
                .ToList();

            var result = new StringBuilder();

            foreach (var album in albums)
            {
                result.AppendLine($"-AlbumName: {album.AlbumName}");
                result.AppendLine($"-ReleaseDate: {album.ReleaseDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)}");
                result.AppendLine($"-ProducerName: {album.ProducerName}");
                result.AppendLine("-Songs:");

                int counter = 1;
                foreach (var song in album.Songs)
                {
                    result.AppendLine($"---#{counter}");
                    result.AppendLine($"---SongName: {song.SongName}");
                    result.AppendLine($"---Price: {song.Price:F2}");
                    result.AppendLine($"---Writer: {song.WriterName}");

                    counter++;
                }

                result.AppendLine($"-AlbumPrice: {album.AlbumPrice:F2}");
            }

            return result.ToString().TrimEnd();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var songs = context.Songs
                .ToList()
                .Where(x => x.Duration.TotalSeconds > duration)
                .Select(x => new
                {
                    x.Name,
                    Writer = x.Writer.Name,
                    PerformerFullName = x.SongPerformers
                                        .Select(x => x.Performer.FirstName + " " + x.Performer.LastName)
                                        .FirstOrDefault(),
                    AlbumProducer = x.Album.Producer.Name,
                    x.Duration
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Writer)
                .ThenBy(x => x.PerformerFullName)
                .ToList();

            var sb = new StringBuilder();
            int counter = 1;

            foreach (var song in songs)
            {
                sb.AppendLine($"-Song #{counter}");
                sb.AppendLine($"---SongName: {song.Name}");
                sb.AppendLine($"---Writer: {song.Writer}");
                sb.AppendLine($"---Performer: {song.PerformerFullName}");
                sb.AppendLine($"---AlbumProducer: {song.AlbumProducer}");
                sb.AppendLine($"---Duration: {song.Duration:c}");

                counter++;
            }

            return sb.ToString().TrimEnd();
        }
    }
}
