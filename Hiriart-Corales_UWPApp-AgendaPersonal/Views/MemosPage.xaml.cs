using System;
using System.Diagnostics;
using Hiriart_Corales_UWPApp_AgendaPersonal.Core.Models;
using Hiriart_Corales_UWPApp_AgendaPersonal.ViewModels;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using static Hiriart_Corales_UWPApp_AgendaPersonal.ViewModels.MemosViewModel;

namespace Hiriart_Corales_UWPApp_AgendaPersonal.Views
{
    public sealed partial class MemosPage : Page
    {
        public MemosViewModel ViewModel { get; } = new MemosViewModel();

        public MemosPage()
        {
            InitializeComponent();
            MemosList.ItemsSource = ReadMemos((App.Current as App).ConnectionString);         
        }

        private void NuevoBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Llama a la vista de nuevoMemoPage
            this.Frame.Navigate(typeof(NuevoMemoPage));
            Actualizar();
        }

        private void EditarBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Llama a la vista de editarNotificacionPage
            if (MemosList.SelectedItem != null)//Solo entrar si hay algo seleccionado para evitar errores
            {
                Memo seleccionado = (Memo)MemosList.SelectedItem;//Obtener fecha de la entrada a borrar
                this.Frame.Navigate(typeof(EditarMemoPage), seleccionado);//Llamar a la vista pasando el item seleccionado
                Actualizar();
            }
        }

        private async void EliminarBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {          
            //Abre ventana de confirmación para preguntar si de quiere borrar la entrada
            if (MemosList.SelectedItem != null)//Solo entrar si hay algo seleccionado para evitar errores
            {
                Memo seleccionado = (Memo)MemosList.SelectedItem;//Obtener fecha de la entrada a borrar
                ContentDialog confirmarBorrado = new ContentDialog//Crea la ventana con boton de confirmacion y cancelacion
                {
                    Title = "Borrar notificacion",
                    Content = "¿Está seguro de que quiere borrar la notificacion " + seleccionado.Contenido + "?",
                    PrimaryButtonText = "Confirmar",
                    CloseButtonText = "Cancelar"
                };
                ContentDialogResult respuesta = await confirmarBorrado.ShowAsync();
                //Si se da click a Confirmar se borra, sino no
                if (respuesta == ContentDialogResult.Primary)//El boton primario confirma el borrado
                {
                    DeleteMemo((App.Current as App).ConnectionString, seleccionado.MemoID);
                    Actualizar();
                }
                else//Si no fue el primario, se le dio click al de Cancelar, o se salio de la ventana, entonces no se debería borrar
                {
                    //No se borra, se cancelo la accion
                } 
            }
        }     

        private void ActualizarBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Actualizar();
        }

        private async void AyudaBoton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Abre ventana que explica como usar la barra de comandos
            var ayuda = new MessageDialog("De clic en el boton de + para añadir una entrada nueva, el ícono \n" +
                "del lapiz le permite editar la entrada que tenga seleccionada, el ícono \n" +
                "del basurero borra la entrada seleccionada, y el boton Actualizar recarga la página");
            ayuda.Title = "Información";
            await ayuda.ShowAsync();
        }

        private void Actualizar()
        {
            MemosList.ItemsSource = ReadMemos((App.Current as App).ConnectionString);
        }
    }
}
