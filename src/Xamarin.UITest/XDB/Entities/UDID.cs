using System;
namespace Xamarin.UITest.XDB.Services.OSX
{
    /// <summary>
    /// UDID of iOS simulator or iOS physical device.
    /// </summary>
	public class UDID
	{
		private readonly string Value;

        /// <summary>
        /// Creates UDID instance from provided string.
        /// </summary>
        /// <param name="UDID"><see cref="string"/> with UDID.</param>
		public UDID(string UDID)
		{
			Value = UDID;
		}

        /// <summary>
        /// Returnes a <see cref="string"/> that represents current UDID.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Checks if this UDID is for simulator.
        /// </summary>
        /// <returns>true if UDID is for Simulator. false otherwise.</returns>
        public bool IsSimulator => Guid.TryParse(Value, out _);

        /// <summary>
        /// Checks if this UDID is for physical device.
        /// </summary>
        /// <returns>true if UDID is for physical device. false - otherwise.</returns>
        public bool IsPhysicalDevice => !Guid.TryParse(Value, out _);
    }
}

