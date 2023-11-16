using FFmpeg.NET;
using SpotifyExplode;
using SpotifyExplode.Playlists;
using SpotifyExplode.Tracks;
using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

Console.Title = "Download Music Online - Spotify";
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Download Music Online - Spotify by J0nathan550");
Console.ResetColor();

bool IsMainAppWorks = true;
bool IsDialogEnded = false;

while (IsMainAppWorks)
{
    try
    {
        SpotifyClient spotify = new();
        string? playlistLink = string.Empty;
        while (string.IsNullOrEmpty(playlistLink))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Write a link to playlist (Spotify):");
            playlistLink = Console.ReadLine();
            Console.ResetColor();
            if (string.IsNullOrEmpty(playlistLink))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The link to playlist is empty.");
                Console.ResetColor();
            }
        }
        List<Track> tracks = (await spotify.Playlists.GetAsync((PlaylistId)playlistLink, new CancellationToken())).Tracks;
        int tracksDownloading = 0;
        string couldNotInstall = string.Empty;
        if (!Directory.Exists("Music")) Directory.CreateDirectory("Music");
        if (Directory.GetFiles("Music").Length != 0)
        {
            while (!IsDialogEnded)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("The directory contains files.\nIf you will type 'y' files inside of that directory will be deleted and new files will be installed.\nIf you will type 'n' the installing of new files will be aborted and you will not lose any of the files.");
                Console.ResetColor();
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.Y)
                {
                    int num = 0;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Deleting all of the music inside of the folder:");
                    Console.ResetColor();
                    foreach (string enumerateFile in Directory.EnumerateFiles("Music"))
                    {
                        num++;
                        FileInfo fileInfo = new(enumerateFile);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{num} - Deleting: " + fileInfo.Name);
                        Console.ResetColor();
                        File.Delete(enumerateFile);
                    }
                    IsDialogEnded = true;
                }
                else if (key.Key == ConsoleKey.N)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("App is ended, press any key to close the window.");
                    Console.ResetColor();
                    Console.ReadKey();
                    IsMainAppWorks = false;
                    return;
                }
            }
        }
        foreach (var track in tracks)
        {

            tracksDownloading++;
            string author = track.Artists[0].Name;
            string title = track.Title;
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Currently downloaded tracks: " + tracksDownloading + "\n" + "Downloading: " + author + " - " + title); // XXX - XXX
            Console.ResetColor();

            using HttpClient client = new();
            try
            {
                var youtube = new YoutubeClient();

                var id = spotify.Tracks.GetYoutubeIdAsync(track.Id).Result; // getting the ID of the video from youtube 

                var streamManifest = await youtube.Videos.Streams.GetManifestAsync("https://youtube.com/watch?v=" + id); // trying to get stream audio that we will download

                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate(); // getting only audio stream 

                await youtube.Videos.Streams.DownloadAsync(streamInfo, "Music/" + author + " - " + title + $".{streamInfo.Container}"); // downloading using YouTubeExplode.
            }
            catch (HttpRequestException ex) // internet problems or whatever. 
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            catch (Exception) // sometimes the ID of the video that your API gives can be null inside of YouTubeExplode (video doesn't exist or whatever), so better to catch also some exception to stop downloading and skip to another one.
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not download: " + author + " - " + title);
                couldNotInstall += author + " - " + title + "\n";
                Console.ResetColor();
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Downloading is finished, files that we couldn't download: \n" + couldNotInstall);
        Console.WriteLine("Trying to convert .WEBM or .MP4 files to .MP3: ");
        Console.ResetColor();
        couldNotInstall = string.Empty;
        tracksDownloading = 0;
        foreach (string enumerateFile in Directory.EnumerateFiles("Music"))
        {
            FileInfo info = new(enumerateFile);
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                tracksDownloading++;
                Console.WriteLine(tracksDownloading + " - Converting: " + info.Name + " to " + info.Name + ".mp3");
                MediaFile mediaFile = await new Engine("ffmpeg.exe").ConvertAsync(new InputFile(info.FullName), new OutputFile("Music/" + Path.GetFileNameWithoutExtension(info.FullName) + ".mp3"), new CancellationToken());
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not convert file: " + info.Name);
                couldNotInstall = couldNotInstall + info.Name + "\n";
                Console.ResetColor();
            }
        }
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Converting completed!\nFiles that are not converted: \n" + couldNotInstall);
        Console.ResetColor();
        tracksDownloading = 0;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Clearing up useless .WEBM or .MP4 files:\n");
        Console.ResetColor();
        foreach (string enumerateFile in Directory.EnumerateFiles("Music"))
        {
            FileInfo fileInfo = new(enumerateFile);
            if (fileInfo.Extension != ".mp3")
            {
                tracksDownloading++;
                Console.ForegroundColor = (ConsoleColor)14;
                Console.WriteLine(tracksDownloading + " - Deleting: " + fileInfo.Name);
                File.Delete(fileInfo.FullName);
            }
        }
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Opening folder: " + AppDomain.CurrentDomain.BaseDirectory + @"\Music\" + "\nPress any key to continue.");
        if (Directory.Exists("Music"))
        {
            Process.Start("explorer.exe", "Music");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("The specified directory does not exist.");
            Console.ResetColor();
        }
        Console.ReadKey();
        Console.ResetColor();
        Console.Clear();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Some error occured, try again!\n" + ex.Message);
        Console.ResetColor();
    }
}