using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Hiriart_Corales_UWPApp_AgendaPersonal.Core.Models;
using Hiriart_Corales_UWPApp_AgendaPersonal.ViewModels;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static Hiriart_Corales_UWPApp_AgendaPersonal.ViewModels.MemosViewModel;

namespace Hiriart_Corales_UWPApp_AgendaPersonal.Views
{
    public sealed partial class EditarMemoPage : Page
    {

        ObservableCollection<Evento> Eventos = new ObservableCollection<Evento>();
        Memo seleccionadoMemoPage;//variable global para sacar datos del seleccionado originalmente, sobre todo para el ID para poder editar

        protected override void OnNavigatedTo(NavigationEventArgs e)//Metodo a usar en caso de pasar parametros durante la navegacion
        {
            if (!e.Parameter.Equals(null))//No continuar si es null, habrian errores
            {
                this.seleccionadoMemoPage = (Memo)e.Parameter;

                //Llenar los campos con los datos ya conocidos de la entrada seleccionada
                if (seleccionadoMemoPage.Fecha.HasValue)//Si tiene un evento asociado, tendra fecha y se peude mandar como atributo
                {
                    this.eventosListBox.ItemsSource = EventosRelacionados((App.Current as App).ConnectionString, (DateTimeOffset)((DateTime)seleccionadoMemoPage.Fecha).Date);
                }
                //Si el memo no tiene evento relacionado no hace falta mandar a buscar
                
                Evento evento = new Evento();
                foreach (Evento even in this.eventosListBox.Items)
                {
                    evento = even;
                }
                if (evento.Fecha.Equals(DateTime.MinValue))//Si no hay evento relacionado la fecha sera la minima, entonces se deja en null el picker
                {
                    this.fechaCalendarDatePicker.Date = null;
                }
                else//Si hay evento, hay una fecha que poner
                {
                    this.fechaCalendarDatePicker.Date = evento.Fecha.Date;
                }        
                this.contenidoTextBox.Text = seleccionadoMemoPage.Contenido;
            }
            else//Volver a la pantalla principal si es null, quiere decir que no se selecciono nada
            {
                this.Frame.Navigate(typeof(MemosPage));
            }
        }

        public EditarMemoPage()
        {
            InitializeComponent();
        }

        private async void AyudaBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var ayuda = new MessageDialog("Utilice el simbolo de guardado para añadir una entrada \n" +
                "de dario, la flecha hacia la izquirda le permite volver a la pantalla principal de Notificaciones");
            ayuda.Title = "Información";
            await ayuda.ShowAsync();
        }

        private void VolverBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Llama a la vista NotificionesPage
            this.Frame.Navigate(typeof(MemosPage));
        }

        private async void GuardarBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Validar())
            {
                Evento evento = (Evento)this.eventosListBox.SelectedItem;
                if (this.eventosListBox.SelectedItem == null)
                    evento = new Evento();

                bool exito = UpdateMemo((App.Current as App).ConnectionString, this.seleccionadoMemoPage.MemoID,
                    this.contenidoTextBox.Text, evento.EventoID);
                if (exito)
                {
                    var ingresoExito = new MessageDialog("Se ha editado la entrada, puede seguir editandola \n" +
                         "o volver a la pantalla principal de Memos");
                    ingresoExito.Title = "Entrada editada correctamente";
                    await ingresoExito.ShowAsync();
                }
                else
                {
                    var errorBase = new MessageDialog("Ha ocurrido un error con la base de datos, \nno se puede ingresar el memo");
                    errorBase.Title = "Error";
                    await errorBase.ShowAsync();
                }
            }
            else
            {
                var faltanDatos = new MessageDialog("No se han ingresado todos los datos, intente \nde nuevo");
                faltanDatos.Title = "Error";
                await faltanDatos.ShowAsync();
            }
        }

        private bool Validar()
        {
            if (this.fechaCalendarDatePicker.Date!=null && this.eventosListBox.SelectedItem!=null && !String.IsNullOrEmpty(this.contenidoTextBox.Text))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void LeerEventos(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (this.fechaCalendarDatePicker.Date!=null)
            {
                this.eventosListBox.ItemsSource = EventosRelacionados((App.Current as App).ConnectionString, (DateTimeOffset)this.fechaCalendarDatePicker.Date);
            }
            
        }
    }
}
