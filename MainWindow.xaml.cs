using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;

namespace SRR
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        const int storyModeStartAddress = 0x2CB9F0;
        const int firstStageDarkEntry = 0x4C1BC4;
        const int missionOffset = 0xC;
        const int stageOffset = 0x50;
        Dictionary<int, int> stageAssociationIDMap = new Dictionary<int, int>
        {
            { 5, 100 }, // first stage
            { 6, 200 },
            { 7, 201 },
            { 8, 202 },
            { 9, 300 },
            { 10, 301 },
            { 11, 302 },
            { 12, 400 },
            { 13, 401 },
            { 14, 402 },
            { 15, 403 },
            { 16, 404 },
            { 17, 500 },
            { 18, 501 },
            { 19, 502 },
            { 20, 503 },
            { 21, 504 },
            { 22, 600 },
            { 23, 601 },
            { 24, 602 },
            { 25, 603 },
            { 26, 604 },
            { 27, 700 }, // last stage
            { 28, 210 }, // first sub boss
            { 29, 310 },
            { 30, 410 },
            { 31, 411 },
            { 32, 412 },
            { 33, 510 },
            { 34, 511 }, // last sub boss
            { 35, 610 }, // first boss
            { 36, 611 },
            { 37, 612 },
            { 38, 613 },
            { 39, 614 },
            { 40, 615 },
            { 41, 616 },
            { 42, 617 },
            { 43, 618 },
            { 44, 710 } // last boss
        };

        private void Randomize_Click(object sender, RoutedEventArgs e)
        {
            int randomNumber = -1;
            int terminateRoute = -1;
            int seed = CalculateSeed(SeedInputBox.Text);
            Random random = new Random(seed);

            int storyModeFirstStage = 0x38000000;

            randomNumber = random.Next(5, 44);

            MessageBox.Show("Route Start: " + randomNumber + ": " + stageAssociationIDMap[randomNumber]);

            storyModeFirstStage = storyModeFirstStage + stageAssociationIDMap[randomNumber];

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (ofd.FileName == string.Empty)
                return;
            var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.ReadWrite);
            
            stream.Position = 0x2CB9F0;
            byte[] bytesFirstStage = BitConverter.GetBytes(storyModeFirstStage);
            Array.Reverse(bytesFirstStage);
            stream.Write(bytesFirstStage);

            for (int i = 5; i < 45; i++)
            {
                // dark

                randomNumber = random.Next(5, 44);
                terminateRoute = random.Next(0, 10);

                stream.Position = firstStageDarkEntry + stageOffset * (i - 5);
                byte[] bytes;

                if (terminateRoute == 0) {
                    // 10% chance for ending the route
                    bytes = BitConverter.GetBytes(-2);
                }
                else
                {
                    bytes = BitConverter.GetBytes(randomNumber);
                }
                Array.Reverse(bytes);
                stream.Write(bytes);

                // normal

                randomNumber = random.Next(5, 44);
                terminateRoute = random.Next(0, 10);

                stream.Position = firstStageDarkEntry + stageOffset * (i - 5) + missionOffset;

                if (terminateRoute == 0)
                {
                    // 10% chance for ending the route
                    bytes = BitConverter.GetBytes(-2);
                }
                else
                {
                    bytes = BitConverter.GetBytes(randomNumber);
                }
                Array.Reverse(bytes);
                stream.Write(bytes);

                // hero

                randomNumber = random.Next(5, 44);
                terminateRoute = random.Next(0, 10);

                stream.Position = firstStageDarkEntry + stageOffset * (i - 5) + missionOffset * 2;

                if (terminateRoute == 0)
                {
                    // 10% chance for ending the route
                    bytes = BitConverter.GetBytes(-2);
                }
                else
                {
                    bytes = BitConverter.GetBytes(randomNumber);
                }
                Array.Reverse(bytes);
                stream.Write(bytes);
            }
            stream.Close();
            MessageBox.Show("DONE");
        }

        static int CalculateSeed(string seedString)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(seedString));
                // Convert the first 4 bytes of the hash to an integer for the seed
                int seed = BitConverter.ToInt32(hashBytes, 0);
                return seed;
            }
        }
    }
}
