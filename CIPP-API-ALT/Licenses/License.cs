﻿using CIPP_API_ALT.Common;

namespace CIPP_API_ALT.Licenses
{
    /// <summary>
    /// Class used to define a M365 License SKU
    /// </summary>
    public class License
    {
        public readonly static List<License> Licenses;
        private enum SearchType { SkuName, SkuId };

        // Static constructor populates License objects from CSV file first time static License is called
        static License()
        {
            Licenses = Utilities.CsvToObjectList<License>(ApiEnvironment.LicenseConversionTableFile);
        }

        // Constructor builds a new license representing M365 License SKU
        public License(string productDisplayName, string stringId, string guid, string servicePlanName, string servicePlanId, string servicePlansIncludedFriendlyNames)
        {
            ProductDisplayName = productDisplayName;
            StringId = stringId;
            Guid = guid;
            ServicePlanName = servicePlanName;
            ServicePlanId = servicePlanId;
            ServicePlansIncludedFriendlyNames = servicePlansIncludedFriendlyNames;
        }

        /// <summary>
        /// Accepts a SKU Name or SKU ID and returns Product Name of the last match found
        /// </summary>
        /// <param name="skuName">SKU Name of the M365 License</param>
        /// <param name="skuId">SKU ID of the M365 License</param>
        /// <returns>Product Name that matches the last license that contains the supplied search parameters</returns>
        public static string ConvertSkuName(string skuName = "", string skuId = "")
        {
            string SearchLicenses(string value, SearchType search)
            {
                License? found = null;

                switch (search)
                {
                    case SearchType.SkuName:
                        found = Licenses.FindLast(x => x.StringId.Equals(value));
                        break;
                    case SearchType.SkuId:
                        found = Licenses.FindLast(y => y.Guid.Equals(value));
                        break;
                    default:
                        break;
                }

                if (found != null)
                {
                    return found.ProductDisplayName;
                }

                return value;
            }

            if (!string.IsNullOrEmpty(skuName))
            {
                return SearchLicenses(skuName, SearchType.SkuName);
            }

            return SearchLicenses(skuId, SearchType.SkuId);
        }

        public string ProductDisplayName {  get; set;  }
        public string StringId { get; set; }
        public string Guid { get; set; }
        public string ServicePlanName { get; set; }
        public string ServicePlanId { get; set; }
        public string ServicePlansIncludedFriendlyNames { get; set; }
    }
}

