using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VietnamFoodGuide.Models.Entities
{
    [Table("qr_scans")]
    public class QRScan
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("device_id")]
        [MaxLength(255)]
        public string DeviceId { get; set; }

        [Required]
        [Column("qr_code")]
        [MaxLength(500)]
        public string QRCode { get; set; }

        [Column("scan_date")]
        public DateTime ScanDate { get; set; }

        [Column("device_name")]
        [MaxLength(255)]
        public string DeviceName { get; set; }

        [Column("os_version")]
        [MaxLength(100)]
        public string OSVersion { get; set; }
    }
}
