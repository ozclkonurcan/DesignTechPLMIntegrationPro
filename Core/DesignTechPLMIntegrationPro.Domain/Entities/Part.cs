using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Domain.Entities
{
    public class ProdMgmtParts
    {
        public string Context { get; set; } = string.Empty;
        public List<Part> Value { get; set; }
    }


    public class Part : BaseEntity
    {

        [Key]
        public string? ID { get; set; }
        public string? Number { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public State? State { get; set; }
        public string? MuhasebeKodu { get; set; } = "0000000";
        public string? MuhasebeAdi { get; set; }
        public string? BirimAdi { get; set; }
        public string? BirimKodu { get; set; }

        public string? PlanlamaTipiKodu { get; set; } = "P";
        public string? Fai { get; set; }
        public string? PLM { get; set; } = "E";
        public CLASSIFICATION? CLASSIFICATION { get; set; }
        public string? EntegrasyonDurumu { get; set; }
        public string? EntegrasyonTarihi { get; set; }
        public List<Alternates>? Alternates { get; set; }


        public DateTime? CreatedOn { get; set; }
        public DateTime? LastModified { get; set; }
        public string? Version { get; set; }
        public string? VersionID { get; set; }
    }

    public class Creator
    {
        [Key]
        public string? ID { get; set; }
        public string? Identity { get; set; }
        public string? Name { get; set; }
    }
    public class WTUsers
    {
        [JsonProperty("value")]
        public List<User> Users { get; set; }
    }

    public class User
    {

        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("EMail")]
        public string EMail { get; set; }

        [JsonProperty("FullName")]
        public string FullName { get; set; }

    }


    public class AnaPart : BaseEntity
    {

        [Key]
        public string? ID { get; set; }
        public string? Number { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public State? State { get; set; }
        public string? MuhasebeKodu { get; set; } = "0000000";
        public string? BirimKodu { get; set; }

        public string? PlanlamaTipiKodu { get; set; } = "P";
        public string? Fai { get; set; } = "H";
        public string? PLM { get; set; } = "E";
        public CLASSIFICATION? CLASSIFICATION { get; set; }


    }
    public class AnaPartCancelled : BaseEntity
    {

        [Key]
        public string? Number { get; set; }
        public State? State { get; set; }
    }
    public class AnaPartCancelledLOG : BaseEntity
    {

        [Key]
        public string? Number { get; set; }
        public string? Name { get; set; }
        public State? State { get; set; }
    }
    public class MuadilPart : BaseEntity
    {

        public string? Number { get; set; }
        public List<Alternates2>? Alternates { get; set; }

    }
    public class RemovePart : BaseEntity
    {

        public string? Number { get; set; }
        public string? MuadilPartNumber { get; set; }
    }

    #region CLASSTIFICATION CLASS

    public class CLASSIFICATION
    {
        private string classificationHierarchy;

        public string ClfNodeHierarchyDisplayName
        {
            get { return classificationHierarchy; }
            set
            {
                classificationHierarchy = value;
                // Ayrıca ClassificationHierarchy'yi güncelle
                ClassificationHierarchy = value;
            }
        }

        public string ClassificationHierarchy { get; set; } = "NULL parça";
    }


    //public class CLASSIFICATION
    //{
    //	public string ClfNodeInternalName { get; set; }
    //	public string ClfNodeDisplayName { get; set; }
    //	public string ClfNodeHierarchyDisplayName { get; set; }
    //	public List<ClassificationAttribute> ClassificationAttributes { get; set; }
    //}

    public class ClassificationAttribute
    {
        [Key]
        public string InternalName { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }
        public string DisplayValue { get; set; }
    }
    #endregion



    #region Alternates Classı

    public class Alternates
    {
        [Key]
        public string? ID { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastModified { get; set; }
        public string? ObjectType { get; set; }
        public AlternatePart? AlternatePart { get; set; }
    }

    public class ReplacementType
    {
        [Key]
        public string Value { get; set; }
        public string Display { get; set; }
    }

    public class AlternatePart : BaseEntity
    {

        [Key]
        public string? ID { get; set; }
        public string? Number { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Version { get; set; }
        public State? State { get; set; }

        public string? MuhasebeAdi { get; set; }
        public string? MuhasebeKodu { get; set; } = "0000000";
        public string? BirimAdi { get; set; }
        public string? BirimKodu { get; set; }
        public DateTime? LastModified { get; set; }
        public string? PlanlamaTipiKodu { get; set; } = "P";
        public string? Fai { get; set; }
        public string? PLM { get; set; } = "E";
        public CLASSIFICATION? CLASSIFICATION { get; set; }





    }





    //Yedek ALternates

    public class Alternates2
    {
        [Key]
        public AlternatePart2? AlternatePart { get; set; }
    }



    public class AlternatePart2 : BaseEntity
    {

        [Key]
        public string? Number { get; set; }
        public bool isCancel { get; set; }

    }
    #endregion




    public class AlternateNumber
    {
        public string name { get; set; } = string.Empty;
        public string WTPartNumber { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
    public class AssemblyMode
    {
        [Key]
        public string Value { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    public class ConfigurableModule
    {
        [Key]
        public string Value { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    public class DefaultTraceCode
    {
        [Key]
        public string Value { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    public class DefaultUnit
    {
        [Key]
        public string Value { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    public class Source
    {
        [Key]
        public string Value { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    public class State
    {
        [Key]
        public string Value { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    public class TypeIcon
    {
        [Key]
        public string Path { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
    }

    public class WorkInProgressState
    {
        [Key]
        public string Value { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    public class BomType
    {
        [Key]
        public string Value { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }
}
