using System.Linq;
using NUnit.Framework;

namespace IDRPG3D.EditorTools.Tests
{
    public sealed class IDRPG3DRouteChineseGuideTests
    {
        [Test]
        public void GuideContainsCoreSplineComputerTranslations()
        {
            var entries = IDRPG3DRouteChineseGuide.Sections.SelectMany(section => section.Entries).ToArray();

            Assert.IsTrue(entries.Any(entry => entry.English == "Type" && entry.Chinese == "曲线类型"));
            Assert.IsTrue(entries.Any(entry => entry.English == "Sample Mode" && entry.Chinese == "采样模式"));
            Assert.IsTrue(entries.Any(entry => entry.English == "Placement Mode" && entry.Chinese == "放置模式"));
            Assert.IsTrue(entries.Any(entry => entry.English == "Editor Update Mode" && entry.Chinese == "编辑器更新模式"));
            Assert.IsTrue(entries.Any(entry => entry.English == "SplineFollower" && entry.Chinese == "路线跟随器"));
            Assert.IsTrue(entries.Any(entry => entry.English == "Node Type" && entry.Chinese == "节点类型"));
        }

        [Test]
        public void GuideSectionsHaveDescriptionsForEveryEntry()
        {
            foreach (var section in IDRPG3DRouteChineseGuide.Sections)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(section.Title));
                Assert.IsTrue(section.Entries.Count > 0);
                foreach (var entry in section.Entries)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.English));
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Chinese));
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Description));
                }
            }
        }

        [Test]
        public void GuideContainsLearningSectionsForAllMainDreamteckAreas()
        {
            var sectionTitles = IDRPG3DRouteChineseGuide.Sections.Select(section => section.Title).ToArray();

            Assert.Contains("Spline Computer 主面板", sectionTitles);
            Assert.Contains("Edit 路线点编辑", sectionTitles);
            Assert.Contains("点选择 / 点批量操作", sectionTitles);
            Assert.Contains("SplineUser / Follower 常用组件", sectionTitles);
            Assert.Contains("Node 节点组件 Inspector", sectionTitles);
            Assert.Contains("IDRPG3D 路线辅助工具", sectionTitles);
        }
    }
}
