using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime;
using System.Drawing;

namespace HelloDart.Components
{
    public class AWSComponent
    {
        private AWSComponent()
        {
            this.Load();
        }

        private AWSCredentials _credentials;

        private static AWSComponent _instance = null;

        public static AWSComponent Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AWSComponent();
                }

                return _instance;
            }
        }

        private void Load()
        {
            using (StreamReader r = new StreamReader("amazonconfig.json"))
            {
                string json = r.ReadToEnd();
                _credentials = JsonConvert.DeserializeObject<AWSCredentials>(json);
            }
        }

        public DetectFacesResponse IdentifyFaces(byte[] request)
        {
            AmazonRekognitionClient rekoClient = new AmazonRekognitionClient(_credentials, Amazon.RegionEndpoint.USEast2);

            DetectFacesRequest dfr = new DetectFacesRequest();

            Amazon.Rekognition.Model.Image img = new Amazon.Rekognition.Model.Image();

            img.Bytes = new MemoryStream(request);
            dfr.Image = img;

            return rekoClient.DetectFaces(dfr);
        }

        public CompareFacesResponse CompareFaces(byte[] source, byte[] target)
        {
            AmazonRekognitionClient rekoClient = new AmazonRekognitionClient(_credentials, Amazon.RegionEndpoint.USWest2);

            CompareFacesRequest cfr = new CompareFacesRequest();

            Amazon.Rekognition.Model.Image sourceImage = new Amazon.Rekognition.Model.Image();
            Amazon.Rekognition.Model.Image targetImage = new Amazon.Rekognition.Model.Image();

            var sourceStream = new MemoryStream(source);
            var targetStream = new MemoryStream(target);

            sourceImage.Bytes = sourceStream;
            targetImage.Bytes = targetStream;

            cfr.SourceImage = sourceImage;
            cfr.TargetImage = targetImage;

            return rekoClient.CompareFaces(cfr);
        }
    }


    public class AWSCredentials: Amazon.Runtime.AWSCredentials
    {
        [JsonProperty(PropertyName = "Username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "Password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "Region")]
        public string Region { get; set; }

        public override ImmutableCredentials GetCredentials()
        {
           return new ImmutableCredentials(this.Username, this.Password, null);
        }
    }
}
