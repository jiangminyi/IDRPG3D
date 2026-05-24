using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace IDRPG3D.EditorTools.Tests
{
    public sealed class IDRPG3DLocalServerProcessTests
    {
        [Test]
        public void CreateGameServerCommandUsesProjectRelativeServerEntry()
        {
            var command = IDRPG3DLocalServerProcess.CreateGameServerCommand("D:/Game/IDRPG3D");

            Assert.AreEqual("dotnet", command.FileName);
            Assert.AreEqual("D:/Game/IDRPG3D", command.WorkingDirectory);
            Assert.IsTrue(command.Arguments.Contains("GameServer/APP/Main/IDRPG3D.GameServer.Main.csproj"));
            Assert.IsTrue(command.Arguments.Contains("-m Develop -g 1"));
        }

        [Test]
        public void ParseNetstatProcessIdsFindsTcpAndUdpPortOwners()
        {
            var lines = new[]
            {
                "  TCP    127.0.0.1:11001        0.0.0.0:0              LISTENING       39364",
                "  UDP    127.0.0.1:20000        *:*                                    39364",
                "  TCP    127.0.0.1:8081         0.0.0.0:0              LISTENING       43000",
            };

            var processIds = IDRPG3DLocalPortOwners.ParseProcessIdsByPorts(lines, new[] { 20000, 11001 });

            CollectionAssert.AreEquivalent(new[] { 39364 }, processIds);
        }

        [Test]
        public void ExpectedProcessNameMatchesGameServerExecutable()
        {
            Assert.IsTrue(IDRPG3DLocalPortOwners.IsExpectedProcessName("IDRPG3D.GameServer.Main", new[] { "IDRPG3D.GameServer.Main" }));
            Assert.IsFalse(IDRPG3DLocalPortOwners.IsExpectedProcessName("OtherServer", new[] { "IDRPG3D.GameServer.Main" }));
        }

        [Test]
        public void CreateMongoExpressCommandUsesPowerShellScript()
        {
            var command = IDRPG3DLocalServerProcess.CreateMongoExpressCommand("D:/Game/IDRPG3D");

            Assert.IsTrue(command.FileName.ToLowerInvariant().Contains("powershell"));
            Assert.AreEqual("D:/Game/IDRPG3D", command.WorkingDirectory);
            Assert.IsTrue(command.Arguments.Contains("Scripts/start-mongo-express.ps1"));
        }

        [Test]
        public void AppendLogKeepsNewestLines()
        {
            var buffer = new IDRPG3DLocalServerLogBuffer(3);

            buffer.Append("1");
            buffer.Append("2");
            buffer.Append("3");
            buffer.Append("4");

            CollectionAssert.AreEqual(new[] { "2", "3", "4" }, buffer.Snapshot);
        }

        [Test]
        public void ManualLogScrollDisablesAutoScroll()
        {
            var controller = new IDRPG3DLocalServerLogScrollController();

            controller.NotifyManualScroll();

            Assert.IsFalse(controller.AutoScroll);
        }

        [Test]
        public void JumpLatestReEnablesAutoScrollAndMovesToBottom()
        {
            var controller = new IDRPG3DLocalServerLogScrollController
            {
                Position = new Vector2(0f, 24f)
            };
            controller.NotifyManualScroll();

            controller.JumpToLatest();

            Assert.IsTrue(controller.AutoScroll);
            Assert.AreEqual(float.MaxValue, controller.Position.y);
        }

        [Test]
        public void LogViewCacheKeepsRepaintLinesStableUntilNextLayout()
        {
            var cache = new IDRPG3DLocalServerLogViewCache();

            cache.UpdateForEvent(isLayout: true, new[] { "layout-line" });
            cache.UpdateForEvent(isLayout: false, new[] { "layout-line", "late-line" });

            CollectionAssert.AreEqual(new[] { "layout-line" }, cache.Lines);

            cache.UpdateForEvent(isLayout: true, new[] { "layout-line", "late-line" });

            CollectionAssert.AreEqual(new[] { "layout-line", "late-line" }, cache.Lines);
        }
    }
}
