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
    
    public partial class User_Page
    {
        public long UserPageID { get; set; }
        public string Username { get; set; }
        public string PageName { get; set; }
    
        public virtual MasterPage MasterPage { get; set; }
        public virtual MasterUser MasterUser { get; set; }
    }
}
