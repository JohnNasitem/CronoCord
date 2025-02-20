//***********************************************************************************
//Program: GetTimeIndexUnitTest.cs
//Description: Test unit for Availability.GetTimeIndex
//Date: Feb 19, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CronoCord.Classes;
using CronoCord.Modules;
using System.Linq;
using static CronoCord.Classes.Availability;

namespace CronoCordTestProject.Availabilities
{
    [TestClass]
    public class GetTimeIndexUnitTest
    {
        /// <summary>
        /// Test with valid inputs
        /// </summary>
        [TestMethod]
        public void ValidInputs()
        {
            // Leading zeros
            Assert.AreEqual(24 + (3 * 2) + 2, AvailabilityModule.GetTimeIndex("03:50 PM"));

            // No leading zeros
            Assert.AreEqual(0 + (9 * 2) + 1, AvailabilityModule.GetTimeIndex("9:24 AM"));

            // No space between minutes and am/pm
            Assert.AreEqual(0 + (1 * 2) + 0, AvailabilityModule.GetTimeIndex("1:00am"));

            // Midnight
            Assert.AreEqual(0, AvailabilityModule.GetTimeIndex("12:00 am"));

            // Noon
            Assert.AreEqual(24, AvailabilityModule.GetTimeIndex("12:00 pm"));
        }


        /// <summary>
        /// Test with invalid inputs
        /// </summary>
        [TestMethod]
        public void InvalidInputs()
        {
            // Missing am/pm
            Assert.AreEqual(-1, AvailabilityModule.GetTimeIndex("03:50"));

            // Missing hour
            Assert.AreEqual(-1, AvailabilityModule.GetTimeIndex(":24 AM"));

            // Missing minutes
            Assert.AreEqual(-1, AvailabilityModule.GetTimeIndex("12: am"));
            Assert.AreEqual(-1, AvailabilityModule.GetTimeIndex("12 am"));

            // Missing :
            Assert.AreEqual(-1, AvailabilityModule.GetTimeIndex("1200 pm"));
        }
    }
}
