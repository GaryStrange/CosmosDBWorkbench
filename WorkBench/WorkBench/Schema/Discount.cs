using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkBench.DataAccess.SchemaAttributes;

namespace WorkBench.Schema
{
    public class Discount : Document
    {
        public Discount()
        {
            SpendRestriction = new SpendRestriction();
            UsageRestriction = new UsageRestriction();
            UserRestriction = new UserRestriction();
            Validity = new DateRestriction();
        }

        public string Description { get; set; }

        public string DiscountCode { get; set; }

        public SpendRestriction SpendRestriction { get; set; }

        public UsageRestriction UsageRestriction { get; set; }

        public UserRestriction UserRestriction { get; set; }

        public DateRestriction Validity { get; set; }

        public Promotion Promotion { get; set; }

        public bool IsActive { get; set; }
    }

    public class SpendRestriction
    {
        public decimal MinimumSpend { get; set; }

        public decimal MaximumSpend { get; set; }
    }

    public class UsageRestriction
    {
        public bool HasUsageLimit { get; set; }

        public int MaxUsages { get; set; }
    }

    public class UserRestriction
    {
        public bool HasCustomerLimit { get; set; }

        public int MaxCustomers { get; set; }
    }

    public class DateRestriction
    {
        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }
    }

    public class DeliveryCountryRestriction
    {
        private IEnumerable<string> _allowedDeliveryCountries;

        public IEnumerable<string> AllowedDeliveryCountries
        {
            get { return _allowedDeliveryCountries ?? (_allowedDeliveryCountries = Enumerable.Empty<string>()); }
            set { _allowedDeliveryCountries = value; }
        }
    }

    public enum PromotionType
    {
        None = 0,
        PercentageOff = 1,
        AmountOff = 2,
        DeliveryAmountOff = 3
    }

    public class Promotion : Document
    {
        public Promotion()
        {
            SpendRestriction = new SpendRestriction();
            UsageRestriction = new UsageRestriction();
            UserRestriction = new UserRestriction();
            Validity = new DateRestriction();
            DeliveryCountryRestriction = new DeliveryCountryRestriction();
        }


        public string Name { get; set; }

        public decimal BenefitValue { get; set; }

        public DeliveryCountryRestriction DeliveryCountryRestriction { get; set; }

        public DateRestriction Validity { get; set; }

        public SpendRestriction SpendRestriction { get; set; }

        public UsageRestriction UsageRestriction { get; set; }

        public UserRestriction UserRestriction { get; set; }

        public bool IsActive { get; set; }

        public int CampaignCategoryId { get; set; }

        public PromotionType PromotionType { get; set; }

        public bool IsExcludeMarkDown { get; set; }
    }
}
