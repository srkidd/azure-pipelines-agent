using Microsoft.TeamFoundation.DistributedTask.Logging;
using Microsoft.VisualStudio.Services.Agent;
using Microsoft.VisualStudio.Services.Agent.Worker;
using Microsoft.VisualStudio.Services.Agent.Worker.Build;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Test.L0.SecretMaskerTests
{
    internal class RegexMaskingL0
{
        private ISecretMasker initSecretMasker()
        {
            var testSecretMasker = new SecretMasker();
            testSecretMasker.AddRegex(AdditionalMaskingRegexes.UrlSecretPattern);

            return testSecretMasker;
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "SecretMasker")]
        public void IsRegexHintMasked()
        {
            var testSecretMasker = initSecretMasker();

            Assert.Equal(
               "https://simpledomain@example.com",
               testSecretMasker.MaskSecrets("https://simpledomain@example.com"));
        }
    }
}
