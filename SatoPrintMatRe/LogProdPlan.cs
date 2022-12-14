//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SatoPrintMatRe
{
    using System;
    using System.Collections.Generic;
    
    public partial class LogProdPlan
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LogProdPlan()
        {
            this.LogProdDetails = new HashSet<LogProdDetail>();
        }
    
        public long ProdPlanID { get; set; }
        public System.DateTime ProdDate { get; set; }
        public int RecpNameID { get; set; }
        public int StaID { get; set; }
        public Nullable<int> CurrentBatch { get; set; }
        public string CurrentLot { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LogProdDetail> LogProdDetails { get; set; }
        public virtual MasterStation MasterStation { get; set; }
        public virtual Plan Plan { get; set; }
    }
}
