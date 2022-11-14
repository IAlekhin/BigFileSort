using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigFileSort
{
    public class Helpers
    {
        public static readonly Dictionary<int, string> Suffixes = new()
        {
            { 0, "B"},
            { 1, "KB"},
            { 2, "MB"},
            { 3, "GB"},
        };

        public static (double, string) BytesToString(long byteCount)
        {
            if (byteCount < 0)
                throw new Exception("Invalid input. Accepts only positive numbers.");

            if (byteCount == 0)
                return (0, Suffixes[0]);

            int place = Convert.ToInt32(Math.Floor(Math.Log(byteCount, 1024)));

            if (place > 3) place = 3;// up to GB

            double num = Math.Round(byteCount / Math.Pow(1024, place), 1);

            return (Math.Sign(byteCount) * num, Suffixes[place]);
        }


        public static void UpdateFile(string tempPath, Dictionary<string, StreamWriter> writers, Dictionary<string, string> paths, string firstLetter)
        {
            var path = Path.GetFileNameWithoutExtension(paths[firstLetter]).Substring(1);
            int count;

            if (string.IsNullOrEmpty(path))
            {
                count = 1;
            }
            else
            {
                count = int.Parse(path);
                count++;
            }

            writers[firstLetter] = new StreamWriter(Path.Combine(tempPath, $"{firstLetter}{count}.txt"));
            paths[firstLetter] = $"{firstLetter}{count}.txt";
        }

        internal static async Task SortFileInMemory(string fileName)
        {
            var itemsList = new List<Tuple<long, string>>();

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

                    itemsList.Add(new Tuple<long, string>(num, lineSplit[1]));
                }
            }

            itemsList.Sort((row1, row2) => SortTuple(row1, row2));

            File.WriteAllText(fileName, string.Empty);

            using (var fileStream = new StreamWriter(fileName))
            {
                foreach (var item in itemsList)
                    fileStream.WriteLine($"{item.Item1}. {item.Item2}");
            }
        }

        private static int SortTuple(Tuple<long, string> row1, Tuple<long, string> row2)
        {
            int res = row1.Item2.CompareTo(row2.Item2);
            if (res == 0)
                return row1.Item1.CompareTo(row2.Item1);

            return res;
        }
    }
}
