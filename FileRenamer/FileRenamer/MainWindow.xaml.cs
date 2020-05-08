using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.ObjectModel;

namespace FileRenamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataObject dataObject;

        public MainWindow()
        {
            InitializeComponent();
            UISetup();
            DataContext = new DataObject();
            dataObject = DataContext as DataObject;

            dataObject.FileList.CollectionChanged += (sender, e) =>
            {
                if (dataObject.FileList.Count > 0)
                {
                    Button_RemoveAll.IsEnabled = true;
                }
                else
                {
                    Button_RemoveAll.IsEnabled = false;
                }
            };
        }

        private void UISetup()
        {
            Button_RemoveSelected.IsEnabled = false;
            Button_RemoveAll.IsEnabled = false;
            Button_Rename.IsEnabled = false;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                dataObject.FileList.Clear();

                foreach (string filePath in openFileDialog.FileNames)
                {
                    File newFile = new File(filePath);
                    dataObject.FileList.Add(newFile);
                }

                LoadExifListBox();
            }

        }

        public void LoadExifListBox()
        {
            if (dataObject.FileList.Count == 0)
            {
                dataObject.PropList.Clear();
                return;
            }
            ListBox_ImageData.SelectionChanged -= ListBox_ImageData_SelectionChanged;
            dataObject.PropList.Clear();
            bool firstFile = true;
            foreach (File f in dataObject.FileList)
            {
                // Load image properties
                firstFile = LoadImageProperties(f.FilePath, firstFile);
            }
            ListBox_ImageData.SelectionChanged += ListBox_ImageData_SelectionChanged;

            if (dataObject.PropList.Count == 0)
            {
                MessageBoxResult result = MessageBox.Show(this, "The selected files do not share enough common data.\n\nPlease select another set of files or remove a few files from the list until they share common data.","Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (ListBox_ImageData.SelectedIndex == -1)
            {
                ListBox_ImageData.SelectedIndex = 0;
            }
        }

        private bool LoadImageProperties(string filePath, bool firstFile)
        {
            using (Bitmap image = new Bitmap(filePath))
            {
                if (firstFile)
                {
                    foreach (PropertyItem pi in image.PropertyItems)
                    {
                        Prop p = new Prop(pi);
                        dataObject.PropList.Add(p);
                    }
                    firstFile = false;
                }
                else
                {
                    List<Prop> tempPropList = new List<Prop>(dataObject.PropList);
                    foreach (Prop p in tempPropList)
                    {
                        if (dataObject.PropList.Count == 0) break;
                        if (!image.PropertyIdList.Contains(p.Id)) dataObject.PropList.Remove(p);
                    }
                }
            }

            return firstFile;
        }


        private void ListBox_ImageData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_ImageData.SelectedIndex == -1)
            {
                Button_Rename.IsEnabled = false;
                return;
            }

            Button_Rename.IsEnabled = true;
            ListBox_ImageData.SelectionChanged -= ListBox_ImageData_SelectionChanged;
            Prop prop = (Prop)ListBox_ImageData.SelectedItem;
            // signature of a prop (id, name and type, not including value and len)
            foreach (File file in dataObject.FileList)
            {
                PropertyItem propItem;
                using (Bitmap bm = new Bitmap(file.FilePath))
                {
                    propItem = bm.GetPropertyItem(prop.Id);
                }

                // Actual prop of the current image file (id, name, type, value and len)
                byte[] propData = propItem.Value;
                int propDataLength = propItem.Len;

                string result = "";
                int num_items, item_size;
                string newFileName = "";

                switch (prop.Type)
                {
                    case Utilities.ExifPropertyDataTypes.ByteArray:
                    case Utilities.ExifPropertyDataTypes.UByteArray:
                        newFileName =
                            BitConverter.ToString(propData);
                        break;

                    case Utilities.ExifPropertyDataTypes.String:
                        newFileName = Encoding.UTF8.GetString(
                            propData, 0, propDataLength - 1);
                        break;

                    case Utilities.ExifPropertyDataTypes.UShortArray:
                        result = "";
                        item_size = 2;
                        num_items = propDataLength / item_size;
                        for (int i = 0; i < num_items; i++)
                        {
                            ushort value = BitConverter.ToUInt16(
                                propData, i * item_size);
                            result += ", " + value.ToString();
                        }
                        if (result.Length > 0) result = result.Substring(2);
                        newFileName = "[" + result + "]";
                        break;

                    case Utilities.ExifPropertyDataTypes.ULongArray:
                        result = "";
                        item_size = 4;
                        num_items = propDataLength / item_size;
                        for (int i = 0; i < num_items; i++)
                        {
                            uint value = BitConverter.ToUInt32(
                                propData, i * item_size);
                            result += ", " + value.ToString();
                        }
                        if (result.Length > 0) result = result.Substring(2);
                        newFileName = "[" + result + "]";
                        break;

                    case Utilities.ExifPropertyDataTypes.ULongFractionArray:
                        result = "";
                        item_size = 8;
                        num_items = propDataLength / item_size;
                        for (int i = 0; i < num_items; i++)
                        {
                            uint numerator = BitConverter.ToUInt32(
                                propData, i * item_size);
                            uint denominator = BitConverter.ToUInt32(
                                propData,
                                i * item_size + item_size / 2);
                            result += ", " + numerator.ToString() +
                                "/" + denominator.ToString();
                        }
                        if (result.Length > 0) result = result.Substring(2);
                        newFileName = "[" + result + "]";
                        break;

                    case Utilities.ExifPropertyDataTypes.LongArray:
                        result = "";
                        item_size = 4;
                        num_items = propDataLength / item_size;
                        for (int i = 0; i < num_items; i++)
                        {
                            int value = BitConverter.ToInt32(
                                propData, i * item_size);
                            result += ", " + value.ToString();
                        }
                        if (result.Length > 0) result = result.Substring(2);
                        newFileName = "[" + result + "]";
                        break;

                    case Utilities.ExifPropertyDataTypes.LongFractionArray:
                        result = "";
                        item_size = 8;
                        num_items = propDataLength / item_size;
                        for (int i = 0; i < num_items; i++)
                        {
                            int numerator = BitConverter.ToInt32(
                                propData, i * item_size);
                            int denominator = BitConverter.ToInt32(
                                propData,
                                i * item_size + item_size / 2);
                            result += ", " + numerator.ToString() +
                                "/" + denominator.ToString();
                        }
                        if (result.Length > 0) result = result.Substring(2);
                        newFileName = "[" + result + "]";
                        break;
                }

                file.NewFileName = newFileName + file.FileExtension;
            }

            ListBox_ImageData.SelectionChanged += ListBox_ImageData_SelectionChanged;

        }

        private void Button_Rename_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Are you sure you want to rename image file(s)?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                return;
            }
            List<File> tempList = CloneList(dataObject.FileList);
            int total = tempList.Count;
            List<string> ErrorMessages = new List<string>();
            foreach (File f in tempList)
            {
                try
                {
                    System.IO.File.Move(f.FilePath, f.FileDirectory + @"\" + f.NewFileName);
                    dataObject.FileList.Remove(f);
                }
                catch (Exception ex)
                {
                    ErrorMessages.Add(f.FileName + ": " + ex.Message);
                }
            }

            if (ErrorMessages.Count == 0)
            {
                MessageBox.Show("Files successfully renamed", "Result");
            }
            else
            {
                string error = "";
                error += total - ErrorMessages.Count + " files were renamed." + Environment.NewLine + Environment.NewLine;
                error += ErrorMessages.Count + " files weren't renamed:" + Environment.NewLine + Environment.NewLine;
                foreach (string errorMessage in ErrorMessages)
                {
                    error += errorMessage + Environment.NewLine;
                }
                MessageBox.Show(error, "Result");
            }

            if (total != ErrorMessages.Count)
            {
                LoadExifListBox();
            }
        }

        private List<File> CloneList(ObservableCollection<File> input)
        {
            List<File> newList = new List<File>();
            foreach (File f in input)
            {
                newList.Add(f);
            }
            return newList;
        }

        private void ListBox_Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Files.SelectedItems.Count > 0)
            {
                Button_RemoveSelected.IsEnabled = true;
            }
            else
            {
                Button_RemoveSelected.IsEnabled = false;
            }
        }

        private void Button_RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            List<File> selectedFiles = new List<File>();
            foreach (File f in ListBox_Files.SelectedItems)
            {
                selectedFiles.Add(f);
            }

            foreach (File f in selectedFiles)
            {
                dataObject.FileList.Remove(f);
            }

            LoadExifListBox();
        }

        private void Button_RemoveAll_Click(object sender, RoutedEventArgs e)
        {
            dataObject.FileList.Clear();

            ListBox_ImageData.SelectionChanged -= ListBox_ImageData_SelectionChanged;
            dataObject.PropList.Clear();
            ListBox_ImageData.SelectionChanged += ListBox_ImageData_SelectionChanged;
        }
    }
}
