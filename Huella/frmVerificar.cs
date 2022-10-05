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
    public partial class frmVerificar : CaptureForm
    {
        int individuo = 0;
        int cont = 0;
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
            string Fecha = DateTime.Now.ToString("dd-MM-yyyy");
            string Hora = DateTime.Now.ToString("hh:mm:ss tt");
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
                        using (asistenciaEntities contexto = new asistenciaEntities())
                        {
                            var data = contexto.horario_asignado.Where(x => x.estudiante_id == emp.id).FirstOrDefault();
                            var estadoRegistro = contexto.registro.Where(x => x.horario_asignado_id == data.id && x.tipo_registro == 2).FirstOrDefault();
                            var horario = contexto.horario.Where(x => x.id == data.horario_id).FirstOrDefault();

                            if (estadoRegistro == null)
                            {
                                cont = 0;
                                individuo = emp.id;
                                int estadoHorario = -2;

                                if (IsWithinTime(horario.hora_entrada, DateTime.Now.TimeOfDay))
                                {
                                    MakeReport("ENTRADA " + emp.nombre + " " + emp.apellido + " " + Fecha + " " + Hora  + "A tiempo");
                                    estadoHorario = 1;
                                }
                                else
                                {
                                    MakeReport("ENTRADA " + emp.nombre + " " + emp.apellido + " " + Fecha + " " + Hora  + "TARDE");
                                    estadoHorario = -2;
                                }

                                registro reg = new registro()
                                {
                                    horario_asignado_id = data.id,
                                    tipo_registro = 2,
                                    fecha = DateTime.Now,
                                    entrada = DateTime.Now.TimeOfDay,
                                    salida = null,
                                    estado = estadoHorario,
                                };
                                contexto.registro.Add(reg);
                                contexto.SaveChanges();
                                break;
                            }
                            else
                            {
                                if (cont == 0 && individuo == emp.id)
                                {
                                    MakeReport("ESTA MARCANDO SU SALIDA  " + emp.nombre + " " + emp.apellido + " " + Fecha + " " + Hora + "\nPARA CONFIRMAR VUELVA A COLOCAR SU DEDO");
                                    cont = 1;
                                    break;
                                }
                                else if (cont == 1 && individuo == emp.id)
                                {
                                    MakeReport("SALIDA  " + emp.nombre + " " + emp.apellido + " " + Fecha + " " + Hora);

                                    var regg = contexto.registro.SingleOrDefault(b => b.id == estadoRegistro.id);

                                    if(regg != null)
                                    {
                                        regg.tipo_registro = 1;
                                        regg.salida = DateTime.Now.TimeOfDay;
                                        contexto.SaveChanges();
                                        estadoRegistro = null;
                                        cont = 0;
                                        break;
                                    }
                                }else
                                {
                                    individuo = emp.id;
                                    cont = 0;
                                    break;
                                }
                            }
                        }

                    }else
                    {
                        MakeReport("HUELLA DESCONOCIDA");
                        break;
                    }
                    
                }
            }
        }



        public frmVerificar()
        {
            contexto = new asistenciaEntities();
            InitializeComponent();
        }
    }
}
