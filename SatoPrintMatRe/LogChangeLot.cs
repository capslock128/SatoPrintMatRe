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
    
    public partial class LogChangeLot
    {
        public long ChangeLotID { get; set; }
        public System.DateTime ProdDate { get; set; }
        public int RecpNameID { get; set; }
        public int BatchNo { get; set; }
        public string LotBefore { get; set; }
        public string LotChange { get; set; }
        public Nullable<long> RemarkID { get; set; }
        public string RemarkTxt { get; set; }
        public string Username { get; set; }
        public Nullable<System.DateTime> Datetime { get; set; }
    
        public virtual Plan Plan { get; set; }
    }
}
