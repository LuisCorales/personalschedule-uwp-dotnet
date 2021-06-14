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
    public class DiarioViewModel : ObservableObject
    {
        public DiarioViewModel()
        {
        }

        //El siguiente metodo es static para poder llamarlo sin instanciar, también se le prodía colocar en una librería de funciones para SQL
        public static ObservableCollection<Diario> ReadDiarios(string connectionString)//Metodo para recuperar datos
        {
            const string GetDiariosQuery = "select Diarios.DiarioID, Diarios.Fecha, Diarios.Contenido, Eventoes.Titulo " +//Definicion de lo que queremos de Diario
                "from Diarios left join Eventoes on Eventoes.DiarioID=Diarios.DiarioID";
            /* left join permite sacar de dos tablas al mismo tiempo, siempre y cuando haya clave foranea que les relacione, 
             * se debe poner tabla.Atributo para sacar porque hay dos tablas involucradas, left join coge todos, tengan o no relacion con la otra tabla
            */

            var diarios = new ObservableCollection<Diario>();//Coleccion de diario para almacenar las entradas de la tabla
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDiariosQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var diario = new Diario();//Una instancia de diario para ir guardando y almacenando lo que se lea de la base
                                    diario.DiarioID = reader.GetInt32(0);//El parametro dentro de estos gets indica la posicion del atributo dentro de la tabla                                  
                                    diario.Fecha = reader.GetDateTime(1);
                                    //se usa "reader[numeroColumna] as tipoDato" para evitar errores por null
                                    diario.Contenido = (string)reader[2];
                                    /* Ahora se transforman los IDs de eventos en títulos de evento,
                                     * se hace luego de que acabe el anterior ExecuteReader de cmd pues
                                     * debe acabar el comando antes de ejecutar otro
                                    */

                                    string titulos = "";
                                    //Lista para guardar los titulos de los eventos, instanciado para no tener problemas de null al asignar                                 

                                    if (titulos.Equals(""))//Si no hay nada pone el primero sin ningun formato
                                    {
                                        titulos = reader[3] as string;

                                    }
                                    else//Si ya habia algo, pone una coma y luego el otro
                                    {
                                        titulos += ", " + reader[3] as string;
                                    }
                                    //Se hace lo mismo que antes en este reader, para problemas de null

                                    diario.Eventos = titulos;
                                    
                                    diarios.Add(diario);//Aniade el diario que se creo antes a la coleccion
                                }
                            }
                        }
                    }
                }     
                return diarios;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        public static bool CreateDiario(string connectionString, DateTimeOffset fecha, string contenido, List<int?> eventos)
        {
            DateTime fechaEntrada = fecha.Date;//transformar de DateTiemOffset a DateTime

            //Primero insertar una nueva entrada de Diarios
            const string insertarDiario = "insert into Diarios(Fecha, Contenido) " +
                "values(@Fecha, @Contenido)";
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
                            cmd.Parameters.AddWithValue("@Fecha", fechaEntrada);
                            cmd.Parameters.AddWithValue("@Contenido", contenido);
                            cmd.ExecuteNonQuery();

                            //Ahora, asociar los eventos al Diario, en caso de ser necesario
                            if (eventos.Count != 0)
                            {
                                //Primero, obtener el ID del ultimo diario, es decir el que se ingreso ahora, para poder asociar
                                List<int?> idDiarios = new List<int?>();//Lista para guardar los ids leidos
                                const string ids = "select DiarioID from Diarios";
                                cmd.CommandText = ids;
                                using(SqlDataReader lector = cmd.ExecuteReader())
                                {
                                    while (lector.Read())
                                    {
                                        idDiarios.Add(lector[0] as int?);
                                    }
                                }
                                int ultimaEntrada=0;
                                foreach (int id in idDiarios)//Obtener ultima entrada, debe ser el id mas alto
                                {
                                    if (id>ultimaEntrada)
                                    {
                                        ultimaEntrada = id;
                                    }
                                }

                                //Asociar cada evento de la lista con la entrada de diario
                                foreach (int id in eventos)
                                {
                                    //En la lista de eventos
                                    const string editarAsociacion = "update ListaEventoes set IDDiario=@ultima where ListaEventoID=@evento";
                                    cmd.CommandText = editarAsociacion;
                                    cmd.Parameters.AddWithValue("@ultima", ultimaEntrada);
                                    cmd.Parameters.AddWithValue("@evento", id);
                                    cmd.ExecuteNonQuery();

                                    //En el evento en si
                                    const string editarEVento = "update Eventoes set DiarioID=@ultimaEnt where EventoID=@event";
                                    cmd.CommandText = editarEVento;
                                    cmd.Parameters.AddWithValue("@ultimaEnt", ultimaEntrada);
                                    cmd.Parameters.AddWithValue("@event", id);
                                    cmd.ExecuteNonQuery();

                                }                               
                            }
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

        public static bool DeleteDiario(string connectionString, int id)
        {
            const string borrar = "delete from Diarios where DiarioID=@ID";

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

        public static bool UpdateDiario(string connectionString, int DiarioID, DateTimeOffset fecha, string contenido, List<int?> eventos)
        {
            DateTime fechaNueva = fecha.Date;//transformar de DateTiemOffset a DateTime

            const string editarDiario = "update Diarios set Fecha=@fecha, Contenido=@contenido where DiarioID=@id";

            try
            {
                using (SqlConnection conexion = new SqlConnection(connectionString))
                {
                    conexion.Open();
                    if(conexion.State == ConnectionState.Open)
                    {
                        using (SqlCommand consola = conexion.CreateCommand())
                        {
                            consola.CommandText = editarDiario;
                            consola.Parameters.AddWithValue("@fecha", fecha);
                            consola.Parameters.AddWithValue("@contenido", contenido);
                            consola.Parameters.AddWithValue("@id", DiarioID);
                            consola.ExecuteNonQuery();

                            //Se quitan los eventos que se hayan desasociado en esta entrada, primero se leen los actuales
                            List<int> eventosQuitados = new List<int>();//Lista para guardar los ids de elementos a quitar
                            const string eventosActuales = "select ListaEventoID from ListaEventoes where IDDiario=@IDeste";//Buscar eventos ya asociados
                            consola.CommandText = eventosActuales;
                            consola.Parameters.AddWithValue("@IDeste", DiarioID);
                            using (SqlDataReader lector = consola.ExecuteReader())
                            {
                                while (lector.Read())
                                {
                                    //Primero se aniaden todos los que estan asociados, luego se filtran los que si se quedan
                                    eventosQuitados.Add(lector.GetInt32(0));
                                }
                            }

                            //Dejar en eventosQuitados solo los que ya no se seleccionaron en la interfaz
                            foreach (int id in eventos)
                            {
                                //Si se tiene en los ya asociados uno de los seleccionados en View, se borra de la lista de Quitados porque aun debe estar asociado
                                if (eventosQuitados.Contains(id))
                                {
                                    eventosQuitados.Remove(id);
                                }
                            }

                            //Ahora si, se quitan los que ya no esten asociados
                            foreach (int id in eventosQuitados)
                            {
                                const string editarAsociacion = "update ListaEventoes set IDDiario=NULL where ListaEventoID=@eventoQuitado";//Se desasocia con null
                                consola.CommandText = editarAsociacion;                               
                                consola.Parameters.AddWithValue("@eventoQuitado", id);
                                consola.ExecuteNonQuery();
                            }

                            //Se aniaden los nuevos eventos que se hayan asociado con esta entrada de diario
                            foreach (int id in eventos)
                            {
                                //En la lista
                                const string editarAsociacion = "update ListaEventoes set IDDiario=@ID where ListaEventoID=@evento";
                                consola.CommandText = editarAsociacion;
                                consola.Parameters.AddWithValue("@IDDiario", DiarioID);
                                consola.Parameters.AddWithValue("@evento", id);
                                consola.ExecuteNonQuery();

                                //En el evento en si
                                const string editarEVento = "update Eventoes set DiarioID=@ultimaEnt where EventoID=@event";
                                consola.CommandText = editarEVento;
                                consola.Parameters.AddWithValue("@ultimaEnt", DiarioID);
                                consola.Parameters.AddWithValue("@event", id);
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

        public static ObservableCollection<Evento> EventosVinculados(string connectionString, int DiarioID)
        {
            const string GetEventosQuery = "select Eventoes.EventoID, Eventoes.Fecha, Eventoes.Inicio, Eventoes.Fin, " +//Definicion de lo que queremos de Evento
                "Eventoes.Titulo, Eventoes.Descripcion, Eventoes.Ubicacion, Eventoes.EsSerie, Eventoes.Dias from Eventoes " +
                "inner join ListaEventoes on ListaEventoes.IDDiario = @ID and ListaEventoes.ListaEventoID=Eventoes.EventoID";
            //inner join es parecido a left join, pero coge solo los que tienen relacion con la otra tabla
            //Sacamos todo de un evento, solo si al buscar ese evento en ListaEventoes (despues del and) la entrada en tal tabla tiene un IDDiario igual al que se eta editando

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

                            cmd.Parameters.AddWithValue("@ID", DiarioID);
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
