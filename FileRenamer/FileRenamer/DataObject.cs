using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileRenamer
{
    class DataObject
    {
        public ObservableCollection<File> FileList { get; set; }
        public ObservableCollection<Prop> PropList { get; set; }

        public DataObject()
        {
            FileList = new ObservableCollection<File>();
            PropList = new ObservableCollection<Prop>();
        }
    }
}
