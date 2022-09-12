using System.Collections;
using System.Text;

namespace UhOh
{
    public class Program
    {
        public const string Uh = "Uh";
        public const string Oh = "Oh";
        public const string uhohFileExtension = ".uhoh";

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
                if (args.Length < 3)
                {
                    Console.WriteLine("File extension of restored file required");
                    return;
                }
                var restoredFileExtension = args[2];
                if (!restoredFileExtension.StartsWith("."))
                {
                    Console.WriteLine($"{restoredFileExtension} is not a valid file extension");
                }

                if (Path.GetExtension(pathToFile) != uhohFileExtension)
                {
                    Console.WriteLine($"{pathToFile} is not a valid {uhohFileExtension} file");
                    return;
                }

                Restore(pathToFile, restoredFileExtension);
                Console.WriteLine("Restoration complete");
            }
        }

        private static void Expand(string pathToFile)
        {
            var fileBytes = File.ReadAllBytes(pathToFile);

            var fileBits = new BitArray(fileBytes);
            StringBuilder sb = new StringBuilder();
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

            string newPathToFile = Path.ChangeExtension(pathToFile, uhohFileExtension);
            File.WriteAllText(newPathToFile, sb.ToString());
        }

        private static void Restore(string pathToFile, string restoredFileExtension)
        {
            var fileString = File.ReadAllText(pathToFile);

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
            string pathToRestoredFile = Path.ChangeExtension(pathToFile, restoredFileExtension);
            File.WriteAllBytes(pathToRestoredFile, bytes);
        }
    }
}