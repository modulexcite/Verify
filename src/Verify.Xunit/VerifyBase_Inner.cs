﻿using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace VerifyXunit
{
    public partial class VerifyBase :
        XunitContextBase
    {
        public async Task Verify(string input, string extension)
        {
            Guard.AgainstNull(input, nameof(input));
            input = ApplyScrubbers(input);
            input = input.Replace("\r\n", "\n");
            var (receivedPath, verifiedPath) = GetFileNames(extension);
            FileHelpers.DeleteIfEmpty(verifiedPath);
            if (!File.Exists(verifiedPath))
            {
                FileHelpers.WriteEmpty(verifiedPath);
                ClipboardCapture.Append(receivedPath, verifiedPath);
                if (DiffRunner.FoundDiff)
                {
                    await FileHelpers.WriteText(verifiedPath, "");
                    DiffRunner.Launch(receivedPath, verifiedPath);
                }

                throw new Exception($"First verification. {Context.UniqueTestName}.verified{extension} not found. Verification command has been copied to the clipboard.");
            }

            var verifiedText = await FileHelpers.ReadText(verifiedPath);
            verifiedText = verifiedText.Replace("\r\n", "\n");
            try
            {
                Assert.Equal(verifiedText, input);
            }
            catch (EqualException exception)
            {
                await FileHelpers.WriteText(receivedPath, input);
                ClipboardCapture.Append(receivedPath, verifiedPath);
                exception.PrefixWithCopyCommand();
                DiffRunner.Launch(receivedPath, verifiedPath);

                throw;
            }
        }
    }
}