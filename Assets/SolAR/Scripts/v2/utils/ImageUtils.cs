using System;
using UnityEngine.Experimental.Rendering;
using Com.Bcom.Solar.Gprc;
using Google.Protobuf;

namespace Com.BCom.SolAR
{
    public class ImageUtils
    {
        static public Frame ApplyCompression(Frame f)
        {
            switch (f.Image.ImageCompression)
            {
                case ImageCompression.None:
                    {
                        // Do nothing
                        break;
                    }
                case ImageCompression.Png:
                case ImageCompression.Jpg:
                    {
                        f.Image.Data = ByteString.CopyFrom(
                            ApplyCompression(
                                f.Image.Layout,
                                f.Image.Width,
                                f.Image.Height,
                                f.Image.Data.ToByteArray(),
                                f.Image.ImageCompression));
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("Unkown Image compression kind");
                    }
            }
            return f;
        }

        static private byte[] ApplyCompression(ImageLayout imLayout, uint imWidth, uint imHeight, byte[] imData, ImageCompression imageCompression)
        {
            GraphicsFormat format;
            switch (imLayout)
            {
                case ImageLayout.Rgb24: format = GraphicsFormat.B8G8R8A8_UNorm; break;
                case ImageLayout.Grey8: format = GraphicsFormat.R8_UNorm; break;
                case ImageLayout.Grey16: format = GraphicsFormat.R16_UNorm; break;
                default: throw new ArgumentException("Unkown image layout");
            }

            switch (imageCompression)
            {
                case ImageCompression.Png: return UnityEngine.ImageConversion.EncodeArrayToPNG(imData, format, imWidth, imHeight);
                case ImageCompression.Jpg: return UnityEngine.ImageConversion.EncodeArrayToJPG(imData, format, imWidth, imHeight);
                case ImageCompression.None: throw new ArgumentException("None should not be used here"); // return imData;
                default: throw new ArgumentException("Unknown image compression");

            }
        }
    }
}
