using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DrBits
{
    abstract class UseFunc
    {

        protected static IEnumerable<string> Split(string str, int chunkSize)
        {
            for (int i = 0; i < str.Length; i += chunkSize)
                yield return str.Substring(i, Math.Min(chunkSize, str.Length - i));
        }

        protected string BitsToString(short meaningBitsLen, byte[] bytes)
        {
            string res = "";

            // ...
            // [1]
            // [0]
            // Start with end of mas,
            // Transform bits to string.
            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                byte word = bytes[i];
                for (int j = 0; j < 8; j++)
                {
                    res += ((word & 1) == 0 ? "0" : "1");
                    word >>= 1;
                }
            }

            // Delete useless zero bits
            // 00101 => ..101
            if (bytes.Length == 1)
            {
                res = res.Substring(0, meaningBitsLen);
            }
            else
            {
                res = res.Substring(res.Length - meaningBitsLen, meaningBitsLen);
            }

            char[] vs = res.ToCharArray();
            // 1010 => 0101
            Array.Reverse(vs);

            return new string(vs);
        }

        protected byte ToBytes(string value)
        {
            // Add zero chars to the begining.
            string zero = new string('0', 8 - value.Length);
            return Convert.ToByte(zero + value, 2);
        }

        protected byte[] ToBytes(string[] strMas)
        {
            List<byte> bytes = new List<byte>();

            // Add zero chars to the end of last elem in string[].
            string zero = new string('0', 8 - strMas[strMas.Length - 1].Length);
            strMas[strMas.Length - 1] += zero;

            foreach (var elem in strMas)
            {
                bytes.Add(Convert.ToByte(elem, 2));
            }

            return bytes.ToArray();
        }

        protected string ZeroToString(string first)
        {
            string zero = new string('0', 8 - first.Length);
            return (zero + first);
        }
    }

    class Coding : UseFunc
    {
        private byte lastBits;
        private string path;

        public Coding(string encodedFile)
        {
            path = encodedFile;
        }

        public void Encode(Dictionary<char, string> dictionary, string codingString)
        {
            var strMas = Split(codingString, 8).ToArray();
            lastBits = (byte)strMas[strMas.Length - 1].Length;
            List<byte> bytes = new List<byte>(ToBytes(strMas));

            DictionaryToBytes(dictionary);

            using (FileStream fstream = new FileStream(path, FileMode.Append))
            {
                fstream.Write(bytes.ToArray(), 0, bytes.Count);
            }
        }

        private void DictionaryToBytes(Dictionary<char, string> dictionary)
        {
            List<byte> bytes = new List<byte>();

            // Dictionary to bytes
            foreach (var elem in dictionary)
            {
                // Logic in file:
                // (Char)(MeaningBitsLength)(MeaningBits)...

                int lenght = elem.Value.Length;

                bytes.AddRange(BitConverter.GetBytes(elem.Key));
                bytes.AddRange(BitConverter.GetBytes((short)lenght));

                // MeaningBits:
                if (lenght <= 8)
                {
                    bytes.Add(ToBytes(elem.Value));
                }
                else
                {
                    bytes.AddRange(ToBytes(Split(elem.Value, 8).ToArray()));
                }
            }

            // Write bytes to File
            using (FileStream fstream = new FileStream(path, FileMode.Create))
            {
                fstream.WriteByte(lastBits);
                fstream.Write(BitConverter.GetBytes((short)bytes.Count), 0, 2);
                fstream.Write(bytes.ToArray(), 0, bytes.Count);
            }
        }
    }

    class Decoding : UseFunc
    {
        private byte lastBits;
        private string codingPart = "";
        private string resultadoPath;

        public Decoding(string resultadoPath)
        {
            this.resultadoPath = resultadoPath;
        }

        public string Decode(string path)
        {
            List<byte> bytes = new List<byte>();
            string result = "";

            var dictionary = DictionaryFromBytes(path);

            while (codingPart.Length > 0)
            {
                foreach (var elem in dictionary)
                {
                    if (codingPart.StartsWith(elem.Value))
                    {
                        result += elem.Key;
                        codingPart = codingPart.Substring(elem.Value.Length);
                    }
                }

            }

            foreach (var elem in result)
            {
                bytes.Add(Convert.ToByte(elem));
            }

            using (FileStream fstream = new FileStream(resultadoPath, FileMode.Create))
            {
                fstream.Write(bytes.ToArray(), 0, bytes.Count);
            }

            return result;
        }

        private Dictionary<char, string> DictionaryFromBytes(string path)
        {
            Dictionary<char, string> dictionary = new Dictionary<char, string>();
            byte[] bytes;
            string dictionaryString;

            using (FileStream fstream = new FileStream(path, FileMode.Open))
            {
                // First byte - len of last meaning bits in coding string
                lastBits = (byte)fstream.ReadByte();

                // Next 2 bytes - len of bytes for future dictionary
                byte[] dictionaryLen = new byte[2];
                fstream.Read(dictionaryLen, 0, 2);
                int length = BitConverter.ToInt16(dictionaryLen, 0);
                bytes = new byte[length];

                // Read bytes for future dictionary
                fstream.Read(bytes, 0, bytes.Length);

                // Read other bytes (that mean coding string)
                FileInfo fileInfo = new FileInfo(path);
                byte[] str = new byte[fileInfo.Length - length - 3];
                fstream.Read(str, 0, str.Length);


                foreach (var elem in str)
                    codingPart += ZeroToString(Convert.ToString(elem, 2));

                // Delete useless zero bits in coding text
                codingPart = codingPart.Substring(0, codingPart.Length - 8 + lastBits);
            }

            for (int i = 0; i < bytes.Length;)
            {
                byte[] twoBytes = new byte[2];

                // 2 byte => char
                twoBytes[0] = bytes[i++];
                twoBytes[1] = bytes[i++];

                // Transform to char
                char dictionaryChar = BitConverter.ToChar(twoBytes, 0);

                // 2 byte => Meaning bytes (short)
                twoBytes[0] = bytes[i++];
                twoBytes[1] = bytes[i++];

                // Transform to short
                short meaningBitsLen = BitConverter.ToInt16(twoBytes, 0);

                byte[] meaningBits;
                if(meaningBitsLen%8 == 0)
                {
                    meaningBits = new byte[meaningBitsLen / 8];
                }
                else
                {
                    meaningBits = new byte[meaningBitsLen / 8 + 1];
                }

                // Copy meaning bytes to another massive
                for (int j = 0; j <= meaningBits.Length - 1; j++)
                {
                    meaningBits[j] = bytes[i++];
                }

                dictionaryString = BitsToString(meaningBitsLen, meaningBits);
                
                // Add Pair to dictionary
                dictionary.Add(dictionaryChar, dictionaryString);
            }

            return dictionary;
        }
    }
}