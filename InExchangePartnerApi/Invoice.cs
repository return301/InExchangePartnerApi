using System;
using System.Globalization;

namespace InExchangePartnerApi

{
    public class Invoice
    {
        //header values
        public string? InvoiceNumber { get; set; } //Mandatory - Invoice number
        public DateTime IssueDate { get; set; } //Mandatory - Invoice date
        public DateTime DueDate { get; set; } //Mandatory - Due date
        public string? InvoiceNote { get; set; } // Generic invoice note, header level
        public string? InvoiceCurrencyCode { get; set; } //Mandatory
        public string? ReferenceCode { get; set; } //Buyer reference code/person
        public string? BuyerOrderNumber { get; set; } //Buyer purchase order number
        public string? SellerOrderNumber { get; set; } //Seller order number/invoiced object
        public string? AccountingCost { get; set; } //Buyer accounting string
        public DateTime InvoicePeriodStart { get; set; } //Invoiced period start date
        public DateTime InvoicePeriodEnd { get; set; } //Invoiced period end date
        public string? ContractNumber { get; set; } //Referenced contract number
        public string? PaymentTerms { get; set; } //Payment terms, e.g. "30 days net"

        //seller-sender-supplier details
        public string? SellerInternalIdentifier { get; set; } //Internal identifier for seller company
        public string? SellerGLN { get; set; } //Seller GLN number, issued by GS1
        public string? SellerORG { get; set; } //Mandatory - Seller organizational (or equivalent) number, digits only for SE
        public string? SellerVAT { get; set; }  //Seller VAT (or equivalent) value
        public string? SellerCountryCode { get; set; } //Mandatory - Seller country code according to iso-3166-alpha2
        public string? SellerCompanyName { get; set; } //Seller legal company name
        public string? SellerStreetName { get; set; } //Seller streetname
        public string? SellerAdditionalStreetName { get; set; } //Seller additional streetname
        public string? SellerPostalZone { get; set; } //Seller postalzone
        public string? SellerCityName { get; set; } //Seller cityname
        public string? SellerContactName { get; set; } //Seller contact person - name
        public string? SellerContactElectronicMail { get; set; } //Seller contact person - email
        public string? SellerContactTelephone { get; set; } //Seller contact person - telephone
        public string? BankGiro { get; set; } //Payment account: Swedish bankgiro, digits only
        public string? PlusGiro { get; set; } //Payment account: Swedish plusgiro, digits only
        public string? Iban { get; set; } //Payment account: IBAN, also set "bic/swift" below
        public string? Bic { get; set; } //BIC in relation to IBAN or Bank account
        public string? BankAccount { get; set; } //Payment account: local bank account, also set "bic" above, no special characters
        public string? PaymentReference { get; set; } //Payment account: OCR/payment reference when paying

        //buyer-receiver-customer details
        public string? BuyerInternalIdentifier { get; set; } //Internal identifier for buyer, e.g. customer number 
        public string? BuyerGLN { get; set; } //Buyer GLN number, issued by GS1
        public string? BuyerORG { get; set; } //Mandatory - Seller organizational (or equivalent) number, digits only for SE
        public string? BuyerVAT { get; set; } //Buyer VAT (or equivalent) value
        public string? BuyerCountryCode { get; set; } //Mandatory - Buyer country code according to iso-3166-alpha2
        public string? BuyerCompanyName { get; set; } //Buyer legal company name
        public string? BuyerStreetName { get; set; } //Buyer streetname
        public string? BuyerAdditionalStreetName { get; set; } //Buyer additional streetname
        public string? BuyerPostalZone { get; set; } //Buyer postalzone
        public string? BuyerCityName { get; set; } //Buyer cityname
        public string? BuyerContactName { get; set; } //Buyer contact person  - name
        public string? BuyerContactElectronicMail { get; set; } //Buyer contact person - email
        public string? BuyerContactTelephone { get; set; } //buyer contact person - telephone

        //legal totals, used for validating previous values
        public float? LineTotalAmount { get; set; } //total of all lines
        public float? TaxExclusiveAmount { get; set; } // total excl. VAT
        public float? TaxInclusiveAmount { get; set; } // total incl. VAT (without rounding)
        public float? RoundingAmount { get; set; } // rounding amount
        public float? PayableAmount { get; set; } // amount after rounding


        //invoice line list
        public List<InvoiceLine>? lines = new();

        public void AddInvoiceLine(string description, float quantity, string unitcode, float unitPrice, float lineAmount, int vatRate)
        {
            lines?.Add(new InvoiceLine { Name = description, Quantity = quantity, UnitCode = unitcode, UnitPrice = unitPrice, LineAmount = lineAmount, VatRate = vatRate });
        }

    }

    //add invoice lines, minimum of one, needs to validate against the provided legal total
    public class InvoiceLine
    {
        public string? Name { get; set; } //description for line item
        public float Quantity { get; set; } // quantity
        public string? UnitCode { get; set; } //unitcode, Unecerec20/21
        public float UnitPrice { get; set; }   // price per unit, unlimited decimals possible
        public float LineAmount { get; set; } // line amount, two decimals rounded only
        public int VatRate { get; set; } // vat %
    }
}