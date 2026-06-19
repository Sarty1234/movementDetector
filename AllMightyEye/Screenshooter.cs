using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AllMightyEye
{
    public class Screenshooter
    {
        public static ImageCompareResult CompareImages(int[] img1, int[] img2)
        {
            const int step = 1;
            ImageCompareResult res = new ImageCompareResult();

            int totalPixels = img1.Length;
            if (totalPixels != img2.Length)
            {
                return res;
            }


            int matchingPixels = 0;
            for (int i = 0; i < totalPixels; i += step)
            {
                if (img1[i] == img2[i])
                {
                    matchingPixels++;
                }
            }


            res.matchingPixels = matchingPixels;
            res.comparedPixels = totalPixels / step;
            res.matchingPercent = matchingPixels;
            res.differenceInPixels = res.comparedPixels - res.matchingPixels;
            if (res.comparedPixels != 0)
            {
                res.matchingPercent = (float)res.matchingPixels / (float)res.comparedPixels;
            }


            return res;
        }


        public class ImageCompareResult
        {
            public int comparedPixels = 0;
            public int matchingPixels = 0;
            public float matchingPercent = 0;
            public int differenceInPixels = 0;
        };








        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        private static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, [Out] int[] lpvBits, ref BITMAPINFO lpbmi, uint uUsage);

        private const int SRCCOPY = 0x00CC0020;

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public uint bmiColors;
        }






        private IntPtr hScreenDC = IntPtr.Zero;
        private IntPtr hMemoryDC = IntPtr.Zero;
        private IntPtr hBitmap = IntPtr.Zero;
        private IntPtr hOldBitmap = IntPtr.Zero;

        private BITMAPINFO bmi;
        private int[] pixelData = null;

        // Track currently allocated capacities
        private int allocatedWidth = 0;
        private int allocatedHeight = 0;

        public Screenshooter()
        {
            // Screen and Memory DCs can still be initialized once and kept alive forever
            hScreenDC = CreateDC("DISPLAY", null, null, IntPtr.Zero);
            hMemoryDC = CreateCompatibleDC(hScreenDC);
        }

        public int[] CaptureScreenRectangle(int x, int y, int width, int height)
        {
            // 1. Check if we need to reallocate/resize our buffers
            if (width != allocatedWidth || height != allocatedHeight)
            {
                ResizeBuffers(width, height);
            }

            // 2. Perform the copy using the updated/existing handles
            BitBlt(hMemoryDC, 0, 0, width, height, hScreenDC, x, y, SRCCOPY);
            GetDIBits(hMemoryDC, hBitmap, 0, (uint)height, pixelData, ref bmi, 0);

            return pixelData;
        }

        private void ResizeBuffers(int width, int height)
        {
            // Clean up previous bitmap handles to prevent GDI leaks
            if (hBitmap != IntPtr.Zero)
            {
                SelectObject(hMemoryDC, hOldBitmap);
                DeleteObject(hBitmap);
            }

            // Reallocate GDI Bitmap with new dimensions
            hBitmap = CreateCompatibleBitmap(hScreenDC, width, height);
            hOldBitmap = SelectObject(hMemoryDC, hBitmap);

            // Reallocate the managed array only if the total area required grows
            int requiredLength = width * height;
            if (pixelData == null || pixelData.Length < requiredLength)
            {
                pixelData = new int[requiredLength];
            }

            // Update Bitmap Metadata Structure
            bmi = new BITMAPINFO();
            bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
            bmi.bmiHeader.biWidth = width;
            bmi.bmiHeader.biHeight = -height; // Top-to-bottom
            bmi.bmiHeader.biPlanes = 1;
            bmi.bmiHeader.biBitCount = 32;
            bmi.bmiHeader.biCompression = 0;

            // Store new tracking states
            allocatedWidth = width;
            allocatedHeight = height;
        }

        
        public void Dispose()
        {
            if (hMemoryDC != IntPtr.Zero)
            {
                if (hBitmap != IntPtr.Zero)
                {
                    SelectObject(hMemoryDC, hOldBitmap);
                    DeleteObject(hBitmap);
                }
                DeleteDC(hMemoryDC);
            }
            if (hScreenDC != IntPtr.Zero)
            {
                DeleteDC(hScreenDC);
            }
        }
    }
}