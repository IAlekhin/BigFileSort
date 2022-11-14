// See https://aka.ms/new-console-template for more information

using System.Text;

Console.WriteLine("Enter file size in GB:");
var fileSize = Console.ReadLine();

if (!int.TryParse(fileSize, out int size))
    throw new Exception("Invalid file size");

long sizeBytes = size*1024*1024*1024;

Console.WriteLine("Enter a file path:");
var fileName = Console.ReadLine();

if (string.IsNullOrEmpty(fileName) || !(new FileInfo(fileName).Exists))
    throw new Exception($"File {fileName} does not exist");

Random random = new Random();
const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

const int BufferSize = 4096;// define bufferSize
File.Create(fileName).Close();
await using (var fileStream = File.OpenWrite(fileName))
await using (var stream = new StreamWriter(fileStream, Encoding.UTF8, BufferSize))
{
    int count = 0;
    while (true)
    {
        string strPart = new string(Enumerable.Repeat(chars, random.Next(30)).Select(s => s[random.Next(s.Length)]).ToArray());
        int intPart = random.Next();
        await stream.WriteLineAsync($"{intPart}. {strPart}");
        if (count % 13 == 0)
        {
            int intPart2 = random.Next();
            await stream.WriteLineAsync($"{intPart2}. {strPart}");
            count = 0;
        }
        count++;
        if (new FileInfo(fileName).Length >= sizeBytes)
            return;
    }
}