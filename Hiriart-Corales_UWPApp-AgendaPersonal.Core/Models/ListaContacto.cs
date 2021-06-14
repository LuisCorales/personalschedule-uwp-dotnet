using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Hiriart_Corales_UWPApp_AgendaPersonal.Core.Models
{
    public class ListaContacto : INotifyPropertyChanged
    {
        public int ListaContactoID { get; set; }
        public Nullable<int> IDEvento { get; set; }
        public string NombreApellido { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
