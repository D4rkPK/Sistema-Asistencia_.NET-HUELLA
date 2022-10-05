using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Huella
{
    public partial class frmTest : CaptureForm
    {
        private DPFP.Template Template;
        private DPFP.Verification.Verification Verificator;
        private asistenciaEntities contexto;

        public void Verify(DPFP.Template template)
        {
            Template = template;
            ShowDialog();
        }

        protected override void Init()
        {
            base.Init();
            base.Text = "Verificación de Huella Digital";
            Verificator = new DPFP.Verification.Verification();     // Create a fingerprint template verificator
            UpdateStatus(0);
        }

        private void UpdateStatus(int FAR)
        {
            // Show "False accept rate" value
            SetStatus(String.Format("", FAR));
        }

        public static bool IsWithinTime(TimeSpan stringNowTime, TimeSpan stringStartTime)
        {

            var nowTime = stringNowTime;
            var startTime = stringStartTime;

            if ((nowTime > startTime))
            {
                return true;
            }

            return false;
        }

        protected override void Process(DPFP.Sample Sample)
        {
            base.Process(Sample);

            // Process the sample and create a feature set for the enrollment purpose.
            DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);

            // Check quality of the sample and start verification if it's good
            // TODO: move to a separate task
            if (features != null)
            {
                // Compare the feature set with our template
                DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();

                DPFP.Template template = new DPFP.Template();
                Stream stream;

                foreach (var emp in contexto.estudiante)
                {
                    stream = new MemoryStream(emp.huella);
                    template = new DPFP.Template(stream);

                    Verificator.Verify(features, template, ref result);
                    UpdateStatus(result.FARAchieved);
                    if (result.Verified)
                    {
                        MakeReport("LA HUELLA ES DE " + emp.nombre + " " + emp.apellido);        
                        break;
                    }else
                    {
                        MakeReport("HUELLA DESCONOCIDA");
                        break;
                    } 
                }
            }
        }

        public frmTest()
        {
            contexto = new asistenciaEntities();
            InitializeComponent();
        }

        private void frmTest_Load(object sender, EventArgs e)
        {

        }
    }
}
