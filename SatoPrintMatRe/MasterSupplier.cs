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
    
    public partial class MasterSupplier
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MasterSupplier()
        {
            this.MasterIngs = new HashSet<MasterIng>();
        }
    
        public long SupID { get; set; }
        public string SupName { get; set; }
        public string SupCode { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MasterIng> MasterIngs { get; set; }
    }
}
