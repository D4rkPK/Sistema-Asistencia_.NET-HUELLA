using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Huella
{
    public partial class frmRegistrar : Form
    {
        private DPFP.Template Template;

        public frmRegistrar()
        {
            InitializeComponent();

            try
            {
                using (asistenciaEntities contexto = new asistenciaEntities())
                {
                    var practicante = contexto.temp_estudiante.First();

                    var data = contexto.estudiante.Where(x => x.id == practicante.estudiante_id).FirstOrDefault();

                    txtNombre.Text = data.nombre + " " + data.apellido;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void btnRegistrarHuella_Click(object sender, EventArgs e)
        {
            CapturarHuella capturar = new CapturarHuella();
            capturar.OnTemplate += this.OnTemplate;
            capturar.ShowDialog();
        }

        private void OnTemplate(DPFP.Template template)
        {
            this.Invoke(new Function(delegate ()
            {
                Template = template;
                btnAgregar.Enabled = (Template != null);
                if (Template != null)
                {
                    MessageBox.Show("La plantilla de huellas dactilares está lista para la verificación de huellas dactilares.", "Fingerprint Enrollment");
                    txtHuella.Text = "Huella capturada correctamente";
                }
                else
                {
                    MessageBox.Show("La plantilla de huellas dactilares no es válida. Repita el registro de huellas dactilares.", "Fingerprint Enrollment");
                }
            }));
        }

        private void frmRegistrar_Load(object sender, EventArgs e)
        {
            Listar();
        }

        private void Limpiar()
        {
            txtNombre.Text = "";
        }

        private void Listar()
        {
            try
            {
                using (asistenciaEntities contexto = new asistenciaEntities())
                {
                    var estudiantes = from db in contexto.estudiante
                                      select new
                                      {
                                          ID = db.id,
                                          ESTUDIANTE = db.nombre
                                      };
                    if (estudiantes != null)
                    {
                        dgvListar.DataSource = estudiantes.ToList();
                    }
                }
            }
            catch(Exception e)
            {

                MessageBox.Show(e.Message);
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] streamHuella = Template.Bytes;

                var data = (dynamic)null;
                using (asistenciaEntities contexto = new asistenciaEntities())
                {
                    var practicante = contexto.temp_estudiante.First();

                    data = contexto.estudiante.Where(x => x.id == practicante.estudiante_id).FirstOrDefault();

                }

                estudiante estu = new estudiante()
                {
                    id = data.id,
                    nombre = data.nombre,
                    apellido = data.apellido,
                    correo = data.correo,
                    cui = data.cui,
                    carne = data.carne,
                    area_id = data.area_id,
                    universidad_id = data.universidad_id,
                    estado_huella = true,
                    huella = streamHuella
                };

                using (asistenciaEntities contexto = new asistenciaEntities())
                {
                    contexto.Entry(estu).State = System.Data.EntityState.Modified;
                    contexto.SaveChanges();
                }


                MessageBox.Show("Registro agregado a la BD correctamente");
                Limpiar();
                Listar();
                Template = null;
                btnAgregar.Enabled = false;


            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
    }
}
