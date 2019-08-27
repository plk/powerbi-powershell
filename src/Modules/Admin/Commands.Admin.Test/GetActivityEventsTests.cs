﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerBI.Commands.Common.Test;
using Microsoft.PowerBI.Commands.Profile.Test;
using Microsoft.PowerBI.Common.Api;
using Microsoft.PowerBI.Common.Api.ActivityEvent;
using Microsoft.PowerBI.Common.Api.Encryption;
using Microsoft.PowerBI.Common.Api.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Microsoft.PowerBI.Commands.Admin.Test
{
    [TestClass]
    public class GetActivityEventsTests
    {
        private static CmdletInfo GetPowerBIActivityEventsCmdletInfo => new CmdletInfo($"{GetActivityEvents.CmdletVerb}-{GetActivityEvents.CmdletName}", typeof(GetActivityEvents));
        private static string StartDateTime = "2019-08-15T20:00:00Z";
        private static string EndDateTime = "2019-08-15T22:00:00Z";

        [TestMethod]
        [TestCategory("Interactive")]
        [TestCategory("SkipWhenLiveUnitTesting")] // Ignore for Live Unit Testing
        public void EndToEndGetPowerBIActivityEvents()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                ProfileTestUtilities.ConnectToPowerBI(ps);
                var parameters = new Dictionary<string, string>()
                {
                    { nameof(GetActivityEvents.StartDateTime), StartDateTime },
                    { nameof(GetActivityEvents.EndDateTime), EndDateTime },
                    { nameof(GetActivityEvents.Filter), "ViewReport" },
                };

                ps.AddCommand(GetPowerBIActivityEventsCmdletInfo).AddParameters(parameters);

                // Act
                var result = ps.Invoke();

                // Assert
                TestUtilities.AssertNoCmdletErrors(ps);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(CmdletInvocationException))]
        public void EndToEndGetPowerBIActivityEventsWithoutLogin()
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // Arrange
                ProfileTestUtilities.SafeDisconnectFromPowerBI(ps);
                var parameters = new Dictionary<string, string>()
                {
                    { nameof(GetActivityEvents.StartDateTime), StartDateTime },
                    { nameof(GetActivityEvents.EndDateTime), EndDateTime },
                    { nameof(GetActivityEvents.Filter), "ViewReport" },
                };

                ps.AddCommand(GetPowerBIActivityEventsCmdletInfo).AddParameters(parameters);

                // Act
                var result = ps.Invoke();

                // Assert
                Assert.Fail("Should not have reached this point");
            }
        }

        [TestMethod]
        public void GetPowerBIActivityEvents()
        {
            // Arrange
            object obj1 = new object();
            object obj2 = new object();
            IList<object> ActivityEventEntities = new List<object>
            {
                obj1,
                obj2
            };

            var activityEventResponse = new ActivityEventResponse();
            activityEventResponse.ActivityEventEntities = ActivityEventEntities;
            activityEventResponse.ContinuationToken = "next-page";

            var client = new Mock<IPowerBIApiClient>();
            client.Setup(x => x.Admin.GetActivityEvents($"'{StartDateTime}'", $"'{EndDateTime}'", null, null)).Returns(activityEventResponse);

            var initFactory = new TestPowerBICmdletInitFactory(client.Object);
            var cmdlet = new GetActivityEvents(initFactory)
            {
                StartDateTime = StartDateTime,
                EndDateTime = EndDateTime,
            };

            // Act
            cmdlet.InvokePowerBICmdlet();

            // Assert
            AssertExpectedUnitTestResults(activityEventResponse, initFactory);
        }

        [TestMethod]
        public void GetPowerBIActivityEventsWithInvalidStartDateTime()
        {
            // Arrange
            var activityEventResponse = new ActivityEventResponse
            {
                ActivityEventEntities = new List<object>
                {
                    new object()
                },
                ContinuationToken = "next-page"
            };

            string invalidStartDateTime = "Some-invalid-startDateTime";
            var client = new Mock<IPowerBIApiClient>();
            client.Setup(x => x.Admin.GetActivityEvents($"'{invalidStartDateTime}'", $"'{EndDateTime}'", null, null)).Returns(activityEventResponse);

            var initFactory = new TestPowerBICmdletInitFactory(client.Object);
            var cmdlet = new GetActivityEvents(initFactory)
            {
                StartDateTime = invalidStartDateTime,
                EndDateTime = EndDateTime,
            };

            // Act
            cmdlet.InvokePowerBICmdlet();

            // Assert
            AssertGetActivityEventsNeverCalled(client, initFactory);
        }

        [TestMethod]
        public void GetPowerBIActivityEventsWithInvalidEndDateTime()
        {
            // Arrange
            var activityEventResponse = new ActivityEventResponse
            {
                ActivityEventEntities = new List<object>
                {
                    new object()
                },
                ContinuationToken = "next-page"
            };

            string invalidEndDateTime = "Some-invalid-endDateTime";
            var client = new Mock<IPowerBIApiClient>();
            client.Setup(x => x.Admin.GetActivityEvents($"'{StartDateTime}'", $"'{invalidEndDateTime}'", null, null)).Returns(activityEventResponse);

            var initFactory = new TestPowerBICmdletInitFactory(client.Object);
            var cmdlet = new GetActivityEvents(initFactory)
            {
                StartDateTime = StartDateTime,
                EndDateTime = invalidEndDateTime,
            };

            // Act
            cmdlet.InvokePowerBICmdlet();

            // Assert
            AssertGetActivityEventsNeverCalled(client, initFactory);
        }

        private static void AssertExpectedUnitTestResults(ActivityEventResponse expectedResponse, TestPowerBICmdletInitFactory initFactory)
        {
            Assert.IsFalse(initFactory.Logger.ErrorRecords.Any());
            var results = initFactory.Logger.Output.ToList();
            ActivityEventResponse actualResponse = JsonConvert.DeserializeObject<ActivityEventResponse>(results[0].ToString());
            Assert.AreEqual(expectedResponse.ActivityEventEntities.Count(), actualResponse.ActivityEventEntities.Count());
        }

        private static void AssertGetActivityEventsNeverCalled(Mock<IPowerBIApiClient> client, TestPowerBICmdletInitFactory initFactory)
        {
            Assert.IsFalse(initFactory.Logger.ErrorRecords.Any());
            var results = initFactory.Logger.Output.ToList();
            Assert.IsFalse(results.Any());
            client.Verify(x => x.Admin.GetActivityEvents(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}