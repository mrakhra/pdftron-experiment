using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace rakhrapdftron.Views.PdftronViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnOpenViewerButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ViewerPage());
        }
    }
}