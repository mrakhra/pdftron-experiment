using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using rakhrapdftron.Services;
using rakhrapdftron.Views;

namespace rakhrapdftron
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //DependencyService.Register<MockDataStore>();
            MainPage = new Views.PdftronViews.MainPage();
        }

        public static async System.Threading.Tasks.Task ClosePdfDocumentAsync()
        {
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}