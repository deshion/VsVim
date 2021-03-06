﻿using System;
using Xunit;

namespace Vim.UnitTest
{
    public abstract class VimDataTest : VimTestBase
    {
        private readonly VimData _vimDataRaw;
        private readonly IVimData _vimData;
        private readonly IVimGlobalSettings _globalSettings;

        protected VimDataTest()
        {
            _globalSettings = new GlobalSettings();
            _vimDataRaw = new VimData(_globalSettings);
            _vimData = _vimDataRaw;
        }

        public sealed class CurrentDirectoryTest : VimDataTest
        {
            /// <summary>
            /// The startup value for CurrentDirectory should be a non-empty string
            /// </summary>
            [WpfFact]
            public void Initial()
            {
                Assert.False(string.IsNullOrEmpty(_vimData.CurrentDirectory));
            }

            /// <summary>
            /// Setting the current directory should move the previous value to PreviousCurrentDirectory
            /// </summary>
            [WpfFact]
            public void SetUpdatePrevious()
            {
                var old = _vimData.CurrentDirectory;
                _vimData.CurrentDirectory = @"c:\";
                Assert.Equal(old, _vimData.PreviousCurrentDirectory);
            }
        }

        public abstract class LastSearchDataTest : VimDataTest
        {
            public sealed class EventTest : LastSearchDataTest
            {
                private int _runCount;

                public EventTest()
                {
                    _globalSettings.HighlightSearch = true;
                    _vimData.LastSearchData = new SearchData("cat", SearchPath.Forward);
                    _vimData.DisplayPatternChanged += delegate { _runCount++; };
                }

                [WpfFact]
                public void PatternChanged()
                {
                    _vimData.LastSearchData = new SearchData("dog", SearchPath.Forward);
                    Assert.Equal(1, _runCount);
                }

                /// <summary>
                /// If the pattern is the same then nothing actually changed.  No need to 
                /// raise the event
                /// </summary>
                [WpfFact]
                public void PatternDataSame()
                {
                    _vimData.LastSearchData = _vimData.LastSearchData;
                    Assert.Equal(0, _runCount);
                }

                /// <summary>
                /// The path is not a part of the DisplayPattern and hence if it changes then
                /// it shouldn't effect the DisplayPattern valueh
                /// </summary>
                [WpfFact]
                public void PathChanged()
                {
                    _vimData.LastSearchData = new SearchData("dog", SearchPath.Forward);
                    _runCount = 0;
                    _vimData.LastSearchData = new SearchData("dog", SearchPath.Backward);
                    Assert.Equal(0, _runCount);
                }

                /// <summary>
                /// Nothing changed if the highlight is disabled 
                /// </summary>
                [WpfFact]
                public void PatternChangedHighlightDisabled()
                {
                    _globalSettings.HighlightSearch = false;
                    _runCount = 0;
                    Assert.Equal(0, _runCount);
                }
            }

            public sealed class DisplayPatternTest : LastSearchDataTest
            {
                public DisplayPatternTest()
                {
                    _globalSettings.HighlightSearch = true;
                }

                [WpfFact]
                public void Standard()
                {
                    _vimData.LastSearchData = new SearchData("dog", SearchPath.Forward);
                    Assert.Equal("dog", _vimData.DisplayPattern);
                }

                [WpfFact]
                public void HighlightDisabled()
                {
                    _globalSettings.HighlightSearch = false;
                    _vimData.LastSearchData = new SearchData("dog", SearchPath.Forward);
                    Assert.True(string.IsNullOrEmpty(_vimData.DisplayPattern));
                }
            }
        }

        public abstract class SuspendedResumedTest : VimDataTest
        {
            public sealed class EventTest : SuspendedResumedTest
            {
                private int _runCount;

                public EventTest()
                {
                    _globalSettings.HighlightSearch = true;
                    _vimData.LastSearchData = new SearchData("cat", SearchPath.Forward);
                    _vimData.DisplayPatternChanged += delegate { _runCount++; };
                }

                [WpfFact]
                public void Suspend()
                {
                    _vimData.SuspendDisplayPattern();
                    Assert.Equal(1, _runCount);
                }

                /// <summary>
                /// Multiple suspends should have no effect.  Once it's suspended it's suspended until 
                /// it's resumed
                /// </summary>
                [WpfFact]
                public void SuspendRedundant()
                {
                    _vimData.SuspendDisplayPattern();
                    _vimData.SuspendDisplayPattern();
                    Assert.Equal(1, _runCount);
                }

                [WpfFact]
                public void SupsendHighlightDisabled()
                {
                    _globalSettings.HighlightSearch = false;
                    _runCount = 0;
                    _vimData.SuspendDisplayPattern();
                    Assert.Equal(0, _runCount);
                }

                [WpfFact]
                public void Resume()
                {
                    _vimData.SuspendDisplayPattern();
                    _runCount = 0;
                    _vimData.ResumeDisplayPattern();
                    Assert.Equal(1, _runCount);
                }

                /// <summary>
                /// If it is already displaying then another call to display has no effect
                /// </summary>
                [WpfFact]
                public void ResumeRedundant()
                {
                    _vimData.ResumeDisplayPattern();
                    Assert.Equal(0, _runCount);
                }

                [WpfFact]
                public void ResumeHighlightDisabled()
                {
                    _globalSettings.HighlightSearch = false;
                    _vimData.SuspendDisplayPattern();
                    _runCount = 0;
                    _vimData.ResumeDisplayPattern();
                    Assert.Equal(0, _runCount);
                }
            }

            public sealed class DisplayPatternTest : SuspendedResumedTest
            {
                public DisplayPatternTest()
                {
                    _globalSettings.HighlightSearch = true;
                    _vimData.LastSearchData = new SearchData("cat", SearchPath.Forward);
                }

                [WpfFact]
                public void Suspend()
                {
                    _vimData.SuspendDisplayPattern();
                    Assert.True(string.IsNullOrEmpty(_vimData.DisplayPattern));
                }

                [WpfFact]
                public void Resume()
                {
                    _vimData.SuspendDisplayPattern();
                    _vimData.ResumeDisplayPattern();
                    Assert.Equal("cat", _vimData.DisplayPattern);
                }

                [WpfFact]
                public void ResumeHighlightDisabled()
                {
                    _globalSettings.HighlightSearch = false;
                    _vimData.SuspendDisplayPattern();
                    _vimData.ResumeDisplayPattern();
                    Assert.True(string.IsNullOrEmpty(_vimData.DisplayPattern));
                }
            }
        }
    }
}
