using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using rakhrapdftron.UWP.Renderers;
using rakhrapdftron.Views.PdftronViews;
using Xamarin.Forms.Platform.UWP;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Threading;
using pdftron.PDF;

[assembly: ExportRenderer(typeof(ViewerPage), typeof(ViewerPageRenderer))]

namespace rakhrapdftron.UWP.Renderers
{
    public class ViewerPageRenderer : PageRenderer
    {
        private Windows.UI.Xaml.Controls.Page page;
        private Application app;
        private bool IsChangesMade;
        private pdftron.PDF.PDFViewCtrl mPdfViewCtrl;
        private pdftron.PDF.Tools.ToolManager mToolManager;

        private pdftron.PDF.Tools.Controls.AnnotationCommandBar mAnnotationToolbar;
        private Button closeButton;
        private Button resavePageButton;
        private Button addNewPageButton;

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                return;
            }

            try
            {
                app = Application.Current;

                SetupUserInterface();

                this.Children.Add(page);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"      ERROR: ", ex.Message);
            }
        }

        private void SetupUserInterface()
        {
            IsChangesMade = false;
            mPdfViewCtrl = new PDFViewCtrl();
            string path = Path.Combine(Windows.Storage.ApplicationData.Current.TemporaryFolder.Path, "sample.pdf");
            PDFDoc doc = new PDFDoc(path);
            mPdfViewCtrl.SetDoc(doc);
            mPdfViewCtrl.SetPagePresentationMode(pdftron.PDF.PDFViewCtrlPagePresentationMode.e_single_page);

            mToolManager = new pdftron.PDF.Tools.ToolManager(mPdfViewCtrl);
            mToolManager.EnablePopupMenuOnLongPress = true;
            mToolManager.IsPopupMenuEnabled = true;
            mToolManager.PanToolTextSelectionMode = pdftron.PDF.Tools.ToolManager.TextSelectionBehaviour.AlwaysPan;
            mToolManager.TextMarkupAdobeHack = true;

            mToolManager.AnnotationAdded += MToolManager_AnnotationAdded;
            mToolManager.AnnotationEdited += MToolManager_AnnotationEdited;
            mToolManager.AnnotationGroupAdded += MToolManager_AnnotationGroupAdded;
            mToolManager.AnnotationGroupEdited += MToolManager_AnnotationGroupEdited;
            mToolManager.AnnotationGroupPreEdited += MToolManager_AnnotationGroupPreEdited;
            mToolManager.AnnotationGroupPreRemoved += MToolManager_AnnotationGroupPreRemoved;
            mToolManager.AnnotationGroupRemoved += MToolManager_AnnotationGroupRemoved;
            mToolManager.AnnotationPreRemoved += MToolManager_AnnotationPreRemoved;
            mToolManager.AnnotationRemoved += MToolManager_AnnotationRemoved;

            mAnnotationToolbar = new pdftron.PDF.Tools.Controls.AnnotationCommandBar(mToolManager);

            var stackPanel = new StackPanel();
            var horizontalStackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            closeButton = new Button()
            {
                Content = "Close"
            };

            resavePageButton = new Button()
            {
                Content = "Resave first page"
            };

            addNewPageButton = new Button()
            {
                Content = "New Page"
            };

            horizontalStackPanel.Children.Add(closeButton);
            horizontalStackPanel.Children.Add(resavePageButton);
            horizontalStackPanel.Children.Add(addNewPageButton);

            stackPanel.Children.Add(horizontalStackPanel);
            stackPanel.Children.Add(mAnnotationToolbar);
            stackPanel.Children.Add(mPdfViewCtrl);
            closeButton.Click += CloseButton_Click;
            resavePageButton.Click += resavePageButton_Click;
            addNewPageButton.Click += AddNewPageButton_Click;

            page = new Windows.UI.Xaml.Controls.Page();
            page.Content = stackPanel;
        }

        /// <summary>
        /// Insert file should have name insertPage.pdf
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddNewPageButton_Click(object sender, RoutedEventArgs e)
        {
            string insertNewPath = Path.Combine(Windows.Storage.ApplicationData.Current.TemporaryFolder.Path, "insertPage.pdf");
            using (var pdfDoc = new PDFDoc(insertNewPath))
            {
                var doc = mPdfViewCtrl.GetDoc();
                doc.InsertPages(1, pdfDoc, 1, pdfDoc.GetPageCount(), PDFDocInsertFlag.e_none);
                mPdfViewCtrl.Update();
                mPdfViewCtrl.UpdateLayout();
                await Save();
            }
        }

        /// <summary>
        /// resave first page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void resavePageButton_Click(object sender, RoutedEventArgs e)
        {
            using (var pdfDoc = new PDFDoc())
            {
                var doc = mPdfViewCtrl.GetDoc();
                var firstPage = doc.GetPage(1);
                pdfDoc.PagePushFront(firstPage);
                doc.PageRemove(doc.GetPageIterator(1));
                doc.InsertPages(1, pdfDoc, 1, pdfDoc.GetPageCount(), PDFDocInsertFlag.e_none);
                mPdfViewCtrl.Update();
                mPdfViewCtrl.UpdateLayout();
            }
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            closeButton.Click -= CloseButton_Click;
            resavePageButton.Click -= resavePageButton_Click;
            mToolManager.AnnotationAdded -= MToolManager_AnnotationAdded;
            mToolManager.AnnotationEdited -= MToolManager_AnnotationEdited;
            mToolManager.AnnotationGroupAdded -= MToolManager_AnnotationGroupAdded;
            mToolManager.AnnotationGroupEdited -= MToolManager_AnnotationGroupEdited;
            mToolManager.AnnotationGroupPreEdited -= MToolManager_AnnotationGroupPreEdited;
            mToolManager.AnnotationGroupPreRemoved -= MToolManager_AnnotationGroupPreRemoved;
            mToolManager.AnnotationGroupRemoved -= MToolManager_AnnotationGroupRemoved;
            mToolManager.AnnotationPreRemoved -= MToolManager_AnnotationPreRemoved;
            mToolManager.AnnotationRemoved -= MToolManager_AnnotationRemoved;

            if (IsChangesMade)
            {
                mPdfViewCtrl.DocUnlock();
                mPdfViewCtrl.CloseDoc();
                mPdfViewCtrl.Dispose();

                string workFilePath = Path.Combine(Windows.Storage.ApplicationData.Current.TemporaryFolder.Path, "sampleWork.pdf");
                string filePath = Path.Combine(Windows.Storage.ApplicationData.Current.TemporaryFolder.Path, "sample.pdf");
                File.Delete(filePath);
                File.Copy(workFilePath, filePath);
                //File.Delete(workFilePath);
                await rakhrapdftron.App.ClosePdfDocumentAsync();
                IsChangesMade = false;
                return;
            }

            mPdfViewCtrl.DocUnlock();
            mPdfViewCtrl.CloseDoc();
            mPdfViewCtrl.Dispose();
            await rakhrapdftron.App.ClosePdfDocumentAsync();
        }

        private async void MToolManager_AnnotationRemoved(pdftron.PDF.IAnnot annotation, int pageNumber)
        {
            await Save();
        }

        private async void MToolManager_AnnotationPreRemoved(pdftron.PDF.IAnnot annotation, int pageNumber)
        {
            await Save();
        }

        private async void MToolManager_AnnotationGroupRemoved(System.Collections.Generic.Dictionary<pdftron.PDF.IAnnot, int> annotationGroup)
        {
            await Save();
        }

        private async void MToolManager_AnnotationGroupPreRemoved(System.Collections.Generic.Dictionary<pdftron.PDF.IAnnot, int> annotationGroup)
        {
            await Save();
        }

        private async void MToolManager_AnnotationGroupPreEdited(System.Collections.Generic.Dictionary<pdftron.PDF.IAnnot, int> annotationGroup)
        {
            await Save();
        }

        private async void MToolManager_AnnotationGroupEdited(System.Collections.Generic.Dictionary<pdftron.PDF.IAnnot, int> annotationGroup)
        {
            await Save();
        }

        private async void MToolManager_AnnotationGroupAdded(System.Collections.Generic.Dictionary<pdftron.PDF.IAnnot, int> annotationGroup)
        {
            await Save();
        }

        private async void MToolManager_AnnotationEdited(pdftron.PDF.IAnnot annotation, int pageNumber)
        {
            await Save();
        }

        private async void MToolManager_AnnotationAdded(pdftron.PDF.IAnnot annotation, int pageNumber)
        {
            await Save();
        }

        private async Task Save(string fileName = "sampleWork.pdf")
        {
            IsChangesMade = true;
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.TemporaryFolder.Path, fileName);
            var doc = mPdfViewCtrl.GetDoc();
            await doc.SaveAsync(path, pdftron.SDF.SDFDocSaveOptions.e_remove_unused);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            page.Arrange(new Windows.Foundation.Rect(0, 0, finalSize.Width, finalSize.Height));
            return finalSize;
        }
    }
}