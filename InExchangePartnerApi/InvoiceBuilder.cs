using InExchangePartnerApi;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;

namespace InExchangePartnerApi
{
    public class InvoiceBuilder
    {
        public InvoiceBuilder(Invoice i)
        {
            string fileName = "Invoice_" + i.InvoiceNumber?.ToString() + ".xml";

            using (XmlWriter w = XmlWriter.Create(fileName))
            {

                /* All comments marked "- Placeholder" are placed according to schema location
                 * extend functionality by replacing these with new functions, keeping it's original position
                 */



                //start tag and xmlns declarations
                w.WriteStartElement("Invoice", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
                w.WriteAttributeString("xmlns", "cbc", null, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                w.WriteAttributeString("xmlns", "cac", null, "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                w.WriteAttributeString("xmlns", "ext", null, "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
                w.WriteAttributeString("xmlns", "cnt", null, "urn:oasis:names:specification:ubl:schema:xsd:CountryIdentificationCode-2");
                w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");

                //build header objects
                BuildBasicElement(w, "UBLVersionID", "2.1", null, null);
                BuildBasicElement(w, "CustomizationID", "urn:cen.eu:en16931:2017#compliant#urn:fdc:peppol.eu:2017:poacc:billing:3.0", null, null);
                BuildBasicElement(w, "ProfileID", "urn:fdc:peppol.eu:2017:poacc:billing:01:1.0", null, null);
                BuildBasicElement(w, "ID", i.InvoiceNumber, null, null);
                BuildBasicElement(w, "IssueDate", i.IssueDate.ToString("yyyy'-'MM'-'dd"), null, null);
                BuildBasicElement(w, "DueDate", i.DueDate.ToString("yyyy'-'MM'-'dd"), null, null);
                BuildBasicElement(w, "InvoiceTypeCode", "380", null, null); //set invoice type code according to UNCL1001 subset
                BuildBasicElement(w, "Note", i.InvoiceNote, null, null);

                //TaxPointDate - Placeholder
                BuildBasicElement(w, "DocumentCurrencyCode", i.InvoiceCurrencyCode, null, null);

                //TaxCurrencyCode - Placeholder
                BuildBasicElement(w, "AccountingCost", i.AccountingCost, null, null);
                BuildBasicElement(w, "BuyerReference", i.ReferenceCode ?? "n/a", null, null); // according to PEPPOL-EN16931-R003
                BuildInvoicePeriod(w, i);
                BuildOrderNumbers(w, i);

                //BillingReference - Placeholder
                //DespatchDocumentReference - Placeholder
                //ReceiptDocumentReference - Placeholder
                //OriginatorDocumentReference - Placeholder
                //ContractDocumentReference - Placeholder
                //AdditionalDocumentReference - Placeholder
                //ProjectReference - Placeholder

                BuildParty(w, i, "AccountingSupplierParty");
                BuildParty(w, i, "AccountingCustomerParty");

                //PayeeParty - Placeholder
                //TaxRepresentativeParty - Placeholder
                //Delivery - Placeholder

                BuildPaymentAccounts(w, i);
                BuildPaymentTerms(w, i.PaymentTerms);

                //AllowanceCharge - Placeholder
                BuildTaxTotal(w, i);
                BuildLegalMonetaryTotal(w, i);
                BuildInvoiceLines(w, i);



                //EOF
                w.WriteEndElement();
                w.Flush();
            }
        }


        private static void BuildBasicElement(XmlWriter w, string name, string? value, string? attributeName, string? attributeValue)
        {
            if (value != null)
                w.WriteElementString("cbc", name, null, value);
        }

        private static void BuildInvoicePeriod(XmlWriter w, Invoice i)
        {
            if (i.InvoicePeriodStart != default | i.InvoicePeriodEnd != default)
            {
                w.WriteStartElement("cac", "InvoicePeriod", null);
                BuildBasicElement(w, "StartDate", i.InvoicePeriodStart.ToString("yyyy'-'MM'-'dd"), null, null);
                BuildBasicElement(w, "EndDate", i.InvoicePeriodEnd.ToString("yyyy'-'MM'-'dd"), null, null);
                w.WriteEndElement();
            }


        }

        private static void BuildOrderNumbers(XmlWriter w, Invoice i)
        {
            if (i.BuyerOrderNumber != null | i.SellerOrderNumber != null)
            {
                w.WriteStartElement("cac", "OrderReference", null);
                BuildBasicElement(w, "ID", i.BuyerOrderNumber ?? "n/a", null, null);
                BuildBasicElement(w, "SalesOrderID", i.SellerOrderNumber, null, null);
                w.WriteEndElement();
            }
        }

        private static void BuildParty(XmlWriter w, Invoice i, string partyType)
        {
            if (partyType == "AccountingSupplierParty")
            {
                w.WriteStartElement("cac", partyType, null);
                w.WriteStartElement("cac", "Party", null);
                BuildEndpointID(w, i.SellerCountryCode, i.SellerGLN, i.SellerORG, i.SellerVAT);
                w.WriteStartElement("cac", "PartyIdentification", null);
                BuildBasicElement(w, "ID", i.SellerInternalIdentifier ?? i.SellerGLN ?? i.SellerORG ?? i.SellerVAT, null, null);
                w.WriteEndElement();
                BuildCompanyName(w, i.SellerCompanyName);
                BuildPostalAddress(w, i.SellerStreetName, i.SellerAdditionalStreetName, i.SellerCityName, i.SellerPostalZone, i.SellerCountryCode);
                BuildPartyTaxScheme(w, i.SellerCountryCode, i.SellerVAT);
                BuildPartyLegalEntity(w, i.SellerCountryCode, i.SellerORG, i.SellerCompanyName);
                BuildPartyContact(w, i.SellerContactName, i.SellerContactTelephone, i.SellerContactElectronicMail);
                w.WriteEndElement();
                w.WriteEndElement();
            }
            else if (partyType == "AccountingCustomerParty")
            {
                w.WriteStartElement("cac", partyType, null);
                w.WriteStartElement("cac", "Party", null);
                BuildEndpointID(w, i.BuyerCountryCode, i.BuyerGLN, i.BuyerORG, i.BuyerVAT);
                w.WriteStartElement("cac", "PartyIdentification", null);
                BuildBasicElement(w, "ID", i.BuyerInternalIdentifier ?? i.BuyerGLN ?? i.BuyerORG ?? i.BuyerVAT, null, null);
                w.WriteEndElement();
                BuildCompanyName(w, i.BuyerCompanyName);
                BuildPostalAddress(w, i.BuyerStreetName, i.BuyerAdditionalStreetName, i.BuyerCityName, i.BuyerPostalZone, i.BuyerCountryCode);
                BuildPartyTaxScheme(w, i.BuyerCountryCode, i.BuyerVAT);
                BuildPartyLegalEntity(w, i.BuyerCountryCode, i.BuyerORG, i.BuyerCompanyName);
                BuildPartyContact(w, i.BuyerContactName, i.BuyerContactTelephone, i.BuyerContactElectronicMail);
                w.WriteEndElement();
                w.WriteEndElement();
            }

        }

        private static void BuildPaymentAccounts(XmlWriter w, Invoice i)
        {
            if (i.BankGiro != null & i.SellerCountryCode == "SE")
            {
                w.WriteStartElement("cac", "PaymentMeans", null);
                BuildBasicElement(w, "PaymentMeansCode", "30", null, null);
                BuildBasicElement(w, "PaymentID", i.PaymentReference, null, null);
                w.WriteStartElement("cac", "PayeeFinancialAccount", null);
                BuildBasicElement(w, "ID", i.BankGiro, null, null);
                w.WriteStartElement("cac", "FinancialInstitutionBranch", null);
                BuildBasicElement(w, "ID", "SE:BANKGIRO", null, null);
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteEndElement();
            }
            if (i.PlusGiro != null & i.SellerCountryCode == "SE")
            {
                w.WriteStartElement("cac", "PaymentMeans", null);
                w.WriteElementString("cbc", "PaymentMeansCode", null, "30");
                BuildBasicElement(w, "PaymentID", i.PaymentReference, null, null);
                w.WriteStartElement("cac", "PayeeFinancialAccount", null);
                BuildBasicElement(w, "ID", i.PlusGiro, null, null);
                w.WriteStartElement("cac", "FinancialInstitutionBranch", null);
                BuildBasicElement(w, "ID", "SE:PLUSGIRO", null, null);
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteEndElement();
            }
            if (i.Iban != null & i.Bic != null)
            {
                w.WriteStartElement("cac", "PaymentMeans", null);
                w.WriteElementString("cbc", "PaymentMeansCode", null, "30");
                BuildBasicElement(w, "PaymentID", i.PaymentReference, null, null);
                w.WriteStartElement("cac", "PayeeFinancialAccount", null);
                BuildBasicElement(w, "ID", i.Iban, null, null);
                w.WriteStartElement("cac", "FinancialInstitutionBranch", null);
                BuildBasicElement(w, "ID", i.Bic, null, null);
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteEndElement();
            }
            if (i.BankAccount != null)
            {
                w.WriteStartElement("cac", "PaymentMeans", null);
                w.WriteElementString("cbc", "PaymentMeansCode", null, "30");
                BuildBasicElement(w, "PaymentID", i.PaymentReference, null, null);
                w.WriteStartElement("cac", "PayeeFinancialAccount", null);
                BuildBasicElement(w, "ID", i.BankAccount, null, null);
                w.WriteStartElement("cac", "FinancialInstitutionBranch", null);
                BuildBasicElement(w, "ID", i.Bic, null, null);
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteEndElement();
            }

        }

        private static void BuildPaymentTerms(XmlWriter w, string? terms)
        {
            if (terms != null)
            {
                w.WriteStartElement("cac", "PaymentTerms", null);
                BuildBasicElement(w, "Note", terms, null, null);
                w.WriteEndElement();
            }
        }

        private static void BuildEndpointID(XmlWriter w, string? countryCode, string? gln, string? org, string? vat)
        {
            w.WriteStartElement("cbc", "EndpointID", null);
            string calculatedSchemeID;
            string? calculatedIdentifier;
            if (gln != null)
            {
                w.WriteAttributeString("schemeID", "0088");
                w.WriteString(gln);
            }
            else
            {
                switch (countryCode)
                {
                    case "NO":
                        calculatedSchemeID = "0192";
                        calculatedIdentifier = org;
                        break;
                    case "DK":
                        calculatedSchemeID = "0184";
                        calculatedIdentifier = vat ?? ("DK" + org);
                        break;
                    case "FI":
                        calculatedSchemeID = "0037";
                        calculatedIdentifier = org;
                        break;
                    default:
                        calculatedSchemeID = "0007";
                        calculatedIdentifier = org;
                        break;

                }

                w.WriteAttributeString("schemeID", calculatedSchemeID);
                w.WriteString(calculatedIdentifier);
            }
            w.WriteEndElement();
        }




        private static void BuildCompanyName(XmlWriter w, string? name)
        {
            w.WriteStartElement("cac", "PartyName", null);
            BuildBasicElement(w, "Name", name, null, null);
            w.WriteEndElement();
        }

        private static void BuildPostalAddress(XmlWriter w, string? streetName, string? additionalStreetName, string? cityName, string? postalZone, string? countryCode)
        {
            w.WriteStartElement("cac", "PostalAddress", null);
            BuildBasicElement(w, "StreetName", streetName, null, null);
            BuildBasicElement(w, "AdditionalStreetName", additionalStreetName, null, null);
            BuildBasicElement(w, "CityName", cityName, null, null);
            BuildBasicElement(w, "PostalZone", postalZone, null, null);
            w.WriteStartElement("cac", "Country", null);
            BuildBasicElement(w, "IdentificationCode", countryCode, null, null);
            w.WriteEndElement();
            w.WriteEndElement();
        }

        private static void BuildPartyTaxScheme(XmlWriter w, string? countryCode, string? vat)
        {
            if (vat != null)
            {
                w.WriteStartElement("cac", "PartyTaxScheme", null);
                BuildBasicElement(w, "CompanyID", vat, null, null);
                w.WriteStartElement("cac", "TaxScheme", null);
                BuildBasicElement(w, "ID", "VAT", null, null);
                w.WriteEndElement();
                w.WriteEndElement();

            }

            switch (countryCode)
            {
                case "SE":
                    w.WriteStartElement("cac", "PartyTaxScheme", null);
                    BuildBasicElement(w, "CompanyID", "Godkänd för F-skatt", null, null);
                    w.WriteStartElement("cac", "TaxScheme", null);
                    BuildBasicElement(w, "ID", "TAX", null, null);
                    w.WriteEndElement();
                    w.WriteEndElement();
                    break;
                case "NO":
                    w.WriteStartElement("cac", "PartyTaxScheme", null);
                    BuildBasicElement(w, "CompanyID", "Foretaksregisteret", null, null);
                    w.WriteStartElement("cac", "TaxScheme", null);
                    BuildBasicElement(w, "ID", "TAX", null, null);
                    w.WriteEndElement();
                    w.WriteEndElement();
                    break;
                default:
                    break;

            }
        }

        private static void BuildPartyLegalEntity(XmlWriter w, string? countryCode, string? org, string? companyName)
        {
            w.WriteStartElement("cac", "PartyLegalEntity", null);
            BuildBasicElement(w, "RegistrationName", companyName, null, null);
            w.WriteStartElement("cbc", "CompanyID", null);
            switch (countryCode)
            {
                case "SE":
                    w.WriteAttributeString("schemeID", "0007");
                    break;
                case "NO":
                    w.WriteAttributeString("schemeID", "0192");
                    break;
                case "FI":
                    w.WriteAttributeString("schemeID", "0037");
                    break;
                default:
                    break;
            }
            w.WriteString(org);
            w.WriteEndElement();
            w.WriteEndElement();
        }

        private static void BuildPartyContact(XmlWriter w, string? name, string? phone, string? email)
        {
            if (name != null & phone != null & email != null)
            {
                w.WriteStartElement("cac", "Contact", null);
                BuildBasicElement(w, "Name", name, null, null);
                BuildBasicElement(w, "Telephone", phone, null, null);
                BuildBasicElement(w, "ElectronicMail", email, null, null);
                w.WriteEndElement();
            }

        }

        private static void BuildLegalMonetaryTotal(XmlWriter w, Invoice i)
        {
            w.WriteStartElement("cac", "LegalMonetaryTotal", null);

            w.WriteStartElement("cbc", "LineExtensionAmount", null);
            w.WriteAttributeString("currencyID", i.InvoiceCurrencyCode);
            w.WriteString(i.LineTotalAmount.ToString()); ;
            w.WriteEndElement();

            w.WriteStartElement("cbc", "TaxExclusiveAmount", null);
            w.WriteAttributeString("currencyID", i.InvoiceCurrencyCode);
            w.WriteString(i.TaxExclusiveAmount.ToString()); ;
            w.WriteEndElement();

            w.WriteStartElement("cbc", "TaxInclusiveAmount", null);
            w.WriteAttributeString("currencyID", i.InvoiceCurrencyCode);
            w.WriteString(i.TaxInclusiveAmount.ToString()); ;
            w.WriteEndElement();

            w.WriteStartElement("cbc", "PayableRoundingAmount", null);
            w.WriteAttributeString("currencyID", i.InvoiceCurrencyCode);
            w.WriteString(i.RoundingAmount.ToString()); ;
            w.WriteEndElement();

            w.WriteStartElement("cbc", "PayableAmount", null);
            w.WriteAttributeString("currencyID", i.InvoiceCurrencyCode);
            w.WriteString(i.PayableAmount.ToString()); ;
            w.WriteEndElement();


            w.WriteEndElement();
        }

        private static void BuildTaxTotal(XmlWriter w, Invoice i)
        {
            float taxableAmount25 = 0;
            float taxableAmount12 = 0;
            float taxableAmount6 = 0;
            float taxableAmount0 = 0;

            //iterate through lines, add to taxable for each vatRate
            if (i.lines != null)
            {
                foreach (var line in i.lines)
                {
                    if (line.VatRate == 25)
                        taxableAmount25 += line.LineAmount;
                    else if (line.VatRate == 12)
                        taxableAmount12 += line.LineAmount;
                    else if (line.VatRate == 6)
                        taxableAmount6 += line.LineAmount;
                    else if (line.VatRate == 0)
                        taxableAmount0 += line.LineAmount;
                }
            }



            //awayfromzero according to rounding rules
            double taxAmount25 = Math.Round(taxableAmount25 * 0.25F, 2, MidpointRounding.AwayFromZero);
            double taxAmount12 = Math.Round(taxableAmount12 * 0.12F, 2, MidpointRounding.AwayFromZero);
            double taxAmount6 = Math.Round(taxableAmount6 * 0.06F, 2, MidpointRounding.AwayFromZero);

            //begin tax total
            w.WriteStartElement("cac", "TaxTotal", null);

            //total tax amount
            w.WriteStartElement("cbc", "TaxAmount", null);
            w.WriteAttributeString("currencyID", i.InvoiceCurrencyCode);
            w.WriteString(taxAmount25.ToString());
            w.WriteEndElement();

            //TaxSubTotal for vatRate 25, implement logic to iterate through each where value != 0
            w.WriteStartElement("cac", "TaxSubtotal", null);

            w.WriteStartElement("cbc", "TaxableAmount", null);
            w.WriteAttributeString("currencyID", i.InvoiceCurrencyCode);
            w.WriteString(taxableAmount25.ToString());
            w.WriteEndElement();

            w.WriteStartElement("cbc", "TaxAmount", null);
            w.WriteAttributeString("currencyID", i.InvoiceCurrencyCode);
            w.WriteString(taxAmount25.ToString());
            w.WriteEndElement();

            w.WriteStartElement("cac", "TaxCategory", null);
            w.WriteElementString("cbc", "ID", null, "S");
            w.WriteElementString("cbc", "Percent", null, "25");
            w.WriteStartElement("cac", "TaxScheme", null);
            w.WriteElementString("cbc", "ID", null, "VAT");
            w.WriteEndElement();
            w.WriteEndElement();
            //end taxsubtotal
            w.WriteEndElement();

            //end taxtotal
            w.WriteEndElement();
        }
        private static void BuildInvoiceLines(XmlWriter w, Invoice inv)
        {
            //Build lines based on List of InvoiceLine objects
            int lineId = 1;
            if (inv.lines != null)
            {
                foreach (var line in inv.lines)
                {
                    w.WriteStartElement("cac", "InvoiceLine", null);
                    BuildBasicElement(w, "ID", (lineId).ToString(), null, null);
                    w.WriteStartElement("cbc", "InvoicedQuantity", null);
                    w.WriteAttributeString("unitCode", line.UnitCode);
                    w.WriteString(line.Quantity.ToString());
                    w.WriteEndElement();

                    w.WriteStartElement("cbc", "LineExtensionAmount", null);
                    w.WriteAttributeString("currencyID", inv.InvoiceCurrencyCode);
                    w.WriteString(line.LineAmount.ToString());
                    w.WriteEndElement();
                    //item start
                    w.WriteStartElement("cac", "Item", null);
                    BuildBasicElement(w, "Name", line.Name, null, null);

                    w.WriteStartElement("cac", "ClassifiedTaxCategory", null);
                    w.WriteStartElement("cbc", "ID", null);
                    if (line.VatRate == 0) //"E" for exception (0% VAT), "S" for Standard (25/12/6 for SE)
                        w.WriteString("E");
                    else
                        w.WriteString("S");
                    w.WriteEndElement();
                    BuildBasicElement(w, "Percent", line.VatRate.ToString(), null, null);

                    w.WriteStartElement("cac", "TaxScheme", null);
                    BuildBasicElement(w, "ID", "VAT", null, null);
                    w.WriteEndElement();

                    w.WriteEndElement();


                    w.WriteEndElement();
                    //item end

                    w.WriteStartElement("cac", "Price", null);
                    w.WriteStartElement("cbc", "PriceAmount", null);
                    w.WriteAttributeString("currencyID", inv.InvoiceCurrencyCode);
                    w.WriteString(line.UnitPrice.ToString());
                    w.WriteEndElement();
                    w.WriteEndElement();


                    w.WriteEndElement();
                    lineId++;
                }
            }


        }


    }
}