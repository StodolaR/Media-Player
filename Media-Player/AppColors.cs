using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Media_Player
{
    public class AppColors
    {
        public GradientStopCollection leftBG;
        private double[] leftBGStops = { 0, 1 };
        private int[] leftBGByteSubs = { 2,128 };
        public GradientStopCollection rightBG;
        private double[] rightBGStops = { 0, 0.25, 0.3, 0.35, 0.55, 0.6, 0.65, 1};
        private int[] rightBGByteSubs = { 0, 32, 128, 68, 84, -80, 90, 128 };
        public GradientStopCollection playlistBG;
        //public SolidColorBrush buttonColor;
        //public SolidColorBrush clickColor;
        public AppColors(SolidColorBrush? basicColor)
        {
            leftBG = new GradientStopCollection();
            rightBG = new GradientStopCollection();
            playlistBG = new GradientStopCollection();
            if (basicColor != null)
            {
                int basicR = basicColor.Color.R;
                int basicG = basicColor.Color.G;
                int basicB = basicColor.Color.B;
                GradientStop stop;
                for (int i = 0; i < 2; i++)
                {
                    byte adjustedR = (byte)(basicR - leftBGByteSubs[i]);
                    byte adjustedG = (byte)(basicG - leftBGByteSubs[i]);
                    byte adjustedB = (byte)(basicB - leftBGByteSubs[i]);
                    stop = new GradientStop(Color.FromRgb(adjustedR, adjustedG, adjustedB), leftBGStops[i]);
                    leftBG.Add(stop);
                    stop = new GradientStop(Color.FromRgb(adjustedR, adjustedG, adjustedB), leftBGStops[1-i]);
                    playlistBG.Add(stop);
                }
                for (int i = 0;i < 8; i++)
                {
                    byte adjustedR = (byte)(basicR - rightBGByteSubs[i]);
                    byte adjustedG = (byte)(basicG - rightBGByteSubs[i]);
                    byte adjustedB = (byte)(basicB - rightBGByteSubs[i]);
                    stop = new GradientStop(Color.FromRgb(adjustedR, adjustedG, adjustedB), rightBGStops[i]);
                    rightBG.Add(stop);
                }
                
            }

        }
    }
}
