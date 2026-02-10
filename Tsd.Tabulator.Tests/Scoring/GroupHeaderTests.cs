using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Tsd.Tabulator.Core.Scoring;

namespace Tsd.Tabulator.Tests.Scoring
{
    public class GroupHeaderTests
    {
        [Fact]
        public void GameDay_GroupHeaders_ComputesCorrectSpans()
        {
            // Arrange
            var definition = ScoreSheetDefinitions.GameDay;
            
            // Get ordered criteria
            var orderedCriteria = definition.Criteria.OrderBy(c => c.Order).ToList();
            
            // Act - Compute group headers using same logic as ScoreSheetTabVM
            var headers = ComputeGroupHeaders(orderedCriteria);
            
            // Assert
            Assert.Equal(3, headers.Count);
            
            // Fight Song: 3 columns starting at index 0
            Assert.Equal("Fight Song", headers[0].Text);
            Assert.Equal(0, headers[0].StartIndex);
            Assert.Equal(3, headers[0].Span);
            
            // Spirit Raising: 3 columns starting at index 3
            Assert.Equal("Spirit Raising", headers[1].Text);
            Assert.Equal(3, headers[1].StartIndex);
            Assert.Equal(3, headers[1].Span);
            
            // Performance: 4 columns starting at index 6
            Assert.Equal("Performance", headers[2].Text);
            Assert.Equal(6, headers[2].StartIndex);
            Assert.Equal(4, headers[2].Span);
        }

        [Fact]
        public void OkStateStandard_NoGroupHeaders_ReturnsEmpty()
        {
            // Arrange
            var definition = ScoreSheetDefinitions.Standard;
            var orderedCriteria = definition.Criteria.OrderBy(c => c.Order).ToList();
            
            // Act
            var headers = ComputeGroupHeaders(orderedCriteria);
            
            // Assert
            Assert.Empty(headers);
        }

        [Fact]
        public void SpiritRally_NoGroupHeaders_ReturnsEmpty()
        {
            // Arrange
            var definition = ScoreSheetDefinitions.SpiritRally;
            var orderedCriteria = definition.Criteria.OrderBy(c => c.Order).ToList();
            
            // Act
            var headers = ComputeGroupHeaders(orderedCriteria);
            
            // Assert
            Assert.Empty(headers);
        }

        private List<TestGroupHeader> ComputeGroupHeaders(IReadOnlyList<ScoreCriterionDefinition> orderedCriteria)
        {
            var headers = new List<TestGroupHeader>();
            bool hasGroups = orderedCriteria.Any(c => c.Group != null);
            
            if (!hasGroups)
                return headers;

            int currentIndex = 0;
            int i = 0;

            while (i < orderedCriteria.Count)
            {
                var criterion = orderedCriteria[i];
                
                if (criterion.Group == null)
                {
                    i++;
                    currentIndex++;
                    continue;
                }

                string groupName = criterion.Group;
                int startIndex = currentIndex;
                int span = 0;

                while (i < orderedCriteria.Count && orderedCriteria[i].Group == groupName)
                {
                    span++;
                    i++;
                }

                headers.Add(new TestGroupHeader(groupName, startIndex, span));
                currentIndex += span;
            }

            return headers;
        }

        private class TestGroupHeader
        {
            public string Text { get; }
            public int StartIndex { get; }
            public int Span { get; }

            public TestGroupHeader(string text, int startIndex, int span)
            {
                Text = text;
                StartIndex = startIndex;
                Span = span;
            }
        }
    }
}
