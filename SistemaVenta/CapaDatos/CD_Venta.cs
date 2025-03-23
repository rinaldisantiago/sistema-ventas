﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaEntidad;

namespace CapaDatos
{
    public class CD_Venta
    {
        public int ObtenerCorrelativo()
        {
            int idcorrelativo = 0;

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {

                try
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select count(*) + 1 from Venta");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    idcorrelativo = Convert.ToInt32(cmd.ExecuteScalar());

                }

                catch (Exception ex)
                {
                    idcorrelativo = 0;
                }
            }

            return idcorrelativo;
        }

        public bool RestarStock(int IdProducto, int Cantidad)
        {
            bool respuesta = true;

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {

                try
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("update producto set Stock = Stock - @Cantidad where IdProducto = @IdProducto");
                    
                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@Cantidad", Cantidad);
                    cmd.Parameters.AddWithValue("@IdProducto", IdProducto);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    respuesta = cmd.ExecuteNonQuery() > 0 ? true : false;

                }

                catch (Exception ex)
                {
                    respuesta = false;
                }
            }

            return respuesta;
        }

        public bool SumarStock(int IdProducto, int Cantidad)
        {
            bool respuesta = true;

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {

                try
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("update producto set Stock = Stock + @Cantidad where IdProducto = @IdProducto");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@Cantidad", Cantidad);
                    cmd.Parameters.AddWithValue("@IdProducto", IdProducto);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    respuesta = cmd.ExecuteNonQuery() > 0 ? true : false;

                }

                catch (Exception ex)
                {
                    respuesta = false;
                }
            }

            return respuesta;
        }

        public bool Registrar(Venta obj, DataTable DetalleVenta, out string Mensaje)
        {
            bool respuesta = false;
            Mensaje = "";

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("SP_RegistrarVenta", oconexion);
                    cmd.Parameters.AddWithValue("IdUsuario", obj.oUsuario.IdUsuario);
                    cmd.Parameters.AddWithValue("TipoDocumento", obj.TipoDocumento);
                    cmd.Parameters.AddWithValue("NumeroDocumento", obj.NumeroDocumento);
                    cmd.Parameters.AddWithValue("DocumentoCliente", obj.DocumentoCliente);
                    cmd.Parameters.AddWithValue("NombreCliente", obj.NombreCliente);
                    cmd.Parameters.AddWithValue("MontoPago", obj.MontoPago);
                    cmd.Parameters.AddWithValue("MontoCambio", obj.MontoCambio);
                    cmd.Parameters.AddWithValue("MontoTotal", obj.MontoTotal);
                    cmd.Parameters.AddWithValue("DetalleVenta", DetalleVenta);
                    cmd.Parameters.Add("Resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    respuesta = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
                    Mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }

                catch (Exception ex)
                {
                    respuesta = false;
                    Mensaje = ex.Message;
                }
            }

            return respuesta;
        }

        public Venta ObtenerVenta(string numero)
        {
            Venta obj = new Venta();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select v.IdVenta, u.NombreCompleto,");
                    query.AppendLine("v.DocumentoCliente, v.NombreCliente,");
                    query.AppendLine("v.TipoDocumento, v.NumeroDocumento,");
                    query.AppendLine("v.MontoPago, v.MontoCambio, v.MontoTotal,");
                    query.AppendLine("convert(char(10),v.FechaRegistro,103)[FechaRegistro]");
                    query.AppendLine("from Venta v");
                    query.AppendLine("inner join Usuario u on u.IdUsuario = v.IdUsuario");
                    query.AppendLine("where v.NumeroDocumento = @numero");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        while (dr.Read())
                        {
                            obj = new Venta()
                            {
                                IdVenta = int.Parse(dr["IdVenta"].ToString()),
                                oUsuario = new Usuario() { NombreCompleto = dr["NombreCompleto"].ToString() },
                                DocumentoCliente = dr["DocumentoCliente"].ToString(),
                                NombreCliente = dr["NombreCliente"].ToString(),
                                TipoDocumento = dr["TipoDocumento"].ToString(),
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),
                                MontoPago = Convert.ToDecimal(dr["MontoPago"].ToString()),
                                MontoCambio = Convert.ToDecimal(dr["MontoCambio"].ToString()),
                                MontoTotal = Convert.ToDecimal(dr["MontoTotal"].ToString()),
                                FechaRegistro = dr["FechaRegistro"].ToString()
                            };
                        }
                    }
                }

                catch
                {
                    obj = new Venta();
                }
            }
            return obj;
        }

        public List<Detalle_Venta> ObtenerDetalleVenta(int idventa)
        {
            List<Detalle_Venta> oLista = new List<Detalle_Venta>();
            {
                try
                {
                    using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                    {
                        conexion.Open();
                        StringBuilder query = new StringBuilder();

                        query.AppendLine("select p.Nombre, dv.PrecioVenta, dv.Cantidad, dv.SubTotal from DetalleVenta dv");
                        query.AppendLine("inner join Producto p on p.IdProducto = dv.IdProducto");
                        query.AppendLine("where dv.IdVenta = @idventa");

                        SqlCommand cmd = new SqlCommand(query.ToString(), conexion);
                        cmd.Parameters.AddWithValue("@idventa", idventa);
                        cmd.CommandType = System.Data.CommandType.Text;

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                oLista.Add(new Detalle_Venta()
                                {
                                    oProducto = new Producto() { Nombre = dr["Nombre"].ToString() },
                                    PrecioVenta = Convert.ToDecimal(dr["PrecioVenta"].ToString()),
                                    Cantidad = Convert.ToInt32(dr["Cantidad"].ToString()),
                                    SubTotal = Convert.ToDecimal(dr["SubTotal"].ToString())
                                });
                            }
                        }
                    }
                }

                catch
                {
                    oLista = new List<Detalle_Venta>();
                }
            }
            return oLista;
        }
    }
}
