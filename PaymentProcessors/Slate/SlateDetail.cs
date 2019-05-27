using EcommerceImport.Data;
using EcommerceImport.PaymentProcessors.Slate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static EcommerceImport.Utilities;
using static EcommerceImport.Logging;


namespace EcommerceImport.Data
{
    public partial class SlateDetail
    {
        
        public void Import()
        {
            SetupVariables();

            if (GLAccountsValid())
            {
                CreateWhitworthInvoice();

                if (Convert.ToDouble(WEB_Invoices.WEB_Invoice_Items.Sum(item => item.Cost) ?? 0) != netAmount)
                {
                    //Bug here? No error message being sent?
                }
            }
        }

        private void SetupVariables()
        {
            WEB_Invoices = this.WEB_Invoices;
            SetNumber = this.SetNumber;
            SlateDetailGLNumber = this.SlateDetailGLNumber;
            SlateDetailGLNumber1 = this.SlateDetailGLNumber1;
        }

        private void CreateWhitworthInvoice()
        {
            AddMainInvoice();
            AddCustomer();
            AddSetNumber();
            AddPayment();
            AddItems();
            SaveInvoiceToDatabase();
        }

        private static void SaveInvoiceToDatabase()
        {
            db.SaveChanges();
        }

        private void AddItems()
        {
            if (IsEnrollmentDeposit(paymentDescription))
            {
                AddEnrollmentDepositItems();
            }
                     else
            {
                AddRegularEventItems();
            }
        }

        private void AddRegularEventItems()
        {
            //Using GL account numbers provided by Slate import file
            AddMainPaymentItem(SlateDetailGLNumber.accountNumber);
            AddFeeItem(SlateDetailGLNumber1.accountNumber);
        }

               private void AddEnrollmentDepositItems()
        {
            if (action == "Rejected")
            {
                AddRejectedItem();
            }
            else
            {
                if (netAmount == 350)
                {
                    AddItem(ENROLLMENT_DEPOSIT_MAIN_ITEM_TYPE_ID, ENROLLMENT_DEPOSIT_MAIN_AMOUNT);
                }

                AddItem(ENROLLMENT_DEPOSIT_HOUSING_ITEM_TYPE_ID, ENROLLMENT_DEPOSIT_HOUSING_AMOUNT);
                AddItem(ENROLLMENT_DEPOSIT_MATRICULATION_ITEM_TYPE_ID, ENROLLMENT_DEPOSIT_MATRICULATION_AMOUNT);
            }
            
        }

        private void AddRejectedItem()
        {
            fee = netAmount;
            netAmount = 0;
            AddFeeItem(SlateDetailGLNumber1.accountNumber);
        }

        private void AddMainPaymentItem(string accountNumber)
        {
            int itemTypeID = new ItemTypeCreator(paymentDescription + " - Main Payment Portion", accountNumber, SLATE_BANK_GL_ACCOUNT).GetItemType(GL_ACCOUNT_CHARGETYPE);

            double paymentAmount = netAmount + fee;

            AddItem(itemTypeID, paymentAmount);
        }
        
        private bool Refunded()
        {
            return action == "Refunded";
        }

        private void AddFeeItem(string accountNumber)
        {
            int itemTypeID = new ItemTypeCreator(paymentDescription + " - Fee Portion", SLATE_BANK_GL_ACCOUNT, accountNumber).GetItemType(GL_ACCOUNT_CHARGETYPE);
            AddItem(itemTypeID, fee);
        }

        private void AddItem(int itemTypeID, double amount)
        {
            WEB_Invoice_Items item = new WEB_Invoice_Items();

            if (Refunded() && ItemTypeIsStudentAccountCredit(itemTypeID))
            {
                amount = amount * -1;
            }

            item.Invoice = WEB_Invoices.Inv_NO;
            item.Item_Type = itemTypeID;
            item.Set_Number = SetNumber.Set_Number;
            item.Active = true;
            item.Cost = Convert.ToDecimal(amount);
            item.Quantity = 1;
            item.Total = Convert.ToDecimal(amount);
            item.Purchase_Date = date;
            item.Process_Date = date;
            item.Imported = true;
            item.Cancel = false;

            WEB_Invoices.WEB_Invoice_Items.Add(item);
        }

        private bool ItemTypeIsStudentAccountCredit(int itemTypeID)
        {
            return db.WEB_ItemType.Any(it => it.ChargeType == STUDENT_ACCOUNT_CREDIT_CHARGETYPE && it.ID == itemTypeID);
        }



        private void AddMainInvoice()
        {
            WEB_Invoices = new WEB_Invoices();
            
            WEB_Invoices.Inv_DATE = date;
            WEB_Invoices.Inv_TYPE = SLATE_PAYMENTS_INVOICE_TYPE;
        }

        private void AddPayment()
        {
            WEB_Payments payment = new WEB_Payments();

            payment.Invoice = WEB_Invoices.Inv_NO;
            payment.Set_Number = SetNumber.Set_Number;
            payment.Total = Convert.ToDecimal(netAmount + fee);
            payment.Type = 1;
            payment.Data_Date = date;

            if (Refunded())
            {
                payment.Cancel_Date = date;
                payment.Cancel_Processed = "1";
            }
            else
            {
                payment.Paid_Date = date;
                payment.Payment_Processed = "1";
           }

            WEB_Invoices.WEB_Payments.Add(payment);
        }

        private void AddSetNumber()
        {
            SetNumber = new SetNumber();

            WEB_Invoices.SetNumbers.Add(SetNumber);
        }

        private void AddCustomer()
        {
            WEB_Customer customer = new WEB_Customer();
            customer.FULL_NAME = firstName + " " + lastName;
            customer.FIRST_NAME = firstName;
            customer.LAST_NAME = lastName;
            customer.Whit_ID = whitworthID;

            WEB_Invoices.WEB_Customer = customer;
        }

        public override string ToString()
        {
            List<string> displayProperties = new List<string>();

            displayProperties.Add(firstName);
            displayProperties.Add(lastName);
            displayProperties.Add(whitworthID);
            displayProperties.Add(paymentDescription);
            displayProperties.Add(netAmount.ToString());
            displayProperties.Add(SlateDetailGLNumber.accountNumber);

            return String.Join(", ", displayProperties);
        }

        public bool GLAccountsValid()
        {
            bool accountsValid = true;

            if (paymentDescription.ToLower().Trim() != "whitworth deposit")
            {
                if (!ValidateGLAccount(SlateDetailGLNumber))
                {
                    accountsValid = false;
                }

                if (!ValidateGLAccount(SlateDetailGLNumber1))
                {
                    accountsValid = false;
                }
            }

            return accountsValid;
        }

        private bool ValidateGLAccount(SlateDetailGLNumber account)
        {
            if (!account.AccountIsValid())
            {
                logger.LogNotification(new InvalidGLAccountError(this, account));

                return false;
            }

            return true;
        }

        public int GetInvoiceNumber()
        {
            if (WEB_Invoices != null)
            {
                return WEB_Invoices.Inv_NO;
            }

            return 0;
        }
    }

    
}
