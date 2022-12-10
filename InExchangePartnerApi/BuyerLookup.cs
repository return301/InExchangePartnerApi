using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InExchangePartnerApi
{
    // class created based on json object definition: https://test.inexchange.se/docs/ApiReference#buyer-lookup
    public class BuyerLookup
    {
        public Party[]? parties { get; set; }
        public int? potentialMatches { get; set; }
        public bool? exactHit { get; set; }
    }

    public class Party
    {
        public string? companyId { get; set; }
        public string? name { get; set; }
        public string? altName { get; set; }
        public string? gln { get; set; }
        public string? streetName { get; set; }
        public string? postBox { get; set; }
        public string? city { get; set; }
        public string? postalZone { get; set; }
        public string? countryCode { get; set; }
        public string? phoneNo { get; set; }
        public string? orgNo { get; set; }
        public string? vatNo { get; set; }
        public string? receiveElectronicInvoiceCapability { get; set; }
        public string? sendsElectronicOrderCapability { get; set; }
        public bool? hasInvoiceChecks { get; set; }
        public string? address { get; set; }
        public string? address2 { get; set; }
        public string? postalCode { get; set; }
        public string? connectionStatus { get; set; }
        public Invoicechecks? invoiceChecks { get; set; }
        public object[]? peppolParticipantIdentifiers { get; set; }
    }

    public class Invoicechecks
    {
        public bool? buyerReferenceNo { get; set; }
        public bool? buyerReferenceName { get; set; }
        public bool? buyerOrderNo { get; set; }
        public bool? buyerName { get; set; }
    }
}
