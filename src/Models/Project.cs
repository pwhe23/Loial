﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace Loial
{
    [Table("Project")]
    public class Project
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(50), Required]
        public string Name { get; set; }

        [StringLength(100), Required]
        public string Repository { get; set; }

        [StringLength(50), Required]
        public string Branch { get; set; }

        [MaxLength, Required]
        public string Command { get; set; }

        public int BuildNumber { get; set; }
        public bool IsActive { get; set; }
        public bool IsRunning { get; set; }

        public string GetFolder(string basePath)
        {
            var path = Path.GetFullPath(Path.Combine(basePath, @"..\Projects", Name));
            Directory.CreateDirectory(path);
            return path;
        }

        public string GetLogFilePath(string basePath, int buildNumber)
        {
            var path = Path.Combine(GetFolder(basePath), "Logs", buildNumber + ".log");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }
    };
}
