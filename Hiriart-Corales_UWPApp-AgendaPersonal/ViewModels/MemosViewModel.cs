using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Hiriart_Corales_UWPApp_AgendaPersonal.Core.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Hiriart_Corales_UWPApp_AgendaPersonal.ViewModels
{
    public class MemosViewModel : ObservableObject
    {
        public MemosViewModel()
        {
        }

        public static ObservableCollection<Memo> ReadMemos(string connectionString)//Metodo para recuperar datos
        {
            const string GetMemosQuery = "select Memos.MemoID, Memos.Contenido, Eventoes.Titulo, Eventoes.Fecha from Memos " +
                "left join Eventoes on Eventoes.MemoID=Memos.MemoID";//Definicion de lo que queremos de Memo

            var memos = new ObservableCollection<Memo>();//Coleccion de notificacion para almacenar las entradas de la tabla
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetMemosQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {                                
                                while (reader.Read())
                                {
                                    var memo = new Memo();//Una instancia de artistas para ir guardando y almacenando lo que se lea de la base
                                    memo.MemoID = reader.GetInt32(0);//El parametro dentro de estos gets indica la posicion del atributo dentro de la tabla
                                    //se usan castings con el reader[numeroColumna] para que los null se creen solos al leer
                                    memo.Contenido = reader[1] as string;
                                    memo.Evento = reader[2] as string;
                                    memo.Fecha = reader[3] as DateTime?;
                                    memos.Add(memo);//Aniade el memo que se creo antes a la coleccion
                                }                                                                            
                            }
                        }
                    }
                }
                return memos;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        public static bool CreateMemo(string connectionString, string contenido, int evento)
        {
            try
            {
                const string crearMemo = "insert into Memos(Contenido) values(@contenido)";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = crearMemo;
                            cmd.Parameters.AddWithValue("@contenido", contenido);
                            cmd.ExecuteNonQuery();

                            //Id de ultimo memo
                            List<int> memoIDs = new List<int>();//Para guardar memos leidos
                            const string memos = "select MemoID from Memos";
                            cmd.CommandText = memos;
                            using (SqlDataReader lector = cmd.ExecuteReader())
                            {
                                while (lector.Read())
                                {
                                    memoIDs.Add(lector.GetInt32(0));
                                }
                            }
                            int ultimoMemo=0;
                            foreach (int id in memoIDs)
                            {
                                if (id>ultimoMemo)
                                {
                                    ultimoMemo = id;
                                }
                            }

                            //Cambiar Memo ID en Evento correcto
                            const string asociaMemo = "update Eventoes set MemoID=@idMemo where EventoID=@idEvento";
                            cmd.CommandText = asociaMemo;
                            cmd.Parameters.AddWithValue("@idMemo", ultimoMemo);
                            cmd.Parameters.AddWithValue("@idEvento", evento);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return false;
            }
        }

        public static bool DeleteMemo(string connectionString, int memo)
        {
            const string borrarMemo = "delete from Memos where MemoID=@memo";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand consola = conn.CreateCommand())
                        {
                            const string actualizaEvento = "update Eventoes set MemoID=NULL where MemoID=@id";
                            consola.CommandText = actualizaEvento;
                            consola.Parameters.AddWithValue("@id", memo);
                            consola.ExecuteNonQuery();

                            consola.CommandText = borrarMemo;
                            consola.Parameters.AddWithValue("@memo", memo);
                            consola.ExecuteNonQuery();                            
                        }
                    }
                }
                return true;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return false;
            }
        }

        public static bool UpdateMemo(string connectionString, int memo, string contenido, int evento)
        {
            const string actualizar = "update Memos set Contenido=@contenido where MemoID=@memo";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand consola = conn.CreateCommand())
                        {
                            consola.CommandText = actualizar;
                            consola.Parameters.AddWithValue("@contenido", contenido);
                            consola.Parameters.AddWithValue("@memo", memo);
                            consola.ExecuteNonQuery();

                            //Cambiar Memo ID en Evento correcto
                            const string asociaMemo = "update Eventoes set MemoID=@idMemo where EventoID=@idEvento";
                            consola.CommandText = asociaMemo;
                            consola.Parameters.AddWithValue("@idMemo", memo);
                            consola.Parameters.AddWithValue("@idEvento", evento);
                            consola.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return false;
            }
        }

        public static ObservableCollection<Evento> EventosRelacionados(string connectionString, DateTimeOffset date)//Metodo para recuperar datos
        {
            const string GetEventosQuery = "select EventoID, Fecha, Inicio, Fin, " +//Definicion de lo que queremos de Evento
                "Titulo, Descripcion, Ubicacion, EsSerie, Dias from Eventoes " +
                "where Fecha=@fechaEv";

            DateTime fecha = (DateTime)date.Date;

            var eventos = new ObservableCollection<Evento>();//Coleccion de evento para almacenar las entradas de la tabla
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {

                            cmd.CommandText = GetEventosQuery;
                            cmd.Parameters.AddWithValue("@fechaEv", fecha);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var evento = new Evento();//Una instancia de evento para ir guardando y almacenando lo que se lea de la base
                                    evento.EventoID = reader.GetInt32(0);//El parametro dentro de estos gets indica la posicion del atributo dentro de la tabla                                  
                                    evento.Fecha = reader.GetDateTime(1);
                                    evento.Inicio = reader.GetDateTime(2);
                                    evento.Fin = reader.GetDateTime(3);
                                    //se usa "reader[numeroColumna] as tipoDato" para evitar errores por null
                                    evento.Titulo = reader[4] as string;
                                    evento.Descripcion = reader[5] as string;
                                    evento.Ubicacion = reader[6] as string;
                                    evento.EsSerie = reader.GetBoolean(7);
                                    evento.Dias = reader[8] as string;

                                    eventos.Add(evento);//Aniade el evento que se creo antes a la coleccion
                                }
                            }
                        }
                    }
                }
                return eventos;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }
    }   
}
