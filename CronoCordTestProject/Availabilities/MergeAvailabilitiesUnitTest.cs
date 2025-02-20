//***********************************************************************************
//Program: MergeAvailabilitiesUnitTest.cs
//Description: Test unit for Availability.MergeOverlappingSlots
//Date: Feb 19, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CronoCord.Classes;
using CronoCord.Modules;
using System.Linq;
using static CronoCord.Classes.Availability;

namespace CronoCordTestProject
{
    [TestClass]
    public class MergeAvailabilitiesUnitTest
    {
        /// <summary>
        /// Test cause where user id is the different and there is overlaps between the different users
        /// </summary>
        [TestMethod]
        public void DifferentUserYesOverlaps()
        {
            var availabilities = new List<Availability>
            {
                new Availability(1, 100, 200, Recurring.N),
                new Availability(2, 150, 250, Recurring.N),
            };

            var merged = AvailabilityModule.MergeOverlappingSlots(availabilities);

            string outputString = string.Join("\n", merged.Select(a => a.ToString()));
            string expected = "UserID: 1 - StartTimeUnix: 100 - EndTimeUnix: 200 - Recurring: N\n" +
                              "UserID: 2 - StartTimeUnix: 150 - EndTimeUnix: 250 - Recurring: N";

            Assert.AreEqual(expected, outputString);
        }



        /// <summary>
        /// Test cause where user id is the same and some are overlapping and some are not
        /// </summary>
        [TestMethod]
        public void SameUserMixedOverlaps()
        {
            var availabilities = new List<Availability>
            {
                //Overlap on the right
                new Availability(3, 100, 200, Recurring.N),
                new Availability(3, 150, 250, Recurring.N),

                //Overlap on the left
                new Availability(3, 600, 700, Recurring.N),
                new Availability(3, 550, 650, Recurring.N),

                // Doesnt overlap
                new Availability(3, 1000, 1100, Recurring.N),
                new Availability(3, 2000, 3000, Recurring.N),
            };

            var merged = AvailabilityModule.MergeOverlappingSlots(availabilities);

            string outputString = string.Join("\n", merged.Select(a => a.ToString()));
            string expected = "UserID: 3 - StartTimeUnix: 100 - EndTimeUnix: 250 - Recurring: N\n" +
                              "UserID: 3 - StartTimeUnix: 550 - EndTimeUnix: 700 - Recurring: N\n" +
                              "UserID: 3 - StartTimeUnix: 1000 - EndTimeUnix: 1100 - Recurring: N\n" +
                              "UserID: 3 - StartTimeUnix: 2000 - EndTimeUnix: 3000 - Recurring: N";

            Assert.AreEqual(expected, outputString);
        }



        /// <summary>
        /// Test cause where user id is the same and there is overlaps
        /// </summary>
        [TestMethod]
        public void SameUserYesOverlaps()
        {
            var availabilities = new List<Availability>
            {
                //Overlap on the right
                new Availability(3, 100, 200, Recurring.N),
                new Availability(3, 150, 250, Recurring.N),

                //Overlap on the left
                new Availability(3, 600, 700, Recurring.N),
                new Availability(3, 550, 650, Recurring.N),
            };

            var merged = AvailabilityModule.MergeOverlappingSlots(availabilities);

            string outputString = string.Join("\n", merged.Select(a => a.ToString()));
            string expected = "UserID: 3 - StartTimeUnix: 100 - EndTimeUnix: 250 - Recurring: N\n" +
                              "UserID: 3 - StartTimeUnix: 550 - EndTimeUnix: 700 - Recurring: N";

            Assert.AreEqual(expected, outputString);
        }



        /// <summary>
        /// Test cause where user id is the same but times dont overlap
        /// </summary>
        [TestMethod]
        public void SameUserNoOverlaps()
        {
            var availabilities = new List<Availability>
            {
                new Availability(4, 100, 110, Recurring.N),
                new Availability(4, 120, 130, Recurring.N),
                new Availability(4, 190, 200, Recurring.N),
            };

            var merged = AvailabilityModule.MergeOverlappingSlots(availabilities);

            string outputString = string.Join("\n", merged.Select(a => a.ToString()));
            string expected = "UserID: 4 - StartTimeUnix: 100 - EndTimeUnix: 110 - Recurring: N\n" +
                              "UserID: 4 - StartTimeUnix: 120 - EndTimeUnix: 130 - Recurring: N\n" +
                              "UserID: 4 - StartTimeUnix: 190 - EndTimeUnix: 200 - Recurring: N";

            Assert.AreEqual(expected, outputString);
        }



        /// <summary>
        /// Test cases where the availability fully contains another availability
        /// </summary>
        [TestMethod]
        public void AvailabilityContainsOther()
        {
            var availabilities = new List<Availability>
            {
                // Contained comes first
                new Availability(1, 5, 8, Recurring.N), // Contains next
                new Availability(1, 2, 10, Recurring.N), // Contained by previous

                // Contained comes last
                new Availability(1, 30, 40, Recurring.N), // Contains next
                new Availability(1, 32, 38, Recurring.N), // Contained by previous
            };

            var merged = AvailabilityModule.MergeOverlappingSlots(availabilities);

            string outputString = string.Join("\n", merged.Select(a => a.ToString()));
            string expected = "UserID: 1 - StartTimeUnix: 2 - EndTimeUnix: 10 - Recurring: N\n" +
                              "UserID: 1 - StartTimeUnix: 30 - EndTimeUnix: 40 - Recurring: N";

            Assert.AreEqual(expected, outputString);
        }



        /// <summary>
        /// Test cases where the availability fully contains another availability
        /// </summary>
        [TestMethod]
        public void AvailabilityTouchesOther()
        {
            var availabilities = new List<Availability>
            {
                new Availability(1, 5, 10, Recurring.N),
                new Availability(1, 10, 15, Recurring.N), 
            };

            var merged = AvailabilityModule.MergeOverlappingSlots(availabilities);

            string outputString = string.Join("\n", merged.Select(a => a.ToString()));
            string expected = "UserID: 1 - StartTimeUnix: 5 - EndTimeUnix: 15 - Recurring: N";

            Assert.AreEqual(expected, outputString);
        }



        /// <summary>
        /// Test cases where the some time slots dont overlap but are joined by others
        /// </summary>
        [TestMethod]
        public void ChainedOverlaps()
        {
            var availabilities = new List<Availability>
            {
                new Availability(1, 10, 20, Recurring.N), // Does not overlap with last slot
                new Availability(1, 15, 35, Recurring.N), // Overlaps with both
                new Availability(1, 30, 40, Recurring.N), // Does not overlap with first slot
            };

            var merged = AvailabilityModule.MergeOverlappingSlots(availabilities);

            string outputString = string.Join("\n", merged.Select(a => a.ToString()));
            string expected = "UserID: 1 - StartTimeUnix: 10 - EndTimeUnix: 40 - Recurring: N";

            Assert.AreEqual(expected, outputString);
        }



        /// <summary>
        /// Test cases where there is only 1 availability
        /// </summary>
        [TestMethod]
        public void SingleAvailability()
        {
            var availabilities = new List<Availability>
            {
                new Availability(1, 10, 20, Recurring.N)
            };

            var merged = AvailabilityModule.MergeOverlappingSlots(availabilities);

            string outputString = string.Join("\n", merged.Select(a => a.ToString()));
            string expected = "UserID: 1 - StartTimeUnix: 10 - EndTimeUnix: 20 - Recurring: N";

            Assert.AreEqual(expected, outputString);
        }



        /// <summary>
        /// Test cases where there is no availabilities
        /// </summary>
        [TestMethod]
        public void NoAvailabilites()
        {
            var availabilities = new List<Availability>();

            var merged = AvailabilityModule.MergeOverlappingSlots(availabilities);

            string outputString = string.Join("\n", merged.Select(a => a.ToString()));
            string expected = "";

            Assert.AreEqual(expected, outputString);
        }
    }
}
