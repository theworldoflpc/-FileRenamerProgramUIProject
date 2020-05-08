using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileRenamer
{
    public class Prop
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Utilities.ExifPropertyDataTypes Type { get; set; }

        public Prop(PropertyItem pi)
        {
            Id = pi.Id;
            string tempName = Enum.GetName(typeof(Utilities.ExifPropertyTypes), pi.Id);
            Name = tempName != null ? tempName : "Unknown Property";
            Type = (Utilities.ExifPropertyDataTypes)pi.Type;
        }
    }
}
