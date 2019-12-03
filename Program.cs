using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MyTree;
using DrBits;

namespace HuffmanCode
{
    class WorkWithCollections
    {
        string source;

        public WorkWithCollections(string textPath)
        {
            using (StreamReader read = new StreamReader(textPath))
                source = read.ReadToEnd();
        }

        public void DistinctSymbols(ref Dictionary<char, double> map)
        {
            var buf = source.Distinct();

            foreach (var elem in buf)
            {
                int count = source.Count(item => elem == item);
                map.Add(elem, Math.Round(Probability(count, source.Length), 5));
            }

            map = map.OrderBy(item => item.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public string CodingText(Dictionary<char, string> dictionary)
        {
            string result = "";

            foreach (var elem in source)
            {
                dictionary.TryGetValue(elem, out string buf);
                if (buf != default)
                {
                    result += buf;
                }
            }

            return result;
        }

        private double Probability(int count, int len)
        {
            return (count / (double)len);
        }

        public void MapView(Dictionary<char, double> map)
        {
            foreach (var elem in map)
                Console.WriteLine(elem.Key + ";" + elem.Value);

            Console.WriteLine("Sum is {0}", map.Values.Sum());
            Console.WriteLine();
            Console.WriteLine();
        }

        public void DictionaryView(Dictionary<char, string> dictionary)
        {
            var dic = dictionary.OrderBy(str => str.Value.Length);
            foreach (var elem in dic)
            {
                if (elem.Key == '\n')
                    Console.WriteLine("\\n" + "   " + elem.Value);
                else if (elem.Key == '\r')
                    Console.WriteLine("\\r" + "   " + elem.Value);
                else
                    Console.WriteLine(elem.Key + "   " + elem.Value);
            }
        }

        public double AverageCodeLen(Dictionary<char, double> map, Dictionary<char, string> dictionary)
        {
            double result = 0;
            foreach (var elem in dictionary)
            {
                map.TryGetValue(elem.Key, out double value);
                result += value * elem.Value.Length;
            }
            return result;
        }

        public double Entropy(Dictionary<char, double> map)
        {
            double result = 0;
            foreach (var elem in map)
            {
                result += (elem.Value * Math.Log(elem.Value, 2));
            }
            return (-result);
        }
    }

    class Program
    {
        private const string textPath = "..\\text\\byte.txt";
        private const string encodedFile = "..\\text\\Encode.txt";
        private const string resultadoNormal = "..\\text\\Decode.txt";

        static void Main(string[] args)
        {
            Dictionary<char, double> map = new Dictionary<char, double>();
            Dictionary<char, string> dictionary = new Dictionary<char, string>();
            WorkWithCollections useDict = new WorkWithCollections(textPath);

            // Highlight original symbols.
            useDict.DistinctSymbols(ref map);

            // Init tree with map.
            Tree tree = new Tree(map);

            // Init dictionary with tree.
            tree.InitDictionary(dictionary);
            useDict.DictionaryView(dictionary);
            Console.WriteLine();

            // Encode text with dictionary.
            string encodedString = useDict.CodingText(dictionary);

            // Encode dictionary && encoded string to file.
            Coding drBits = new Coding(encodedFile);
            drBits.Encode(dictionary, encodedString);

            // Decode encodedFile to resultado file
            Decoding decoding = new Decoding(resultadoNormal);
            Console.WriteLine(decoding.Decode(encodedFile).Length);

            Console.WriteLine("Average code length: " + useDict.AverageCodeLen(map, dictionary));
            Console.WriteLine("Entropy: " + useDict.Entropy(map));
        }
    }
}
