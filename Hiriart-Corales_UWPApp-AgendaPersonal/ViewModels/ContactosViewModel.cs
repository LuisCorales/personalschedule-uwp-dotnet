using System;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Hiriart_Corales_UWPApp_AgendaPersonal.Core.Models;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using System.Collections.Generic;

namespace Hiriart_Corales_UWPApp_AgendaPersonal.ViewModels
{
    public class ContactosViewModel : ObservableObject
    {
        public ContactosViewModel()
        {
        }

        //El siguiente metodo es static para poder llamarlo sin instanciar, también se le prodía colocar en una librería de funciones para SQL
        public static ObservableCollection<Contacto> ReadContactos(string connectionString)//Metodo para recuperar datos
        {
            const string GetContactosQuery = "select ContactoID, Nombre, Apellido, Telefono, Email, Organizacion, " +//Definicion de lo que queremos de Contacto
                "FechaNacimiento, InformacionAdicional from Contactoes";

            var contactos = new ObservableCollection<Contacto>();//Coleccion de contactos para almacenar las entradas de la tabla
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetContactosQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var contacto = new Contacto();//Una instancia de artistas para ir guardando y almacenando lo que se lea de la base
                                    contacto.ContactoID = reader.GetInt32(0);//El parametro dentro de estos gets indica la posicion del atributo dentro de la tabla
                                    //se usan castings con el reader[numeroColumna] para que los null se creen solos al leer
                                    contacto.Nombre = reader[1] as string;
                                    contacto.Apellido = reader[2] as string;
                                    contacto.Telefono = reader[3] as string;
                                    contacto.Email = reader[4] as string;
                                    contacto.Organizacion = reader[5] as string;
                                    contacto.FechaNacimiento = reader.GetDateTime(6);
                                    contacto.InformacionAdicional = reader[7] as string;

                                    contactos.Add(contacto);//Aniade el contacto que se creo antes a la coleccion
                                }
                            }
                        }
                    }
                }
                return contactos;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        public static bool CreateContacto(string connectionString, string nombre, string apellido, string telefono, string email,
            string organizacion, DateTimeOffset fecha, string informacionAdicional)
        {
            DateTime fechaNacimiento = fecha.Date;//transformar de DateTiemOffset a DateTime

            //Primero insertar una nueva entrada de Contactos
            const string insertarDiario = "insert into Contactoes(Nombre, Apellido, Telefono, Email, Organizacion, FechaNacimiento, InformacionAdicional) " +
                "values(@nombre, @apellido, @telefono, @email, @organizacion, @fechaNac, @info)";
            //Este tipo de sentencia con @Valor, permite llenar esos parametros luego, es más seguro porque evita inyección SQL, mejor acostumbarse a usar, Atte. Diego

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = insertarDiario;
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.Parameters.AddWithValue("@apellido", apellido);
                            cmd.Parameters.AddWithValue("@telefono", telefono);
                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.Parameters.AddWithValue("@organizacion", organizacion);
                            cmd.Parameters.AddWithValue("@fechaNac", fechaNacimiento);
                            cmd.Parameters.AddWithValue("@info", informacionAdicional);
                            cmd.ExecuteNonQuery();

                            //Crear nueva entrada en ListaContactoes
                            const string entradaLista = "insert into ListaContactoes(IDEvento, NombreApellido) " +
                                "values(NULL, @nombreApellido)";
                            cmd.CommandText = entradaLista;
                            string nombreAp = nombre + " " + apellido;
                            cmd.Parameters.AddWithValue("@nombreApellido", nombreAp);
                            cmd.ExecuteNonQuery();
                        }
                    }

                }
                return true;//Retorna true si es que se pudo ingesar el dato
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return false;//Retorna false si algo paso, asi no se borra nada en los TextBox
            }
        }

        public static bool DeleteContacto(string connectionString, int id)
        {
            const string borrar = "delete from Contactoes where ContactoID=@ID";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = borrar;
                            cmd.Parameters.AddWithValue("@ID", id);
                            cmd.ExecuteNonQuery();

                            //Borrar tambien la entrada de la lista
                            const string borrarLista = "delete from ListaContactoes where ListaContactoID=@idLista";
                            cmd.CommandText = borrarLista;
                            cmd.Parameters.AddWithValue("@idLista", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;//Retornar true para notificar borrado exitoso
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return false;//Retorna false si no se borro para notificar
            }
        }

        public static bool UpdateContacto(string connectionString, int ContactoID, string nombre, string apellido, string telefono, string email,
            string organizacion, DateTimeOffset fecha, string informacionAdicional)
        {
            DateTime fechaNacimiento = fecha.Date;//transformar de DateTimeOffset a DateTime

            const string editarDiario = "update Contactoes set Nombre=@nombre, Apellido=@apellido, Telefono=@telefono, Email=@email, " +
                "Organizacion=@organizacion, FechaNacimiento=@fechaNac, InformacionAdicional=@info where ContactoID=@id";

            try
            {
                using (SqlConnection conexion = new SqlConnection(connectionString))
                {
                    conexion.Open();
                    if (conexion.State == ConnectionState.Open)
                    {
                        using (SqlCommand consola = conexion.CreateCommand())
                        {
                            consola.CommandText = editarDiario;
                            consola.Parameters.AddWithValue("@nombre", nombre);
                            consola.Parameters.AddWithValue("@apellido", apellido);
                            consola.Parameters.AddWithValue("@telefono", telefono);
                            consola.Parameters.AddWithValue("@email", email);
                            consola.Parameters.AddWithValue("@organizacion", organizacion);
                            consola.Parameters.AddWithValue("@fechaNac", fechaNacimiento);
                            consola.Parameters.AddWithValue("@info", informacionAdicional);
                            consola.Parameters.AddWithValue("@id", ContactoID);
                            consola.ExecuteNonQuery();

                            //Actualizar la entrada en al tabla de lista
                            const string actualizarLista = "update ListaContactoes set NombreApellido=@nombreApellido where ListaContactoID=@idLista";
                            consola.CommandText = actualizarLista;
                            string nombreAp = nombre + " " + apellido;
                            consola.Parameters.AddWithValue("@nombreApellido", nombreAp);
                            consola.Parameters.AddWithValue("@idLista", ContactoID);
                            consola.ExecuteNonQuery();
                        }
                    }
                }
                return true;//Notficar edicion exitosa
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return false;//Avisar que no se pudo actualizar
            }
        }
    }
}
