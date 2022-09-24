using System.Collections;
using System.Text;

namespace UhOh
{
    public class Program
    {
        public const string Uh = "Uh";
        public const string Oh = "Oh";
        public const string UhohFileExtension = ".uhoh";
        public const string MetdataDelimiter = ";";

        public static class Verbs
        {
            public const string Expand = "expand";
            public const string Restore = "restore";
            public static readonly HashSet<string> ValidVerbs = new HashSet<string>() { Expand, Restore };
        }

        public static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("Invalid number of arguments");
                return;
            }

            var verb = args[0].ToLower();
            var pathToFile = args[1];

            if (!Verbs.ValidVerbs.Contains(verb))
            {
                Console.WriteLine($"Invalid verb {args[0]}");
                return;
            }

            if (!File.Exists(pathToFile))
            {
                Console.WriteLine($"Invalid path to file provided");
            }

            if (verb == Verbs.Expand)
            {
                Expand(pathToFile);
                Console.WriteLine("Expansion complete");
            }
            else
            {
                if (Path.GetExtension(pathToFile) != UhohFileExtension)
                {
                    Console.WriteLine($"{pathToFile} is not a valid {UhohFileExtension} file");
                    return;
                }

                Restore(pathToFile);
                Console.WriteLine("Restoration complete");
            }
        }

        private static void Expand(string pathToFile)
        {
            var fileBytes = File.ReadAllBytes(pathToFile);

            var fileBits = new BitArray(fileBytes);
            StringBuilder sb = new StringBuilder();

            string fileExtension = Path.GetExtension(pathToFile);
            if (!string.IsNullOrWhiteSpace(fileExtension))
            {
                // add it as metadata inside the .uhoh file
                sb.Append($"{fileExtension}{MetdataDelimiter}");
            }

            for (int i = 0; i < fileBits.Length; i++)
            {
                if (fileBits[i])
                {
                    sb.Append(Uh);
                }
                else
                {
                    sb.Append(Oh);
                }
            }

            string newPathToFile = Path.ChangeExtension(pathToFile, UhohFileExtension);
            File.WriteAllText(newPathToFile, sb.ToString());
        }

        private static void Restore(string pathToFile)
        {
            var fullFileString = File.ReadAllText(pathToFile);

            var split = fullFileString.Split(MetdataDelimiter);

            string fileExtension = string.Empty;
            if (split.Length > 1)
            {
                fileExtension = split[0];
            }

            var fileString = split[split.Length - 1];

            BitArray bitArray = new BitArray(fileString.Length / 2);
            
            int bufLength = 2;
            int start = 0;
            bool isDone = start + bufLength > fileString.Length;
            int iteration = 0;
            while (!isDone)
            {
                string buf = fileString.Substring(start, bufLength);
                if (buf == Uh)
                {
                    bitArray[iteration] = true;
                }
                // BitArray's values are false when initialized
                //else
                //{
                //    bitArray[iteration] = false;
                //}

                // shift buffer
                start += bufLength;
                iteration++;
                isDone = start + bufLength > fileString.Length;
            }

            byte[] bytes = new byte[bitArray.Length / 8 + (bitArray.Length % 8 == 0 ? 0 : 1)];
            bitArray.CopyTo(bytes, 0);
            string pathToRestoredFile;
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                pathToRestoredFile = Path.GetFileNameWithoutExtension(pathToFile);
            }
            else
            {
                pathToRestoredFile = Path.ChangeExtension(pathToFile, fileExtension);
            }
            File.WriteAllBytes(pathToRestoredFile, bytes);
        }
    }
}