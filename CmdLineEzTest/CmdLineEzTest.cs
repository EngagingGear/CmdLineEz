using System.Collections.Generic;
using System.Linq;
using CmdLineEzNs;
using Xunit;

// ReSharper disable once CheckNamespace
namespace CmdLinEzTestNs
{
    public class CmdLineEzTest
    {
        [Fact]
        public void SimpleFlag()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test");
            cmdLine.Process(new[] {"/test"});
            Assert.Equal(true, cmdLine.FlagVal("test"));
        }

        [Fact]
        public void SimpleParam()
        {
            var cmdLine = new CmdLineEz()
                .Param("test");
            cmdLine.Process("/test myTestValue".Split(" "));
            Assert.Equal("myTestValue", cmdLine.ParamVal("test"));
        }

        [Fact]
        public void SimpleParamList()
        {
            var cmdLine = new CmdLineEz()
                .ParamList("test");
            cmdLine.Process("/test myTestValue1 myTestValue2 myTestValue3".Split(" "));
            var list = cmdLine.ParamListVal("test");
            Assert.True(CompareList(list, "myTestValue1", "myTestValue2", "myTestValue3"));
        }

        [Fact]
        public void SimpleFlagUnspecifiedParam()
        {
            var cmdLine = new CmdLineEz()
                .Param("test1")
                .Flag("test");
            cmdLine.Process("/test1 testVal".Split(" "));
            Assert.Null(cmdLine.FlagVal("test"));
        }


        [Fact]
        public void SimpleParamUnspecifiedParam()
        {
            var cmdLine = new CmdLineEz()
                .Param("test1")
                .Flag("test");
            cmdLine.Process(new[] { "/test" });
            Assert.Null(cmdLine.ParamVal("test1"));
        }

        [Fact]
        public void SimpleParamListUnspecifiedParam()
        {
            var cmdLine = new CmdLineEz()
                .ParamList("test1")
                .Flag("test");
            cmdLine.Process(new[] { "/test" });
            Assert.Null(cmdLine.ParamListVal("test1"));
        }

        [Fact]
        public void MultipleParams()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test")
                .Param("test2");

            cmdLine.Process("/test /test2 myTestValue".Split(" "));
            var test = cmdLine.FlagVal("test");
            var test2 = cmdLine.ParamVal("test2");
            Assert.Equal(true, test);
            Assert.Equal("myTestValue", test2);

            // Check order doesn't matter
            cmdLine.Process("/test2 myTestValue /test".Split(" "));
            test = cmdLine.FlagVal("test");
            test2 = cmdLine.ParamVal("test2");
            Assert.Equal(true, test);
            Assert.Equal("myTestValue", test2);
        }


        [Fact]
        public void Trailing()
        {
            var cmdLine = new CmdLineEz()
                .AllowTrailing();

            cmdLine.Process("test1 test2 test3".Split(" "));
            var val = cmdLine.TrailingVal();
            Assert.True(CompareList(val, "test1", "test2", "test3"));
        }

        [Fact]
        public void ParamListAndTrailing()
        {
            var cmdLine = new CmdLineEz()
                .ParamList("test")
                .Flag("test2")
                .AllowTrailing();
            cmdLine.Process("/test myTestValue1 myTestValue2 myTestValue3 /test2 trailing1 trailing2".Split(" "));
            Assert.True(CompareList(cmdLine.ParamListVal("test"),
                "myTestValue1", "myTestValue2", "myTestValue3"));
            Assert.True(CompareList(cmdLine.TrailingVal(),
                "trailing1","trailing2"));
            Assert.True(cmdLine.FlagVal("test2"));
        }

        [Fact]
        public void SimpleFlagWithAction()
        {
            bool flag = false;
            var cmdLine = new CmdLineEz()
                .Flag("test", (_, v) => flag = v);
            cmdLine.Process(new[] { "/test" });
            Assert.True(flag);
        }

        [Fact]
        public void SimpleParamWithAction()
        {
            string s = "";
            var cmdLine = new CmdLineEz()
                .Param("test",  (_, val) => s= val);
            cmdLine.Process("/test myTestValue".Split(" "));
            Assert.Equal("myTestValue", s);
        }

        [Fact]
        public void SimpleParamListWithAction()
        {
            List<string> list = new List<string>();
            var cmdLine = new CmdLineEz()
                .ParamList("test", (_, v) => list = v);
            cmdLine.Process("/test myTestValue1 myTestValue2 myTestValue3".Split(" "));
            Assert.True(CompareList(list, "myTestValue1", "myTestValue2", "myTestValue3"));
        }

        [Fact]
        public void MultipleParamsWithAction()
        {
            bool f = false;
            string s = "";
            var cmdLine = new CmdLineEz()
                .Flag("test", (_,v) => f = v)
                .Param("test2", (_, v) => s = v);

            cmdLine.Process("/test /test2 myTestValue".Split(" "));
            Assert.True(f);
            Assert.Equal("myTestValue", s);
        }


        [Fact]
        public void TrailingWithAction()
        {
            List<string> list = new List<string>();
            var cmdLine = new CmdLineEz()
                .AllowTrailing((v) => list = v);

            cmdLine.Process("test1 test2 test3".Split(" "));
            Assert.True(CompareList(list, "test1", "test2", "test3"));
        }

        [Fact]
        public void ParamListAndTrailingWithAction()
        {
            List<string> paramList = new List<string>();
            bool flag = false;
            List<string> trailing = new List<string>();
            var cmdLine = new CmdLineEz()
                .ParamList("test", (_,v) => paramList = v)
                .Flag("test2", (_, v) => flag = v)
                .AllowTrailing((l) => trailing = l);
            cmdLine.Process("/test myTestValue1 myTestValue2 myTestValue3 /test2 trailing1 trailing2".Split(" "));
            Assert.True(CompareList(paramList, "myTestValue1", "myTestValue2", "myTestValue3"));
            Assert.True(CompareList(trailing, "trailing1", "trailing2"));
            Assert.True(flag);

        }

        [Fact]
        public void ParamAndTrailing()
        {
            var cmdLine = new CmdLineEz()
                .Param("test")
                .AllowTrailing();
            cmdLine.Process("/test myTestValue1 trailing1 trailing2".Split(" "));
            Assert.True(CompareList(cmdLine.TrailingVal(),
                "trailing1", "trailing2"));
            Assert.Equal("myTestValue1", cmdLine.ParamVal("test"));
        }

        [Fact]
        public void IgnoreCaseSpecHasUpperCase()
        {
            var cmdLine = new CmdLineEz()
                .Flag("Test");
            cmdLine.Process(new[] { "/test" });
            Assert.Equal(true, cmdLine.FlagVal("Test"));
        }

        [Fact]
        public void IgnoreCaseArgsHasUpperCase()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test");
            cmdLine.Process(new[] { "/Test" });
            Assert.Equal(true, cmdLine.FlagVal("test"));
        }

        [Fact]
        public void DoNotIgnoreCaseSpecHasUpperCase()
        {
            var cmdLine = new CmdLineEz()
                .Flag("Test")
                .ConsiderCase();
            bool hasException = false;
            try
            {
                cmdLine.Process(new[] {"/test"});
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void DoNotIgnoreCaseSpecHasUpperCaseAndSemiAmbiguousMatch()
        {
            var cmdLine = new CmdLineEz()
                .Flag("Test")
                .Flag("Test2")
                .ConsiderCase();
            cmdLine.Process(new[] { "/Test" });
            Assert.True(cmdLine.FlagVal("Test"));
        }

        [Fact]
        public void DoNotIgnoreCaseArgsHasUpperCase()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test")
                .ConsiderCase();
            bool hasException = false;
            try
            {
                cmdLine.Process(new[] { "/Test" });
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void PartialMatch()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test")
                .Flag("temp");
            cmdLine.Process(new[] { "/tes" });
            Assert.Equal(true, cmdLine.FlagVal("test"));
        }

        [Fact]
        public void AmbiguousPartialMatch()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test")
                .Flag("temp");
            bool hasException = false;
            try
            {
                cmdLine.Process(new[] { "/te" });
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void EnsureProcessedBeforeUsing()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test");
            // No call to cmdLine.Process
            bool hasException = false;
            try
            {
                var _ = cmdLine.FlagVal("test");
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void AllowDifferentAndMixedFlags()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test1")
                .Flag("test2")
                .Flag("test3");
            cmdLine.Process("/test1 --test2 -test3".Split(" "));
            Assert.True(cmdLine.FlagVal("test1"));
            Assert.True(cmdLine.FlagVal("test2"));
            Assert.True(cmdLine.FlagVal("test3"));
        }

        [Fact]
        public void TrailingNotSpecified()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test");
            bool hasException = false;
            try
            {
                // Trailing not allowed by default
                cmdLine.Process("/test test1 test2 test3".Split(" "));
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void DuplicateFlagArg()
        {
            bool hasException = false;
            try
            {
                var _ = new CmdLineEz()
                    .Flag("test")
                    .Flag("test");
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void DuplicateParamArg()
        {
            bool hasException = false;
            try
            {
                var _ = new CmdLineEz()
                    .Param("test")
                    .Param("test");
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void DuplicateParamListArg()
        {
            bool hasException = false;
            try
            {
                var _ = new CmdLineEz()
                    .ParamList("test")
                    .ParamList("test");
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void DuplicateMultiTypeArg1()
        {
            bool hasException = false;
            try
            {
                var _ = new CmdLineEz()
                    .ParamList("test")
                    .Param("test");
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void DuplicateMultiTypeArg2()
        {
            bool hasException = false;
            try
            {
                var _ = new CmdLineEz()
                    .ParamList("test")
                    .Flag("test");
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void DuplicateMultiTypeArg3()
        {
            bool hasException = false;
            try
            {
                var _ = new CmdLineEz()
                    .Param("test")
                    .Flag("test");
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void TrailingTwice()
        {
            bool hasException = false;
            try
            {
                var _ = new CmdLineEz()
                    .AllowTrailing()
                    .AllowTrailing();
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }


        [Fact]
        public void DuplicateRunTimeFlag()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test");
            bool hasException = false;
            try
            {
                cmdLine.Process("/test /test".Split(" "));
            }
            catch
            {
                hasException = true;
            }

            Assert.True(hasException);
        }

        [Fact]
        public void DuplicateRunTimeParam()
        {
            var cmdLine = new CmdLineEz()
                .Param("test");
            bool hasException = false;
            try
            {
                cmdLine.Process("/test myTestValue /test myTestValue".Split(" "));
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void DuplicateRunTimeParamList()
        {
            var cmdLine = new CmdLineEz()
                .ParamList("test");
            bool hasException = false;
            try
            {
                cmdLine.Process("/test myTestValue1 myTestValue2 myTestValue3 /test myTestValue4".Split(" "));
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void ParamMissingArg()
        {
            bool hasException = false;
            try
            {
                var cmdLine = new CmdLineEz()
                    .Param("test");
                cmdLine.Process("/test".Split(" "));
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void ParamListMissingArg()
        {
            bool hasException = false;
            try
            {
                var cmdLine = new CmdLineEz()
                    .ParamList("test");
                cmdLine.Process("/test".Split(" "));
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void SimpleFlagInvalidType()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test");
            cmdLine.Process(new[] { "/test" });
            bool hasException = false;
            try
            {
                var _ = cmdLine.ParamVal("test");
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void SimpleParamInvalidType()
        {
            var cmdLine = new CmdLineEz()
                .Param("test");
            cmdLine.Process("/test myTestVal".Split(" "));
            bool hasException = false;
            try
            {
                var _ = cmdLine.FlagVal("test");
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void SimpleParamListInvalidType()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test");
            cmdLine.Process(new[] { "/test" });
            bool hasException = false;
            try
            {
                var _ = cmdLine.ParamListVal("test");
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void FailWhenNoParams()
        {
            var cmdLine = new CmdLineEz();
            bool hasException = false;
            try
            {
                cmdLine.Process(new[] {"/test"});
            }
            catch
            {
                hasException = true;
            }

            Assert.True(hasException);
        }

        [Fact]
        public void FailWhenMissingRequired()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test")
                .Flag("test2")
                .Required("test");
            bool hasException = false;
            try
            {
                cmdLine.Process(new[] { "/test2" });
            }
            catch
            {
                hasException = true;
            }

            Assert.True(hasException);
        }

        [Fact]
        public void FailWhenRequiredHasNoMatch()
        {
            bool hasException = false;
            try
            {
                var _ = new CmdLineEz()
                    .Flag("test")
                    .Flag("test2")
                    .Required("test4");
            }
            catch
            {
                hasException = true;
            }

            Assert.True(hasException);
        }

        [Fact]
        public void FailWhenRequiredBeforeStart()
        {
            bool hasException = false;
            try
            {
                var _ = new CmdLineEz()
                    .Required("test")
                    .Flag("test");
            }
            catch
            {
                hasException = true;
            }

            Assert.True(hasException);
        }

        [Fact]
        public void RequiredFirst()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test")
                .RequiredFirst("test")
                .Param("otherflag");
            cmdLine.Process("/test /otherflag param".Split(" "));
            Assert.True(cmdLine.FlagVal("test"));
            Assert.Equal("param", cmdLine.ParamVal("otherflag"));
        }

        [Fact]
        public void RequiredFirstButMissing()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test")
                .RequiredFirst("test")
                .Param("otherflag");
            bool hasException = false;
            try
            {
                cmdLine.Process("/otherflag param".Split(" "));
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void OptionaFirstButNotFirst()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test")
                .OptionalFirst("test")
                .Param("otherflag");
            bool hasException = false;
            try
            {
                cmdLine.Process("/otherflag param /test".Split(" "));
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        [Fact]
        public void OptionalFirst()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test")
                .OptionalFirst("test")
                .Param("otherflag");
            cmdLine.Process("/test /otherflag param".Split(" "));
            Assert.True(cmdLine.FlagVal("test"));
            Assert.Equal("param", cmdLine.ParamVal("otherflag"));
        }

        [Fact]
        public void OptionalFirstButMissing()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test")
                .OptionalFirst("test")
                .Param("otherflag");
            cmdLine.Process("/otherflag param".Split(" "));
            Assert.Equal("param", cmdLine.ParamVal("otherflag"));
        }




        [Fact]
        public void RequiredFirstButNotFirst()
        {
            var cmdLine = new CmdLineEz()
                .Flag("test")
                .RequiredFirst("test")
                .Param("otherflag");
            bool hasException = false;
            try
            {
                cmdLine.Process("/otherflag param /test".Split(" "));
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }


        [Fact]
        public void RequiredFirstButNotSpecified()
        {
            bool hasException = false;
            try
            {
                var _= new CmdLineEz()
                    .Param("otherflag")
                    .RequiredFirst("test");
            }
            catch
            {
                hasException = true;
            }
            Assert.True(hasException);
        }

        // Utilities
        private bool CompareList(List<string> l1, params string[] compareTo)
        {
            return CompareList(l1, compareTo.ToList());
        }

        private bool CompareList(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count)
                return false;
            for(var i=0; i < list1.Count; i++)
                if (list1[i] != list2[i])
                    return false;
            return true;
        }

    }
}
