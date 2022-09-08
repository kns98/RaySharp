using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Reflection;

namespace minlightcsfs
{
    public static class Scanf
    {

        public static string getLine(TextReader f)
        {
            bool empty = true;
            string s = "";
            while (empty)
            {
                string i = f.ReadLine().Trim();
                if (i.Length == 0)
                {
                    empty = false;
                }
                else
                {
                    s = i;
                }
            }
            return s;
        }

        public static t sscanf<a, b, c, d, t>(PrintfFormat<a, b, c, d, t> pf, string s)
        {
            string formatStr = pf.Value;
            string[] constants = formatStr.Split(new string[]
            {
                "%s"
            }, StringSplitOptions.None);
            string text = "^";
            string text2 = "(.*?)";
            string[] array = constants;
            string[] array2 = array;
            if (array2 == null)
            {
                throw new ArgumentNullException("array");
            }
            string[] array3 = new string[array2.Length];
            string separator = text2;
            string str = text;
            for (int i = 0; i < array3.Length; i++)
            {
                array3[i] = Regex.Escape(array2[i]);
            }
            Regex regex = new Regex(str + string.Join(separator, array3) + "$");
            GroupCollection groups = regex.Match(s).Groups;
            GroupCollection source = groups;
            IEnumerable<Group> source2 = SeqModule.Cast<Group>(source);
            IEnumerable<Group> source3 = SeqModule.Skip<Group>(1, source2);
            IEnumerable<object> matches = SeqModule.Map<Group, object>(Scanf.matches_32.@_instance, source3);
            IEnumerable<object> source4 = matches;
            return LanguagePrimitives.IntrinsicFunctions.UnboxGeneric<t>(FSharpValue.MakeTuple(SeqModule.ToArray<object>(source4), typeof(t)));
        }


        internal sealed class matches_32 : FSharpFunc<Group, object>
        {

            internal matches_32()
            {
            }


            public override object Invoke(Group g)
            {
                string value = g.Value;
                string text = value;
                return text;
            }


            internal static readonly Scanf.matches_32 @_instance = new Scanf.matches_32();
        }
    }
}
