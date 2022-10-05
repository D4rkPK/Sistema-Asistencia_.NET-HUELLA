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
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

            try
            {
                using (asistenciaEntities contexto = new asistenciaEntities())
                {
                    var practicante = contexto.temp_estudiante.First();

                    var data = contexto.estudiante.Where(x => x.id == practicante.estudiante_id).FirstOrDefault();
                    
                    if (data == null)
                    {
                        
                    }
                    else
                    {
                        frmRegistrar registrar = new frmRegistrar();
                        registrar.ShowDialog();
                    }
                }
            }
            catch (Exception e)
            {
                frmVerificar verificar = new frmVerificar();
                verificar.ShowDialog();
                
            }
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            frmTest test = new frmTest();
            test.ShowDialog();
        }

        private void btnMarcaje_Click(object sender, EventArgs e)
        {
            frmVerificar verificar = new frmVerificar();
            verificar.ShowDialog();
        }
    }
}
