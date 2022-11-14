// See https://aka.ms/new-console-template for more information
using BigFileSort;
using System.Text;

//choose file here

Console.WriteLine("Enter a file path:");
var fileName = Console.ReadLine();

if (string.IsNullOrEmpty(fileName) || !(new FileInfo(fileName).Exists))
    throw new Exception($"File {fileName} does not exist");

FileInfo fileInfo = new FileInfo(fileName);

long memorySize = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;

long availableMemSize = memorySize / 3;

// compare sizes. If file is bigger, then split into smaller ones
if (fileInfo.Length > availableMemSize)
{
    string tempPath = Path.Combine(Path.GetTempPath(), "BigFileSortFolder");
    Directory.CreateDirectory(tempPath);

    var writers = new Dictionary<string, StreamWriter>();
    var paths = new Dictionary<string, string>();

    //Split file to files by first letter
    const int BufferSize = 4096;// define bufferSize
    await using (var fileStream = File.OpenRead(fileName))
    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
    {
        string line;
        while ((line = await streamReader.ReadLineAsync()) != null)
        {
            var lineSplit = line.Split(". ");
            if (lineSplit.Length == 0)
                throw new Exception("Invalid file format");

            bool isParse = long.TryParse(lineSplit[0], out var num);

            if (!isParse)
                throw new Exception("Invalid file format");

            string firstLetter = lineSplit[1][1].ToString();

            if (!writers.ContainsKey(firstLetter))
            {
                string tmpPath = Path.Combine(tempPath, $"{firstLetter}.txt");
                File.Create(tmpPath).Close();
                writers.Add(firstLetter, new StreamWriter(tmpPath));
                paths.Add(firstLetter, tmpPath);
            }

            if (new FileInfo(paths[firstLetter]).Length >= availableMemSize)
                Helpers.UpdateFile(tempPath, writers, paths, firstLetter);

            writers[firstLetter].WriteLine(line);
        }

        foreach (var row in writers)
            row.Value.Dispose();
    }

    foreach (var file in Directory.GetFiles(tempPath).OrderBy(x => x))
        await Helpers.SortFileInMemory(file);

    using (var outputStream = File.Create(fileName))
    {
        foreach (var file in Directory.GetFiles(tempPath).OrderBy(x => x))
        {
            using var inputStream = File.OpenRead(file);

            inputStream.CopyTo(outputStream);
        }
    }

    Directory.Delete(tempPath, true);
}
else
{
    await Helpers.SortFileInMemory(fileName);
}
