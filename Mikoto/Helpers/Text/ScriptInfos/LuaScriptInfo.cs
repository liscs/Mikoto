using Microsoft.Scripting.Utils;
using NLua;
using System.IO;
using System.Text;

namespace Mikoto.Helpers.Text
{
    internal class LuaScriptInfo : ScriptInfo
    {
        protected override string Name { get => "Lua"; }
        protected override string FileExtension { get => "lua"; }
        protected override string FolderName { get => "lua"; }


        protected override TextPreProcesFunction? GetMethod(string scriptFile)
        {
            Lua lua = new();
            lua.State.Encoding = Encoding.UTF8;
            string script = File.ReadAllText(scriptFile);
            try
            {
                // 执行 Lua 脚本
                lua.DoString(script);
                // 获取 Lua 函数
                LuaFunction? function = GetAllCustomLuaFunctionNames(lua).FirstOrDefault();
                if (function != null)
                {
                    return p => (function?.Call(p).First() as string)!;
                }
                else
                {
                    Error = $"{scriptFile} contains no function";
                    return null;
                }
            }
            catch (Exception ex)
            {
                Error = $"{scriptFile} Execute Error{Environment.NewLine}{ex}";
                return null;
            }


        }


        // 获取所有自定义 Lua 函数的名称
        static List<LuaFunction> GetAllCustomLuaFunctionNames(Lua lua)
        {
            List<LuaFunction> customFunctionNames = new();

            // 获取全局环境中的所有键值对
            LuaTable globals = lua.GetTable("_G");
            foreach (KeyValuePair<object, object> keyValuePair in globals)
            {
                if (keyValuePair.Key is string key)
                {
                    var value = keyValuePair.Value;

                    // 过滤标准库函数和表，仅保留自定义函数
                    if (value is LuaFunction function && !IsStandardLibraryFunction(key))
                    {
                        customFunctionNames.Add(function);
                    }
                }
            }

            return customFunctionNames;
        }

        // 判断是否为标准库函数
        static bool IsStandardLibraryFunction(string functionName)
        {
            // 标准库函数和表的名称
            HashSet<string> standardLibraryFunctions =
            [
                "assert", "collectgarbage", "dofile", "error", "getmetatable", "ipairs", "load", "loadfile", "next",
                "pairs", "pcall", "print", "rawequal", "rawget", "rawlen", "rawset", "require", "select", "setmetatable",
                "tonumber", "tostring", "type", "xpcall", "_G", "_VERSION", "coroutine", "debug", "io", "math", "os",
                "package", "string", "table", "utf8", "warn"
            ];

            return standardLibraryFunctions.Contains(functionName);
        }




        protected override void ReleaseResourse()
        {
        }

        protected override void InitEngine()
        {
        }
    }


}

