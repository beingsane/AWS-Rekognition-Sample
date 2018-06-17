using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelloDart.Extensions;
using System.Collections.Concurrent;
using System.IO;
using Amazon.Rekognition.Model;
using System.Drawing.Imaging;

namespace HelloDart.Components
{
    public class ImageComponent
    {
        public static System.Drawing.Image IdentifyDartMembers(byte[] img)
        {
            var facesOutput = AWSComponent.Instance.IdentifyFaces(img);

            ConcurrentBag<BoundingBox> dartFaces = new ConcurrentBag<BoundingBox>();

            if (facesOutput.FaceDetails.Count > 0)
            {
                var dartMemberSource = GetAllDartMembers();

                foreach (var member in dartMemberSource)
                {
                    if (dartFaces.Count >= facesOutput.FaceDetails.Count)
                    {
                        break;
                    }

                    var response = AWSComponent.Instance.CompareFaces(member.ImageToByteArray(), img);

                    var facesRekognition = response.FaceMatches.Where(fm => fm.Similarity > 70).ToList();

                    facesRekognition.ForEach((item) =>
                    {
                        dartFaces.Add(item.Face.BoundingBox);
                    });
                }
            }
            return DrawDartMembers(img.ImageFromBytes(), dartFaces.ToList());
        }

        public static List<System.Drawing.Image> GetAllDartMembers()
        {
            String searchFolder = string.Format("{0}\\DartMembers", Directory.GetCurrentDirectory());
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            var files = GetFilesFrom(searchFolder, filters, false);

            List<System.Drawing.Image> images = new List<System.Drawing.Image>();

            foreach (var file in files)
            {
                images.Add(System.Drawing.Image.FromFile(file));
            }

            return images;
        }

        public static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }

        public static System.Drawing.Image DrawDartMembers(System.Drawing.Image source, List<BoundingBox> bondings)
        {
            if (bondings.Count > 0)
            {
                System.Drawing.Bitmap facesHighlighted = new Bitmap((System.Drawing.Image)source.Clone());
                Pen pen = new Pen(Color.Orange, 3);

                using (var graphics = Graphics.FromImage(facesHighlighted))
                {
                    foreach (var bb in bondings)
                    {
                        graphics.DrawRectangle(pen, x: facesHighlighted.Width * bb.Left,
                            y: facesHighlighted.Height * bb.Top,
                            width: facesHighlighted.Width * bb.Width,
                            height: facesHighlighted.Height * bb.Height);
                    }
                }
                return facesHighlighted;
            }
            return source;
        }
    }
}
