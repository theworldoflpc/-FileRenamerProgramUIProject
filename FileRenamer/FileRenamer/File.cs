using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FileRenamer
{
    public class File : INotifyPropertyChanged
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string FileDirectory { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private string newFileName;
        public string NewFileName {
            get { return newFileName; }
            set
            {
                if (newFileName != value)
                {
                    newFileName = value;
                    NotifyPropertyChanged("NewFileName");
                }
            }
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public File(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            FileExtension = Path.GetExtension(filePath);
            FileDirectory = Path.GetDirectoryName(filePath);
            NewFileName = "";
        }

    }
}
