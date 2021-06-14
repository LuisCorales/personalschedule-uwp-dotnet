using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Hiriart_Corales_UWPApp_AgendaPersonal.Core.Models
{
    public class Evento : INotifyPropertyChanged
    {             
        public int EventoID { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Ubicacion { get; set; }
        public bool EsSerie { get; set; }
        public string Dias { get; set; }

        public string Contactos { get; set; }
        public string Notificacion { get; set; }
        public string Memo { get; set; }
        public int NotificacionID { get; set; }
        public int MemoID { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
