using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolERP.Shared.Models
{ 
    /// <summary>
  /// Entity mapped to the RoomCoolingTypes table.
  /// </summary>
    public class RoomCoolingType
    {
        public int Result { get; set; }       // maps to RESULT
        public string Message { get; set; }   // maps to MESSAGE
        public int RoomCoolingTypeId { get; set; }
        public string RoomCoolingTypeName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int CompanyId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// Row shape returned to the List grid and to the Copy-Session table
    /// (matches the ENTITY_COLUMNS 'RoomCoolingTypes' row(r,i) renderer in
    /// copySessionModal.js).
    /// </summary>
    public class RoomCoolingTypeListDto
    {
        public int RoomCoolingTypeId { get; set; }
        public string RoomCoolingTypeName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int CompanyID { get; set; }
        public int SessionID { get; set; }
        public int UserID { get; set; }
        public string? IPAddress { get; set; }
    }

    /// <summary>
    /// View model bound by the Add/Edit form.
    /// </summary>
    public class RoomCoolingTypeFormViewModel
    {
        public int RoomCoolingTypeId { get; set; }

       
        public string RoomCoolingTypeName { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class RoomCoolingTypePageViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public List<RoomCoolingType> RoomCoolingType { get; set; } = new List<RoomCoolingType>();
    }
    public class RoomCoolingTypeAddViewModel
    {
        public PagePermissions Permissions { get; set; } = PagePermissions.Denied;
        public RoomCoolingType RoomCoolingType { get; set; } = new RoomCoolingType();
        public RoomCoolingType? EditRoomCoolingType { get; set; } = new RoomCoolingType();
    }
}
