/**
 * @copyright Copyright (c) 2023 B-com http://www.b-com.com/
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

using UnityEngine.Experimental.Rendering;

using Google.Protobuf;

using Com.Bcom.Solar.Gprc;


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
