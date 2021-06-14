using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Hiriart_Corales_UWPApp_AgendaPersonal.Core.Models
{
    public class Contacto : INotifyPropertyChanged
    {
        public int ContactoID { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Organizacion { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string InformacionAdicional { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
