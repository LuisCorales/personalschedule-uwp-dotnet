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
    public class EventosViewModel : ObservableObject
    {
        public EventosViewModel()
        {
        }

        public static ObservableCollection<Evento> ReadEventos(string connectionString)//Metodo para recuperar datos
        {
            const string GetEventosQuery = "select Eventoes.EventoID, Eventoes.Fecha, Eventoes.Inicio, Eventoes.Fin, " +//Definicion de lo que queremos de Evento
                "Eventoes.Titulo, Eventoes.Descripcion, Eventoes.Ubicacion, Eventoes.EsSerie, Eventoes.Dias, Eventoes.NotificacionID, " +
                "Eventoes.MemoID, ListaContactoes.NombreApellido, Memos.Contenido, Notificacions.Titulo from Eventoes " +
                "left join ListaContactoes on ListaContactoes.IDEvento=Eventoes.EventoID " +
                "left join Memos on Memos.MemoID=Eventoes.MemoID " +
                "left join Notificacions on Notificacions.NotificacionID=Eventoes.NotificacionID";

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
                                    evento.NotificacionID = reader[9]==DBNull.Value?0:reader.GetInt32(9);//0 si es null para no leer nada en view, no hace falta
                                    evento.MemoID = reader[10] == DBNull.Value ? 0 : reader.GetInt32(10);

                                    // Ahora se obtienen los contactos asociados
                                    string nombres = "";
                                    //Lista para guardar los nombres de los contactos, instanciado para no tener problemas de null al asignar
                                                                                          
                                    if (nombres.Equals(""))//Si no hay nada pone el primero sin ningun formato
                                    {
                                        nombres = reader[11] as string;
                                    }
                                    else//Si ya habia algo, pone una coma y luego el otro
                                    {
                                        nombres += ", " + reader[11] as string;
                                    }
                                    //Se hace lo mismo que antes en este reader, para problemas de null
                                        
                                    evento.Contactos = nombres;

                                    evento.Memo = reader[12] as string;

                                    evento.Notificacion = reader[13] as string;

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

        public static bool CreateEvento(string connectionString, DateTimeOffset tiempo, TimeSpan ini, TimeSpan fi,
            string titulo, string descripcion, string ubicacion, bool esSerie, string dias, List<int?> contactos)
        {
            DateTime fecha = tiempo.Date;//transformar de DateTimeOffset a DateTime
            DateTime inicio = (DateTime)(fecha + ini);//cast por si son null
            DateTime fin = (DateTime)(fecha + fi);

            //Primero insertar una nueva entrada de Diarios
            const string insertarEvento = "insert into Eventoes(NotificacionID, MemoID, Fecha, Inicio, Fin, Titulo, Descripcion, Ubicacion, EsSerie, " +
                "Dias) values(NULL, NULL, @date, @i, @f, @titulo, @descripcion, @ubicacion, @esSerie, @dias)";
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
                            cmd.CommandText = insertarEvento;
                            cmd.Parameters.AddWithValue("@date", fecha);
                            cmd.Parameters.AddWithValue("@i", inicio);
                            cmd.Parameters.AddWithValue("@f", fin);
                            cmd.Parameters.AddWithValue("@titulo", titulo);
                            cmd.Parameters.AddWithValue("@descripcion", descripcion);
                            cmd.Parameters.AddWithValue("@ubicacion", ubicacion);
                            cmd.Parameters.AddWithValue("@esSerie", esSerie);
                            cmd.Parameters.AddWithValue("@dias", dias);
                            cmd.ExecuteNonQuery();

                            //Ahora, asociar los contactos al evento, en caso de ser necesario
                            if (contactos.Count != 0)
                            {
                                //Primero, obtener el ID del ultimo evento, es decir el que se ingreso ahora, para poder asociar
                                List<int?> idEventos = new List<int?>();//Lista para guardar los ids leidos
                                const string ids = "select EventoID from Eventoes";
                                cmd.CommandText = ids;
                                using (SqlDataReader lector = cmd.ExecuteReader())
                                {
                                    while (lector.Read())
                                    {
                                        idEventos.Add(lector[0] as int?);
                                    }
                                }
                                int ultimaEntrada = 0;
                                foreach (int id in idEventos)//Obtener ultima entrada, debe ser el id mas alto
                                {
                                    if (id > ultimaEntrada)
                                    {
                                        ultimaEntrada = id;
                                    }
                                }

                                //Asociar cada contacto de la lista con la entrada de evento
                                foreach (int id in contactos)
                                {
                                    const string editarAsociacion = "update ListaContactoes set IDEvento=@ultima where ListaContactoID=@contacto";
                                    cmd.CommandText = editarAsociacion;
                                    cmd.Parameters.AddWithValue("@ultima", ultimaEntrada);
                                    cmd.Parameters.AddWithValue("@contacto", id);
                                    cmd.ExecuteNonQuery();
                                }                 
                            }
                            //Crear nueva entrada de ListaEventoes
                            const string lista = "insert into ListaEventoes(IDDiario, Titulo, FechaEvento) values(NULL, @titEvento, @fechaEvent)";
                            cmd.CommandText = lista;
                            cmd.Parameters.AddWithValue("@titEvento", titulo);
                            cmd.Parameters.AddWithValue("@fechaEvent", fecha);
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

        public static bool DeleteEvento(string connectionString, int id)
        {
            const string borrar = "delete from Eventoes where EventoID=@ID";

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

                            //Borrar entrada de lista
                            const string borrarLista = "delete from ListaEventoes where ListaEventoID=@idLista";
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

        public static bool UpdateEvento(string connectionString, int EventoID, int notif, int memo, DateTimeOffset tiempo, TimeSpan ini, TimeSpan fi,
            string titulo, string descripcion, string ubicacion, bool esSerie, string dias, List<int?> contactos)
        {
            DateTime fecha = tiempo.Date;//transformar de DateTimeOffset a DateTime
            DateTime inicio = (DateTime)(fecha + ini);//cast por si son null
            DateTime fin = (DateTime)(fecha + fi);

            const string editarEvento = "update Eventoes set NotificacionID=@notif, MemoID=@memo, Fecha=@date, Inicio=@i, Fin=@f, Titulo=titulo, " +
                "Descripcion=@descripcion, Ubicacion=@ubicacion, EsSerie=@esSerie, Dias=@dias where EventoID=@id";

            try
            {
                using (SqlConnection conexion = new SqlConnection(connectionString))
                {
                    conexion.Open();
                    if (conexion.State == ConnectionState.Open)
                    {
                        using (SqlCommand consola = conexion.CreateCommand())
                        {
                            consola.CommandText = editarEvento;
                            if (notif!=0)
                            {
                                consola.Parameters.AddWithValue("@notif", notif);
                            }
                            else//Si no se paso nada hay que hacerlo null sino hay error de la base
                            {
                                consola.Parameters.AddWithValue("@notif", DBNull.Value);
                            }                         
                            if (memo!=0)
                            {
                                consola.Parameters.AddWithValue("@memo", memo);
                            }
                            else//Si no se paso nada hay que hacerlo null sino hay error de la base
                            {
                                consola.Parameters.AddWithValue("@memo", DBNull.Value);                              
                            }                           
                            consola.Parameters.AddWithValue("@date", fecha);
                            consola.Parameters.AddWithValue("@i", inicio);
                            consola.Parameters.AddWithValue("@f", fin);
                            consola.Parameters.AddWithValue("@titulo", titulo);
                            consola.Parameters.AddWithValue("@descripcion", descripcion);
                            consola.Parameters.AddWithValue("@ubicacion", ubicacion);
                            consola.Parameters.AddWithValue("@esSerie", esSerie);
                            consola.Parameters.AddWithValue("@dias", dias);
                            consola.Parameters.AddWithValue("@id", EventoID);
                            consola.ExecuteNonQuery();

                            //Actualizar la entrada en al tabla de lista
                            const string actualizarLista = "update ListaEventoes set Titulo=@titEvento, FechaEvento=@fechaEv where ListaEventoID=@idLista";
                            consola.CommandText = actualizarLista;
                            consola.Parameters.AddWithValue("@titEvento", titulo);
                            consola.Parameters.AddWithValue("@fechaEv", fecha);
                            consola.Parameters.AddWithValue("@idLista", EventoID);
                            consola.ExecuteNonQuery();

                            //Se quitan los contactos que se hayan desasociado en esta entrada, primero se leen los actuales
                            List<int> contactosQuitados = new List<int>();//Lista para guardar los ids de elementos a quitar
                            const string contactosActuales = "select ListaContactoID from ListaContactoes where IDEvento=@IDeste";//Buscar contactos ya asociados
                            consola.CommandText = contactosActuales;
                            consola.Parameters.AddWithValue("@IDeste", EventoID);
                            using (SqlDataReader lector = consola.ExecuteReader())
                            {
                                while (lector.Read())
                                {
                                    //Primero se aniaden todos los que estan asociados, luego se filtran los que si se quedan
                                    contactosQuitados.Add(lector.GetInt32(0));
                                }
                            }

                            //Dejar en eventosQuitados solo los que ya no se seleccionaron en la interfaz
                            foreach (int id in contactos)
                            {
                                //Si se tiene en los ya asociados uno de los seleccionados en View, se borra de la lista de Quitados porque aun debe estar asociado
                                if (contactosQuitados.Contains(id))
                                {
                                    contactosQuitados.Remove(id);
                                }
                            }

                            //Ahora si, se quitan los que ya no esten asociados
                            foreach (int id in contactosQuitados)
                            {
                                const string editarAsociacion = "update ListaContactoes set IDEvento=NULL where ListaContactoID=@contactoQuitado";//Se desasocia con null
                                consola.CommandText = editarAsociacion;
                                consola.Parameters.AddWithValue("@contactoQuitado", id);
                                consola.ExecuteNonQuery();
                            }

                            //Se aniaden los nuevos contactos que se hayan asociado con esta entrada de diario
                            foreach (int id in contactos)
                            {
                                const string editarAsociacion = "update ListaContactoes set IDEvento=@IDEvento where ListaContactoID=@contacto";
                                consola.CommandText = editarAsociacion;
                                consola.Parameters.AddWithValue("@IDEvento", EventoID);
                                consola.Parameters.AddWithValue("@contacto", id);
                                consola.ExecuteNonQuery();
                            }                            
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

        public static ObservableCollection<Notificacion> NotificacionRelacionada(string connectionString, int id)//Metodo para recuperar datos
        {
            const string getNotifacionesQuery = "select NotificacionID, Titulo, Hora from Notificacions " +
                "where NotificacionID=@idNotif";//Definicion de lo que queremos de Notificacion

            var notificaciones = new ObservableCollection<Notificacion>();//Coleccion de notificacion para almacenar las entradas de la tabla
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = getNotifacionesQuery;
                            cmd.Parameters.AddWithValue("@idNotif", id);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var notif = new Notificacion();//Una instancia de artistas para ir guardando y almacenando lo que se lea de la base
                                    notif.NotificacionID = reader.GetInt32(0);//El parametro dentro de estos gets indica la posicion del atributo dentro de la tabla
                                    //se usan castings con el reader[numeroColumna] para que los null se creen solos al leer
                                    notif.Titulo = reader[1] as string;
                                    notif.Hora = reader.GetDateTime(2);
                                    notificaciones.Add(notif);//Aniade el memo que se creo antes a la coleccion
                                }
                            }
                        }
                    }
                }
                return notificaciones;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        public static ObservableCollection<Memo> MemoRelacionado(string connectionString, int id)//Metodo para recuperar datos
        {
            const string GetMemosQuery = "select MemoID, Contenido from Memos " +
                "where MemoID=@idMemo";//Definicion de lo que queremos de Memo

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
                            cmd.Parameters.AddWithValue("@idMemo", id);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var memo = new Memo();//Una instancia de artistas para ir guardando y almacenando lo que se lea de la base
                                    memo.MemoID = reader.GetInt32(0);//El parametro dentro de estos gets indica la posicion del atributo dentro de la tabla
                                    //se usan castings con el reader[numeroColumna] para que los null se creen solos al leer
                                    memo.Contenido = reader[1] as string;
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

        //No se usa, solo se leen todos los contactos
        /*
        public static ObservableCollection<Contacto> ContactosVinculados(string connectionString, int EventoID)
        {
            const string GetContactosQuery = "select Contactoes.ContactoID, Contactoes.Nombre, Contactoes.Apellido, " +//Definicion de lo que queremos de Evento
                "Contactoes.Telefono, Contactoes.Email, Contactoes.Organizacion, Contactoes.FechaNacimiento, Contactoes.InformacionAdicional from Eventoes " +
                "inner join ListaContactoes on ListaContactoes.IDEvento = @ID and ListaEventoes.ListaEventoID=Eventoes.EventoID";
            //inner join es parecido a left join, pero coge solo los que tienen relacion con la otra tabla
            //Sacamos todo de un evento, solo si al buscar ese contacto en ListaContactoes (despues del and) la entrada en tal tabla tiene un IDEvento igual al que se esta editando

            var contactos = new ObservableCollection<Contacto>();//Coleccion de evento para almacenar las entradas de la tabla
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {

                            cmd.Parameters.AddWithValue("@ID", EventoID);
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
        */
    }
}
