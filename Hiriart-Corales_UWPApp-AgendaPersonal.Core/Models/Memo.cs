using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Hiriart_Corales_UWPApp_AgendaPersonal.Core.Models
{
    public class Memo : INotifyPropertyChanged
    {
        public int MemoID { get; set; }
        public string Contenido { get; set; }

        public string Evento { get; set; }

        public DateTime? Fecha { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
