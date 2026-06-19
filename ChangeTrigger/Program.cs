using System;
using System.Runtime.InteropServices;




// See https://aka.ms/new-console-template for more information
//Thread.Sleep(5000);
int[] img1 = FastScreenshot.CaptureScreenRectangle(0, 0, 1920, 1080);
//Console.Beep(800, 200);
//Thread.Sleep(5000);
int[] img2 = FastScreenshot.CaptureScreenRectangle(0, 0, 1920, 1080);
//Console.Beep(800, 200);
ImageComparator.ImageCompareResult res = ImageComparator.CompareImages(img1, img2);
Console.WriteLine(res.differenceInPixels);
Console.WriteLine(100 - res.matchingPercent * 100);
//Console.Beep(800, 200);






public class ImageComparator
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
}



public class FastScreenshot
{
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

    /// <summary>
    /// Captures a specific rectangular region of the screen very fast.
    /// </summary>
    /// <param name="x">The X coordinate of the top-left corner of the rectangle.</param>
    /// <param name="y">The Y coordinate of the top-left corner of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    /// <returns>An int[] array representing the BGRA32 pixel data.</returns>
    public static int[] CaptureScreenRectangle(int x, int y, int width, int height)
    {
        // Create a DC for the entire display safely managing multi-monitor coordinates
        IntPtr hScreenDC = CreateDC("DISPLAY", null, null, IntPtr.Zero);
        IntPtr hMemoryDC = CreateCompatibleDC(hScreenDC);
        IntPtr hBitmap = CreateCompatibleBitmap(hScreenDC, width, height);
        IntPtr hOldBitmap = SelectObject(hMemoryDC, hBitmap);

        // BitBlt now starts copying from (x, y) on the screen into (0, 0) of our bitmap
        BitBlt(hMemoryDC, 0, 0, width, height, hScreenDC, x, y, SRCCOPY);

        // Allocate array based exactly on requested dimensions
        int[] pixelData = new int[width * height];

        BITMAPINFO bmi = new BITMAPINFO();
        bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
        bmi.bmiHeader.biWidth = width;
        bmi.bmiHeader.biHeight = -height; // Negative forces standard top-to-bottom orientation
        bmi.bmiHeader.biPlanes = 1;
        bmi.bmiHeader.biBitCount = 32;
        bmi.bmiHeader.biCompression = 0;

        // Extract bits directly into our managed int array
        GetDIBits(hMemoryDC, hBitmap, 0, (uint)height, pixelData, ref bmi, 0);

        // Cleanup
        SelectObject(hMemoryDC, hOldBitmap);
        DeleteObject(hBitmap);
        DeleteDC(hMemoryDC);
        DeleteDC(hScreenDC);

        return pixelData;
    }
}