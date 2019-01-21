using System;
using System.IO;
using Xunit;

namespace IPA.Tests
{
    public class ShortcutTest
    {
        [Fact]
        public void CanDealWithEmptyFiles()
        {
            Shortcut.Create(".lnk", "", "", "", "", "", "");
        }

        [Fact]
        public void CanDealWithLongFiles()
        {
            Shortcut.Create(".lnk", Path.Combine(Path.GetTempPath(), string.Join("_", new string[500])), "", "", "", "", "");
        }

        [Fact]
        public void CantDealWithNull()
        {
            Assert.Throws<ArgumentException>(() => Shortcut.Create(".lnk", null, "", "", "", "", ""));
        }

        [Fact]
        public void CanDealWithWeirdCharacters()
        {
            Shortcut.Create(".lnk", "äöü", "", "", "", "", "");
        }
    }
}
