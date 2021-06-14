using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Hiriart_Corales_UWPApp_AgendaPersonal.Core.Models
{
    public class Diario : INotifyPropertyChanged
    {
        public int DiarioID { get; set; }       
        public DateTime Fecha { get; set; }
        public string Contenido { get; set; }

        public string Eventos { get; set; }//Al ser tipo string, ya es nullable

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
