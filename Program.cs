using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Runtime.InteropServices;




// See https://aka.ms/new-console-template for more information
int[] img1 = LoadImageAsIntArray("Images\\1.png");
int[] img2 = LoadImageAsIntArray("Images\\2.png");
ImageCompareResult res = CompareImages(img1, img2);
Console.WriteLine(res.differenceInPixels);
Console.WriteLine(100 - res.matchingPercent * 100);



ImageCompareResult CompareImages(int[] img1, int[] img2)
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

int[] LoadImageAsIntArray(string filePath)
{
    using (Image<Rgba32> image = Image.Load<Rgba32>(filePath))
    {
        int totalPixels = image.Height * image.Width;
        int[] pixelArray = new int[totalPixels];

        image.CopyPixelDataTo(MemoryMarshal.Cast<int, Rgba32>(pixelArray));

        return pixelArray;
    }
}


class ImageCompareResult
{
    public int comparedPixels = 0;
    public int matchingPixels = 0;
    public float matchingPercent = 0;
    public int differenceInPixels = 0;
};

