using EcommerceImport.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceImport.Data
{
    public partial class WEB_ItemType
    {
        public ICollection<WEB_ItemType_CreditGLs> creditGLs = new HashSet<WEB_ItemType_CreditGLs>();
        
        
    }
}
