﻿#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com>

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    N.I.N.A. is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    N.I.N.A. is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with N.I.N.A..  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion "copyright"

using FreeImageAPI;
using FreeImageAPI.Metadata;
using NINA.Model.ImageData;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NINA.Utility.RawConverter {

    internal class FreeImageConverter : IRawConverter {

        public FreeImageConverter() {
            DllLoader.LoadDll(Path.Combine("FreeImage", "FreeImage.dll"));
        }

        public Task<IImageData> Convert(
            MemoryStream s,
            int bitDepth,
            string rawType,
            ImageMetaData metaData,
            CancellationToken token = default(CancellationToken)) {
            return Task.Run(() => {
                using (MyStopWatch.Measure()) {
                    FIBITMAP img;
                    int left, top, imgWidth, imgHeight;
                    FREE_IMAGE_FORMAT format = FREE_IMAGE_FORMAT.FIF_RAW;
                    img = FreeImage.LoadFromStream(s, (FREE_IMAGE_LOAD_FLAGS)8, ref format);

                    FreeImage.GetMetadata(FREE_IMAGE_MDMODEL.FIMD_COMMENTS, img, "Raw.Frame.Width", out MetadataTag widthTag);
                    FreeImage.GetMetadata(FREE_IMAGE_MDMODEL.FIMD_COMMENTS, img, "Raw.Frame.Height", out MetadataTag heightTag);
                    FreeImage.GetMetadata(FREE_IMAGE_MDMODEL.FIMD_COMMENTS, img, "Raw.Frame.Left", out MetadataTag leftTag);
                    FreeImage.GetMetadata(FREE_IMAGE_MDMODEL.FIMD_COMMENTS, img, "Raw.Frame.Top", out MetadataTag topTag);
                    left = int.Parse(leftTag.ToString());
                    top = int.Parse(topTag.ToString());
                    imgWidth = int.Parse(widthTag.ToString());
                    imgHeight = int.Parse(heightTag.ToString());

                    using (var memStream = new MemoryStream()) {
                        FreeImage.SaveToStream(img, memStream, FREE_IMAGE_FORMAT.FIF_TIFF, FREE_IMAGE_SAVE_FLAGS.TIFF_NONE);
                        memStream.Position = 0;

                        var decoder = new TiffBitmapDecoder(memStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

                        CroppedBitmap cropped = new CroppedBitmap(decoder.Frames[0], new System.Windows.Int32Rect(left, top, imgWidth, imgHeight));

                        ushort[] outArray = new ushort[cropped.PixelWidth * cropped.PixelHeight];
                        cropped.CopyPixels(outArray, 2 * cropped.PixelWidth, 0);
                        FreeImage.UnloadEx(ref img);

                        var imageArray = new ImageArray(flatArray: outArray, rawData: s.ToArray(), rawType: rawType);
                        var data = new ImageData(
                            imageArray: imageArray,
                            width: cropped.PixelWidth,
                            height: cropped.PixelHeight,
                            bitDepth: bitDepth,
                            isBayered: true,
                            metaData: metaData);
                        return Task.FromResult<IImageData>(data);
                    }
                }
            });
        }
    }
}