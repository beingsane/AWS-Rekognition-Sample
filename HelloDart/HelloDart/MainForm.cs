using System;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Linq;
using System.Globalization;
using System.ComponentModel;
using HelloDart.Components;
using HelloDart.Extensions;
using System.Threading.Tasks;

namespace HelloDart
{
    public partial class MainForm : Form
    {
        public DirectX.Capture.Filter Camera { get; set; }
        public DirectX.Capture.Capture CaptureInfo { get; set; }
        public DirectX.Capture.Filters CamContainer { get; set; }
        private SpeechSynthesizer _synthesizer;
        private readonly CultureInfo _defaultCulture = new CultureInfo("pt-BR");
        public System.Drawing.Image _image;

        public MainForm()
        {
            InitializeComponent();

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CamContainer = new DirectX.Capture.Filters();

            ConfigureSpeechSynthesizer();

            ConfigureWorkers();

            try
            {
                Camera = CamContainer.VideoInputDevices[0];
                CaptureInfo = new DirectX.Capture.Capture(Camera, null);
                CaptureInfo.PreviewWindow = this.picWebCam;
                CaptureInfo.FrameCaptureComplete += UpdateImageEvent;
                CaptureInfo.CaptureFrame();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var response = ImageComponent.IdentifyDartMembers(_image.ImageToByteArray());

            _image = null;

            pbSave.Invoke(new MethodInvoker(
            delegate ()
            {
                pbSave.Image = response;
            }));

            pbLoading.Invoke(new MethodInvoker(
            delegate ()
            {
               pbLoading.Visible = false;
            }));

            pbSearch.Invoke(new MethodInvoker(
            delegate ()
            {
                pbSearch.Visible = true;
            }));
        }

        private void pbSearch_Click(object sender, EventArgs e)
        {
            new Task(() =>
            {
                _synthesizer.SpeakAsync("Análisando ...");

            }).Start();

            pbSearch.Visible = false;
            pbLoading.Visible = true;
            Worker.RunWorkerAsync();
        }

        private void UpdateImageEvent(PictureBox frame)
        {
            try
            {
                _image = frame.Image;
                pbSave.Image = _image;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro " + ex.Message);
            }
        }

        private void ConfigureSpeechSynthesizer()
        {
            _synthesizer = new SpeechSynthesizer();

            var installedVoices = _synthesizer.GetInstalledVoices();

            var voice = installedVoices.Where(i => i.VoiceInfo.Culture.IetfLanguageTag == _defaultCulture.IetfLanguageTag).First();

            voice = voice ?? installedVoices.First();

            _synthesizer.SelectVoice(voice.VoiceInfo.Name);
        }

        private void ConfigureWorkers()
        {
            Worker.DoWork += Worker_DoWork;
        }

        private void pvShot_Click(object sender, EventArgs e)
        {
            CaptureInfo.CaptureFrame();
        }
    }
}
