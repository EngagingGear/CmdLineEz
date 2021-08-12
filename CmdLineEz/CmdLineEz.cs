﻿using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global
// ReSharper disable once CheckNamespace

namespace CmdLineEzNs
{
    /// <summary>
    /// Processor for command line arguments.
    /// </summary>
   public class CmdLineEz
    {
        /// <summary>
        /// Constructor, use fluid functions on thi like Flag, Param, Required, ParamList, AllowTrailing, ConsiderCase
        /// <code>
        ///    new cmdLineEz = new CmdLineEz()
        ///            .Flag("flag-name") // a single flag argument
        ///            .Param("param")   // takes one parameter
        ///            .Required("param")// param is required
        ///            .ParamList("list")// A list of parameters
        ///            .AllowTrailing()  // arguments not used by the flags are passed to a trailing array
        /// </code>
        ///
        /// So you can pass in command arguments like this:
        ///      program.exe /param myParam /list item1 item2 item3 /flag-name trailing1 trailing2
        /// Which will set
        ///    * flag-name parameter to true (obtain from FlagVal("flag-name")
        ///    * param parameter to "myParam" (obtain from the ParamVal("param")
        ///    * list parameter to {"item1", "item2", "item3"} (obtain from ParamList("list")
        ///    * and the trailing parameters to {"trailing1", "trailing2"}
        /// </summary>
        // ReSharper disable once EmptyConstructor
        public CmdLineEz()
        {

        }

        /// <summary>
        /// Once you have set up the fields, pass the args array into this to process it and set the
        /// values.
        /// </summary>
        /// <param name="args">Array of args, generally speaking pass in args from the Main function
        /// </param>
        public void Process(string[] args)
        {
            if (_params == null)
                throw new ArgumentException("No parameter specification loaded.");
            _values = new();
            _paramsIndexed = new();
            foreach (var param in _params)
                _paramsIndexed[param.Name] = param;

            var requiredParams = new HashSet<string>(_params.Where(p => p.Required).Select(p => p.Name));
            
            for (int paramNum = 0; paramNum < args.Length; paramNum++)
            {
                var param = args[paramNum];
                if (param.StartsWith("/"))
                    param = param.Substring(1);
                else if (param.StartsWith("--"))
                    param = param.Substring(2);
                else if (param.StartsWith("-"))
                    param = param.Substring(1);
                else
                {
                    // Not a param, so scan the reset in to remaining.
                    if (_remainderAction == null)
                        throw new ArgumentException("Extra arguments, and no trailing parameters specified.");
                    _trailingList = new List<string>();
                    for (; paramNum < args.Length; paramNum++)
                        _trailingList.Add(args[paramNum]);
                    _remainderAction?.Invoke(_trailingList);
                    break;
                }

                List<CmdLineParam> cmdLineParamList;
                if (_ignoreCase)
                    cmdLineParamList = _params.Where(p => p.Name.ToLower().StartsWith(param.ToLower())).ToList();
                else
                    cmdLineParamList = _params.Where(p => p.Name.StartsWith(param)).ToList();
                if (! cmdLineParamList.Any())
                    throw new ArgumentException($"Invalid parameter {param}");
                else if (cmdLineParamList.Count > 1)
                {
                    // If we have an ambiguous prefix, check if one parameter matches in full.
                    // For example, if we have two parameters defined /test, /test1 if the user
                    // passes /test then it matches both on the prefix, but we prefer the full match
                    // in this specific case.
                    if (_ignoreCase)
                        cmdLineParamList = _params.Where(p => p.Name.ToLower() == param.ToLower()).ToList();
                    else
                        cmdLineParamList = _params.Where(p => p.Name == param).ToList();
                    if(cmdLineParamList.Count != 1)
                        throw new ArgumentException($"Ambiguous parameter {param}");
                }

                var cmdParam = cmdLineParamList.First();
                if (_requiredFirst != null && paramNum == 0 && cmdParam.Name != _requiredFirst)
                    throw new ArgumentException(
                        $"Parameter {cmdParam.Name} must be first");
                if (_optionalFirst != null && paramNum != 0 && cmdParam.Name == _optionalFirst)
                    throw new ArgumentException(
                        $"Parameter {cmdParam.Name} must be first");
                requiredParams.Remove(cmdParam.Name);
                switch (cmdParam.Type)
                {
                    case CmdLineParamType.Flag:
                        if (_values.ContainsKey(cmdParam.Name))
                            throw new ArgumentException($"Duplicate parameter {cmdParam.Name}");
                        if(cmdParam.Store != null)
                            cmdParam.Store.Invoke(cmdParam.Name, (bool?) true);
                        else
                            _values[cmdParam.Name] = (bool?)true;
                        break;
                    case CmdLineParamType.Param:
                        if (_values.ContainsKey(cmdParam.Name))
                            throw new ArgumentException($"Duplicate parameter {cmdParam.Name}");
                        paramNum++;
                        if (paramNum < args.Length)
                        {
                            if (cmdParam.Store != null)
                                cmdParam.Store.Invoke(cmdParam.Name, args[paramNum]);
                            else
                                _values[cmdParam.Name] = args[paramNum];
                        }
                        else
                        {
                            throw new ArgumentException($"Parameter {cmdParam.Name} missing parameter");
                        }
                        break;
                    case CmdLineParamType.ParamList:
                        if (_values.ContainsKey(cmdParam.Name))
                            throw new ArgumentException($"Duplicate parameter {cmdParam.Name}");
                        paramNum++;
                        if (paramNum < args.Length)
                        {
                            var list = new List<string>();
                            while (paramNum < args.Length)
                            {
                                var val = args[paramNum++];
                                if (val.StartsWith("/") || val.StartsWith("-"))
                                {
                                    paramNum -= 2;
                                    break;
                                }
                                list.Add(val);
                            }
                            if (cmdParam.Store != null)
                                cmdParam.Store.Invoke(cmdParam.Name, list);
                            else
                                _values[cmdParam.Name] = list;
                        }
                        else
                        {
                            throw new ArgumentException($"Parameter {cmdParam.Name} must have at least one parameter");
                        }
                        break;
                }
            }

            if (requiredParams.Any())
                throw new ArgumentException($"Missing required parameters: {string.Join(", ", requiredParams)}");
        }

        /// <summary>
        /// Get the value for flag parameter with given name. If no such parameter was specified
        /// throws an exception, if no such parameter was passed on the command line return null.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool? FlagVal(string name, bool? defaultValue = null)
        {
            if (_paramsIndexed == null || 
                !_paramsIndexed.ContainsKey(name) ||
                _paramsIndexed[name].Type != CmdLineParamType.Flag)
                throw new ArgumentException($"Parameter {name} not defined or is not a flag value");
            if (_values.ContainsKey(name))
                return _values[name] as bool?;
            else
                return defaultValue;
        }

        /// <summary>
        /// Get the value for param parameter with given name. If no such parameter was specified
        /// throws an exception, if no such parameter was passed on the command line return null.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string ParamVal(string name)
        {
            if (_paramsIndexed == null ||
                !_paramsIndexed.ContainsKey(name) ||
                _paramsIndexed[name].Type != CmdLineParamType.Param)
                throw new ArgumentException($"Parameter {name} not defined or is not a param value");
            if (_values.ContainsKey(name))
                return _values[name] as string;
            else
                return null;
        }

        /// <summary>
        /// Get the value for param list parameter with given name. If no such parameter was specified
        /// throws an exception, if no such parameter was passed on the command line return null.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<string> ParamListVal(string name)
        {
            if (_paramsIndexed == null ||
                !_paramsIndexed.ContainsKey(name) ||
                _paramsIndexed[name].Type != CmdLineParamType.ParamList)
                throw new ArgumentException($"Parameter {name} not defined or is not a param list value");
            if (_values.ContainsKey(name))
                return _values[name] as List<string>;
            else
                return null;
        }

        /// <summary>
        /// Get the trailing items. Note you must specify AllowTrailing() to the object to allows this.
        /// </summary>
        /// <returns></returns>
        public List<string> TrailingVal()
        {
            return _trailingList;
        }

        /// <summary>
        /// Specify a flag parameter, of given name, and optionally an action function called
        /// when it is found. If no action function you can obtain its value from FlagVal(name) function.
        /// Note this returns this to allow chaining of these specifications.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public CmdLineEz Flag(string name, Action<string, bool> store = null)
        {
            _params ??= new List<CmdLineParam>();
            if(_params.Any(p => p.Name == name))
                throw new ArgumentException($"Duplicate parameter defined {name}");
            Action<string, object> fn = null;
            if (store != null)
                fn = (s, v) => store(s, (bool) v);
            _params.Add(new CmdLineParam
                {
                    Name = name,
                    Type = CmdLineParamType.Flag,
                    Required = false,
                    Store = fn,
                });
            return this;
        }

        /// <summary>
        /// Specify a param parameter, of given name, and optionally an action function called
        /// when it is found. If no action function you can obtain its value from ParamVal(name) function.
        /// Note this returns this to allow chaining of these specifications.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public CmdLineEz Param(string name, Action<string, string> store = null)
        {
            _params ??= new List<CmdLineParam>();
            if (_params.Any(p => p.Name == name))
                throw new ArgumentException($"Duplicate parameter defined {name}");
            Action<string, object> fn = null;
            if (store != null)
                fn = (s, v) => store(s, v as string);
            _params.Add(new CmdLineParam
            {
                Name = name,
                Type = CmdLineParamType.Param,
                Required = false,
                Store = fn,
            });
            return this;
        }

        /// <summary>
        /// Specify a param list parameter, of given name, and optionally an action function called
        /// when it is found. If no action function you can obtain its value from ParamListVal(name) function.
        /// Note this returns this to allow chaining of these specifications.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public CmdLineEz ParamList(string name, Action<string, List<string>> store = null)
        {
            _params ??= new List<CmdLineParam>();
            if (_params.Any(p => p.Name == name))
                throw new ArgumentException($"Duplicate parameter defined {name}");
            Action<string, object> fn = null;
            if (store != null)
                fn = (s, v) => store(s, v as List<string>);
            _params.Add(new CmdLineParam
            {
                Name = name,
                Type = CmdLineParamType.ParamList,
                Required = false,
                Store = fn,
            });
            return this;
        }

        /// <summary>
        /// Specify that the command line allows trailing parameters.
        /// Returns "this" to allow fluid chaining.
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public CmdLineEz AllowTrailing(Action<List<string>> store = null)
        {
            _params ??= new List<CmdLineParam>();
            if (_remainderAction != null)
                throw new ArgumentException("May only specify one AllowTrailing");
            _remainderAction = store ?? ((list) => _trailingList = list);
            return this;
        }

        /// <summary>
        /// Indicates that the specified parameter is required -- it is an error to omit it.
        /// If it is omitted then Process will throw an exception.
        /// Returns "this" to allow fluid chaining.
        /// </summary>
        /// <returns></returns>
        public CmdLineEz Required(string name)
        {
            if (_params == null)
                throw new ArgumentException($"Required parameter not defined {name}");
            var param = _params.FirstOrDefault(p => p.Name == name);
            if (param == null)
                throw new ArgumentException($"Required parameter not defined {name}");
            param.Required = true;
            return this;
        }

        /// <summary>
        /// Indicates that the specified parameter is required -- it is an error to omit it, and that
        /// it must be the first parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CmdLineEz RequiredFirst(string name)
        {
            if (_params == null)
                throw new ArgumentException($"Required parameter not defined {name}");
            var param = _params.FirstOrDefault(p => p.Name == name);
            if (param == null)
                throw new ArgumentException($"Required parameter not defined {name}");
            param.Required = true;
            _requiredFirst = name;
            return this;
        }

        /// <summary>
        /// Indicates that the specified parameter is optional, but if provided must be first.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CmdLineEz OptionalFirst(string name)
        {
            if (_params == null)
                throw new ArgumentException($"Optional first parameter not defined {name}");
            var param = _params.FirstOrDefault(p => p.Name == name);
            if (param == null)
                throw new ArgumentException($"Optional first parameter not defined {name}");
            param.Required = false;
            _optionalFirst = name;
            return this;
        }


        /// <summary>
        /// Normally case is ignored when matching parameter names, call this to turn that rule
        /// off and match on case.
        /// Returns "this" to allow fluid chaining.
        /// </summary>
        /// <returns></returns>
        public CmdLineEz ConsiderCase()
        {
            _ignoreCase = false;
            return this;
        }

        private enum CmdLineParamType { Flag, Param, ParamList };

        private class CmdLineParam
        {
            public string Name;
            public CmdLineParamType Type;
            public bool Required;
            public Action<string, object> Store { get; init; }
        }

        private Dictionary<string, object> _values;
        private List<CmdLineParam> _params;
        private Dictionary<string, CmdLineParam> _paramsIndexed;
        private Action<List<string>> _remainderAction;
        private List<string> _trailingList;
        private bool _ignoreCase = true;
        private string _requiredFirst;
        private string _optionalFirst;
    }
}
