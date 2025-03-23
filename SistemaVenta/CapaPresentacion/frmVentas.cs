using CapaEntidad;
using CapaNegocio;
using CapaPresentacion.Modales;
using CapaPresentacion.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public partial class frmVentas: Form
    {
        private Usuario _Usuario;
        private Inicio formularioPrincipal;
        public frmVentas(Inicio formInicio,Usuario oUsuario = null)
        {
            _Usuario = oUsuario;
            InitializeComponent();
            formularioPrincipal = formInicio;
        }

        public static class ControladorPresentacion
        {
            public static Inicio FormularioInicio { get; set; }
        }

        private void frmVentas_Load(object sender, EventArgs e)
        {
            cboTipoDocumento.Items.Add(new OpcionCombo() { Valor = "Boleta", Texto = "Boleta" });
            cboTipoDocumento.Items.Add(new OpcionCombo() { Valor = "Factura", Texto = "Factura" });
            cboTipoDocumento.DisplayMember = "Texto";
            cboTipoDocumento.ValueMember = "Valor";
            cboTipoDocumento.SelectedIndex = 0;

            txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtIdProducto.Text = "0";

            txtPagarCon.Text = "";
            txtCambio.Text = "";
            txtTotalPagar.Text = "0";
        }

        private void btnBuscarCliente_Click(object sender, EventArgs e)
        {
            using (var modal = new mdCliente())
            {
                var result = modal.ShowDialog();

                if (result == DialogResult.OK)
                {
                    txtDocCliente.Text = modal._Cliente.Documento;
                    txtNombreCliente.Text = modal._Cliente.NombreCompleto;
                    txtCodProducto.Select();
                }

                else
                {
                    txtDocCliente.Select();
                }
            }
        }

        private void btnBuscarProducto_Click(object sender, EventArgs e)
        {
            using (var modal = new mdProducto())
            {
                var result = modal.ShowDialog();

                if (result == DialogResult.OK)
                {
                    txtIdProducto.Text = modal._Producto.IdProducto.ToString();
                    txtCodProducto.Text = modal._Producto.Codigo;
                    txtProducto.Text = modal._Producto.Nombre;
                    txtPrecio.Text = modal._Producto.PrecioVenta.ToString("0.00");
                    txtStock.Text = modal._Producto.Stock.ToString();
                    txtCantidad.Select();
                }

                else
                {
                    txtCodProducto.Select();
                }
            }
        }

        private void txtCodProducto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                Producto oProducto = new CN_Producto().Listar().Where(p => p.Codigo == txtCodProducto.Text && p.Estado == true).FirstOrDefault();

                if (oProducto != null)
                {
                    txtCodProducto.BackColor = Color.Honeydew;
                    txtIdProducto.Text = oProducto.IdProducto.ToString();
                    txtProducto.Text = oProducto.Nombre;
                    txtPrecio.Text = oProducto.PrecioVenta.ToString("0.00");
                    txtStock.Text = oProducto.Stock.ToString();
                    txtCantidad.Select();
                }

                else
                {
                    txtCodProducto.BackColor = Color.MistyRose;
                    txtIdProducto.Text = "0";
                    txtProducto.Text = "";
                    txtPrecio.Text = "";
                    txtStock.Text = "";
                    txtCantidad.Value = 1;
                }
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            decimal precio= 0;

            if (int.Parse(txtIdProducto.Text) == 0)
            {
                MessageBox.Show("Seleccione un producto", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!decimal.TryParse(txtPrecio.Text, out precio))
            {
                MessageBox.Show("Ingrese un valor de precio válido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (Convert.ToInt32(txtStock.Text) < Convert.ToInt32(txtCantidad.Value.ToString()))
            {
                MessageBox.Show("La cantidad no puede ser mayor al stock", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            bool respuesta = new CN_Venta().RestarStock(
                Convert.ToInt32(txtIdProducto.Text),
                Convert.ToInt32(txtCantidad.Value.ToString())
                );

            if (respuesta)
            {
                dgvData.Rows.Add(new object[] {
                txtIdProducto.Text,
                txtProducto.Text,
                precio.ToString("0.00"),
                txtCantidad.Value.ToString(),
                (txtCantidad.Value * precio).ToString("0.00")
            });
            }

            CalcularTotal();
            LimpiarProducto();
            txtCodProducto.Select();
            VerificarDatosEnDataGridView();
        }

        public void VerificarDatosEnDataGridView()
        {

            if (dgvData.Rows.Count > 0)
            {
                formularioPrincipal.MiMenuStrip.Items["menuUsuarios"].Enabled = false;
                formularioPrincipal.MiMenuStrip.Items["menuMantenedor"].Enabled = false;
                formularioPrincipal.MiMenuStrip.Items["menuVentas"].Enabled = false;
                formularioPrincipal.MiMenuStrip.Items["menuCompras"].Enabled = false;
                formularioPrincipal.MiMenuStrip.Items["menuClientes"].Enabled = false;
                formularioPrincipal.MiMenuStrip.Items["menuProveedores"].Enabled = false;
                formularioPrincipal.MiMenuStrip.Items["menuReportes"].Enabled = false;
                formularioPrincipal.MiMenuStrip.Items["menuAcercaDe"].Enabled = false;
            }
            
            else
            {
                formularioPrincipal.MiMenuStrip.Items["menuUsuarios"].Enabled = true;
                formularioPrincipal.MiMenuStrip.Items["menuMantenedor"].Enabled = true;
                formularioPrincipal.MiMenuStrip.Items["menuVentas"].Enabled = true;
                formularioPrincipal.MiMenuStrip.Items["menuCompras"].Enabled = true;
                formularioPrincipal.MiMenuStrip.Items["menuClientes"].Enabled = true;
                formularioPrincipal.MiMenuStrip.Items["menuProveedores"].Enabled = true;
                formularioPrincipal.MiMenuStrip.Items["menuReportes"].Enabled = true;
                formularioPrincipal.MiMenuStrip.Items["menuAcercaDe"].Enabled = true;
            }
        }

        private void LimpiarProducto()
        {
            txtIdProducto.Text = "0";
            txtCodProducto.Text = "";
            txtProducto.Text = "";
            txtPrecio.Text = "";
            txtStock.Text = "";
            txtCantidad.Value = 1;
        }

        private void CalcularTotal()
        {
            decimal total = 0;
            if (dgvData.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvData.Rows)
                {
                    total += Convert.ToDecimal(row.Cells["SubTotal"].Value.ToString());
                }
            }

            txtTotalPagar.Text = total.ToString("0.00");
        }

        private void dgvData_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (e.ColumnIndex == 5)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                var w = Properties.Resources.delete25.Width;
                var h = Properties.Resources.delete25.Height;
                var x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
                var y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

                e.Graphics.DrawImage(Properties.Resources.delete25, new Rectangle(x, y, w, h));
                e.Handled = true;
            }
        }

        private void dgvData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvData.Columns[e.ColumnIndex].Name == "btnEliminar")
            {
                int indice = e.RowIndex;
                if (indice >= 0)
                {
                    bool respuesta = new CN_Venta().SumarStock(
                            Convert.ToInt32(dgvData.Rows[indice].Cells["IdProducto"].Value.ToString()),
                            Convert.ToInt32(dgvData.Rows[indice].Cells["Cantidad"].Value.ToString())
                    );

                    if (respuesta)
                    {
                        if (MessageBox.Show("¿Está seguro de eliminar el producto de la lista?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            string idProducto = dgvData.Rows[indice].Cells["IdProducto"].Value.ToString();
                            dgvData.Rows.RemoveAt(indice);
                            CalcularTotal();
                            txtPagarCon.Select();

                            if (txtIdProducto.Text == idProducto)
                            {
                                txtStock.Text = new CN_Producto().Listar().Where(p => p.IdProducto == Convert.ToInt32(txtIdProducto.Text)).FirstOrDefault().Stock.ToString();
                            }
                        }
                    }
                    
                    else
                    {
                        MessageBox.Show("No se pudo sumar el stock del producto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            VerificarDatosEnDataGridView();
        }

        private void txtPrecio_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (txtPrecio.Text.Trim().Length == 0 && e.KeyChar.ToString() == ",")
            {
                e.Handled = true;
            }
            else if (Char.IsControl(e.KeyChar) || e.KeyChar.ToString() == ",")
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtPagarCon_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (txtPagarCon.Text.Trim().Length == 0 && e.KeyChar.ToString() == ",")
            {
                e.Handled = true;
            }
            else if (Char.IsControl(e.KeyChar) || e.KeyChar.ToString() == ",")
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void CalcularCambio()
        {
            if (txtTotalPagar.Text.Trim() == "")
            {
                MessageBox.Show("No existen productos en la venta", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            decimal pagarCon;
            decimal total = Convert.ToDecimal(txtTotalPagar.Text);

            if (txtPagarCon.Text.Trim() == "")
            {
                txtPagarCon.Text = "0";
            }

            if (decimal.TryParse(txtPagarCon.Text.Trim(), out pagarCon))
            {
                if (pagarCon < total)
                {
                    MessageBox.Show("El monto a pagar no puede ser menor al total", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    txtPagarCon.Select();
                }

                else
                {
                    decimal cambio = pagarCon - total;
                    txtCambio.Text = cambio.ToString("0.00");
                }  
            }
            
            else
            {
                MessageBox.Show("Ingrese un valor de pago válido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void txtPagarCon_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                CalcularCambio();
            }
        }

        private void btnCrearVenta_Click(object sender, EventArgs e)
        {
            //Condicional solo si el negocio realiza ventas únicamente con clientes registrados
            if (txtDocCliente.Text == "")
            {
                MessageBox.Show("Debe ingresar el documento del cliente", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            //Condicional solo si el negocio realiza ventas únicamente con clientes registrados
            if (txtNombreCliente.Text == "")
            {
                MessageBox.Show("Debe ingresar el nombre del cliente", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (txtPagarCon.Text == "")
            {
                MessageBox.Show("Debe ingresar el precio de pago", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (dgvData.Rows.Count < 1)
            {
                MessageBox.Show("Debe agregar productos a la venta", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DataTable detalle_venta = new DataTable();
            
            detalle_venta.Columns.Add("IdProducto", typeof(int));
            detalle_venta.Columns.Add("PrecioVenta", typeof(decimal));
            detalle_venta.Columns.Add("Cantidad", typeof(int));
            detalle_venta.Columns.Add("SubTotal", typeof(decimal));

            foreach (DataGridViewRow fila in dgvData.Rows)
            {
                detalle_venta.Rows.Add(
                    new object[] {
                        Convert.ToInt32(fila.Cells["IdProducto"].Value.ToString()),
                        fila.Cells["Precio"].Value.ToString(),
                        fila.Cells["Cantidad"].Value.ToString(),
                        fila.Cells["SubTotal"].Value.ToString()
                    }
                );
            }

            int idcorrelativo = new CN_Venta().ObtenerCorrelativo();
            string numerodocumento = string.Format("{0:00000}", idcorrelativo);
            CalcularCambio();

            Venta oVenta = new Venta()
            {
                oUsuario = new Usuario() { IdUsuario = _Usuario.IdUsuario },
                TipoDocumento = ((OpcionCombo)cboTipoDocumento.SelectedItem).Texto,
                NumeroDocumento = numerodocumento,
                DocumentoCliente = txtDocCliente.Text,
                NombreCliente = txtNombreCliente.Text,
                MontoPago = Convert.ToDecimal(txtPagarCon.Text),
                MontoCambio = Convert.ToDecimal(txtCambio.Text),                
                MontoTotal = Convert.ToDecimal(txtTotalPagar.Text)
            };

            string mensaje = string.Empty;
            bool respuesta = new CN_Venta().Registrar(oVenta, detalle_venta, out mensaje);

            if (respuesta)
            {
                var result = MessageBox.Show("Numero de venta generada:\n" + numerodocumento + "\n\n¿Desea copiar al portapapeles?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    Clipboard.SetText(numerodocumento);
                }

                txtDocCliente.Text = "";
                txtNombreCliente.Text = "";
                dgvData.Rows.Clear();
                CalcularTotal();
                txtPagarCon.Text = "";
                txtCambio.Text = "";
            }
            else
            {
                MessageBox.Show(mensaje, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            VerificarDatosEnDataGridView();
        }
    }
}
