using System.Collections.Generic;
using System.Linq;
using CmdLineEzNs;
using Xunit;

// ReSharper disable once CheckNamespace
namespace CmdLinEzTestNs
{
    public class CmdLineEzTest
    {
        private static readonly string TestJson =
            @$"{{
	            ""Flag1"": true,
	            ""flag2"": false,
	            ""pAram1"": ""configuration file param value 1"",
	            ""param2"": ""configuration file param value 2"",
	            ""Param3"": ""configuration file param value 3"",
	            ""ParamList1"": [
		            ""configuration file list value 1"",
		            ""configuration file list value 2"",
		            ""configuration file list value 3""
	            ]
            }}";


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

        [Fact]
        public void DefaultFlag()
        {
            const bool expectedValue = true;
            const string parameterName = "test";

            var cmdLine = new CmdLineEz()
                .Flag(parameterName, defaultValue: expectedValue);
            cmdLine.Process(new string[0]);

            Assert.Equal(expectedValue, cmdLine.FlagVal(parameterName));
        }


        [Fact]
        public void DefaultParameter()
        {
            const string expectedValue = "default value";
            const string parameterName = "test";

            var cmdLine = new CmdLineEz()
                .Param(parameterName, defaultValue: expectedValue);
            cmdLine.Process(new string[0]);

            Assert.Equal(expectedValue, cmdLine.ParamVal(parameterName));
        }

        [Fact]
        public void DefaultParameterList()
        {
            var expectedValues = new List<string> {"default value 1", "default value 2", "default value 3"};
            const string parameterName = "test";

            var cmdLine = new CmdLineEz()
                .ParamList(parameterName, defaultValues: expectedValues);
            cmdLine.Process(new string[0]);

            Assert.True(CompareList(expectedValues, cmdLine.ParamListVal(parameterName)));
        }

        [Fact]
        public void DefaultConfiguration()
        {
            const string paramList1Val1 = "default value 1";
            const string paramList1Val2 = "default value 2";
            const string paramList1Val3 = "default value 3";

            const string configurationParameterName = "config";

            var defaultConfiguration = new
            {
                Flag1 = true,
                Flag2 = false,
                Param1 = "test value 1",
                Param2 = "test value 2",
                ParamList1 = new List<string> {paramList1Val1, paramList1Val2, paramList1Val3}
            };

            var cmdLine = new CmdLineEz()
                .Config(configurationParameterName, defaultConfiguration)
                .Flag("flag1")
                .Flag("flag2")
                .Param("param1")
                .Param("param2")
                .ParamList("paramlist1");
            cmdLine.Process(new string[0]);

            Assert.Equal(defaultConfiguration.Flag1, cmdLine.FlagVal("flag1"));
            Assert.Equal(defaultConfiguration.Flag2, cmdLine.FlagVal("flag2"));
            Assert.Equal(defaultConfiguration.Param1, cmdLine.ParamVal("param1"));
            Assert.Equal(defaultConfiguration.Param2, cmdLine.ParamVal("param2"));
            Assert.True(CompareList(defaultConfiguration.ParamList1, cmdLine.ParamListVal("paramlist1")));
        }

        [Fact]
        public void PassedFlagHasPrecedenceOverDefaultFlag()
        {
            const bool expectedValue = true;
            const bool defaultValue = false;
            const string parameterName = "test";

            var cmdLine = new CmdLineEz()
                .Flag(parameterName, defaultValue: defaultValue);
            cmdLine.Process(new[] { "/test" });

            Assert.Equal(expectedValue, cmdLine.FlagVal(parameterName));
        }

        [Fact]
        public void PassedParameterHasPrecedenceOverDefaultParameter()
        {
            const string expectedValue = "test value";
            const string defaultValue = "default value";
            const string parameterName = "test";

            var cmdLine = new CmdLineEz()
                .Param(parameterName, defaultValue: defaultValue);
            cmdLine.Process(new[] { "/test", expectedValue });

            Assert.Equal(expectedValue, cmdLine.ParamVal(parameterName));
        }

        [Fact]
        public void PassedParameterListHasPrecedenceOverDefaultParameterList()
        {
            var defaultList = new List<string>() { "default value 1", "default value 2" };

            const string expectedValue1 = "expected value 1";
            const string expectedValue2 = "expected value 2";
            const string expectedValue3 = "expected value 3";

            var expectedList = new List<string> { expectedValue1, expectedValue2, expectedValue3 };

            const string parameterName = "test";

            var cmdLine = new CmdLineEz()
                .ParamList(parameterName, defaultValues: defaultList);
            cmdLine.Process(new[] { "/test", expectedValue1, expectedValue2, expectedValue3 });

            Assert.True(CompareList(expectedList, cmdLine.ParamListVal(parameterName)));
        }

        [Fact]
        public void DefaultValueHasPrecedenceOverDefaultConfigurationObject()
        {
            const string paramList1Val1 = "list value 1";
            const string paramList1Val2 = "list value 2";
            const string paramList1Val3 = "list value 3";

            const string configurationParameterName = "config";

            var defaultConfiguration = new
            {
                Flag1 = true,
                Flag2 = false,
                Param1 = "test value 1",
                Param2 = "test value 2",
                ParamList1 = new List<string> { paramList1Val1, paramList1Val2, paramList1Val3 }
            };

            var defaultParameterList = new List<string>() {"default list value 1", "default list value 2"};
            var cmdLine = new CmdLineEz()
                .Config(configurationParameterName, defaultConfiguration)
                .Flag("flag1", defaultValue: false)
                .Flag("flag2", defaultValue: true)
                .Param("param1", defaultValue: "default value 1")
                .Param("param2", defaultValue: "default value 2")
                .Param("param3")
                .ParamList("paramlist1", defaultValues: defaultParameterList);
            cmdLine.Process(new string[0]);

            Assert.Equal(false, cmdLine.FlagVal("flag1"));
            Assert.Equal(true, cmdLine.FlagVal("flag2"));
            Assert.Equal("default value 1", cmdLine.ParamVal("param1"));
            Assert.Equal("default value 2", cmdLine.ParamVal("param2"));
            Assert.True(CompareList(defaultParameterList, cmdLine.ParamListVal("paramlist1")));
        }

        [Fact]
        public void ConfigurationFileHasPrecedenceOverDefaultConfigurationObject()
        {
            //this values provided by test json
            const bool configurationFileFlag1 = true;
            const bool configurationFileFlag2 = false;
            const string configurationFileParam1 = "configuration file param value 1";
            const string configurationFileParam2 = "configuration file param value 2";
            const string configurationFileParam3 = "configuration file param value 3";
            const string configurationFileListValue1 = "configuration file list value 1";
            const string configurationFileListValue2 = "configuration file list value 2";
            const string configurationFileListValue3 = "configuration file list value 3";
            var configurationFileParameterList = new List<string>
                {configurationFileListValue1, configurationFileListValue2, configurationFileListValue3};

            const string defaultParamListValue1 = "default list value 1";
            const string defaultParamListValue2 = "default list value 2";
            const string defaultParamListValue3 = "default list value 3";

            const string configurationParameterName = "config";

            var defaultConfiguration = new
            {
                Flag1 = true,
                Flag2 = false,
                Param1 = "configuration object parameter value 1",
                Param2 = "configuration object parameter value 2",
                Param4 = "configuration object parameter value 4",
                Param5 = "configuration object parameter value 5",
                ParamList1 = new List<string> { defaultParamListValue1, defaultParamListValue2, defaultParamListValue3 }
            };

            var cmdLine = new CmdLineEzForTest()
                .Config(configurationParameterName, defaultConfiguration)
                .Flag("flag1", defaultValue: false)
                .Flag("flag2", defaultValue: true)
                .Flag("flag3", defaultValue: true)
                .Param("param1", defaultValue: "default value 1")
                .Param("param2", defaultValue: "default value 2")
                .Param("param3")
                .Param("param4", defaultValue: "default value 4")
                .Param("param5")
                .ParamList("paramlist1", defaultValues: new List<string>() {"default list value 1", "default list value 2"});
            
            cmdLine.Process(new[] {"/config", "testfile.json", "/param2", "command line parameter value 2"});

            //flag1 defined in configuration file so we expect it
            //as configuration file has precedence over default object and parameter defaults
            Assert.Equal(configurationFileFlag1, cmdLine.FlagVal("flag1"));

            //flag2 defined in configuration file so we expect it
            //as configuration file has precedence over default object and parameter defaults
            Assert.Equal(configurationFileFlag2, cmdLine.FlagVal("flag2"));

            //flag3 was only defined as parameter default so we expect it
            Assert.Equal(true, cmdLine.FlagVal("flag3"));

            //param1 defined in configuration file so we expect it
            //as configuration file has precedence over default object and parameter defaults
            Assert.Equal(configurationFileParam1, cmdLine.ParamVal("param1"));

            //param2 defined in command line so we expect it
            //as command line parameter value has precedence over all other parameter values
            Assert.Equal("command line parameter value 2", cmdLine.ParamVal("param2"));

            //param3 defined in configuration file so we expect it
            //as configuration file has precedence over default object and parameter defaults
            Assert.Equal(configurationFileParam3, cmdLine.ParamVal("param3"));

            //param4 defined both in default object and parameter defaults so we expect parameter defaults
            //as it has precedence over default object
            Assert.Equal("default value 4", cmdLine.ParamVal("param4"));

            //param5 defined both only in default object so we expect it
            //as it has precedence over default object
            Assert.Equal(defaultConfiguration.Param5, cmdLine.ParamVal("param5"));


            Assert.True(CompareList(configurationFileParameterList, cmdLine.ParamListVal("paramlist1")));
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

        public class CmdLineEzForTest : CmdLineEz
        {
            protected override string OpenFileAsText(string path)
            {
                return TestJson;
            }
        }
    }
}
