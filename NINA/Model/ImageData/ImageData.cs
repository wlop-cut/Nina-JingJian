﻿#region "copyright"

/*
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

/*
 * Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com>
 * Copyright 2019 Dale Ghent <daleg@elemental.org>
 */

#endregion "copyright"

using NINA.Utility;
using NINA.Utility.Enum;
using NINA.Utility.ImageAnalysis;
using NINA.Utility.RawConverter;
using nom.tam.fits;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NINA.Model.ImageData {

    public class ImageData : IImageData {

        public ImageData(ushort[] input, int width, int height, int bitDepth, bool isBayered, ImageMetaData metaData)
            : this(
                  imageArray: new ImageArray(flatArray: input),
                  width: width,
                  height: height,
                  bitDepth: bitDepth,
                  isBayered: isBayered,
                  metaData: metaData) {
        }

        public ImageData(IImageArray imageArray, int width, int height, int bitDepth, bool isBayered, ImageMetaData metaData) {
            this.Data = imageArray;
            this.MetaData = metaData;
            this.Properties = new ImageProperties(width: width, height: height, bitDepth: bitDepth, isBayered: isBayered);
            //zhang105
            //this.StarDetectionAnalysis = new StarDetectionAnalysis();
            this.Statistics = new Nito.AsyncEx.AsyncLazy<IImageStatistics>(async () => await Task.Run(() => ImageStatistics.Create(this)));
        }

        public IImageArray Data { get; private set; }

        public ImageProperties Properties { get; private set; }

        public ImageMetaData MetaData { get; private set; }

        public Nito.AsyncEx.AsyncLazy<IImageStatistics> Statistics { get; private set; }

        
        //public IStarDetectionAnalysis StarDetectionAnalysis { get; private set; }

        public IRenderedImage RenderImage() {
            return RenderedImage.Create(source: this.RenderBitmapSource(), rawImageData: this);//逻辑上从这里往后都是图像处理的内容了，包括什么BitmapFromSource等函数，要仔细的读一读，读懂。
        }

        public BitmapSource RenderBitmapSource() {
            return ImageUtility.CreateSourceFromArray(this.Data, this.Properties, PixelFormats.Gray16);
        }

        #region "Save"

        /// <summary>
        ///  Saves file to application temp path
        /// </summary>
        /// <param name="fileType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<string> PrepareSave(FileSaveInfo fileSaveInfo, CancellationToken cancelToken = default(CancellationToken)) {
            var actualPath = string.Empty;
            try {
                using (MyStopWatch.Measure()) {
                    actualPath = await SaveToDiskAsync(fileSaveInfo, cancelToken, false);
                }
            } catch (OperationCanceledException ex) {
                throw ex;
            } catch (Exception ex) {
                Logger.Error(ex);
                throw ex;
            } finally {
            }
            return actualPath;
        }

        /// <summary>
        /// Renames and moves file to destination according to pattern
        /// </summary>
        /// <param name="file"></param>
        /// <param name="targetPath"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public string FinalizeSave(string file, string pattern) {
            try {
                var imagePatterns = GetImagePatterns();
                var fileName = imagePatterns.GetImageFileString(pattern);
                var extension = Path.GetExtension(file);
                var targetPath = Path.GetDirectoryName(file);
                var newFileName = Utility.Utility.GetUniqueFilePath(Path.Combine(targetPath, $"{fileName}{extension}"));

                var fi = new FileInfo(newFileName);
                if (!fi.Directory.Exists) {
                    fi.Directory.Create();
                }

                File.Move(file, newFileName);
                return newFileName;
            } catch (Exception ex) {
                Logger.Error(ex);
                throw ex;
            } finally {
            }
        }

        private ImagePatterns GetImagePatterns() {
            var p = new ImagePatterns();
            var metadata = this.MetaData;
            p.Set(ImagePatternKeys.Filter, metadata.FilterWheel.Filter);
            p.Set(ImagePatternKeys.ExposureTime, metadata.Image.ExposureTime);
            p.Set(ImagePatternKeys.ApplicationStartDate, Utility.Utility.ApplicationStartDate.ToString("yyyy-MM-dd"));
            p.Set(ImagePatternKeys.Date, metadata.Image.ExposureStart.ToString("yyyy-MM-dd"));

            // ExposureStart is initialized to DateTime.MinValue, and we cannot subtract time from that. Only evaluate
            // the $$DATEMINUS12$$ pattern if the time is at least 12 hours on from DateTime.MinValue.
            if (metadata.Image.ExposureStart > DateTime.MinValue.AddHours(12)) {
                p.Set(ImagePatternKeys.DateMinus12, metadata.Image.ExposureStart.AddHours(-12).ToString("yyyy-MM-dd"));
            }

            p.Set(ImagePatternKeys.Time, metadata.Image.ExposureStart.ToString("HH-mm-ss"));
            p.Set(ImagePatternKeys.DateTime, metadata.Image.ExposureStart.ToString("yyyy-MM-dd_HH-mm-ss"));
            p.Set(ImagePatternKeys.FrameNr, metadata.Image.ExposureNumber.ToString("0000"));
            p.Set(ImagePatternKeys.ImageType, metadata.Image.ImageType);
            p.Set(ImagePatternKeys.TargetName, metadata.Target.Name);

            //zhang099
            //if (metadata.Image.RecordedRMS != null) {
            //    p.Set(ImagePatternKeys.RMS, metadata.Image.RecordedRMS.Total);
            //    p.Set(ImagePatternKeys.RMSArcSec, metadata.Image.RecordedRMS.Total * metadata.Image.RecordedRMS.Scale);
            //}

            if (!double.IsNaN(metadata.Focuser.Position)) {
                p.Set(ImagePatternKeys.FocuserPosition, metadata.Focuser.Position);
            }

            if (!double.IsNaN(metadata.Focuser.Temperature)) {
                p.Set(ImagePatternKeys.FocuserTemp, metadata.Focuser.Temperature);
            }

            if (metadata.Camera.Binning == string.Empty) {
                p.Set(ImagePatternKeys.Binning, "1x1");
            } else {
                p.Set(ImagePatternKeys.Binning, metadata.Camera.Binning);
            }

            if (!double.IsNaN(metadata.Camera.Temperature)) {
                p.Set(ImagePatternKeys.SensorTemp, metadata.Camera.Temperature);
            }

            if (metadata.Camera.Gain >= 0) {
                p.Set(ImagePatternKeys.Gain, metadata.Camera.Gain);
            }

            if (metadata.Camera.Offset >= 0) {
                p.Set(ImagePatternKeys.Offset, metadata.Camera.Offset);
            }

            //zhang105
            //if (!double.IsNaN(this.StarDetectionAnalysis.HFR)) {
            //    p.Set(ImagePatternKeys.HFR, this.StarDetectionAnalysis.HFR);
            //}

            if (!double.IsNaN(metadata.WeatherData.SkyQuality)) {
                p.Set(ImagePatternKeys.SQM, metadata.WeatherData.SkyQuality);
            }

            if (!string.IsNullOrEmpty(metadata.Camera.ReadoutModeName)) {
                p.Set(ImagePatternKeys.ReadoutMode, metadata.Camera.ReadoutModeName);
            }

            return p;
        }

        public async Task<string> SaveToDisk(FileSaveInfo fileSaveInfo, CancellationToken token, bool forceFileType = false) {
            string actualPath = string.Empty;
            try {
                using (MyStopWatch.Measure()) {
                    string tempPath = await SaveToDiskAsync(fileSaveInfo, token, forceFileType);
                    actualPath = FinalizeSave(tempPath, fileSaveInfo.FilePattern);
                }
            } catch (OperationCanceledException ex) {
                throw ex;
            } catch (Exception ex) {
                Logger.Error(ex);
                throw ex;
            } finally {
            }
            return actualPath;
        }

        private Task<string> SaveToDiskAsync(FileSaveInfo fileSaveInfo, CancellationToken cancelToken, bool forceFileType = false) {
            return Task.Run(() => {
                string path = string.Empty;
                fileSaveInfo.FilePath = Path.Combine(fileSaveInfo.FilePath, Guid.NewGuid().ToString());

                if (!forceFileType && Data.RAWData != null) {
                    fileSaveInfo.FileType = FileTypeEnum.RAW;
                    path = SaveRAW(fileSaveInfo.FilePath);
                } else {
                    switch (fileSaveInfo.FileType) {
                        case FileTypeEnum.FITS:
                            path = SaveFits(fileSaveInfo);
                            break;

                        case FileTypeEnum.XISF:
                            path = SaveXisf(fileSaveInfo);
                            break;

                        case FileTypeEnum.TIFF:
                        default:
                            path = SaveTiff(fileSaveInfo);
                            break;
                    }
                }

                return path;
            }, cancelToken);
        }

        private string SaveRAW(string path) {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            IImageArray data = Data;
            string uniquePath = Utility.Utility.GetUniqueFilePath(path + "." + data.RAWType);
            File.WriteAllBytes(uniquePath, data.RAWData);
            return uniquePath;
        }

        private string SaveTiff(FileSaveInfo fileSaveInfo) {
            Directory.CreateDirectory(Path.GetDirectoryName(fileSaveInfo.FilePath));
            string uniquePath = Utility.Utility.GetUniqueFilePath(fileSaveInfo.FilePath + ".tif");

            using (FileStream fs = new FileStream(uniquePath, FileMode.Create)) {
                TiffBitmapEncoder encoder = new TiffBitmapEncoder();

                switch (fileSaveInfo.TIFFCompressionType) {
                    case TIFFCompressionTypeEnum.LZW:
                        encoder.Compression = TiffCompressOption.Lzw;
                        break;

                    case TIFFCompressionTypeEnum.ZIP:
                        encoder.Compression = TiffCompressOption.Zip;
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(RenderBitmapSource()));
                encoder.Save(fs);
            }

            return uniquePath;
        }

        private string SaveFits(FileSaveInfo fileSaveInfo) {
            FITS f = new FITS(
                Data.FlatArray,
                Properties.Width,
                Properties.Height
            );

            f.PopulateHeaderCards(MetaData);

            Directory.CreateDirectory(Path.GetDirectoryName(fileSaveInfo.FilePath));
            string uniquePath = Utility.Utility.GetUniqueFilePath(fileSaveInfo.FilePath + ".fits");

            using (FileStream fs = new FileStream(uniquePath, FileMode.Create)) {
                f.Write(fs);
            }

            return uniquePath;
        }

        private string SaveXisf(FileSaveInfo fileSaveInfo) {
            XISFHeader header = new XISFHeader();

            header.AddImageMetaData(Properties, MetaData.Image.ImageType);

            header.Populate(MetaData);

            XISF img = new XISF(header);

            img.AddAttachedImage(Data.FlatArray, fileSaveInfo);

            Directory.CreateDirectory(Path.GetDirectoryName(fileSaveInfo.FilePath));
            string uniquePath = Utility.Utility.GetUniqueFilePath(fileSaveInfo.FilePath + ".xisf");

            using (FileStream fs = new FileStream(uniquePath, FileMode.Create)) {
                img.Save(fs);
            }

            return uniquePath;
        }

        #endregion "Save"

        #region "Load"

        /// <summary>
        /// Loads an image from a given file path
        /// </summary>
        /// <param name="path">File Path to image</param>
        /// <param name="bitDepth">bit depth of each pixel</param>
        /// <param name="isBayered">Flag to indicate if the image is bayer matrix encoded</param>
        /// <param name="rawConverter">Which type of raw converter to use, when image is in RAW format</param>
        /// <param name="ct">Token to cancel operation</param>
        /// <returns></returns>
        public static async Task<IImageData> FromFile(string path, int bitDepth, bool isBayered, RawConverterEnum rawConverter, CancellationToken ct = default(CancellationToken)) {
            if (!File.Exists(path)) {
                throw new FileNotFoundException();
            }
            BitmapDecoder decoder;
            switch (Path.GetExtension(path).ToLower()) {
                case ".gif":
                    decoder = new GifBitmapDecoder(new Uri(path), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    return await BitmapToImageArray(decoder, isBayered);

                case ".tif":
                case ".tiff":
                    decoder = new TiffBitmapDecoder(new Uri(path), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    return await BitmapToImageArray(decoder, isBayered);

                case ".jpg":
                case ".jpeg":
                    decoder = new JpegBitmapDecoder(new Uri(path), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    return await BitmapToImageArray(decoder, isBayered);

                case ".png":
                    decoder = new PngBitmapDecoder(new Uri(path), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    return await BitmapToImageArray(decoder, isBayered);

                case ".xisf":
                    return await XISF.Load(new Uri(path), isBayered);

                case ".fit":
                case ".fits":
                    return await FitsToImageArray(path, bitDepth, isBayered);

                case ".cr2":
                case ".nef":
                case ".raf":
                case ".raw":
                case ".pef":
                case ".dng":
                case ".arw":
                case ".orf":
                    return await RawToImageArray(path, bitDepth, rawConverter, ct);

                default:
                    throw new NotSupportedException();
            }
        }

        private static Task<IImageData> FitsToImageArray(string path, int bitDepth, bool isBayered) {
            return Task.Run(async () => {
                Fits f = new Fits(path);
                ImageHDU hdu = (ImageHDU)f.ReadHDU();
                Array[] arr = (Array[])hdu.Data.DataArray;

                var width = hdu.Header.GetIntValue("NAXIS1");
                var height = hdu.Header.GetIntValue("NAXIS2");
                ushort[] pixels = new ushort[width * height];
                var i = 0;
                foreach (var row in arr) {
                    foreach (short val in row) {
                        pixels[i++] = (ushort)(val + short.MaxValue);
                    }
                }
                // TODO: Add parser for ImageMetaData
                return await Task.FromResult<IImageData>(new ImageData(pixels, width, height, bitDepth, isBayered, new ImageMetaData()));
            });
        }

        private static async Task<IImageData> RawToImageArray(string path, int bitDepth, RawConverterEnum rawConverter, CancellationToken ct) {
            using (var fs = new FileStream(path, FileMode.Open)) {
                using (var ms = new System.IO.MemoryStream()) {
                    fs.CopyTo(ms);
                    var converter = RawConverter.CreateInstance(rawConverter);
                    var rawType = Path.GetExtension(path).ToLower().Substring(1);
                    var data = await converter.Convert(s: ms, bitDepth: bitDepth, rawType: rawType, metaData: new ImageMetaData(), token: ct);
                    return data;
                }
            }
        }

        private static async Task<IImageData> BitmapToImageArray(BitmapDecoder decoder, bool isBayered) {
            var bmp = new FormatConvertedBitmap();
            bmp.BeginInit();
            bmp.Source = decoder.Frames[0];
            bmp.DestinationFormat = System.Windows.Media.PixelFormats.Gray16;
            bmp.EndInit();

            int stride = (bmp.PixelWidth * bmp.Format.BitsPerPixel + 7) / 8;
            int arraySize = stride * bmp.PixelHeight;
            ushort[] pixels = new ushort[bmp.PixelWidth * bmp.PixelHeight];
            bmp.CopyPixels(pixels, stride, 0);
            return new ImageData(pixels, bmp.PixelWidth, bmp.PixelHeight, 16, isBayered, new ImageMetaData());
        }

        #endregion "Load"
    }
}