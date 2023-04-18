﻿using System;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.IO;
using System.Text;

namespace DTV {
    class Converter {
        private readonly int width;
        private readonly int heighth;
        private Bitmap canvas;
        private readonly int pixelSize = 5;
        private string pathToSource;
        private string pathToSave;
        private int byteCount=2;
        public Converter(string saveBMP,string source,int bitC, int h, int w)
        {
            byteCount = bitC > 3 ? byteCount : bitC;
            canvas = new Bitmap(w, h);
            pathToSource = source;
            pathToSave = saveBMP;
            bitC = 2;
            width = w;
            heighth = h;
        }
        private bool IsValid(string path) => File.Exists(path);
        public List<string> ProvideBinaryData() {
            BinaryReader reader = new BinaryReader(File.OpenRead(pathToSource));
            List<string> data = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            for(int amountOfBytes = 0;amountOfBytes<16; amountOfBytes+=2) {
                data.Add($"#{reader.BaseStream.Length.ToString().PadLeft(16, '0').Substring(amountOfBytes,2).PadRight(4,'0')}");
            }
            while (reader.BaseStream.Position < reader.BaseStream.Length) {

                stringBuilder.Append('#');
                for (long curByte = 0; (reader.BaseStream.Position < reader.BaseStream.Length) && (curByte < byteCount); curByte++)
                {
                    stringBuilder.Append(reader.ReadByte().ToString("X").PadLeft(2, '0'));
                }
                data.Add(stringBuilder.ToString());
                stringBuilder.Clear();
            }
            reader.Close();
            return data;
        }
        public void Encode() {
            if (!IsValid(pathToSource)) {
                Console.WriteLine("Provided path is invalid");
                return; 
            }
            var binData = ProvideBinaryData();
            int count = -1;
            for (int i = 0; i < heighth - pixelSize; i+=pixelSize)
            {
                for (int j = 0; j < width - pixelSize; j+= pixelSize)
                {
                    count++;
                    if (count < binData.Count) {
                        //Console.WriteLine(binData[count].PadRight(7, '0'));
                        SetChunkOfPixelsToColor(i, j, ColorTranslator.FromHtml(binData[count].PadRight(7, '0')));
                    }
                }
            } 
            
            canvas.Save(pathToSave);
        }
        public void SetChunkOfPixelsToColor(int h,int w,Color color) {
            for (int a = h; a < h + pixelSize; a++) {
                for (int b = w; b < w+pixelSize; b++)
                {
                    canvas.SetPixel(b,a,color);
                }    
            }
        }

        public byte[] StringToByteArray(string hex)
        {
            List<byte> bytes = new List<byte>();  
            for (int i = 0; i < byteCount*2; i+=2)
            {
                bytes.Add(Convert.ToByte(hex.Substring(i,2), 16));
            }
            return bytes.ToArray();
        }
        private string GetExtention() { return ""; }
        private long RetrieveAmountOfBytes(Bitmap bmp) {
            string temp = "";
            for (int j = 0; j < 8* pixelSize; j += pixelSize)
            {
                temp += bmp.GetPixel(j, 0).R.ToString("X").PadLeft(2, '0');
            }
            return long.Parse(temp);
        }
        public void Decode(string pathToSaveDecodedData) {
            var saveDir = Directory.CreateDirectory($"{pathToSaveDecodedData}\\Decoded");
            StreamWriter writer = new StreamWriter(File.Open($"{saveDir.FullName}\\{DateTime.Now.ToString("HH-mm-ss-fff")}.txt", FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite));
            Color color;
            string bytesToWriteAsString="";
            StringBuilder SB = new StringBuilder();
            List<byte> listOfBytes = new List<byte>();
            Bitmap bitmap = new Bitmap(pathToSave);
            int countOfWrittenBytes = 0;
            long countOfBytesToRead = RetrieveAmountOfBytes(bitmap);
            int i = 8* pixelSize;
            for (int j = 0; j < heighth - pixelSize; j += pixelSize)
            {
                for (; i < width - pixelSize; i += pixelSize)
                {
                    if (countOfWrittenBytes < countOfBytesToRead) {
                        color = bitmap.GetPixel(i, j);
                        bytesToWriteAsString += color.R.ToString("X2");
                        if (byteCount == 2) {
                            bytesToWriteAsString += color.G.ToString("X2");
                        }
                        if (byteCount == 3) {
                            bytesToWriteAsString += color.B.ToString("X2");
                        }
                        foreach (byte b in StringToByteArray(bytesToWriteAsString)) {
                            SB.Append((char)b);
                        }
                        //Console.WriteLine(bytesToWriteAsString);
                        bytesToWriteAsString = "";
                        countOfWrittenBytes+=byteCount;
                    }
                }
                i = 0;
            }
            writer.Write(SB.ToString());
            writer.Close();
        }
    }
    class Program {
        public static void Main(string[] args) {
            Converter converter = new Converter("C:\\Users\\xbox0\\Desktop\\dataToVideo\\dataSave.bmp", "C:\\Users\\xbox0\\Desktop\\dataToVideo\\data.txt",2,1080,1920);
            converter.Encode();
            converter.Decode("C:\\Users\\xbox0\\Desktop\\dataToVideo");
            Console.Read();
        }
    }
}