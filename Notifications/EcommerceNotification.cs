using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;


namespace EcommerceImport
{
    public abstract class Notification
    {
        public string administratorEmail = PROGRAMMER_EMAIL;
        public string message;
        public int invoice;
        public bool notifyAdministrators = false;

        abstract public void SetMessage();

        protected void NotifyAdministrators()
        {

        }
    }

    
}
