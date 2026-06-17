using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Net.Models
{
    public class MenuViewModel
    {
        public int MenuID { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string? DisplayLabel { get; set; }
        public string MenuURL { get; set; } = string.Empty;
        public int? ParentID { get; set; }
        public string? ParentMenuName { get; set; }
        public int DisplayOrder { get; set; }
        public string? MenuIcon { get; set; }
        public string? MenuKey { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class MenuUpsertRequest
    {
        public int MenuID { get; set; }

        [Required(ErrorMessage = "Menu Name is required")]
        [MaxLength(20, ErrorMessage = "Menu Name cannot exceed 20 characters")]
        public string MenuName { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Display Label cannot exceed 200 characters")]
        public string? DisplayLabel { get; set; }

        [Required(ErrorMessage = "Menu URL is required")]
        [MaxLength(300, ErrorMessage = "Menu URL cannot exceed 300 characters")]
        public string MenuURL { get; set; } = string.Empty;

        public int? ParentID { get; set; }
        public int DisplayOrder { get; set; }
        public string? MenuIcon { get; set; }
        public string? MenuKey { get; set; }
        public bool IsActive { get; set; }
    }

    public class MenuOrderRequest
    {
        public int MenuID { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class MenusPageViewModel
    {
        public List<MenuViewModel> Menus { get; set; } = new();
        public List<MenuViewModel> ParentMenus { get; set; } = new();
    }
}
