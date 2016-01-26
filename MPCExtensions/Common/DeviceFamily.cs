using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPCExtensions.Common
{
    /// <summary>
    /// Device Families
    /// </summary>
    public enum DeviceFamily
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Desktop
        /// </summary>
        Desktop = 1,
        /// <summary>
        /// Mobile
        /// </summary>
        Mobile = 2,
        /// <summary>
        /// Team
        /// </summary>
        Team = 3,
        /// <summary>
        /// Windows IoT
        /// </summary>
        IoT = 4,
        /// <summary>
        /// Xbox
        /// </summary>
        Xbox = 5
    }
}
