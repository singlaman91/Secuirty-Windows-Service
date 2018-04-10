using System;

namespace EZLC
{
    public class ConfigParams
    {
        public int ID { get; set; }

        public string Region { get; set; }

        public string DeviceType { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public string FrequencyValue { get; set; }

        public string IssuerName { get; set; }

        public string CertTemplateName { get; set; }

        public string CertTemplateGuidName { get; set; }

        public bool IsActive { get; set; }

        public DateTime AppliedOn { get; set; }

        public DateTime FileCreationDate { get; set; }
    }
}
