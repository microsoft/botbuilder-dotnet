// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Address within a Payment Request.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentAddress
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentAddress"/> class.
        /// </summary>
        public PaymentAddress()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentAddress"/> class.
        /// </summary>
        /// <param name="country">This is the CLDR (Common Locale Data
        /// Repository) region code. For example, US, GB, CN, or JP.</param>
        /// <param name="addressLine">This is the most specific part of the
        /// address. It can include, for example, a street name, a house
        /// number, apartment number, a rural delivery route, descriptive
        /// instructions, or a post office box number.</param>
        /// <param name="region">This is the top level administrative
        /// subdivision of the country. For example, this can be a state, a
        /// province, an oblast, or a prefecture.</param>
        /// <param name="city">This is the city/town portion of the
        /// address.</param>
        /// <param name="dependentLocality">This is the dependent locality or
        /// sublocality within a city. For example, used for neighborhoods,
        /// boroughs, districts, or UK dependent localities.</param>
        /// <param name="postalCode">This is the postal code or ZIP code, also
        /// known as PIN code in India.</param>
        /// <param name="sortingCode">This is the sorting code as used in, for
        /// example, France.</param>
        /// <param name="languageCode">This is the BCP-47 language code for the
        /// address. It's used to determine the field separators and the order
        /// of fields when formatting the address for display.</param>
        /// <param name="organization">This is the organization, firm, company,
        /// or institution at this address.</param>
        /// <param name="recipient">This is the name of the recipient or
        /// contact person.</param>
        /// <param name="phone">This is the phone number of the recipient or
        /// contact person.</param>
        public PaymentAddress(string country = default, IList<string> addressLine = default, string region = default, string city = default, string dependentLocality = default, string postalCode = default, string sortingCode = default, string languageCode = default, string organization = default, string recipient = default, string phone = default)
        {
            Country = country;
            AddressLine = addressLine;
            Region = region;
            City = city;
            DependentLocality = dependentLocality;
            PostalCode = postalCode;
            SortingCode = sortingCode;
            LanguageCode = languageCode;
            Organization = organization;
            Recipient = recipient;
            Phone = phone;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets this is the CLDR (Common Locale Data Repository)
        /// region code. For example, US, GB, CN, or JP.
        /// </summary>
        /// <value>The country by CLDR region code.</value>
        [JsonPropertyName("country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets this is the most specific part of the address. It can
        /// include, for example, a street name, a house number, apartment
        /// number, a rural delivery route, descriptive instructions, or a post
        /// office box number.
        /// </summary>
        /// <value>The most specific part of the address.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("addressLine")]
        public IList<string> AddressLine { get; set; }

        /// <summary>
        /// Gets or sets this is the top level administrative subdivision of
        /// the country. For example, this can be a state, a province, an
        /// oblast, or a prefecture.
        /// </summary>
        /// <value>The region. This is the top level administrative subdivision of the country. (e.g. state, province, etc.)</value>
        [JsonPropertyName("region")]
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets this is the city/town portion of the address.
        /// </summary>
        /// <value>The city or town.</value>
        [JsonPropertyName("city")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets this is the dependent locality or sublocality within a
        /// city. For example, used for neighborhoods, boroughs, districts, or
        /// UK dependent localities.
        /// </summary>
        /// <value>The dependent locality or sublocality within a city.</value>
        [JsonPropertyName("dependentLocality")]
        public string DependentLocality { get; set; }

        /// <summary>
        /// Gets or sets this is the postal code or ZIP code, also known as PIN
        /// code in India.
        /// </summary>
        /// <value>The postal code or ZIP code.</value>
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets this is the sorting code as used in, for example,
        /// France.
        /// </summary>
        /// <value>The sorting code.</value>
        [JsonPropertyName("sortingCode")]
        public string SortingCode { get; set; }

        /// <summary>
        /// Gets or sets this is the BCP-47 language code for the address. It's
        /// used to determine the field separators and the order of fields when
        /// formatting the address for display.
        /// </summary>
        /// <value>The BCP-47 language code for the address.</value>
        [JsonPropertyName("languageCode")]
        public string LanguageCode { get; set; }

        /// <summary>
        /// Gets or sets this is the organization, firm, company, or
        /// institution at this address.
        /// </summary>
        /// <value>The organization, firm, company, or institution at the address.</value>
        [JsonPropertyName("organization")]
        public string Organization { get; set; }

        /// <summary>
        /// Gets or sets this is the name of the recipient or contact person.
        /// </summary>
        /// <value>The recipient or contact person.</value>
        [JsonPropertyName("recipient")]
        public string Recipient { get; set; }

        /// <summary>
        /// Gets or sets this is the phone number of the recipient or contact
        /// person.
        /// </summary>
        /// <value>The phone number of the recipient or contact person.</value>
        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
